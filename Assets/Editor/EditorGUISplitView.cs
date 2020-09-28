using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.UIElements;

public class EditorGUISplitView
{

	public enum Direction
	{
		Horizontal,
		Vertical
	}

	Direction splitDirection;
	float splitNormalizedPosition;
	int splitCount = 1;
	bool resize;
	public Vector2 scrollPosition;
	Rect availableRect;


	public EditorGUISplitView(Direction splitDirection, float splitScale)
	{
		splitNormalizedPosition = splitScale;
		this.splitDirection = splitDirection;
	}

	public void BeginSplitView()
	{
		Rect tempRect;

		if (splitDirection == Direction.Horizontal)
			tempRect = EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true));
		else
			tempRect = EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

		if (tempRect.width > 0.0f)
		{
			availableRect = tempRect;
		}
		if (splitDirection == Direction.Horizontal)
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(availableRect.width * splitNormalizedPosition));
		else
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(availableRect.height * splitNormalizedPosition));
	}

	public void Split()
	{

		GUILayout.EndScrollView();
		//ResizeSplitFirstView();
		if (splitDirection == Direction.Horizontal)
        {
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(availableRect.width * splitNormalizedPosition));
			// TODO 画线

		}
		else
        {
			scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(availableRect.height * splitNormalizedPosition));

		}


	}

	public void EndSplitView()
	{
		GUILayout.EndScrollView();

		if (splitDirection == Direction.Horizontal)
			EditorGUILayout.EndHorizontal();
		else
			EditorGUILayout.EndVertical();
	}

	private void ResizeSplitFirstView()
	{

		// 画矩形方框
		Rect resizeHandleRect;

		if (splitDirection == Direction.Horizontal)
			resizeHandleRect = new Rect(availableRect.width * splitNormalizedPosition, availableRect.y, 2f, availableRect.height);
		else
			resizeHandleRect = new Rect(availableRect.x, availableRect.height * splitNormalizedPosition, availableRect.width, 2f);

		GUI.DrawTexture(resizeHandleRect, EditorGUIUtility.whiteTexture);

		if (splitDirection == Direction.Horizontal)
			EditorGUIUtility.AddCursorRect(resizeHandleRect, MouseCursor.ResizeHorizontal);
		else
			EditorGUIUtility.AddCursorRect(resizeHandleRect, MouseCursor.ResizeVertical);

		if (Event.current.type == EventType.MouseDown && resizeHandleRect.Contains(Event.current.mousePosition))
		{
			resize = true;
		}
		if (resize)
		{
			if (splitDirection == Direction.Horizontal)
				splitNormalizedPosition = Event.current.mousePosition.x / availableRect.width;
			else
				splitNormalizedPosition = Event.current.mousePosition.y / availableRect.height;
		}
		if (Event.current.type == EventType.MouseUp)
			resize = false;
	}
}
