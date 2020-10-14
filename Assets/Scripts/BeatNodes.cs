using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
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

    // TODO1: new feature -- NodeType Edit
    CustomNodeType c_NodeTypes;
    static public string default_name =  "New Node Type";
    public int I_BeatPerMinute;
    public float F_BeatStartOffset;
    public AudioClip AC_ClipToPlay;
    public int I_SoundTracks;
    public string S_SongName { get; set; }
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

    public void SetNode(int beatPos, int sound_track, string typename)
    {
        tempNode = GetNodeByPos(beatPos);
        NodeType nType = GetTypeByName(typename);

        if (tempNode != null)
        {
            int index = l_Nodes.IndexOf(tempNode);
            Node n = new Node(beatPos, sound_track, nType);
            l_Nodes[index] = n;
        }
        else
        {
            for (int i = 0; i < l_Nodes.Count; i++)
            {
                if (l_Nodes[i].i_BeatPos > beatPos)
                {
                    l_Nodes.Insert(i, new Node(beatPos, sound_track, nType));
                    return;
                }
            }
            l_Nodes.Add(new Node(beatPos, sound_track, nType));
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

    public void AdjustNode(int beatPos, string typename)
    {
        NodeType nType = GetTypeByName(typename);
        tempNode = GetNodeByPos(beatPos);
        if (tempNode != null)
        {
            int index = l_Nodes.IndexOf(tempNode);
            Node n = new Node(beatPos, tempNode.i_sound_track, nType);
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

    public List<float> BeatsCenterWithOffset(int beatPos, string  typename, float f_beatEach)
    {
        List<float> beatMids = new List<float>();
        beatMids.Add(F_BeatStartOffset + beatPos * f_beatEach);
        return beatMids;

    }
    #endregion

    #region NodeType Edit
    public bool AddNodeType(string typename, Color color)
    {
        return c_NodeTypes.Insert(typename, color);
    }

    public bool DeleteNodeType(string typename)
    {
        return c_NodeTypes.Delete(typename);
    }

    public int FindNodeType(string typename)
    {
        return c_NodeTypes.Find(typename);
    }

    public bool UpdateNodeType(string old_name, string new_name)
    {
        return c_NodeTypes.Update(old_name, new_name);
    }

    public bool UpdateNodeType(string old_name, Color old_color, string new_name, Color new_color)
    {
        return c_NodeTypes.Update( old_name, old_color, new_name, new_color);
    }

    #endregion

    #region CustomNodeType Interact APIs

    public void InitType()
    {
        c_NodeTypes = new CustomNodeType();
    }
    
    // just for debug
    public void PrintAllNodeType()
    {
        Debug.Log("[BeatNodes.PrintAllNodeType]");
        c_NodeTypes.Print();
    }
    public int GetTypeCount()
    {
        return c_NodeTypes.Count();
    }
    public NodeType GetTypeByName(string typename)
    {
        return c_NodeTypes.GetTypeByName(typename);
    }

    public NodeType GetTypeByIndex(int index)
    {
        return c_NodeTypes.GetTypeByIndex(index);
    }

    public bool InsertType(string typename, Color color)
    {
        return c_NodeTypes.Insert(typename, color);
    }

    public bool DeleteType(string typename)
    {
        return c_NodeTypes.Delete(typename);
    }

    public int FindType(string typename)
    {
        return c_NodeTypes.Find(typename);
    }

    public bool UpdateType(string old_name, string new_name)
    {
        return c_NodeTypes.Update(old_name, new_name);
    }

    public bool UpdateType(string old_name, Color old_color, string new_name, Color new_color)
    {
        return c_NodeTypes.Update(old_name, old_color, new_name, new_color);
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

    public Node(int beatPos, int sound_track, NodeType type)
    {
        i_BeatPos = beatPos;
        i_sound_track = sound_track;
        c_Type = type;
    }

    public int i_BeatPos;
    public int  i_sound_track;
    public BeatType e_Type;

    // TODO1: new feature -- NodeType Edit
    public NodeType c_Type; // 用户自定义节奏点类型

}


public class NodeType
{
    public NodeType(int id, string typename)
    {
        I_Id = id;
        S_TypeName = typename;
        NodeColor = Color.white;
        t2_beatBtn = Resources.Load<Texture2D>("Texture/Editor/BeatBtn-gray");
    }

    public NodeType(int id, string typename, Color color)
    {
        I_Id = id;
        S_TypeName = typename;
        NodeColor = color;
        t2_beatBtn = Resources.Load<Texture2D>("Texture/Editor/BeatBtn-gray");
    }
    public int I_Id;
    public string S_TypeName;
    public Color NodeColor;
    public static Texture2D t2_beatBtn;
}

public class DefaultNodeType
{
    public List<NodeType> l_defaultTypes;

    public DefaultNodeType()
    {
        Debug.Log("BaseClass: DefaultNodeType");
        l_defaultTypes = new List<NodeType>();
        l_defaultTypes.Add(new NodeType(0, "Invalid"));
        l_defaultTypes.Add(new NodeType(1, "Single"));
    }
}

public class CustomNodeType: DefaultNodeType
{
    public List<NodeType> l_customTypes;

    // 插入节奏点类型：如果列表已有相同命名的类型，则返回失败，否则返回成功

    public CustomNodeType():base()
    {
        Debug.Log("DerivedClass: CustomNodeType");
        l_customTypes = new List<NodeType>();
    }
    public NodeType GetTypeByName(string typename)
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
            Debug.LogFormat("[BeatNodes.GetTypeByName] node types doesn't exist {0}", typename);
            return null;
        }
    }
    public NodeType GetTypeByIndex(int index)
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
        if (typename == BeatNodes.default_name)
        {
            typename += new_id.ToString();          
        }
        Debug.LogFormat("[CustomNodeType.Insert] typename {0} is going to be inserted", typename);
        if(!l_defaultTypes.Exists(x => x.S_TypeName == typename) )
        {
            if(!l_customTypes.Exists(x => x.S_TypeName == typename))
            {
                res = true;
                NodeType n = new NodeType(new_id, typename, color);
                if(n == null)
                {
                    Debug.Log("[CustomNodeType.Insert] new Nodetype failed");
                }
                else
                {
                    Debug.LogFormat("[CustomNodeType.Insert] inserted {0}", typename);
                    l_customTypes.Add(n);
                    Debug.LogFormat("[CustomNodeType.Insert] Type Count {0}", l_customTypes.Count);
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
            res = l_customTypes.FindIndex(x => x.S_TypeName == typename);
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
                l_customTypes[index].NodeColor = new_color;
            }
        }
        return res;

    }

    public int Count()
    {
        int res = 0;
        res = l_defaultTypes.Count + l_customTypes.Count;
        //Debug.LogFormat("[BeatNodes.Count] customtype count {0}", l_customTypes.Count);
        return res;
    }

    // used for debug
    public void Print()
    {
        foreach(var t in l_defaultTypes)
        {
            Debug.LogFormat("type id: {0}, name: {1}, color: {2}", t.I_Id, t.S_TypeName, t.NodeColor.ToString());
        }
        foreach(var t in l_customTypes)
        {
            Debug.LogFormat("type id: {0}, name: {1}, color: {2}", t.I_Id, t.S_TypeName, t.NodeColor.ToString());
        }

    }
        
}
