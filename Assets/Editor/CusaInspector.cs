using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BeatNotes))]
public class CusaInspector : Editor {

    BeatNotes beats;
    private void OnEnable()
    {
        //Debug.Log("[CusaInspector.OnEnable]");
        //beats = target as BeatNotes;
        //CusaEditorWindow window =
        //    EditorWindow.GetWindow<CusaEditorWindow>();
        //if (window == null)
        //    Debug.LogError("[CusaInspector.OnEnable] GetWindow error. ");

        
        //const int width = 1180;
        //const int height = 400;
        //var x = (Screen.currentResolution.width - width) / 2;
        //var y = (Screen.currentResolution.height - height) / 2;

        //window.position = new Rect(x, y, width, height);
        //Debug.Log("[CusaInspector.OnEnable] init beats. ");
        //window.InitData(beats, width, height);
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

    }
}
