using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using JetBrains.Annotations;

[Serializable]
public class BeatNotesJson 
{

    public string song_name;
    public string path;
    public int bpm;
    public float offset;
    public int little_beats;
    public int sound_tracks;
    
    public List<NoteJson> nodes;

    [Serializable]
    public class NoteJson
    {
        public NoteJson(int id,int littlePos, int sound_track, BeatType type)
        {
            this.ID = id;
            this.littleBeatPos = littlePos;
            this.sound_track = sound_track;
            this.nodeType = type;
        }
        public int ID;
        public int littleBeatPos;
        public int sound_track;
        public BeatType nodeType;
    }
}
