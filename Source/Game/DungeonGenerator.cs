
using System;
using System.Collections.Generic;
using FlaxEngine;

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

	// private List<Model> modelRooms;
	private List<Room> rooms;
	// private DungeonGeneratorData dungeonData;
	private BoundingBox dungeonBounds;

	// Create a dynamic model

	// Reference to the material
	public MaterialBase Material;

	public override void OnAwake()
	{
		Instance = this;
	}
	public override void OnStart()
	{
		// modelRooms = new List<Model>();
		rooms = new List<Room>();

		dungeonBounds = new BoundingBox(new Vector3(-DungeonWidth, -10, -DungeonWidth), new Vector3(DungeonWidth, 10, DungeonWidth));

	}

	public void GenerateDungeon()
	{
		Debug.Log("Generating dungeon...");

		DestroyDungeon();

		for (int i = 0; i < MaxRooms; i++)
		{
			Model _model = Content.CreateVirtualAsset<Model>();
			rooms.Add(GenerateRoom(_model));

			// modelRooms.Add(_model);
		}

	}

	private Room GenerateRoom(Model _model)
	{
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

		// Create or reuse a child model actor
		var childModel = Actor.AddChild<StaticModel>();
		childModel.Name = "Room" + childModel.ID;
		childModel.Model = _model;
		childModel.LocalScale = new Float3(1); // No scaling applied
		childModel.SetMaterial(0, Material);

		var collider = childModel.AddChild<BoxCollider>();
		collider.AutoResize(true);

		SetPosition(childModel);
		Debug.Log($"Position: {childModel.LocalPosition}");

		Vector2 roomPosition = new Vector2(childModel.LocalPosition.X, childModel.LocalPosition.Z);
		return new Room(roomPosition, Width, Length, Height, childModel);
	}

	private void SetPosition(StaticModel staticModel)
	{
		// Create a random generator
		Random rnd = new Random();

		// Generate a random position inside the bounding box
		Vector3 position = new Vector3(
			rnd.Next((int)dungeonBounds.Minimum.X, (int)dungeonBounds.Maximum.X),
			0, // Keep Y constant at 0 for floor placement
			rnd.Next((int)dungeonBounds.Minimum.Z, (int)dungeonBounds.Maximum.Z)
		);


		// Retrieve the bounding box of the StaticModel
		BoundingBox modelBounds = staticModel.Model.GetBox(staticModel.Transform);
		// DebugDraw.DrawBox(modelBounds, Color.Green, 10.0f);

		// Calculate half-extents of the model
		Vector3 halfExtents = (modelBounds.Maximum - modelBounds.Minimum) * 0.5f;
		// Debug.Log("Half extents: " + halfExtents);
		DebugDraw.DrawBox(new BoundingBox(staticModel.Transform.TransformPoint(position) - modelBounds.Minimum, staticModel.Transform.TransformPoint(position) + modelBounds.Minimum), Color.Green, 1.0f);

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
			staticModel.LocalPosition = position;
		}
		else
		{
			Debug.Log($"{staticModel} hit something at {position}");
		}
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
