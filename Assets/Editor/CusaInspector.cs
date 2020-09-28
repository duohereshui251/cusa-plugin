﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BeatNodes))]
public class CusaInspector : Editor {

    BeatNodes beats;
    private void OnEnable()
    {
        Debug.Log("[CusaInspector.OnEnable]");
        beats = target as BeatNodes;
        CusaEditor window =
            EditorWindow.GetWindow<CusaEditor>();
        if (window == null)
            Debug.LogError("[CusaInspector.OnEnable] GetWindow error. ");

        
        const int width = 1200;
        const int height = 400;
        var x = (Screen.currentResolution.width - width) / 2;
        var y = (Screen.currentResolution.height - height) / 2;

        window.position = new Rect(x, y, width, height);
        Debug.Log("[CusaInspector.OnEnable] init beats. ");
        window.InitData(beats, width, height);
        //window.Show();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

    }
}
