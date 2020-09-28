﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



[Serializable]
public class BeatNodesJson 
{

    public string song_name;
    public string path;
    public int bpm;
    public float offset;
    public int sound_tracks;
    public List<NodeJson> nodes;

    [Serializable]
    public class NodeJson
    {
        public NodeJson(int beatPos, int sound_track, BeatType type)
        {
            this.beatPos = beatPos;
            this.sound_track = sound_track;
            this.nodeType = type;
        }
        public int beatPos;
        public int sound_track;
        public BeatType nodeType;
    }
}
