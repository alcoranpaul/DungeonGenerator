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
	public UIControl generateControl;
	private Button generateButton;

	public override void OnAwake()
	{
		if (generateControl == null || !generateControl.Is<Button>())
		{
			Debug.LogError("Missing or invalid health bar control");
			return;
		}

		generateButton = generateControl.Get<Button>();
		generateButton.Clicked += GenerateDungeon;
	}

	private void GenerateDungeon()
	{
		DungeonGenerator.Instance.GenerateDungeon();
	}

}
