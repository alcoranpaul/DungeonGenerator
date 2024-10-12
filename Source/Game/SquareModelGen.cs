using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game;

/// <summary>
/// SquareModelGen Script.
/// </summary>
public class SquareModelGen : Script
{
	// Variables for width and height
	public float Width = 5.0f;  // X-axis dimension
	public float Height = 5.0f; // Y-axis dimension
	public float Length = 5.0f; // Fixed Z-axis length

	// Create a dynamic model
	private Model _model;

	// Reference to the material
	public MaterialBase Material;

	// Method to generate the cube mesh
	private void UpdateMesh(Mesh mesh)
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

	public override void OnStart()
	{
		// Create the dynamic model with a single LOD and one mesh
		_model = Content.CreateVirtualAsset<Model>();
		_model.SetupLODs(new[] { 1 });

		// Update mesh with initial width, height, and fixed Z-length
		UpdateMesh(_model.LODs[0].Meshes[0]);

		// Create or reuse a child model actor
		var childModel = Actor.GetOrAddChild<StaticModel>();
		childModel.Model = _model;
		childModel.LocalScale = new Float3(1); // No scaling applied
		childModel.SetMaterial(0, Material);
	}

	public override void OnDestroy()
	{
		// Clean up resources
		FlaxEngine.Object.Destroy(ref _model);
	}


}
