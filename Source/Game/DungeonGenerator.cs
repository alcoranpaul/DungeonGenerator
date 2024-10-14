﻿
using System;
using System.Collections.Generic;

using FlaxEngine;
using FlaxEngine.Utilities;

namespace Game;

/// <summary>
/// DungeonGenerator Script.
/// </summary>
public class DungeonGenerator : Script
{
	public static DungeonGenerator Instance { get; private set; }
	public int MaxRooms = 30;
	// public int UnitScale = 5;

	public float roomScale = 10f;

	public int DungeonSize = 10;



	// private List<Model> modelRooms;
	public List<Room> rooms;
	// private DungeonGeneratorData dungeonData;
	private BoundingBox dungeonBounds;

	// Create a dynamic model

	// Reference to the material
	public MaterialBase Material;
	public Prefab debugGridPrefab;
	public Prefab pathfindingDebugPrefab;
	public Prefab roomPrefab;
	public Prefab floorPrefab;

	// public GridSystem<GridObject> GridSystem { get; private set; }
	public Pathfinding Pathfinding { get; private set; }

	public override void OnAwake()
	{
		Instance = this;

	}

	public void GenerateGridSystem()
	{
		// Grid system for room generation
		// GridSystem = new GridSystem<GridObject>(new Vector2(10, 10), 1, (GridSystem<GridObject> gridSystem, GridPosition gridPosition) => { return new GridObject(gridSystem, gridPosition); });
		// GridSystem.CreateDebugObjects(debugGridPrefab);

		// Grid system for hallways



	}
	public override void OnStart()
	{
		// modelRooms = new List<Model>();
		rooms = new List<Room>();

		Pathfinding = new Pathfinding(new Vector2(DungeonSize, DungeonSize), 1);
		SpawnDebug();
	}

	public void SpawnDebug()
	{
		Pathfinding.SpawnDebugObjects(pathfindingDebugPrefab);
	}

	public void GenerateDungeon()
	{
		Debug.Log("Generating dungeon...");

		// Draw Boundary Box
		dungeonBounds = Pathfinding.GetBoundingBox();
		DebugDraw.DrawWireBox(dungeonBounds, Color.Beige, 10.0f);


		DestroyDungeon();
		SpawnRooms();
		GenerateHallwayPaths();

	}

	private void GenerateHallwayPaths()
	{
		Debug.Log($"Generating Hallways");
		float debugTime = 60f;
		List<Delaunay.Point> points = new List<Delaunay.Point>();
		HashSet<Delaunay.Edge> edges = CreateDelaunayTriangulation(points);
		Debug.Log($"Point count: {points.Count}");
		Debug.Log($"Edges count: {edges.Count}");
		List<Delaunay.Edge> hallwayPaths = CalculatePaths(debugTime, points, edges);
		Debug.Log($"Hallway Paths count: {hallwayPaths.Count}");
		// Set Node type to hallway nodes
		foreach (var edge in hallwayPaths)
		{
			GridPosition startingPos = Pathfinding.GridSystem.GetGridPosition(edge.A.VPoint);
			GridPosition end = Pathfinding.GridSystem.GetGridPosition(edge.B.VPoint);

			Pathfinding.GetNode(startingPos).NodeType = Pathfinding.NodeType.Hallway;
			Pathfinding.GetNode(end).NodeType = Pathfinding.NodeType.Hallway;
		}

		foreach (var edge in hallwayPaths)
		{
			GridPosition startingPos = Pathfinding.GridSystem.GetGridPosition(edge.A.VPoint);
			GridPosition end = Pathfinding.GridSystem.GetGridPosition(edge.B.VPoint);
			// DebugDraw.DrawSphere(new BoundingSphere(Pathfinding.GridSystem.GetWorldPosition(startingPos), 15f), Color.AliceBlue, 60f);
			// DebugDraw.DrawSphere(new BoundingSphere(Pathfinding.GridSystem.GetWorldPosition(end), 15f), Color.Aqua, 60f);
			List<GridPosition> paths = Pathfinding.FindPath(startingPos, end);
			if (paths == null) continue;

			for (int i = 0; i < paths.Count - 1; i++)
			{
				Actor floor1 = PrefabManager.SpawnPrefab(floorPrefab, Pathfinding.GridSystem.GetWorldPosition(paths[i]), Quaternion.Identity);
				Actor floot2 = PrefabManager.SpawnPrefab(floorPrefab, Pathfinding.GridSystem.GetWorldPosition(paths[i + 1]), Quaternion.Identity);
				floor1.Parent = Actor;
				floot2.Parent = Actor;
				DebugDraw.DrawLine(
					Pathfinding.GridSystem.GetWorldPosition(paths[i]),
					Pathfinding.GridSystem.GetWorldPosition(paths[i + 1]),
					Color.Red,
					60f
				);
			}
		}
	}

	private List<Delaunay.Edge> CalculatePaths(float debugTime, List<Delaunay.Point> points, HashSet<Delaunay.Edge> edges)
	{
		Debug.Log("Calculating MST ...");
		List<Prim.Edge> weightedEdges = new List<Prim.Edge>();
		foreach (var edge in edges)
		{
			Prim.Edge e = new Prim.Edge(edge.A, edge.B);
			weightedEdges.Add(e);
			// DebugDraw.DrawText($"{e.Distance}", (edge.A.VPoint + edge.B.VPoint) / 2, Color.DarkRed, 8, debugTime, 0.5f);
		}

		List<Delaunay.Edge> finalPaths = Prim.MinimumSpanningTree(weightedEdges, points[0]);

		Debug.Log($"Adding more paths into MST ..." + finalPaths.Count);
		// Add more edges to the MST
		foreach (var edge in edges)
		{
			if (finalPaths.Contains(edge)) continue;

			float rand = Random.Shared.NextFloat();
			if (rand < 0.451f)
				finalPaths.Add(edge);
		}

		Delaunay.Edge.DebugEdges(finalPaths, Color.DarkBlue, 40f);

		return finalPaths;
	}

	private HashSet<Delaunay.Edge> CreateDelaunayTriangulation(List<Delaunay.Point> points)
	{
		Debug.Log("Calculating Delaunay Triangulation...");
		Debug.Log($"Rooms count: {rooms.Count}");
		foreach (var room in rooms)
		{
			Delaunay.Point point = new Delaunay.Point(room.WorldPosition.X, room.WorldPosition.Z);
			points.Add(point);
		}
		Delaunay delaunay = Delaunay.Triangulate(points);
		Debug.Log(delaunay);
		return delaunay.Edges;
	}

	private void SpawnRooms()
	{
		Debug.Log("Spawning rooms ...");
		for (int i = 0; i < MaxRooms; i++)
		{
			GenerateRoom(out Room newRoom);
			rooms.Add(newRoom);
		}
	}

	private void GenerateRoom(out Room _room)
	{
		Random rand = new Random();


		int Width = rand.Next(2, 5);
		int Height = rand.Next(1, 2);
		int Length = rand.Next(2, 5);

		bool isPositionValid = FindValidRoomPosition(Width, Height, Length, out Vector3 position);

		if (!isPositionValid) Debug.LogError("Invalid room position");
		Debug.Log($"Valid position at {position}");
		Actor childModel = PrefabManager.SpawnPrefab(roomPrefab, position, Quaternion.Identity);
		childModel.Parent = Actor;
		childModel.Scale = new Vector3(Width, Height, Length);
		StaticModel model = childModel as StaticModel;
		model.SetMaterial(0, Material);

		GridPosition gridPos = Pathfinding.GridSystem.GetGridPosition(position);
		Pathfinding.ToggleNeighborWalkable(gridPos, Width, Length, false);

		Vector3 worldPos = Pathfinding.GridSystem.GetWorldPosition(gridPos);
		RoomPosition roomPosition = new RoomPosition(worldPos);
		_room = new Room(roomPosition, Width, Height, Length, childModel);
		// Debug.Log($"{_room}");
		return;
	}

	private bool FindValidRoomPosition(int Width, int Height, int Length, out Vector3 _position)
	{

		return FindValidRoomPosition(Width, Height, Length, 5, out _position);
	}

	private bool FindValidRoomPosition(int Width, int Height, int Length, int maxAttemps, out Vector3 _position)
	{
		if (maxAttemps <= 0)
		{
			_position = Vector3.Zero;
			return false;
		}

		// Create a random generator
		Random rnd = new Random();

		// Generate a random position inside the bounding box
		Vector3 position = new Vector3(
			rnd.Next((int)dungeonBounds.Minimum.X, (int)dungeonBounds.Maximum.X),
			0, // Keep Y constant at 0 for floor placement
			rnd.Next((int)dungeonBounds.Minimum.Z, (int)dungeonBounds.Maximum.Z)
		);

		// Check neighbors including base position
		List<GridPosition> neightborhood = Pathfinding.GetNeighborhood(Pathfinding.GridSystem.GetGridPosition(position), Width, Length);
		bool canOccupySpace = true;
		foreach (var neighbor in neightborhood)
		{
			Pathfinding.PathNode node = Pathfinding.GetNode(neighbor);
			// Debug.Log($"Checking neighbor at {neighbor} with node is not null: {node != null}");

			if (node == null) continue;
			// Debug.Log($"Node is: {node}");
			if (!node.IsWalkable)
			{
				canOccupySpace = false;
				break;
			}
		}


		// If there is no hit, set the position
		if (canOccupySpace)
		{
			_position = Pathfinding.GridSystem.GetConvertedWorldPosition(position);
			return true;
		}
		Debug.Log($"Invalid room position, trying again... {maxAttemps - 1}");

		return FindValidRoomPosition(Width, Height, Length, --maxAttemps, out _position);

	}



	public void DestroyDungeon()
	{
		Debug.Log("Destoying dungeon...");
		// If there are rooms in the list
		if (rooms.Count > 0)
		{
			// Iterate through each room and set it to null
			for (int i = 0; i < rooms.Count; i++)
			{

				GridPosition gridPos = Pathfinding.GridSystem.GetGridPosition(rooms[i].WorldPosition.Position3D);
				Pathfinding.ToggleNeighborWalkable(gridPos, rooms[i].Width, rooms[i].Length, true);
				rooms[i] = null;  // Set the room reference to null
			}

			// Now clear the list itself
			rooms.Clear();  // Remove all items from the list
		}

		// If Actor has children, destroy them
		if (Actor.ChildrenCount > 0)
		{
			Actor.DestroyChildren();
		}
	}




	public override void OnDestroy()
	{
		// Clean up resources
		DestroyDungeon();
	}
}
