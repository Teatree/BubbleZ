using UnityEngine;
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
						Box one = GetOneBoxAtPosition(Misc.IndexToPosition(_boundRect, new Index(i, j)), ch);
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
            if (_shooter.transform.position.y < _boundRect.bottom - G.radius && _fired) {
                Destroy(_shooter.gameObject);

                LoadShooterBubble();
                //shouldPark = true;
            }

            WinOrLose();
		}
	}

    private void MoveBall() {
        Ray2D ray = new Ray2D(_shooter.transform.position, _shooter.transform.up);
        RaycastHit2D hit = Physics2D.Raycast(_shooter.transform.position, ballMoveDir, Time.deltaTime * G.bubbleSpeed + 0.1f);
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
	
	private bool IsIndexValid(Index index)
	{
		return index.row >= 0 && index.row < G.rows &&
			index.col >= 0 && index.col < G.cols;
	}
	
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


        SpriteRenderer render = go.GetComponent<SpriteRenderer>();
        render.sprite = getBubbleSprite(type);

        Bubble bubble = go.GetComponent<Bubble>();
        bubble.type = type;

        return bubble;
    }

    private Box GetOneBoxAtPosition(Vector3 position, char hitsChar )
	{
		var go = Instantiate(box, position, Quaternion.identity) as GameObject;
        int hits = (int)System.Char.GetNumericValue(hitsChar) + 1; //TODO: change '0' in Levels file
        //Box.Type type = Box.GetRandomColorFromList(_grid.GetAllUniqueTypes());
        Box.Type type = Box.GetColorByHits(hits);

		SpriteRenderer render = go.GetComponent<SpriteRenderer>();
		render.sprite = getBoxSprite (type);

        Box _box = go.GetComponent<Box>();
        _box.type = type;
        _box.amountOfHits = hits;
        _box.transform.GetChild(0).transform.GetChild(0).gameObject.GetComponent<UnityEngine.UI.Text>().text = "" + _box.amountOfHits;

        return _box;
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
