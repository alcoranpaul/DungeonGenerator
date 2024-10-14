
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

	public float RoomSizeOffset = 0.7f;

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
		Pathfinding.SpawnDebugObjects(pathfindingDebugPrefab);
	}

	public void SpawnDebugObjects()
	{

	}

	public void GenerateDungeon()
	{
		Debug.Log("Generating dungeon...");

		dungeonBounds = Pathfinding.GetBoundingBox();

		DebugDraw.DrawWireBox(dungeonBounds, Color.Beige, 10.0f);

		float debugTime = 60f;

		DestroyDungeon();
		SpawnRooms();


		Debug.Log("Calculating Delaunay Triangulation...");
		List<Delaunay.Point> points = new List<Delaunay.Point>();
		foreach (var room in rooms)
		{
			Delaunay.Point point = new Delaunay.Point(room.WorldPosition.X, room.WorldPosition.Z);
			points.Add(point);
		}
		Delaunay delaunay = Delaunay.Triangulate(points);

		// Print the triangulation
		// Debug.Log(delaunay.ToString());

		Debug.Log("Calculating MST ...");
		List<Prim.Edge> weightedEdges = new List<Prim.Edge>();
		foreach (var edge in delaunay.Edges)
		{
			Prim.Edge e = new Prim.Edge(edge.A, edge.B);
			weightedEdges.Add(e);
			DebugDraw.DrawText($"{e.Distance}", (edge.A.VPoint + edge.B.VPoint) / 2, Color.DarkRed, 8, debugTime, 0.5f);
		}

		List<Delaunay.Edge> mst = Prim.MinimumSpanningTree(weightedEdges, points[0]);

		Debug.Log($"Adding more paths into MST ..." + mst.Count);
		// Add more edges to the MST
		// Delaunay.Edge.DebugEdges(mst, Color.Yellow, 40f);
		foreach (var edge in delaunay.Edges)
		{
			if (mst.Contains(edge)) continue;

			float rand = Random.Shared.NextFloat();
			if (rand < 0.451f)
				mst.Add(edge);
		}

		Delaunay.Edge.DebugEdges(mst, Color.DarkBlue, 40f);
		// Delaunay.DebugTriangulation(delaunay, Color.Aqua, Color.Red, duration: debugTime);


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


		int Width = rand.Next(1, 2);
		int Height = rand.Next(1, 2);
		int Length = rand.Next(1, 3);

		bool isPositionValid = FindValidRoomPosition(Width, Height, Length, out Vector3 position);

		if (!isPositionValid) Debug.LogError("Invalid room position");

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

		GridPosition gridPos0 = Pathfinding.GridSystem.GetGridPosition(position);

		// Check Forward-Backward Direction
		bool isForwardOccupied = false;
		bool isBackwardOccupied = false;
		if (Length > 1)
		{
			Vector3 forward = new Vector3(position.X, 0, position.Z + Length);
			Vector3 backward = new Vector3(position.X, 0, position.Z - Length);

			GridPosition gridForward = Pathfinding.GridSystem.GetGridPosition(forward);
			GridPosition gridBackward = Pathfinding.GridSystem.GetGridPosition(backward);

			isForwardOccupied = (bool)(Pathfinding.GetNode(gridForward)?.IsOccupied());
			isBackwardOccupied = (bool)(Pathfinding.GetNode(gridBackward)?.IsOccupied());
		}

		// Check Left-Right Direction
		bool isLeftOccupied = false;
		bool isRightOccupied = false;
		if (Width > 1)
		{
			Vector3 left = new Vector3(position.X - Width, 0, position.Z);
			Vector3 right = new Vector3(position.X + Width, 0, position.Z);

			GridPosition gridLeft = Pathfinding.GridSystem.GetGridPosition(left);
			GridPosition gridRight = Pathfinding.GridSystem.GetGridPosition(right);

			isLeftOccupied = (bool)(Pathfinding.GetNode(gridLeft)?.IsOccupied());
			isRightOccupied = (bool)(Pathfinding.GetNode(gridRight)?.IsOccupied());
		}

		// Check  Diagonals
		bool isFLeftOccupied = false;
		bool isFRightOccupied = false;
		bool isBLeftOccupied = false;
		bool isBRightOccupied = false;
		if (Length > 1 && Width > 1)
		{
			Vector3 fLeft = new Vector3(position.X - Width, 0, position.Z + Length);
			Vector3 fRight = new Vector3(position.X + Width, 0, position.Z + Length);
			Vector3 bLeft = new Vector3(position.X - Width, 0, position.Z - Length);
			Vector3 bRight = new Vector3(position.X + Width, 0, position.Z - Length);

			GridPosition gridFLeft = Pathfinding.GridSystem.GetGridPosition(fLeft);
			GridPosition gridFRight = Pathfinding.GridSystem.GetGridPosition(fRight);
			GridPosition gridBLeft = Pathfinding.GridSystem.GetGridPosition(bLeft);
			GridPosition gridBRight = Pathfinding.GridSystem.GetGridPosition(bRight);

			isFLeftOccupied = (bool)(Pathfinding.GetNode(gridFLeft)?.IsOccupied());
			isFRightOccupied = (bool)(Pathfinding.GetNode(gridFRight)?.IsOccupied());
			isBLeftOccupied = (bool)(Pathfinding.GetNode(gridBLeft)?.IsOccupied());
			isBRightOccupied = (bool)(Pathfinding.GetNode(gridBRight)?.IsOccupied());
		}

		bool isOccupied = !Pathfinding.GetNode(gridPos0).IsOccupied() && !isForwardOccupied && !isBackwardOccupied && !isLeftOccupied && !isRightOccupied && !isFLeftOccupied && !isFRightOccupied && !isBLeftOccupied && !isBRightOccupied;


		// If there is no hit, set the position
		if (isOccupied)
		{
			_position = Pathfinding.GridSystem.GetConvertedWorldPosition(position);
			// Debug.Log($"Valid room position found at {_position}");

			return true;
		}
		// Debug.Log($"Invalid room position, trying again... {maxAttemps - 1}");

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
