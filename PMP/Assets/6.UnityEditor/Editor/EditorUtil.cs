using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class EditorUtil
{
	public static void DrawGrid(Rect container,float gridSize = 7) {

		if ( Event.current.type != EventType.Repaint ) {
			return;
		}

		Handles.color = new Color(0, 0, 0, 0.15f);
		var step = gridSize;

		// var xDiff = offset.x % step;
		// var xStart = container.xMin + xDiff;
		var xStart = container.xMin + step;
		var xEnd = container.xMax;
		for ( var i = xStart; i < xEnd; i += step ) {
			Handles.DrawLine(new Vector3(i, container.yMin, 0), new Vector3(i, container.yMax, 0));
		}

		// var yDiff = offset.y % step;
		// var yStart = container.yMin + yDiff;
		var yStart = container.yMin + step;
		var yEnd = container.yMax;
		for ( var i = yStart; i < yEnd; i += step ) {
			Handles.DrawLine(new Vector3(0, i, 0), new Vector3(container.xMax, i, 0));
		}

		Handles.color = Color.white;
	}
}
