using System;
using System.Collections.Generic;
using FlaxEngine;
using FlaxEngine.GUI;

namespace Game;

/// <summary>
/// DungeonUI Script.
/// </summary>
public class DungeonUI : Script
{
	public UIControl dungeonControl;
	private Button dungeonButton;

	public UIControl gridControl;
	private Button gridButton;

	public UIControl debugControl;
	private Button debugButton;

	public InputEvent TestEvent = new InputEvent("Test");

	public Actor mouseActor;

	public override void OnAwake()
	{
		if (dungeonControl == null || !dungeonControl.Is<Button>())
		{
			Debug.LogError("Missing or invalid health bar control");
			return;
		}

		dungeonButton = dungeonControl.Get<Button>();
		dungeonButton.Clicked += GenerateDungeon;

		if (gridControl == null || !gridControl.Is<Button>())
		{
			Debug.LogError("Missing or invalid health bar control");
			return;
		}

		gridButton = gridControl.Get<Button>();
		gridButton.Clicked += GenerateGridSystem;

		if (debugControl == null || !debugControl.Is<Button>())
		{
			Debug.LogError("Missing or invalid health bar control");
			return;
		}

		debugButton = debugControl.Get<Button>();
		debugButton.Clicked += ClearDebugDraw;

		TestEvent.Pressed += CalculatePath;
	}



	private void CalculatePath()
	{
		var pos = Input.MousePosition;
		var ray = Camera.MainCamera.ConvertMouseToRay(pos);
		if (Physics.RayCast(ray.Position, ray.Direction, out RayCastHit hit))
		{

			mouseActor.Position = hit.Point;
			GridPosition startingGridPos = new GridPosition(0, 0);
			GridPosition endPos = DungeonGenerator.Instance.Pathfinding.GridSystem.GetGridPosition(hit.Point);
			List<GridPosition> paths = DungeonGenerator.Instance.Pathfinding.FindPath(startingGridPos, endPos);

			for (int i = 0; i < paths.Count - 1; i++)
			{
				DebugDraw.DrawLine(
					DungeonGenerator.Instance.Pathfinding.GridSystem.GetWorldPosition(paths[i]),
					DungeonGenerator.Instance.Pathfinding.GridSystem.GetWorldPosition(paths[i + 1]),
					Color.Red,
					10f
				);
			}
		}

	}



	public override void OnDestroy()
	{
		TestEvent.Dispose();
		base.OnDestroy();
	}

	private void ClearDebugDraw()
	{
		DebugDraw.UpdateContext(IntPtr.Zero, float.MaxValue);
	}

	private void GenerateDungeon()
	{
		ClearDebugDraw();
		DungeonGenerator.Instance.GenerateDungeon();
	}

	private void GenerateGridSystem()
	{
		ClearDebugDraw();
		DungeonGenerator.Instance.SpawnDebugObjects();
	}

}
