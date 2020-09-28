using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BeatType
{
    Invalid = -1,
    Single = 1,
    Longkey = 2,
    Point = 3
}
[CreateAssetMenu(fileName = "Nodes_", menuName = "BeatsNodes")]
public class BeatNodes : ScriptableObject
{
    [SerializeField, HideInInspector]
    List<Node> l_Nodes = new List<Node>();
    public int I_BeatPerMinute;
    public float F_BeatStartOffset;
    public AudioClip AC_ClipToPlay;
    public int I_SoundTracks;
    #region Interact APIs
    Node tempNode;
    public void Clear()
    {
        l_Nodes.Clear();
    }
    public int GetPerfectScore()
    {
        int perfect = 0;
        for (int i = 0; i < l_Nodes.Count; i++)
        {
            switch (l_Nodes[i].e_Type)
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
    public List<Node> GetNodes()
    {
        return l_Nodes;
    }
    public void ForceSort()
    {
        l_Nodes.Sort((left, right) => { return left.i_BeatPos >= right.i_BeatPos ? 1 : -1; });
    }
    public bool ContainsNode(int beatPos, int sound_track)
    {
        tempNode = l_Nodes.Find(p => p.i_BeatPos == beatPos);
        return tempNode != null && tempNode.i_sound_track == sound_track;
    }
    public void SetNode(int beatPos, int sound_track, BeatType type)
    {
        tempNode = GetNodeByPos(beatPos);
        if (tempNode != null)
        {
            int index = l_Nodes.IndexOf(tempNode);
            Node n = new Node(beatPos, sound_track, type);
            l_Nodes[index] = n;
        }
        else
        {
            for (int i = 0; i < l_Nodes.Count; i++)
            {
                if (l_Nodes[i].i_BeatPos > beatPos)
                {
                    l_Nodes.Insert(i, new Node(beatPos, sound_track, type));
                    return;
                }
            }
            l_Nodes.Add(new Node(beatPos, sound_track, type));
        }
    }
    public void AdjustNode(int beatPos, BeatType type)
    {

        tempNode = GetNodeByPos(beatPos);
        if (tempNode != null)
        {
            int index = l_Nodes.IndexOf(tempNode);
            Node n = new Node(beatPos, tempNode.i_sound_track, type);
            l_Nodes[index] = n;
        }
        else
        {
            Debug.LogError("Can't Adjust A Unexisted Node!Pos:" + beatPos);
        }
    }
    public void RemoveNode(int beatPos)
    {
        tempNode = GetNodeByPos(beatPos);
        if (tempNode == null)
        {
            Debug.LogWarning(beatPos.ToString() + " Node Not Found Howdf U Remove A UnAddNode In Editor?");
            return;
        }
        l_Nodes.Remove(tempNode);
    }
    public Node GetNodeByPos(int beatPos)
    {
        return l_Nodes.Find(p => p.i_BeatPos == beatPos);
    }
    public Node GetNodeByIndex(int index)
    {
        return l_Nodes[index];
    }
    public int GetNodeIndex(int beatPos)
    {
        return l_Nodes.FindIndex(p => p.i_BeatPos == beatPos);
    }
    public Dictionary<float, int> GetTotalBeatsCenterWithOffset(float f_beatEach)
    {
        // 节拍和时间的映射表
        Dictionary<float, int> dic = new Dictionary<float, int>();
        for (int i = 0; i < l_Nodes.Count; i++)
        {
            switch (l_Nodes[i].e_Type)
            {
                case BeatType.Single:
                    {
                        dic.Add(F_BeatStartOffset + l_Nodes[i].i_BeatPos * f_beatEach, i);
                    }
                    break;
                case BeatType.Longkey:
                    {

                        dic.Add(F_BeatStartOffset + l_Nodes[i].i_BeatPos * f_beatEach, i);
                    }
                    break;
                case BeatType.Point:
                    {
                        dic.Add(F_BeatStartOffset + l_Nodes[i].i_BeatPos * f_beatEach, i);
                    }
                    break;
            }
        }
        return dic;
    }
    public List<float> BeatsCenterWithOffset(int beatPos, BeatType type, float f_beatEach)
    {
        // TODO: 处理一节拍有多个小拍的情况
        List<float> beatMids = new List<float>();
        switch (type)
        {
            case BeatType.Single:
                {
                    beatMids.Add(F_BeatStartOffset + beatPos * f_beatEach);
                }
                break;
            case BeatType.Longkey:
                {

                    beatMids.Add(F_BeatStartOffset + beatPos * f_beatEach);
                }
                break;
            case BeatType.Point:
                {
                    beatMids.Add(F_BeatStartOffset + beatPos * f_beatEach);
                }
                break;
        }
        return beatMids;
    }
    #endregion
}

[System.Serializable]
public class Node
{
    public Node(int beatPos, int sound_track, BeatType type)
    {
        i_BeatPos = beatPos;
        i_sound_track = sound_track;
        e_Type = type;
    }
    public int i_BeatPos;
    public int  i_sound_track;
    public BeatType e_Type;
}
