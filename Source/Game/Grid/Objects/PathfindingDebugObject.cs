using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game;

/// <summary>
/// PathfindingDebugObject Script.
/// </summary>
public class PathfindingDebugObject : GridDebugObject
{
	public TextRender gCost;
	public TextRender hCost;
	public TextRender fCost;
	private Pathfinding.PathNode pathNode;

	public override void SetGridObject(object gridObject)
	{
		base.SetGridObject(gridObject);
		pathNode = GridObject as Pathfinding.PathNode;
		pathNode.OnDataChanged += (sender, e) => SetText(pathNode.GridPosition.ToString());
		SetText(pathNode.GridPosition.ToString());

	}

	protected override void SetText(string text)
	{
		base.SetText(text);
		if (pathNode.IsWalkable)
		{
			gCost.Text = pathNode.GCost.ToString();
			hCost.Text = pathNode.HCost.ToString();
			fCost.Text = pathNode.FCost.ToString();
		}

		gCost.IsActive = pathNode.IsWalkable;
		hCost.IsActive = pathNode.IsWalkable;
		fCost.IsActive = pathNode.IsWalkable;

	}

	public override void OnDestroy()
	{
		pathNode.OnDataChanged -= (sender, e) => SetText(pathNode.GridPosition.ToString());
		base.OnDestroy();
	}
}
