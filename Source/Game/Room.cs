using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game;

/// <summary>
/// Room Script.
/// </summary>
public class Room
{
	public Vector2 WorldPosition { get; private set; }
	public int Width { get; private set; }
	public int Length { get; private set; }
	public int Height { get; private set; }
	public Actor ModelActor { get; private set; }

	public Room(Vector2 worldPosition, int width, int length, int height, Actor modelActor)
	{
		WorldPosition = worldPosition;
		Width = width;
		Length = length;
		Height = height;
		ModelActor = modelActor;
	}


}
