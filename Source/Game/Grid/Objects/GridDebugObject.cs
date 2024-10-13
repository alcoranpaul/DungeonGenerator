using System;
using System.Collections.Generic;
using FlaxEngine;

namespace Game;

/// <summary>
/// GridDebugObject Script.
/// </summary>
public class GridDebugObject : Script
{
	public TextRender TextRender;
	private GridObject _gridObject;

	public void SetGridObject(GridObject gridObject)
	{
		_gridObject = gridObject;
		SetText(_gridObject.GridPosition.ToString());
	}
	public void SetText(string text)
	{
		TextRender.Text = text;
	}
}
