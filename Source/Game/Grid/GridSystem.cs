using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game;

/// <summary>
/// GridSystem Script.
/// </summary>
public class GridSystem<TGridObject>
{
	public class Position
	{
		public int X;
		public int Z;

		public Position(int x, int z)
		{
			X = x;
			Z = z;
		}

		public override string ToString()
		{
			return $"({X}, {Z})";
		}

	}

	public Vector2 Dimension { get; private set; }
	public float UnitScale { get; private set; }
	public Vector3 Origin { get; private set; }  // Store the origin
												 // Conversion factor from centimeters to meters
	private const float METERS_TO_CM = 100f;
	private readonly TGridObject[,] gridObjects;

	public GridSystem(Vector2 dimension, float unitScale, Func<GridSystem<TGridObject>, Position, TGridObject> createGridObject)
	{
		Dimension = dimension;
		// Convert UnitScale from centimeters to meters
		UnitScale = unitScale * METERS_TO_CM;
		// Convert Origin from centimeters to meters
		Origin = Vector3.Zero * METERS_TO_CM;
		gridObjects = new TGridObject[(int)Dimension.X, (int)Dimension.Y];

		DrawGridBoundingBox();
		for (int x = 0; x < Dimension.X; x++)
		{
			for (int z = 0; z < Dimension.Y; z++)
			{
				Position pos = new Position(x, z);
				gridObjects[x, z] = createGridObject(this, pos);

				BoundingSphere sphere = new BoundingSphere(GetWorldPosition(pos), 5f); // Convert radius to meters
				DebugDraw.DrawSphere(sphere, Color.Red, 20);
				// DebugDraw.DrawLine(GetWorldPosition(pos), GetWorldPosition(pos) + Vector3.Right * .2f, Color.White, 1000);
			}
		}
	}

	public void CreateDebugObjects(Prefab prefab)
	{
		for (int x = 0; x < Dimension.X; x++)
		{
			for (int z = 0; z < Dimension.Y; z++)
			{
				Position gridPos = new Position(x, z);
				Actor debugObj = PrefabManager.SpawnPrefab(prefab, GetWorldPosition(gridPos), Quaternion.Identity);

				if (!debugObj.TryGetScript<GridDebugObject>(out var gridDebugObject)) return;

				gridDebugObject.SetGridObject(GetGridObject(gridPos) as GridObject);
			}
		}
	}


	public BoundingBox GetBoundingBox(out Vector3 minWorldPos, out Vector3 maxWorldPos)
	{
		// Define grid boundaries in grid coordinates
		Position min = new Position(0, 0);
		Position max = new Position((int)Dimension.X - 1, (int)Dimension.Y - 1);

		// Get the world positions with the center offset
		minWorldPos = GetWorldPosition(min);
		maxWorldPos = GetWorldPosition(max);

		return new BoundingBox(minWorldPos, maxWorldPos);
	}
	private void DrawGridBoundingBox()
	{
		// Create a bounding box from the world positions
		BoundingBox gridBounds = GetBoundingBox(out Vector3 minWorldPos, out Vector3 maxWorldPos);

		DebugDraw.DrawSphere(new BoundingSphere(minWorldPos, 5f), Color.Green, 20);
		DebugDraw.DrawSphere(new BoundingSphere(maxWorldPos, 5f), Color.Green, 20);

		// Draw the bounding box
		DebugDraw.DrawWireBox(gridBounds, Color.Beige, 10.0f);
	}

	// Assuming GetWorldPosition is defined elsewhere
	private Vector3 GetWorldPosition(Position pos)
	{
		// Offset the dimensions by 1 to account for zero-based indexing
		float gridSizeX = Dimension.X - 1;
		float gridSizeZ = Dimension.Y - 1;

		const float CENTER_OFFSET = 2f;

		float offsetX = (int)(gridSizeX / CENTER_OFFSET * UnitScale);
		float offsetZ = (int)(gridSizeZ / CENTER_OFFSET * UnitScale);

		float scaledX = pos.X * UnitScale;
		float scaledZ = pos.Z * UnitScale;

		// Translate the grid position, centering the grid on the origin
		return Origin + new Vector3(scaledX - offsetX, 0, scaledZ - offsetZ);
	}

	public TGridObject GetGridObject(Position position)
	{
		return gridObjects[position.X, position.Z];
	}

	public Position GetGridPosition(Vector3 worldPosition)
	{
		return new Position((int)(worldPosition.X / UnitScale), (int)(worldPosition.Z / UnitScale));
	}

}


