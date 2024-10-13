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
	protected object _gridObject;

	public virtual void SetGridObject(object gridObject)
	{
		_gridObject = gridObject;
	}
	protected virtual void SetText(string text)
	{
		TextRender.Text = text;
	}


}
