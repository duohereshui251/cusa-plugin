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
    public List<NoteTypeJson> nodeTypes;
    public List<NoteJson> nodes;

    [Serializable]
    public class NoteJson
    {
        public NoteJson(int id,int littlePos, int sound_track, string type, int startPos, int endPos)
        {
            this.ID = id;
            this.littleBeatPos = littlePos;
            this.sound_track = sound_track;
            this.nodeType = type;
            this.startPos = startPos;
            this.endPos = endPos;
        }
        public int ID;
        public int littleBeatPos;
        public int startPos;
        public int endPos;
        public int sound_track;
        public string nodeType;
    }

    [Serializable]
    public class NoteTypeJson
    {
        public NoteTypeJson(int id, string typename)
        {
            ID = id;
            TypeName = typename;
        }
        public int ID;
        public string TypeName;
    }

}
