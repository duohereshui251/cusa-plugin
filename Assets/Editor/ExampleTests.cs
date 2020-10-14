using UnityEngine;
using System.Collections;
using UnityEditor;

public class TestDrag : EditorWindow
{

    string path;
    Rect rect;
    Vector2 scrollPosition;

    [MenuItem("Window/TestDrag")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(TestDrag));
    }

    void OnGUI()
    {
        //GUI.Button(new Rect(0, 0, 100, 20), "Click Me");
        //GUI.Button(new Rect(0, 30, 100, 20), new GUIContent("Click Me"));

        ReadOnlyTextField("readonly text", 100f);
    }

    void ReadOnlyTextField(string text, float Height)
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(Height));
        {
            // Readonly TextArea with working scrollbars
            GUILayout.TextArea(text, EditorStyles.textField, GUILayout.ExpandHeight(true));
        }
        GUILayout.EndScrollView();
    }
}