using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BubbleManager : MonoBehaviour {

	#region Prefabs
	public GameObject ball = null;
    public GameObject box = null;
    public GameObject cannon = null;
	#endregion

	public GameObject compressor;
	private int _compressorLevel;

	public int timeBeforeNewRoot = 3;
	private int _newRoot;
	
	public float collisionEpsilon = 0.82f;
	
	private BubbleGrid _grid;
	
	private Bubble _shooter = null;
	private float _shootingAngle = 90.0f;
	
	private Bubble _next;
	
	private bool _fired = false;

	private LBRect _boundRect;

    private Vector2 ballMoveDir = Vector2.up;
	
	#region :: Sound ::
	public AudioClip destroyGroupClip;
	public AudioClip hurryClip;
	public AudioClip launchClip;
	public AudioClip loseClip;
	public AudioClip newRootSoloClip;
	public AudioClip nohClip;
	public AudioClip reboundClip;
	public AudioClip stickClip;
	#endregion

	#region :: Bubbles ::
	public Sprite _bubble_1;
	public Sprite _bubble_2;
	public Sprite _bubble_3;
	public Sprite _bubble_4;
	public Sprite _bubble_5;
	public Sprite _bubble_6;
	public Sprite _bubble_7;
	public Sprite _bubble_8;
    #endregion

    #region :: Boxes ::
    public Sprite _box_1;
    public Sprite _box_2;
    public Sprite _box_3;
    public Sprite _box_4;
    public Sprite _box_5;
    public Sprite _box_6;
    public Sprite _box_7;
    public Sprite _box_8;
    #endregion

    #region :: Cached Internal Variables ::
    // cached variables in Update()
    private float _dx;
    private float _dy;

    // cached usage in GetChain()
	private bool[,] _visited;
	#endregion
	
	private enum RemoveType {
		ChainRemoval,
		UnlinkRemoval
	}
	
	void Start ()
	{
		_grid = this.GetComponent<BubbleGrid>();
	
		_visited = new bool[G.rows, G.cols];
		
		cannon.transform.position = GetShooterPosition();
		
		LoadNextLevel();
	}
	
	public void LoadNextLevel()
	{
		Levels.Instance.current++;

		if (_next != null)
		{
			Destroy(_next.gameObject);
			_next = null;		
		}
		if (_shooter != null)
		{
			Destroy(_shooter.gameObject);
			_shooter = null;		
		}
		
		_newRoot = 0;

		float realWidth = G.cols * G.radius * 2.0f;
		float realHeight = Mathf.Sqrt(3.0f) * (G.rows - 1) * G.radius + 2 * G.radius;
		
        _boundRect = new LBRect(-realWidth / 2.0f, -realHeight / 2.0f, realWidth, realHeight);

		iTween.Stop(compressor);
		compressor.transform.position = new Vector3(0, _boundRect.top, -1);
		_compressorLevel = 0;
		
		// load first
		Levels.Instance.Load();
		
		int number = Random.Range(0, Levels.Instance.Count - 1);
		LevelData ld = Levels.Instance.GetLevel(number);
		
		_grid.Reset();
		
		if (ld != null)
		{
			for (int i = 0; i < G.rows; i++)
			{
				for (int j = 0; j < G.cols; j++)
				{
					char ch = ld.data[i, j];
					if (ch != '-')
					{
						Box one = GetOneBoxAtPosition(Misc.IndexToPosition(_boundRect, new Index(i, j)), Box.CharToType(ch));
						_grid.Set(i, j, one);
					}
				}
			}
		}
		
		// initialize the shooter
		LoadShooterBubble();
	}
	
	void Update ()
	{
		if (!_fired) {
#if UNITY_EDITOR
			if (Input.GetMouseButtonUp(0))
			{
				Finger f = new Finger();
				f.x = Input.mousePosition.x;
				f.y = Input.mousePosition.y;
				
				HandleTouchBegan(f);
				HandleTouchEnded(f);
			}
#endif

#if UNITY_IPHONE || UNITY_ANDROID
			if (Input.touchCount > 0) {
				Touch touch = Input.GetTouch(0);
				
				Finger f = new Finger();
				f.x = touch.position.x;
				f.y = touch.position.y;
				
				if (touch.phase == TouchPhase.Began)
					HandleTouchBegan(f);
				if (touch.phase == TouchPhase.Moved)
					HandleTouchMoved(f);
				if (touch.phase == TouchPhase.Ended)
					HandleTouchEnded(f);
				if (touch.phase == TouchPhase.Canceled)
					HandleTouchCanceled(f);
			}
#else
			// mimic the touch event on Desktop platform
			if (Input.GetMouseButtonDown(0)) {
				Finger f = new Finger();
				f.x = Input.mousePosition.x;
				f.y = Input.mousePosition.y;

				HandleTouchBegan(f);
			} else if (Input.GetMouseButtonUp(0)) {
				Finger f = new Finger();
				f.x = Input.mousePosition.x;
				f.y = Input.mousePosition.y;

				HandleTouchEnded(f);
			} else if (Input.GetMouseButton(0)) {
				Finger f = new Finger();
				f.x = Input.mousePosition.x;
				f.y = Input.mousePosition.y;

				HandleTouchMoved(f);
			}
#endif
		}

		if (_fired) {
            MoveBall();

           // ParkingStateInfo info = IsFinalParkingIndex(G.bubbleSpeed, collisionEpsilon, _shooter.transform.position.x, _shooter.transform.position.y, _dx, _dy, true);

            //_dx = info.dx;
            //_dy = info.dy;

            //x = info.x;
            //y = info.y;

            //         if (info.final)
            //         {
            //             //ParkBubble(new Vector3(x, y));                
            //}
            //else
            //{
            //             // moving
            //_shooter.transform.position = new Vector3(x, y, position.z);
            //}


            // All logic goes here
            // crappy way of doing this

            if (_shooter.transform.position.y < _boundRect.bottom - G.radius && _fired) {
                Destroy(_shooter.gameObject);

                LoadShooterBubble();
                //shouldPark = true;
            }

            WinOrLose();
		}
	}

    private void MoveBall() {
        // actual moving
        //_shooter.transform.Translate(ballMoveDir * Time.deltaTime * G.bubbleSpeed);

        //// "bouncing" by rotating the ball

        // attempt #1

        Ray2D ray = new Ray2D(_shooter.transform.position, _shooter.transform.up);
        RaycastHit2D hit = Physics2D.Raycast(_shooter.transform.position, ballMoveDir, Time.deltaTime * G.bubbleSpeed + 0.1f);

      // if () {
            //Vector2 refDir = Vector2.Reflect(ray.direction, hit.normal);
            //float rot = 90 - Mathf.Atan2(refDir.x, refDir.y) * Mathf.Rad2Deg;
            //_shooter.transform.eulerAngles = new Vector3(0, 0, rot);
            //Debug.Log("rot: " + rot);
            //Debug.DrawLine(_shooter.transform.localPosition, _shooter.transform.up, Color.red);
        //}

        // attempt #2

        //if (hit.collider && hit.transform.gameObject.layer == LayerMask.NameToLayer("BoxC")) {
        //    var contactPoint = hit.point;
        //    Vector2 ballLocation = _shooter.transform.position;
        //    var inNormal = (ballLocation - contactPoint).normalized;
        //    ballMoveDir = Vector2.Reflect(ballMoveDir, inNormal);

        //    Debug.Log("happening: " + hit.point);
        //    Debug.Log("ballMoveDir: " + ballMoveDir);
        //}
    }
    
    private void StartFire()
    {
        _fired = true;

        //_dx = Mathf.Cos(_shootingAngle * Mathf.Deg2Rad);
        //_dy = Mathf.Sin(_shootingAngle * Mathf.Deg2Rad);

        Vector3 eulerAngles = cannon.transform.rotation.eulerAngles;
        _shooter.transform.rotation = Quaternion.Euler(0, 0, eulerAngles.z);

        //ballMoveDir = _shooter.transform.rotation.eulerAngles;
        _shooter.GetComponent<Rigidbody2D>().AddForce(new Vector2(_shooter.GetComponent<Bubble>().shootForce, _shooter.GetComponent<Bubble>().shootForce));
        Debug.Log(_shooter);

        AudioManager.Instance.Play(launchClip);
    }

	private Box IsCollidingOthers(Vector3 shooterPosition, float epsilon)
	{
        // collision check from bottom to top
        Box other;

		for (int i = G.rows - 1; i >= 0; i--) {
			for (int j = 0; j < G.cols; j++) {
				other = _grid.Get(i, j);
				if (other != null) {
					var otherPosition = other.transform.position;
					
					if (IsCloseEnough(shooterPosition, otherPosition, epsilon)) {
						return other;
					}						
				}
			}
		}
		
		return null;
	}
	
	private void ParkBubble(Vector3 currentPosition)
	{
		Index index = Misc.PositionToIndex(_boundRect, currentPosition);
		Vector3 position = Misc.IndexToPosition(_boundRect, index);
		
		if (_grid.Get(index) != null)
        {
            D.log("The index ({0}, {1}) should be null, got from position ({2}, {3}).",
                index.row, index.col, currentPosition.x, currentPosition.y);
			return;
		}
		
		if (index.row + _compressorLevel >= G.rows)
		{
			// we lose
			loseCallback();
			return;
		}
		
		_shooter.transform.position = position;
		//_grid.Set(index, _shooter);
		
        //OnChain(index);

        OnUnlink(index);

		_newRoot++;
		if (_newRoot == timeBeforeNewRoot - 1)
		{
			// shake compressor
			iTween.ShakePosition(compressor, iTween.Hash("x", 5.0f, "y", 5.0f, "time", 1.0f, "looptype", "loop"));
		}
		else if (_newRoot > timeBeforeNewRoot)
		{
			_newRoot = 0;

			AudioManager.Instance.Play(newRootSoloClip);
			
			iTween.Stop(compressor);			

			float delta = Mathf.Sqrt(3.0f) * G.radius;
			compressor.transform.MoveBy(0, -delta, 0);
			_compressorLevel++;
			
			// recalculate the bubbles' positions
			_boundRect.top -= delta;
			_grid.Recalculate(_boundRect);
		}
		
		LoadShooterBubble();
	}
	
	private void RemoveBubbles(List<Index> bubbles, RemoveType type)
	{
		for (int i = 0; i < bubbles.Count; i++)
		{
			var b = _grid.Get(bubbles[i]);
			
			if (type == RemoveType.UnlinkRemoval)
			{
				FallEffect fall = b.gameObject.AddComponent<FallEffect>();
				fall.gravity = 1500.0f;
				fall.initialAngle = 0;
				fall.initialVelocity = 0;
				fall.lowerLimit = _boundRect.bottom;				
			}
			else
			{
				FallEffect fall = b.gameObject.AddComponent<FallEffect>();
				fall.gravity = 1500.0f;
				
				if (i == 0)
					fall.initialAngle = 15.0f;
				else if (i == bubbles.Count - 1)
					fall.initialAngle = 180.0f - 15.0f;
				else
					fall.initialAngle = Random.Range(15.0f, 175.0f);
				
				fall.initialVelocity = Random.Range(100.0f, 200.0f);
				fall.lowerLimit = _boundRect.bottom;				
			}
			
			_grid.Remove(bubbles[i]);
		}
	}
	
	//private List<Index> GetChain(Index startIndex)
	//{
	//	Bubble shooterBubble = _shooter;
	//	Bubble.Type shooterType = shooterBubble.type;
		
	//	List<Index> chainList = new List<Index>();

	//	ClearVisitedList();
		
	//	List<Index> dfsList = new List<Index>();
	//	dfsList.Add(startIndex);
	//	_visited[startIndex.row, startIndex.col] = true;
	
	//	while (dfsList.Count != 0) {
	//		// pop the first entry
	//		Index current = dfsList[0];
	//		dfsList.RemoveAt(0);
			
	//		// add this to the final chain list!
	//		chainList.Add(current);
			
	//		Index[] neighbours = Misc.GetNeighbours(current);
			
	//		foreach (var next in neighbours) {
	//			if (InNewChain(next, shooterType)) {
	//				dfsList.Add(next);
	//				_visited[next.row, next.col] = true;
	//			}
	//		}
	//	}
		
	//	return chainList;
	//}
	
	private void ClearVisitedList()
	{
		for (int i = 0; i < G.rows; i++) {
			for (int j = 0; j < G.cols; j++) {
				_visited[i, j] = false;
			}
		}
	}

	private List<Index> GetUnlinked()
	{
		List<Index> dfsList = new List<Index>();
		
		ClearVisitedList();
		
		for (int i = 0; i < G.cols; i++) {
			if (_grid.Get(0, i) != null) {
				dfsList.Add(new Index(0, i));
				_visited[0, i] = true;
			}
		}
		
		while (dfsList.Count != 0) {
			Index current = dfsList[0];
			dfsList.RemoveAt(0);
			
			Index[] neighbours = Misc.GetNeighbours(current);
			
			foreach (var next in neighbours) {
				if (IsIndexValid(next) &&
					_visited[next.row, next.col] == false &&
					_grid.Get(next) != null) {
					dfsList.Add(next);
					_visited[next.row, next.col] = true;
				}
			}
		}
		
		// final processing! those un-visited bubbles are unlinked ones.
		List<Index> unlinked = new List<Index>();
		for (int i = 0; i < G.rows; i++) {
			for (int j = 0; j < G.cols; j++) {
				if (_grid.Get(i, j) != null && _visited[i, j] == false) {
					unlinked.Add(new Index(i, j));
				}
			}
		}
		
		return unlinked;
	}
	
	private bool IsIndexValid(Index index)
	{
		return index.row >= 0 && index.row < G.rows &&
			index.col >= 0 && index.col < G.cols;
	}
	
	//private bool InNewChain(Index next, Bubble.Type type)
	//{
	//	if (IsIndexValid(next) &&
	//		_visited[next.row, next.col] == false &&
	//		_grid.Get (next) != null) {
	//		var bubble = _grid.Get (next);
			
	//		if (type != Bubble.Type.None && bubble.type == type) {
	//			return true;
	//		}
	//	}
		
	//	return false;
	//}

	private Vector3 GetShooterPosition()
	{
		float realHeight = Mathf.Sqrt(3.0f) * (G.rows - 1) * G.radius + 2 * G.radius;
		
		return new Vector3(0, -realHeight / 2.0f - G.radius - 5.0f /* some delta to make it look nice */, -1);
	}
	
	private Vector3 GetNextPosition()
	{
		Vector3 origin = GetShooterPosition();
		origin.x -= G.radius * 2.0f * 2.6f;
		origin.y -= G.radius / 2.0f;
		return origin;
	}
	
	public void LoadShooterBubble()
	{
		if (_grid.Count > 0)
		{
			// with random colors or from a specific sequence!
			if (_next == null)
			{
				_next = GetOneBubbleAtPosition(GetNextPosition());			
				// make it smaller
				_next.transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
			}
	
			_shooter = GetOneBubbleAtPosition(GetShooterPosition(), _next.type);

			Destroy(_next.gameObject);

			_next = GetOneBubbleAtPosition(GetNextPosition());
			// make it smaller
			_next.transform.localScale = new Vector3(0.8f, 0.8f, 1.0f);
		}
		
		_fired = false;
	}

    private Bubble GetOneBubbleAtPosition(Vector3 position, Bubble.Type type = Bubble.Type.None) {
        var go = Instantiate(ball, position, Quaternion.identity) as GameObject;

        //if (type == Bubble.Type.None) {
        //    type = Bubble.GetRandomColorFromList(_grid.GetAllUniqueTypes());
        //}

        SpriteRenderer render = go.GetComponent<SpriteRenderer>();
        render.sprite = getBubbleSprite(type);

        Bubble bubble = go.GetComponent<Bubble>();
        bubble.type = type;

        return bubble;
    }

    private Box GetOneBoxAtPosition(Vector3 position, Box.Type type = Box.Type.None)
	{
		var go = Instantiate(box, position, Quaternion.identity) as GameObject;

        if (type == Box.Type.None)
        {
        	type = Box.GetRandomColorFromList(_grid.GetAllUniqueTypes());
        }

		SpriteRenderer render = go.GetComponent<SpriteRenderer>();
		render.sprite = getBoxSprite (type);

        Box _box = go.GetComponent<Box>();
        _box.type = type;
        _box.amountOfHits = Box.GetRandomHitsAmount();
		return _box;
	}

    private ParkingStateInfo IsFinalParkingIndex(float speed, float epsilon,
        float x, float y,
        float dx, float dy,
        bool playSound = false) {
        bool shouldPark = false;

        float prev_x = x;
        float prev_y = y;

        // moving
        x += dx * speed;
        y += dy * speed;

        //if (x < _boundRect.left + G.radius) {
        //    x = _boundRect.left + G.radius;
        //    dx *= -1;

        //    if (playSound)
        //        AudioManager.Instance.Play(reboundClip);
        //}

        //if (x > _boundRect.right - G.radius) {
        //    x = _boundRect.right - G.radius;
        //    dx *= -1;

        //    if (playSound)
        //        AudioManager.Instance.Play(reboundClip);
        //}

        //if (y > _boundRect.top - G.radius) {
        //    y = _boundRect.top - G.radius;
        //    dy *= -1;
        //    //shouldPark = true;
        //}

        // crappy way of doing this
        if (y < _boundRect.bottom - G.radius && _fired) {
            Destroy(_shooter.gameObject);

            LoadShooterBubble();
            //shouldPark = true;
        }

        Box otherBox = IsCollidingOthers(new Vector3(x, y), epsilon);

        Debug.Log(">>> shouldPark > " + shouldPark);
        Debug.Log(">>> otherBox > " + otherBox);
        if (!shouldPark && otherBox != null) {
            // figure out which direction it should be going through the bounding box/radius of the ball
            //RectTransform b_rect = (RectTransform)b.transform;

            Vector2 boxCurrentPosition = new Vector2();
            boxCurrentPosition.x = otherBox.transform.position.x;
            boxCurrentPosition.y = otherBox.transform.position.y;
            

         /**   Vector2 currentSpeed = new Vector2(dx, dy);

            Vector2 newSpeed = Vector2.Reflect(currentSpeed, boxCurrentPosition);

            Debug.DrawLine(boxCurrentPosition, new Vector3(currentSpeed.x, currentSpeed.y, 100), Color.red, 10.0f);
            Debug.DrawLine(boxCurrentPosition, new Vector3(newSpeed.x, newSpeed.y, 100), Color.blue, 10.0f);

            dx *= Mathf.Sign(newSpeed.x);
            dy *= Mathf.Sign(newSpeed.y);
            Debug.Log("s x = " + Mathf.Sign(newSpeed.x) + " s y= " + Mathf.Sign(newSpeed.y));
            Debug.Log("dx  x = " + dx + " dy y = " + dy);

            Index index = Misc.PositionToIndex(_boundRect, boxCurrentPosition);
            List<Index> _list = new List<Index>();
            _list.Add(index);

            RemoveBubbles(_list, RemoveType.UnlinkRemoval);
    **/

            //Destroy(b.gameObject);
            //shouldPark = true;
        }

        if (shouldPark) {
            Index parkingIndex = Misc.PositionToIndex(_boundRect, new Vector3(x, y));

            if (parkingIndex.row + _compressorLevel == G.rows) {
                loseCallback();
                return new ParkingStateInfo(shouldPark, x, y, dx, dy); ;
            }

            if (_grid.Get(parkingIndex) != null) {
                // we go backtrack to find the first non-colliding point!
                while (!(Mathf.Approximately(x, prev_x) && Mathf.Approximately(y, prev_y))) {
                    // let's try 2 points a step!
                    x -= dx * 2.0f;
                    y -= dy * 2.0f;

                    if (IsCollidingOthers(new Vector3(x, y), epsilon) == null) {
                        break;
                    }
                }

                parkingIndex = Misc.PositionToIndex(_boundRect, new Vector3(x, y));
                Vector3 tempPosition = Misc.IndexToPosition(_boundRect, parkingIndex);
                x = tempPosition.x;
                y = tempPosition.y;
            }
        }

        return new ParkingStateInfo(shouldPark, x, y, dx, dy);
    }

    private bool IsCloseEnough(Vector3 pos1, Vector3 pos2, float collisionEpsilon)
	{
		float distX = pos1.x - pos2.x;
		float distY = pos1.y - pos2.y;
		
		return distX * distX + distY * distY <= (2 * G.radius * collisionEpsilon) * (2 * G.radius * collisionEpsilon);
	}

	#region Touch Handlers
	
	public void HandleTouchBegan(Finger f)
	{
		HandleTouchMoved(f);
	}
	
	public void HandleTouchMoved(Finger f)
	{
		Vector3 touchPosition = Camera.main.ScreenToWorldPoint(new Vector3(f.x, f.y));
		Vector3 midPosition = GetShooterPosition();

		if (touchPosition.y >= midPosition.y)
		{
			_shootingAngle = Mathf.Rad2Deg * Mathf.Atan2(touchPosition.y - midPosition.y, touchPosition.x - midPosition.x);
			_shootingAngle = Mathf.Clamp(_shootingAngle, 0.0f + G.shootingMinAngle, 180.0f - G.shootingMinAngle);
			
			cannon.transform.rotation = Quaternion.Euler(0, 0, _shootingAngle - 90.0f);
		}
	}
	
	public void HandleTouchCanceled(Finger f)
	{
		// do nothing, we don't fire the ball.
	}
	
	public void HandleTouchEnded(Finger f)
	{
		StartFire();
	}

	#endregion

	//private void OnChain(Index index)
	//{
	//	List<Index> chains = GetChain(index);
	//	if (chains.Count >= 3)
	//	{
	//		RemoveBubbles(chains, RemoveType.ChainRemoval);
		
	//		AudioManager.Instance.Play(destroyGroupClip);
	//	}
	//	else
	//	{
	//		AudioManager.Instance.Play(stickClip);
	//	}
	//}

    private void OnUnlink(Index index)
    {
		List<Index> unlinked = GetUnlinked();
		RemoveBubbles(unlinked, RemoveType.UnlinkRemoval);
    }
		
	private void WinOrLose()
	{		
		if (_grid.Count == 0)
		{
			// win
			winCallback();
			return;
		}
		
		if (_grid.MaxRow + _compressorLevel >= G.rows)
		{
			// lose
			loseCallback();
			return;
		}
	}
	
	private void winCallback()
	{
		// we just reload again
		LoadNextLevel ();
	}
	
	private void loseCallback()
	{
		LoadNextLevel ();
	}
	
	/// <summary>
	/// get the sprite according to its bubble type
	/// </summary>
	/// <returns>The bubble sprite.</returns>
	/// <param name="type">Type.</param>
	private Sprite getBubbleSprite(Bubble.Type type)
	{
		switch (type) {
		case Bubble.Type.Color1:
				return _bubble_1;

		case Bubble.Type.Color2:
				return _bubble_2;

		case Bubble.Type.Color3:
				return _bubble_3;

		case Bubble.Type.Color4:
				return _bubble_4;

		case Bubble.Type.Color5:
				return _bubble_5;

		case Bubble.Type.Color6:
				return _bubble_6;

		case Bubble.Type.Color7:
				return _bubble_7;

		case Bubble.Type.Color8:
				return _bubble_8;
		}

		return _bubble_1;
	}

    private Sprite getBoxSprite(Box.Type type) {
        switch (type) {
            case Box.Type.Color1:
                return _box_1;

            case Box.Type.Color2:
                return _box_2;

            case Box.Type.Color3:
                return _box_3;

            case Box.Type.Color4:
                return _box_4;

            case Box.Type.Color5:
                return _box_5;

            case Box.Type.Color6:
                return _box_6;

            case Box.Type.Color7:
                return _box_7;

            case Box.Type.Color8:
                return _box_8;
        }

        return _box_1;
    }
}
