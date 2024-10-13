using System;
using System.Collections.Generic;
using System.IO;
using FlaxEngine;

namespace Game;

/// <summary>
/// Pathfinding Script.
/// </summary>
public class Pathfinding
{

	public class PathNode : IGridObject
	{
		public GridPosition GridPosition { get; private set; }

		public int GCost { get; private set; }
		public int HCost { get; private set; }
		public int FCost { get; private set; }

		public PathNode PreviousNode { get; private set; }
		public bool IsWalkable { get; set; } = true;

		public event EventHandler OnDataChanged;

		public PathNode(GridPosition position)
		{
			GridPosition = position;
			GCost = -1;
			HCost = -1;
			FCost = -1;
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

		public override string ToString()
		{
			return GridPosition.ToString();
		}
	}

	public GridSystem<PathNode> GridSystem { get; private set; }
	private const int MOVE_STRAIGHT_COST = 10;
	private const int MOVE_DIAGONAL_COST = 14;

	public Pathfinding(Vector2 dimension, float unitScale, Prefab debugGridPrefab)
	{

		GridSystem = new GridSystem<PathNode>(dimension, unitScale, (GridSystem<PathNode> gridSystem, GridPosition gridPosition) => { return new PathNode(gridPosition); });

		GetNode(1, 0).IsWalkable = false;
		GetNode(1, 1).IsWalkable = false;
		GetNode(1, 2).IsWalkable = false;
		GetNode(1, 3).IsWalkable = false;
		GetNode(1, 4).IsWalkable = false;
		GridSystem.CreateDebugObjects(debugGridPrefab);
	}

	public List<GridPosition> FindPath(GridPosition start, GridPosition end)
	{
		List<PathNode> openList = new List<PathNode>(); // Nodes to be evaluated
		List<PathNode> closedList = new List<PathNode>(); // Already visited nodes

		// Add Start node to the open list
		PathNode startNode = GridSystem.GetGridObject(start);
		openList.Add(startNode);
		PathNode endNode = GridSystem.GetGridObject(end);

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
				int tentativeGCost = currentNode.GCost + CalculateDistance(currentNode.GridPosition, neighbor.GridPosition);

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
		return null;
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

	private PathNode GetNode(int x, int z)
	{
		GridPosition position = new GridPosition(x, z);
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
