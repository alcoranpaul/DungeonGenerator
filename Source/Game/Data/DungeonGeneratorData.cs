using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game;

/// <summary>
/// DungeonGeneratorData Script.
/// </summary>
public class DungeonGeneratorData
{
	public int MaxRooms = 30;
	public int UnitScale = 5;

	public int MaxRoomHeight = 10;
	public int MinRoomHeight = 1;
	public int MaxRoomWidth = 10;
	public int MinRoomWidth = 1;
	public int MaxRoomLength = 10;
	public int MinRoomLength = 5;

	public int DungeonWidth = 100 * 5;
	public int DungeonHeight = 100;

	public override string ToString()
	{
		return $"MaxRooms: {MaxRooms}, UnitScale: {UnitScale}, MaxRoomHeight: {MaxRoomHeight}, MaxRoomWidth: {MaxRoomWidth}, MaxRoomLength: {MaxRoomLength}, MinRoomHeight: {MinRoomHeight}, MinRoomWidth: {MinRoomWidth}, MinRoomLength: {MinRoomLength}";
	}

}
