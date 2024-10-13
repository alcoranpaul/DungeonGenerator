using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game;

/// <summary>
/// GridObject Script.
/// </summary>
public class GridObject : IGridObject
{
	public GridSystem<GridObject> GridSystem { get; private set; }
	public GridPosition GridPosition { get; private set; }

	public GridObject(GridSystem<GridObject> gridSystem, GridPosition gridPosition)
	{
		GridSystem = gridSystem;
		GridPosition = gridPosition;
	}
}
