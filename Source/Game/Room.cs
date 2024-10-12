using System;
using System.Collections.Generic;
using FlaxEngine;


namespace Game;

/// <summary>
/// Room Script.
/// </summary>
public class Room
{
	public RoomPosition WorldPosition { get; private set; }
	public int Width { get; private set; }
	public int Length { get; private set; }
	public int Height { get; private set; }
	public Actor ModelActor { get; private set; }

	public Room(RoomPosition worldPosition, int width, int length, int height, Actor modelActor)
	{
		WorldPosition = worldPosition;
		Width = width;
		Length = length;
		Height = height;
		ModelActor = modelActor;
	}


}

public struct RoomPosition
{
	public float X;
	public float Z;
	public readonly Vector2 Position2D => new Vector2(X, Z);
	public readonly Vector3 Position3D => new Vector3(X, 0, Z);

	public RoomPosition(float x, float z)
	{
		X = x;
		Z = z;
	}

}
