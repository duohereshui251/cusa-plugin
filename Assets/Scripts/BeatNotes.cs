using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Security.Permissions;
using UnityEngine;


public class justForDebug
{
    static public bool on = true;
}
public enum BeatType
{
    Invalid = -1,
    Single = 1,
    Longkey = 2,
    Point = 3
}
[CreateAssetMenu(fileName = "Notes_", menuName = "BeatsNotes")]
public class BeatNotes : ScriptableObject
{
    [SerializeField, HideInInspector]
    List<Note> l_Notes = new List<Note>();

    // TODO1: new feature -- NoteType Edit
    CustomNoteType c_NoteTypes;
    static public string default_name =  "New Note Type";
    public int I_BeatPerMinute;
    public int I_BeatOffset;
    public float F_BeatStartOffset;
    public int I_LittleBeats = 2;
    public AudioClip AC_ClipToPlay;
    public int I_SoundTracks;
    public string S_SongName { get; set; }
    #region Interact APIs
    Note tempNote;

    public void Clear()
    {
        l_Notes.Clear();
    }
    public int GetPerfectScore()
    {
        int perfect = 0;
        for (int i = 0; i < l_Notes.Count; i++)
        {
            switch (l_Notes[i].e_Type)
            {
                case BeatType.Single:
                    perfect += 1;
                    break;
                case BeatType.Longkey:
                    perfect += 2;
                    break;
                case BeatType.Point:
                    perfect += 3;
                    break;
            }
        }
        return perfect;
    }
    public List<Note> GetNotes()
    {
        return l_Notes;
    }
    public void ForceSort()
    {
        l_Notes.Sort((left, right) => { 
            if(left.i_LittleBeatPos > right.i_LittleBeatPos)
            {
                return 1;
            }
            else
            {
                return -1;
            }
            
        });
    }

    public void SetTrackRandom()
    {
        System.Random rnd = new System.Random();
        foreach (var n in l_Notes)
        {
            n.i_sound_track = rnd.Next(1, I_SoundTracks+1);
        }
    }
    public int GetPerBlockNotes(int blockIndex, int perBlockBeats, int trackIndex)
    {
        int res = 0;
        int startBeatPos = blockIndex * perBlockBeats;
        for(int i = 0; i < perBlockBeats;++i)
        {
            if(ContainsNote(startBeatPos + i, trackIndex))
            {
                res += 1;
            }
        }
        return res;
    }
    public bool ContainsNote(int littleBeatPos, int sound_track)
    {
        tempNote = l_Notes.Find(p => p.i_LittleBeatPos == littleBeatPos );
        return tempNote != null && tempNote.i_sound_track == sound_track;
    }
    //public void SetNote(int littleBeatPos, int sound_track, BeatType type)
    //{
    //    tempNote = GetNoteByPos(littleBeatPos);
    //    if (tempNote != null)
    //    {
    //        int index = l_Notes.IndexOf(tempNote);
    //        Note n = new Note(littleBeatPos, sound_track, type);
    //        l_Notes[index] = n;
    //    }
    //    else
    //    {
    //        for (int i = 0; i < l_Notes.Count; i++)
    //        {
    //            if (l_Notes[i].i_LittleBeatPos > littleBeatPos)
    //            {
    //                l_Notes.Insert(i, new Note(littleBeatPos, sound_track, type));
    //                return;
    //            }
    //        }
    //        l_Notes.Add(new Note(littleBeatPos, sound_track, type));
    //    }
    //}

    public void SetNote(int littleBeatPos, int sound_track, string typename)
    {
        tempNote = GetNoteByPos(littleBeatPos);

        if (tempNote != null)
        {
            int index = l_Notes.IndexOf(tempNote);
            Note n = new Note(littleBeatPos, sound_track, typename);
            l_Notes[index] = n;
        }
        else
        {
            for (int i = 0; i < l_Notes.Count; i++)
            {
                if (l_Notes[i].i_LittleBeatPos > littleBeatPos)
                {
                    l_Notes.Insert(i, new Note(littleBeatPos,sound_track, typename));
                    return;
                }
            }
            l_Notes.Add(new Note(littleBeatPos, sound_track, typename));
        }

    }

    public void SetNoodle(int littleBeatPos, int sound_track, int startPos, int endPos, bool isStart, bool isEnd)
    {
        if (startPos < 0 || endPos < 0)
        {
            return;
        }
        tempNote = GetNoteByPos(littleBeatPos);

        if (tempNote != null)
        {
            int index = l_Notes.IndexOf(tempNote);
            Note n = new Note(littleBeatPos, sound_track, NoteType.Noodle, startPos, endPos);
            n.isStart = isStart;
            n.isEnd = isEnd;
            l_Notes[index] = n;
        }
        else
        {
            for (int i = 0; i < l_Notes.Count; i++)
            {
                if (l_Notes[i].i_LittleBeatPos > littleBeatPos)
                {
                    Note nn = new Note(littleBeatPos, sound_track, NoteType.Noodle, startPos, endPos);
                    nn.isStart = isStart;
                    nn.isEnd = isEnd;
                    l_Notes.Insert(i, nn);
                    return;
                }
            }
            Note n = new Note(littleBeatPos, sound_track, NoteType.Noodle, startPos, endPos);
            l_Notes.Add(n);
        }

    }
    public void AdjustNote(int littleBeatPos, BeatType type)
    {

        tempNote = GetNoteByPos(littleBeatPos);
        if (tempNote != null)
        {
            int index = l_Notes.IndexOf(tempNote);
            Note n = new Note(littleBeatPos, tempNote.i_sound_track, type);
            l_Notes[index] = n;
        }
        else
        {
            Debug.LogError("Can't Adjust A Unexisted Note!Pos:" + littleBeatPos);
        }
    }

    //public void AdjustNote(int littleBeatPos, int littleBeatPos,string typename)
    //{

    //    tempNote = GetNoteByPos(littleBeatPos);
    //    if (tempNote != null)
    //    {
    //        int index = l_Notes.IndexOf(tempNote);
    //        Note n = new Note(littleBeatPos ,tempNote.i_sound_track, typename);
    //        l_Notes[index] = n;
    //    }
    //    else
    //    {
    //        Debug.LogError("Can't Adjust A Unexisted Note!Pos:" + littleBeatPos);
    //    }

    //}
    public void RemoveNote(int littleBeatPos)
    {
        tempNote = GetNoteByPos(littleBeatPos);
        if (tempNote == null)
        {
            Debug.LogWarning(littleBeatPos.ToString() + " Note Not Found Howdf U Remove A UnAddNote In Editor?");
            return;
        }
        l_Notes.Remove(tempNote);
    }
    public Note GetNoteByPos(int littleBeatPos)
    {
        return l_Notes.Find(p => p.i_LittleBeatPos == littleBeatPos);
    }
    public Note GetNoteByIndex(int index)
    {
        return l_Notes[index];
    }
    public int GetNoteIndex(int littleBeatPos)
    {
        return l_Notes.FindIndex(p => p.i_LittleBeatPos == littleBeatPos);
    }
    public Dictionary<float, int> GetTotalBeatsCenterWithOffset(float f_beatEach)
    {
        // 节拍和时间的映射表
        Dictionary<float, int> dic = new Dictionary<float, int>();
        for (int i = 0; i < l_Notes.Count; i++)
        {

            switch (l_Notes[i].e_Type)
            {
                case BeatType.Single:
                    {
                        dic.Add(F_BeatStartOffset + l_Notes[i].i_LittleBeatPos * f_beatEach, i);
                    }
                    break;
                case BeatType.Longkey:
                    {
                        dic.Add(F_BeatStartOffset + l_Notes[i].i_LittleBeatPos * f_beatEach, i);
                    }
                    break;
                case BeatType.Point:
                    {
                        dic.Add(F_BeatStartOffset + l_Notes[i].i_LittleBeatPos * f_beatEach, i);
                    }
                    break;
            }
        }
        return dic;
    }
    public List<float> BeatsCenterWithOffset(int littleBeatPos, BeatType type, float f_beatEach)
    {
        // TODO: 处理一节拍有多个小拍的情况
        List<float> beatMids = new List<float>();
        switch (type)
        {
            case BeatType.Single:
                {
                    beatMids.Add(F_BeatStartOffset + littleBeatPos * f_beatEach);
                }
                break;
            case BeatType.Longkey:
                {

                    beatMids.Add(F_BeatStartOffset + littleBeatPos * f_beatEach);
                }
                break;
            case BeatType.Point:
                {
                    beatMids.Add(F_BeatStartOffset + littleBeatPos * f_beatEach);
                }
                break;
        }
        return beatMids;
    }

    public List<float> BeatsCenterWithOffset(int littleBeatPos, string  typename, float f_beatEach)
    {
        List<float> beatMids = new List<float>();
        beatMids.Add(F_BeatStartOffset + littleBeatPos * f_beatEach);
        return beatMids;

    }
    #endregion

    #region NoteType Edit
    public bool AddNoteType(string typename, Color color)
    {
        return c_NoteTypes.Insert(typename, color);
    }

    public bool DeleteNoteType(string typename)
    {
        return c_NoteTypes.Delete(typename);
    }

    public int FindNoteType(string typename)
    {
        return c_NoteTypes.Find(typename);
    }

    public bool UpdateNoteType(string old_name, string new_name)
    {
        return c_NoteTypes.Update(old_name, new_name);
    }

    public bool UpdateNoteType(string old_name, Color old_color, string new_name, Color new_color)
    {
        return c_NoteTypes.Update( old_name, old_color, new_name, new_color);
    }

    #endregion

    #region CustomNoteType Interact APIs

    public void InitType()
    {
        if(c_NoteTypes == null)
            c_NoteTypes = new CustomNoteType();
    }
    
    // just for debug
    public void PrintAllNoteType()
    {
        Debug.Log("[BeatNotes.PrintAllNoteType]");
        c_NoteTypes.Print();
    }
    public int GetTypeCount()
    {
        return c_NoteTypes.Count();
    }
    public NoteType GetTypeByName(string typename)
    {
        return c_NoteTypes.GetTypeByName(typename);
    }

    public NoteType GetTypeByIndex(int index)
    {
        return c_NoteTypes.GetTypeByIndex(index);
    }

    public bool InsertType(string typename, Color color)
    {
        return c_NoteTypes.Insert(typename, color);
    }

    public bool DeleteType(string typename)
    {
        return c_NoteTypes.Delete(typename);
    }

    public int FindType(string typename)
    {
        return c_NoteTypes.Find(typename);
    }

    public bool UpdateType(string old_name, string new_name)
    {
        return c_NoteTypes.Update(old_name, new_name);
    }

    public bool UpdateType(string old_name, Color old_color, string new_name, Color new_color)
    {
        return c_NoteTypes.Update(old_name, old_color, new_name, new_color);
    }

    public Color GetTypeColor(string typename)
    {
        return c_NoteTypes.GetColor(typename);
    }

    #endregion
}

[System.Serializable]
public class Note
{
    /// <summary>
    /// 
    /// </summary>
    public int i_LittleBeatPos;
    public int i_sound_track;
    public BeatType e_Type;
    /// <summary>
    /// Noodle 开头
    /// </summary>
    public int i_StartPos;
    /// <summary>
    /// Noodle 结尾
    /// </summary>
    public int i_EndPos;
    /// <summary>
    /// Noodle设置是否结束
    /// </summary>
    [NonSerialized]
    public bool isEnd = false;
    [NonSerialized]
    public bool isStart = false;
    // TODO1: new feature -- NoteType Edit
    public string c_Type; // 用户自定义节奏点类型
    public Note(int littleBeatPos, int sound_track, BeatType type)
    {
        i_LittleBeatPos = littleBeatPos;
        i_sound_track = sound_track;
        e_Type = type;
        i_StartPos = littleBeatPos;
        i_EndPos = littleBeatPos;
        
    }

    public Note(int littleBeatPos,int sound_track, string type)
    {
        i_LittleBeatPos = littleBeatPos;
        i_sound_track = sound_track;
        c_Type = type;
        i_StartPos = littleBeatPos;
        i_EndPos = littleBeatPos;
    }
    public Note(int littleBeatPos, int sound_track, string type, int startPos, int endPos)
    {
        i_LittleBeatPos = littleBeatPos;
        i_sound_track = sound_track;
        c_Type = type;
        i_StartPos = startPos;
        i_EndPos = endPos;
    }
}


public class NoteType
{
    public NoteType(int id, string typename)
    {
        I_Id = id;
        S_TypeName = typename;
        NoteColor = Color.white;
    }

    public NoteType(int id, string typename, Color color)
    {
        I_Id = id;
        S_TypeName = typename;
        NoteColor = color;
    }

    public string GetName()
    {
        return S_TypeName;
    }
    public static string Invalid = "Invalid";
    public static string Noodle = "Noodle";
    public static string Single = "Single";
    public int I_Id;
    public string S_TypeName;
    public Color NoteColor;
}

public class DefaultNoteType
{
    public List<NoteType> l_defaultTypes;
    public DefaultNoteType()
    {
        Debug.Log("BaseClass: DefaultNoteType");
        l_defaultTypes = new List<NoteType>();
        l_defaultTypes.Add(new NoteType(0, NoteType.Noodle));
        l_defaultTypes.Add(new NoteType(1, NoteType.Single));
    }

}

public class CustomNoteType: DefaultNoteType
{
    public List<NoteType> l_customTypes;

    // 插入节奏点类型：如果列表已有相同命名的类型，则返回失败，否则返回成功

    public CustomNoteType():base()
    {
        Debug.Log("DerivedClass: CustomNoteType");
        l_customTypes = new List<NoteType>();
    }
    public NoteType GetTypeByName(string typename)
    {
        if(l_defaultTypes.Exists(x => x.S_TypeName == typename))
        {
            return l_defaultTypes.Find(x => x.S_TypeName == typename);
        }
        else if(l_customTypes.Exists(x => x.S_TypeName == typename))
        {
            return l_customTypes.Find(x => x.S_TypeName == typename);
        }
        else
        {
            Debug.LogFormat("[BeatNotes.GetTypeByName] node types doesn't exist {0}", typename);
            return null;
        }
    }
    public NoteType GetTypeByIndex(int index)
    {
        if(index > Count() || index < 0)
        {
            return l_defaultTypes[0];
        }
        if(index < l_defaultTypes.Count)
        {
            return l_defaultTypes[index];

        }else
        {
            return l_customTypes[index - 2];
        }

    }
    public bool Insert(string typename, Color color)
    {
        bool res = false;
        int new_id = 0;
        if (l_customTypes.Count > 0)
        {
            new_id = l_customTypes[l_customTypes.Count - 1].I_Id + 1;
        }
        if (typename == BeatNotes.default_name)
        {
            typename += new_id.ToString();          
        }
        Debug.LogFormat("[CustomNoteType.Insert] typename {0} is going to be inserted", typename);
        if(!l_defaultTypes.Exists(x => x.S_TypeName == typename) )
        {
            if(!l_customTypes.Exists(x => x.S_TypeName == typename))
            {
                res = true;
                NoteType n = new NoteType(new_id, typename, color);
                if(n == null)
                {
                    Debug.Log("[CustomNoteType.Insert] new Notetype failed");
                }
                else
                {
                    Debug.LogFormat("[CustomNoteType.Insert] inserted {0}", typename);
                    l_customTypes.Add(n);
                    Debug.LogFormat("[CustomNoteType.Insert] Type Count {0}", l_customTypes.Count);
                }
            }
        }
        return res;
    }
    // 删除节奏点类型： 如果列表不存在该类型， 则返回失败，否则删除并返回成功
    public bool Delete(string typename)
    {
        // 不能删除默认的
        bool res = l_customTypes.Remove(l_customTypes.Find(x => x.S_TypeName == typename));
        return res;
    }

    // 查找节奏点类型： 如果查找成功， 返回id，如果失败，返回 invalid -1
    public int Find(string typename)
    {
        int res = l_defaultTypes.FindIndex(x => x.S_TypeName == typename);
        if(res == -1)
        {
            res = l_customTypes.FindIndex(x => x.S_TypeName == typename) + 2;
        }
        return res;
    }

    // 更新节奏点类型： 如果更新成功（与已有类型不会产生冲突），则返回成功，否则返回失败
    public bool Update(string old_name, string new_name)
    {
        bool res = false;
        int index = l_defaultTypes.FindIndex(x => x.S_TypeName == old_name);

        // 如果是默认的则不允许更新
        if(index == -1)
        {
            index = l_customTypes.FindIndex(x => x.S_TypeName == old_name);
            if(index != -1)
            {
                res = true;
                l_customTypes[index].S_TypeName = new_name;
            }
        }
        return res;
    }

    public bool Update(string old_name, Color old_color, string new_name, Color new_color)
    {
        bool res = false;
        int index = l_defaultTypes.FindIndex(x => x.S_TypeName == old_name);

        // 如果是默认的则不允许更新
        if (index == -1)
        {
            index = l_customTypes.FindIndex(x => x.S_TypeName == old_name);
            if (index != -1)
            {
                res = true;
                l_customTypes[index].S_TypeName = new_name;
                l_customTypes[index].NoteColor = new_color;
            }
        }
        return res;

    }
    public Color GetColor(string typename)
    {
        int index = Find(typename);
        if(index != -1)
        {
            if(index  < 2)
            {
                return l_defaultTypes[index].NoteColor;
            }
            else
            {
                return l_customTypes[index - 2].NoteColor;
            }
        }
        else
        {
            return Color.white;
        }
    }

    public int Count()
    {
        int res = 0;
        res = l_defaultTypes.Count + l_customTypes.Count;
        //Debug.LogFormat("[BeatNotes.Count] customtype count {0}", l_customTypes.Count);
        return res;
    }

    // used for debug
    public void Print()
    {
        foreach(var t in l_defaultTypes)
        {
            Debug.LogFormat("type id: {0}, name: {1}, color: {2}", t.I_Id, t.S_TypeName, t.NoteColor.ToString());
        }
        foreach(var t in l_customTypes)
        {
            Debug.LogFormat("type id: {0}, name: {1}, color: {2}", t.I_Id, t.S_TypeName, t.NoteColor.ToString());
        }

    }
        
}
