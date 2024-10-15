using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game;

/// <summary>
/// GridObject Script.
/// </summary>
public class GridObject<T> : IGridObject where T : GridObject<T>
{
	public GridSystem<T> GridSystem { get; private set; }
	public GridPosition GridPosition { get; private set; }

	public GridObject(GridSystem<T> gridSystem, GridPosition gridPosition)
	{
		GridSystem = gridSystem;
		GridPosition = gridPosition;
	}
}

