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

	public class PathNode
	{
		public GridPosition Position { get; private set; }
		public int GCost { get; private set; }
		public int HCost { get; private set; }
		public int FCost { get; private set; }

		public PathNode PreviousNode { get; private set; }

		public event EventHandler OnDataChanged;

		public PathNode(GridPosition position)
		{
			Position = position;
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
			return Position.ToString();
		}
	}

	private GridSystem<PathNode> gridSystem;
	private const int MOVE_STRAIGHT_COST = 10;
	private const int MOVE_DIAGONAL_COST = 14;

	public Pathfinding(Vector2 dimension, float unitScale, Prefab debugGridPrefab)
	{

		gridSystem = new GridSystem<PathNode>(dimension, unitScale, (GridSystem<PathNode> gridSystem, GridPosition gridPosition) => { return new PathNode(gridPosition); });


		gridSystem.CreateDebugObjects(debugGridPrefab);
	}

	public List<GridPosition> FindPath(GridPosition start, GridPosition end)
	{
		List<PathNode> openList = new List<PathNode>(); // Nodes to be evaluated
		List<PathNode> closedList = new List<PathNode>(); // Already visited nodes

		// Add Start node to the open list
		PathNode startNode = gridSystem.GetGridObject(start);
		openList.Add(startNode);
		PathNode endNode = gridSystem.GetGridObject(end);

		// Initialize path nodes
		for (int x = 0; x < gridSystem.Dimension.X; x++)
		{
			for (int z = 0; z < gridSystem.Dimension.Y; z++)
			{
				GridPosition pos = new GridPosition(x, z);
				PathNode pathNode = gridSystem.GetGridObject(pos);
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

				// Cost from the start node to the current node
				int tentativeGCost = currentNode.GCost + CalculateDistance(currentNode.Position, neighbor.Position);

				if (tentativeGCost < neighbor.GCost)  // If the new path is shorter
				{
					// Update the neighbor node
					neighbor.SetPreviousNode(currentNode);
					neighbor.SetGCost(tentativeGCost);
					neighbor.SetHCost(CalculateDistance(neighbor.Position, end));

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

		GridPosition position = currentNode.Position;

		if (gridSystem.IsPositionXValid(position.X - 1))
		{
			neighboringNodes.Add(GetNode(position.X - 1, position.Z)); // Left
			if (gridSystem.IsPositionZValid(position.Z - 1))
				neighboringNodes.Add(GetNode(position.X - 1, position.Z - 1)); // Down Left
			if (gridSystem.IsPositionZValid(position.Z + 1))
				neighboringNodes.Add(GetNode(position.X - 1, position.Z + 1)); // Up Left
		}


		if (gridSystem.IsPositionXValid(position.X + 1))
		{
			neighboringNodes.Add(GetNode(position.X + 1, position.Z)); // Right
			if (gridSystem.IsPositionZValid(position.Z - 1))
				neighboringNodes.Add(GetNode(position.X + 1, position.Z - 1)); // Down Right
			if (gridSystem.IsPositionZValid(position.Z + 1))
				neighboringNodes.Add(GetNode(position.X + 1, position.Z + 1)); // Up Right	
		}

		if (gridSystem.IsPositionZValid(position.Z - 1))
			neighboringNodes.Add(GetNode(position.X, position.Z - 1)); // Down

		if (gridSystem.IsPositionZValid(position.Z + 1))
			neighboringNodes.Add(GetNode(position.X, position.Z + 1)); // Up


		return neighboringNodes;
	}

	private PathNode GetNode(int x, int z)
	{
		GridPosition position = new GridPosition(x, z);
		return gridSystem.GetGridObject(position);
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
			gridPath.Add(node.Position);
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
