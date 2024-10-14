﻿using System;
using System.Collections.Generic;
using System.IO;
using FlaxEngine;

namespace Game;

/// <summary>
/// Pathfinding Script.
/// </summary>
public class Pathfinding
{
	public enum NodeType
	{
		Hallway,
		Room,
		Other
	}

	public class PathNode : IGridObject
	{
		public GridSystem<PathNode> GridSystem { get; private set; }
		public GridPosition GridPosition { get; private set; }

		public int GCost { get; private set; }
		public int HCost { get; private set; }
		public int FCost { get; private set; }

		public PathNode PreviousNode { get; private set; }
		public bool IsWalkable { get; private set; }

		public event EventHandler OnDataChanged;
		public NodeType NodeType { get; set; }  // New property to define node type

		public PathNode(GridPosition position, GridSystem<PathNode> gridSystem)
		{
			GridSystem = gridSystem;
			GridPosition = position;
			GCost = -1;
			HCost = -1;
			FCost = -1;
			IsWalkable = true;
		}

		public void SetWalkable(bool flag)
		{
			IsWalkable = flag;
			OnDataChanged?.Invoke(this, EventArgs.Empty);
		}

		public void SetGCost(int gCost)
		{
			GCost = gCost;
			CalculateFCost();
		}

		public void SetHCost(int hCost)
		{
			HCost = hCost;
			CalculateFCost();
		}

		private void CalculateFCost()
		{
			FCost = GCost + HCost;
			OnDataChanged?.Invoke(this, EventArgs.Empty);
		}

		public void SetPreviousNode(PathNode previousNode)
		{
			PreviousNode = previousNode;
		}

		public bool IsOccupied()
		{
			Vector3 pos = GridSystem.GetWorldPosition(GridPosition);
			pos.Y -= 100f;

			DebugDraw.DrawSphere(new BoundingSphere(pos, 5f), Color.Red, 10f);
			if (Physics.RayCastAll(pos, Vector3.Up, out RayCastHit[] hits, 100f))
			{
				foreach (RayCastHit hit in hits)
				{
					if (hit.Collider.HasTag("Pathfinding.Obstacle"))
					{
						return true;
					}
				}
			}
			return false;
		}

		public override string ToString()
		{
			return GridPosition.ToString();
		}
	}

	public GridSystem<PathNode> GridSystem { get; private set; }
	private const int MOVE_STRAIGHT_COST = 10;
	private const int MOVE_DIAGONAL_COST = 14;

	public Pathfinding(Vector2 dimension, float unitScale)
	{

		GridSystem = new GridSystem<PathNode>(dimension, unitScale, (GridSystem<PathNode> gridSystem, GridPosition gridPosition) => { return new PathNode(gridPosition, gridSystem); });


	}



	public void ToggleNeighborWalkable(GridPosition basePosition, int Width, int Length, bool flag)
	{
		Debug.Log($"GridPosition: {basePosition} - {Width} X {Length}... GridSize: {GridSystem.ToGridSize(Width)} X {GridSystem.ToGridSize(Length)}");
		ToggleNodeWalkable(new GridPosition(basePosition.X, basePosition.Z), flag);
		if (Width > 1)
		{
			bool isDivisible = Width % 2 == 0; // Is divisible by 2

			if (isDivisible)
			{
				for (int i = 1; i <= Width / 2; i++)
				{
					ToggleNodeWalkable(new GridPosition(basePosition.X + i, basePosition.Z), flag);
					ToggleNodeWalkable(new GridPosition(basePosition.X - i, basePosition.Z), flag);
				}
			}
			else
			{
				for (int i = 1; i < Width - 1; i++)
				{
					ToggleNodeWalkable(new GridPosition(basePosition.X + i, basePosition.Z), flag);
					ToggleNodeWalkable(new GridPosition(basePosition.X - i, basePosition.Z), flag);
				}
			}
			// int endCount = (Width % 2 != 0) ? Width - 1 : Width / 2;
			// // Left and Right
			// for (int i = 1; i < endCount; i++)
			// {
			// 	ToggleNodeWalkable(new GridPosition(basePosition.X + i, basePosition.Z), flag);
			// 	ToggleNodeWalkable(new GridPosition(basePosition.X - i, basePosition.Z), flag);
			// }
		}

		if (Length > 1)
		{
			bool isDivisible = Length % 2 == 0; // Is divisible by 2

			if (isDivisible)
			{
				for (int i = 1; i <= Length / 2; i++)
				{
					ToggleNodeWalkable(new GridPosition(basePosition.X, basePosition.Z + i), flag);
					ToggleNodeWalkable(new GridPosition(basePosition.X, basePosition.Z - i), flag);
				}
			}
			else
			{
				for (int i = 1; i < Length - 1; i++)
				{
					ToggleNodeWalkable(new GridPosition(basePosition.X, basePosition.Z + i), flag);
					ToggleNodeWalkable(new GridPosition(basePosition.X, basePosition.Z - i), flag);
				}
			}
			// int endCount = (Length % 2 != 0) ? Length - 1 : Length / 2;
			// // Forward and Backward
			// for (int i = 1; i < endCount; i++)
			// {
			// 	ToggleNodeWalkable(new GridPosition(basePosition.X, basePosition.Z + i), flag);
			// 	ToggleNodeWalkable(new GridPosition(basePosition.X, basePosition.Z - i), flag);

			// }


		}

		// if (Width > 1 && Length > 1)
		// {
		// 	int count = 1;
		// 	// Diagonal
		// 	for (int i = 1; i < Width; i++)
		// 	{
		// 		ToggleNodeWalkable(new GridPosition(basePosition.X + i, basePosition.Z + i), flag);
		// 		ToggleNodeWalkable(new GridPosition(basePosition.X - i, basePosition.Z - i), flag);
		// 		ToggleNodeWalkable(new GridPosition(basePosition.X + i, basePosition.Z - i), flag);
		// 		ToggleNodeWalkable(new GridPosition(basePosition.X - i, basePosition.Z + i), flag);
		// 	}
		// }
	}

	private void ToggleNodeWalkable(GridPosition position, bool flag)
	{
		if (!GridSystem.IsPositionValid(position)) return;
		GetNode(position).SetWalkable(flag);
	}

	public BoundingBox GetBoundingBox()
	{
		return GridSystem.GetBoundingBox();
	}

	public void SpawnDebugObjects(Prefab debugGridPrefab)
	{
		GridSystem.CreateDebugObjects(debugGridPrefab);
	}

	public List<GridPosition> FindPath(GridPosition start, GridPosition end)
	{
		List<PathNode> openList = new List<PathNode>(); // Nodes to be evaluated
		List<PathNode> closedList = new List<PathNode>(); // Already visited nodes

		// Add Start node to the open list
		PathNode startNode = GridSystem.GetGridObject(start);
		PathNode endNode = GridSystem.GetGridObject(end);

		// Check if start or end node is not walkable
		if (!startNode.IsWalkable)
		{
			startNode = FindNearestWalkableNode(startNode);
			if (startNode == null)
			{
				Debug.Log("No walkable starting node found.");
				return null;
			}
		}

		if (!endNode.IsWalkable)
		{
			endNode = FindNearestWalkableNode(endNode);
			if (endNode == null)
			{
				Debug.Log("No walkable ending node found.");
				return null;
			}
		}



		openList.Add(startNode);
		// Debug.Log($"Start: {startNode.GridPosition} End: {endNode.GridPosition}");
		DebugDraw.DrawSphere(new BoundingSphere(GridSystem.GetWorldPosition(startNode.GridPosition), 15f), Color.DarkRed, 60f);
		DebugDraw.DrawSphere(new BoundingSphere(GridSystem.GetWorldPosition(endNode.GridPosition), 15f), Color.DarkRed, 60f);


		// Initialize path nodes
		for (int x = 0; x < GridSystem.Dimension.X; x++)
		{
			for (int z = 0; z < GridSystem.Dimension.Y; z++)
			{
				GridPosition pos = new GridPosition(x, z);
				PathNode pathNode = GridSystem.GetGridObject(pos);
				pathNode.SetGCost(int.MaxValue);
				pathNode.SetHCost(0);
				pathNode.SetPreviousNode(null);
			}
		}

		startNode.SetGCost(0);
		startNode.SetHCost(CalculateDistance(start, end));

		while (openList.Count > 0)
		{
			PathNode currentNode = GetLowestFCostNode(openList);

			// If the current node is the end node, return the path
			if (currentNode == endNode)
			{
				return CalculatePath(endNode);
			}
			openList.Remove(currentNode);
			closedList.Add(currentNode);

			foreach (PathNode neighbor in GetNeighboringNodes(currentNode))
			{
				if (closedList.Contains(neighbor)) continue;

				if (!neighbor.IsWalkable)
				{
					closedList.Add(neighbor);
					continue;
				}

				// Cost from the start node to the current node
				int tentativeGCost;
				if (currentNode.NodeType == NodeType.Hallway)
				{
					tentativeGCost = -5;
				}
				else
				{
					tentativeGCost = currentNode.GCost + CalculateDistance(currentNode.GridPosition, neighbor.GridPosition) + 10;
				}


				if (tentativeGCost < neighbor.GCost)  // If the new path is shorter
				{
					// Update the neighbor node
					neighbor.SetPreviousNode(currentNode);
					neighbor.SetGCost(tentativeGCost);
					neighbor.SetHCost(CalculateDistance(neighbor.GridPosition, end));

					if (!openList.Contains(neighbor))
						openList.Add(neighbor);
				}
			}

		}

		// No path found
		Debug.Log("No path found");
		return null;
	}

	private PathNode FindNearestWalkableNode(PathNode node, int searchRadius = 5)
	{
		for (int radius = 1; radius <= searchRadius; radius++)
		{
			for (int x = -radius; x <= radius; x++)
			{
				for (int z = -radius; z <= radius; z++)
				{
					GridPosition neighborPos = new GridPosition(node.GridPosition.X + x, node.GridPosition.Z + z);
					if (!GridSystem.IsPositionValid(neighborPos)) continue;

					PathNode neighborNode = GridSystem.GetGridObject(neighborPos);

					if (neighborNode != null && neighborNode.IsWalkable)
					{
						return neighborNode; // Return the first walkable node found
					}
				}
			}
		}

		return null; // No walkable node found within the search radius
	}


	private List<PathNode> GetNeighboringNodes(PathNode currentNode)
	{
		List<PathNode> neighboringNodes = new List<PathNode>();

		GridPosition position = currentNode.GridPosition;

		if (GridSystem.IsPositionXValid(position.X - 1))
		{
			neighboringNodes.Add(GetNode(position.X - 1, position.Z)); // Left
			if (GridSystem.IsPositionZValid(position.Z - 1))
				neighboringNodes.Add(GetNode(position.X - 1, position.Z - 1)); // Down Left
			if (GridSystem.IsPositionZValid(position.Z + 1))
				neighboringNodes.Add(GetNode(position.X - 1, position.Z + 1)); // Up Left
		}


		if (GridSystem.IsPositionXValid(position.X + 1))
		{
			neighboringNodes.Add(GetNode(position.X + 1, position.Z)); // Right
			if (GridSystem.IsPositionZValid(position.Z - 1))
				neighboringNodes.Add(GetNode(position.X + 1, position.Z - 1)); // Down Right
			if (GridSystem.IsPositionZValid(position.Z + 1))
				neighboringNodes.Add(GetNode(position.X + 1, position.Z + 1)); // Up Right	
		}

		if (GridSystem.IsPositionZValid(position.Z - 1))
			neighboringNodes.Add(GetNode(position.X, position.Z - 1)); // Down

		if (GridSystem.IsPositionZValid(position.Z + 1))
			neighboringNodes.Add(GetNode(position.X, position.Z + 1)); // Up


		return neighboringNodes;
	}

	public PathNode GetNode(int x, int z)
	{
		GridPosition position = new GridPosition(x, z);
		return GridSystem.GetGridObject(position);
	}

	public PathNode GetNode(GridPosition position)
	{
		if (!GridSystem.IsPositionValid(position)) return null;
		return GridSystem.GetGridObject(position);
	}

	private List<GridPosition> CalculatePath(PathNode endNode)
	{
		List<PathNode> path = [endNode];

		PathNode currentNode = endNode; // Starting from the end node
		while (currentNode.PreviousNode != null)
		{
			path.Add(currentNode.PreviousNode);
			currentNode = currentNode.PreviousNode;
		}

		path.Reverse();

		List<GridPosition> gridPath = new List<GridPosition>();
		foreach (PathNode node in path)
		{
			gridPath.Add(node.GridPosition);
		}

		return gridPath;
	}

	private PathNode GetLowestFCostNode(List<PathNode> openList)
	{
		PathNode lowestFCostNode = openList[0];
		for (int i = 1; i < openList.Count; i++)
		{
			if (openList[i].FCost < lowestFCostNode.FCost)
				lowestFCostNode = openList[i];
		}
		return lowestFCostNode;
	}

	public int CalculateDistance(GridPosition a, GridPosition b)
	{
		GridPosition gridPosDistance = a - b;
		int xDistance = Math.Abs(gridPosDistance.X);
		int zDistance = Math.Abs(gridPosDistance.Z);
		int remaining = Math.Abs(xDistance - zDistance);

		return MOVE_DIAGONAL_COST * Mathf.Min(xDistance, zDistance) + (remaining * MOVE_STRAIGHT_COST);
	}
}
