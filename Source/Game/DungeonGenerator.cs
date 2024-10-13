
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
	public int UnitScale = 5;

	public int MaxRoomHeight = 10;
	public int MinRoomHeight = 5;
	public int MaxRoomWidth = 10;
	public int MinRoomWidth = 1;
	public int MaxRoomLength = 10;
	public int MinRoomLength = 1;

	public int DungeonWidth = 100 * 5;
	public int DungeonHeight = 100;

	public float RoomSizeOffset = 0.7f;

	// private List<Model> modelRooms;
	private List<Room> rooms;
	// private DungeonGeneratorData dungeonData;
	private BoundingBox dungeonBounds;

	// Create a dynamic model

	// Reference to the material
	public MaterialBase Material;
	public Prefab debugGridPrefab;
	public Prefab pathfindingDebugPrefab;

	public GridSystem<GridObject> GridSystem { get; private set; }
	public Pathfinding Pathfinding { get; private set; }

	public override void OnAwake()
	{
		Instance = this;

	}

	public void GenerateGridSystem()
	{
		// Grid system for room generation
		GridSystem = new GridSystem<GridObject>(new Vector2(10, 10), 1, (GridSystem<GridObject> gridSystem, GridPosition gridPosition) => { return new GridObject(gridSystem, gridPosition); });
		// GridSystem.CreateDebugObjects(debugGridPrefab);

		// Grid system for hallways
		Pathfinding = new Pathfinding(new Vector2(10, 10), 1, pathfindingDebugPrefab);


	}
	public override void OnStart()
	{
		// modelRooms = new List<Model>();
		rooms = new List<Room>();



	}

	public void GenerateDungeon()
	{
		Debug.Log("Generating dungeon...");
		dungeonBounds = new BoundingBox(new Vector3(-DungeonWidth, -10, -DungeonWidth), new Vector3(DungeonWidth, 10, DungeonWidth));
		DebugDraw.DrawWireBox(dungeonBounds, Color.Beige, 10.0f);

		float debugTime = 60f;
		DestroyDungeon();

		SpawnRooms();

		List<Delaunay.Point> points = new List<Delaunay.Point>();
		foreach (var room in rooms)
		{
			Delaunay.Point point = new Delaunay.Point(room.WorldPosition.X, room.WorldPosition.Z);
			points.Add(point);
		}
		Delaunay delaunay = Delaunay.Triangulate(points);

		// Print the triangulation
		Debug.Log(delaunay.ToString());

		List<Prim.Edge> weightedEdges = new List<Prim.Edge>();
		foreach (var edge in delaunay.Edges)
		{
			Prim.Edge e = new Prim.Edge(edge.A, edge.B);
			weightedEdges.Add(e);
			DebugDraw.DrawText($"{e.Distance}", (edge.A.VPoint + edge.B.VPoint) / 2, Color.DarkRed, 8, debugTime, 0.5f);
		}

		List<Delaunay.Edge> mst = Prim.MinimumSpanningTree(weightedEdges, points[0]);
		// Delaunay.Edge.DebugEdges(mst, Color.Yellow, duration: debugTime);

		// Add more edges to the MST
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
		for (int i = 0; i < MaxRooms; i++)
		{
			Model _model = Content.CreateVirtualAsset<Model>();
			GenerateRoom(_model, out Room newRoom);
			rooms.Add(newRoom);
		}
	}

	private void GenerateRoom(Model _model, out Room _room)
	{
		_room = null;

		// Create the dynamic model with a single LOD and one mesh
		_model.SetupLODs(new[] { 1 });

		// Use System.Random, which is already time-seeded
		var rnd = new Random();

		// Get random values for Width, Height, and Length
		int Width = rnd.Next(MinRoomWidth, MaxRoomWidth);
		int Height = MinRoomHeight;
		int Length = rnd.Next(MinRoomLength, MaxRoomLength);

		// Apply scaling factor
		Width *= UnitScale;
		Height *= UnitScale;
		Length *= UnitScale;

		// Update mesh with initial width (X), fixed-height (Y), and length (Z)
		UpdateMesh(_model.LODs[0].Meshes[0], Width, Height, Length);

		bool isPositionValid = FindValidRoomPosition(_model, Actor.Position, out Vector3 position);

		if (!isPositionValid) return;

		// Create a child model actor
		// WhatIf: Seperate Actor and Data 
		var childModel = Actor.AddChild<StaticModel>();
		childModel.Name = "Room" + childModel.ID;
		childModel.Model = _model;
		childModel.LocalScale = new Float3(1); // No scaling applied
		childModel.SetMaterial(0, Material);

		var collider = childModel.AddChild<BoxCollider>();
		collider.AutoResize(true);
		childModel.LocalPosition = position;


		RoomPosition roomPosition = new RoomPosition(childModel.LocalPosition.X, childModel.LocalPosition.Z);
		_room = new Room(roomPosition, Width, Length, Height, childModel);
		return;
	}

	private bool FindValidRoomPosition(Model model, Vector3 origin, out Vector3 _position)
	{

		BoundingBox modelBounds = model.GetBox(0);
		return FindValidRoomPosition(origin, modelBounds, 5, out _position);
	}

	private bool FindValidRoomPosition(Vector3 origin, BoundingBox modelBounds, int maxAttemps, out Vector3 _position)
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

		// Calculate half-extents of the model
		Vector3 halfExtents = (modelBounds.Maximum - modelBounds.Minimum) * RoomSizeOffset;


		// Perform the BoxCast (or any other logic with halfExtents)
		bool hit = Physics.BoxCast(
			position,                // Position to cast from
			halfExtents,             // Half-extents of the box
			Vector3.Down,            // Direction of the cast
			out var hitInfo,         // Output hit info
			Quaternion.Identity,     // No rotation
			1.0f                     // Max distance to cast
		);

		// If there is no hit, set the position
		if (!hit)
		{
			_position = position;
			return true;
		}

		return FindValidRoomPosition(origin, modelBounds, maxAttemps--, out _position);

	}



	public void DestroyDungeon()
	{
		// If there are rooms in the list
		if (rooms.Count > 0)
		{
			// Iterate through each room and set it to null
			for (int i = 0; i < rooms.Count; i++)
			{
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


	// Method to generate the cube mesh
	private void UpdateMesh(Mesh mesh, int Width, int Height, int Length)
	{



		// Define vertices for a cube (cuboid) with the given width, height, and fixed Z length
		var vertices = new[]
		{
			// Front face (Z = FixedLengthZ / 2)
			 new Float3(-Width / 2, -Height / 2, Length / 2),  // Bottom-left front
			new Float3(Width / 2, -Height / 2, Length / 2),   // Bottom-right front
			new Float3(Width / 2, Height / 2, Length / 2),    // Top-right front
			new Float3(-Width / 2, Height / 2, Length / 2),   // Top-left front

			// Back face (Z = -FixedLengthZ / 2)
			new Float3(-Width / 2, -Height / 2, -Length / 2), // Bottom-left back
			new Float3(Width / 2, -Height / 2, -Length / 2),  // Bottom-right back
			new Float3(Width / 2, Height / 2, -Length / 2),   // Top-right back
			new Float3(-Width / 2, Height / 2, -Length / 2)   // Top-left back
		};

		// Define triangles (each face of the cube is made up of 2 triangles)
		var triangles = new[]
		{
			// Front face
			0, 1, 2,  0, 2, 3,
			// Back face
			4, 6, 5,  4, 7, 6,
			// Left face
			0, 3, 7,  0, 7, 4,
			// Right face
			1, 5, 6,  1, 6, 2,
			// Top face
			3, 2, 6,  3, 6, 7,
			// Bottom face
			0, 4, 5,  0, 5, 1
		};

		// Update the mesh with vertices and triangles
		mesh.UpdateMesh(vertices, triangles, vertices);
	}

	public override void OnDestroy()
	{
		// Clean up resources
		DestroyDungeon();
	}
}
