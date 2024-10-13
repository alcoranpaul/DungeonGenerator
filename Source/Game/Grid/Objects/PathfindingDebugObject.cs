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
		pathNode = _gridObject as Pathfinding.PathNode;
		pathNode.OnDataChanged += (sender, e) => SetText(pathNode.Position.ToString());
		SetText(pathNode.Position.ToString());

	}

	protected override void SetText(string text)
	{
		base.SetText(text);
		gCost.Text = pathNode.GCost.ToString();
		hCost.Text = pathNode.HCost.ToString();
		fCost.Text = pathNode.FCost.ToString();
	}

	public override void OnDestroy()
	{
		pathNode.OnDataChanged -= (sender, e) => SetText(pathNode.Position.ToString());
		base.OnDestroy();
	}
}
