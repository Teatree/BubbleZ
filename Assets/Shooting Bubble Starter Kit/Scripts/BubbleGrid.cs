using UnityEngine;
using System.Collections.Generic;

public class BubbleGrid : MonoBehaviour
{
	private Box[,] _grids;	
	private int _rows;
	private int _cols;
	
	private void Awake()
	{
		_rows = G.rows + 1;
		_cols = G.cols;
		
		_grids = new Box[_rows, _cols];
	}
	
	public Box Get(Index index)
	{
		return _grids[index.row, index.col];
	}
	
	public Box Get(int row, int col)
	{
		return _grids[row, col];
	}
	
	public void Set(Index index, Box box)
	{
		_grids[index.row, index.col] = box;
    }
	
	public void Set(int row, int col, Box box)
	{
		_grids[row, col] = box;
	}
	
	public void Remove(Index index)
	{
		_grids[index.row, index.col] = null;
	}
	
	public void Recalculate(LBRect rect)
	{
		for (int i = 0; i < _rows; i++)
		{
			for (int j = 0 ; j < _cols; j++)
			{
				var one = _grids[i, j];
				if (one != null)
				{
					one.transform.position = Misc.IndexToPosition(rect, new Index(i, j));
				}
			}
		}
	}
	
	public List<Box.Type> GetAllUniqueTypes()
	{
		List<Box.Type> all = new List<Box.Type>();
		
		for (int i = 0; i < G.rows; i++)
		{
			for (int j = 0; j < G.cols; j++)
			{
				var one = _grids[i, j];
				
				if (one != null)
				{
                    Box.Type type = one.type;
					
					if (!all.Contains(type))
					{
						all.Add(type);
					}
				}
			}
		}
		
		return all;
	}
	
	public void Reset()
	{
		for (int i = 0; i < G.rows; i++)
		{
			for (int j = 0; j < G.cols; j++)
			{
				var one = _grids[i, j];
				if (one != null)
				{
					Destroy(one.gameObject);
					_grids[i, j] = null;
				}
			}
		}
	}
	
	public int Count
	{
		get
		{
			int sum = 0;
			for (int i = 0; i < _rows; i++)
			{
				for (int j = 0; j < _cols; j++)
				{
					if (_grids[i, j] != null)
					{
						sum++;
					}
				}
			}
			return sum;
		}
	}
	
	public int MaxRow
	{
		get
		{
			int max = -1;
			for (int i = 0; i < _rows; i++)
			{
				for (int j = 0; j < _cols; j++)
				{
					if (_grids[i, j] != null)
					{
						if (i > max)
						{
							max = i;
						}
					}
				}
			}
			return max;
		}
	}
}
