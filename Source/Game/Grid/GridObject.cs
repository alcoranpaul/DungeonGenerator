using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game;

/// <summary>
/// GridObject Script.
/// </summary>
public class GridObject
{
	public GridSystem<GridObject> GridSystem { get; private set; }
	public GridSystem<GridObject>.Position GridPosition { get; private set; }

	public GridObject(GridSystem<GridObject> gridSystem, GridSystem<GridObject>.Position gridPosition)
	{
		GridSystem = gridSystem;
		GridPosition = gridPosition;
	}
}
