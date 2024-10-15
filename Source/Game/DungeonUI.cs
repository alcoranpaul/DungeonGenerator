using System;
using System.Collections.Generic;
using DunGen;
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
		PluginManager.GetPlugin<DunGen.DunGen>().GenerateDungeon();

	}

	private void GenerateGridSystem()
	{
		ClearDebugDraw();

	}

}
