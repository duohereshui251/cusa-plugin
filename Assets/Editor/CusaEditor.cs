using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UI;
using System.IO;
public class CusaEditor : EditorWindow
{
    // button string
    string PlayButton = "play";
    string StopButton = "stop";
    string PauseButton = "Pause";
    string ReplayButton = "Resume";
    string SaveButton = "Save Data";
    string ClearButton = "Clear";

    // MainView & upsidebar data
    float F_RectWidth; // 整体宽度不变
    int I_SliderHeight;
    int I_BeatsInView; // 固定显示的节拍数量
    int I_ViewBtnScale; // 节拍大小调整
    int I_SliderBeatScale;
    int I_BeatCheckLine; // 判定线位置
    BeatType e_CurNodeType;
    int I_buttonWidth;
    int I_SoundTracks;

    // 节拍的数值信息和样式
    float f_soundTracksWidth;
    float f_viewBeatWidth;
    float f_sliderBeatWidth;
    float f_sliderStartOffset;
    int i_totalBeats;
    Texture2D t2_BeatBtn;
    Texture2D t2_BeatBtn_gray;
    Dictionary<BeatType, Texture2D> BeatBtnDict;

    Texture2D t2_LineTex;


    // 歌曲控制信息

    public BeatNodes beats;
    bool b_isLoadBeats;
    bool b_pause;
    int i_pauseSample;
    float f_curTime;
    float f_totalTime;
    float f_beatEach;
    int i_curSelectPos;

    float f_beatToScale;
    float f_beatCheck;

    // 颜色信息
    Color mainRectColor = Color.green;
    Color upsliderColor = Color.black;
    Color viewColor = Color.gray;
    Color lineColor = Color.gray;
    // 布局数值信息
    float leftViewWidth;
    float rightViewWidth;
    Rect mainRect;


    // 鼠标位置信息
    Vector2 v2_mousePos = Vector2.zero;
    bool b_isLeftClick = false;

    Rect tempRect;

    bool Editable
    {
        get
        {
            return beats.AC_ClipToPlay != null;
        }
    }

    public CusaEditor()
    {
        Debug.Log("[CusaEditor.construct]");
        this.titleContent = new GUIContent("Cusa Editor");
    }
    private void Awake()
    {
        Debug.Log("[CusaEditor.Awake]");
        F_RectWidth = 1000;
        I_BeatCheckLine = 50;
        I_SliderHeight = 80;
        I_BeatsInView = 16;
        I_ViewBtnScale = 35;
        I_SliderBeatScale = 2;
        I_buttonWidth = 60;
        //f_tipWidth = 80;
        e_CurNodeType = BeatType.Single;
        Event.current = new Event();

    }
    private void OnEnable()
    {
        // 由于CusaInspector里window.InitData(beats)调用会比较晚，所以这里一开始基本都会返回错误，但没有影响
        Debug.Log("[CusaEditor.OnEnable]");

        if (!Editable)
        {
            Debug.Log("[CusaEditor.OnEnable] not Editable.");
            return;
        }

    }
    public void InitData(BeatNodes beats, float win_width, float win_height)
    {
        this.beats = beats;
        if (!Editable)
        {
            Debug.Log("[CusaEditor.InitData] not Editable.");
            return;
        }
        // init data

        EAudio.AttachClipTo(beats.AC_ClipToPlay);

        f_totalTime = beats.AC_ClipToPlay.length;

        f_beatEach = 60f / beats.I_BeatPerMinute;
        f_beatCheck = 0f;
        f_beatToScale = 0f;

        i_totalBeats = (int)(f_totalTime / f_beatEach);

        F_RectWidth = win_width;
        leftViewWidth = 180.0f;
        rightViewWidth = F_RectWidth - leftViewWidth;
        f_sliderStartOffset = beats.F_BeatStartOffset > 0 ? rightViewWidth * (beats.F_BeatStartOffset / f_totalTime) : 0;
        //f_sliderBeatWidth = 2.0f;
        f_sliderBeatWidth = (rightViewWidth - f_sliderStartOffset) / i_totalBeats;
        f_viewBeatWidth = 40.0f;

        I_SoundTracks = beats.I_SoundTracks;
        if (I_SoundTracks > 0)
        {
            f_soundTracksWidth = (win_height - I_SliderHeight) / I_SoundTracks;

        }
        if (I_SoundTracks > 2)
        {
            I_SliderHeight = 80 + (I_SoundTracks - 2) * 20;
        }
        t2_BeatBtn = Resources.Load<Texture2D>("Texture/Editor/BeatBtn");
        t2_BeatBtn_gray = Resources.Load<Texture2D>("Texture/Editor/BeatBtn-gray");
        BeatBtnDict = new Dictionary<BeatType, Texture2D>();
        BeatBtnDict.Add(BeatType.Invalid, t2_BeatBtn_gray);
        BeatBtnDict.Add(BeatType.Single, Resources.Load<Texture2D>("Texture/Editor/BeatBtn-blue"));
        BeatBtnDict.Add(BeatType.Point, Resources.Load<Texture2D>("Texture/Editor/BeatBtn-green"));
        BeatBtnDict.Add(BeatType.Longkey, Resources.Load<Texture2D>("Texture/Editor/BeatBtn-red"));

        if (t2_BeatBtn == null)
            Debug.LogError("[CusaEditor.OnEnable] button load failed.");
        t2_LineTex = new Texture2D(1, 1);
        b_pause = false;
        i_pauseSample = 0;
        i_curSelectPos = -1;
        EditorApplication.update += Update;

    }

    private void OnDisable()
    {
        Debug.Log("[CusaEditor.OnDisable]");
        EditorApplication.update -= Update;
        i_pauseSample = 0;
        EAudio.StopClip();
        //EditorUtility.SetDirty(target);
        beats.ForceSort();
    }

    private void Update()
    {
        Repaint();

    }
    [MenuItem("Tool/CusaEditor")]
    public static void Init()
    {
        // TODO: 解决直接从windows打开如何获取beats资源
        Debug.Log("[CusaEditor.Init]");
        CusaEditor window = (CusaEditor)EditorWindow.GetWindow(typeof(CusaEditor), false, "CusaEditor");
        //var x = (Screen.currentResolution.width - win_width) / 2;
        //var y = (Screen.currentResolution.height - win_height) / 2;
        //window.position = new Rect(x, y,win_width, win_height);
    }

    private void OnGUI()
    {
        if (!Editable)
        {
            return;
        }
        Rect tempRect;

        // 1: 整个矩形
        tempRect = EditorGUILayout.BeginHorizontal();
        if (tempRect.width > 0.0f)
        {
            mainRect = tempRect;
        }
        LeftView();
        RightView();
        EditorGUILayout.EndHorizontal();

        v2_mousePos = Vector2.zero;

        if (GUI.changed)
        {
            //InitData();
            // TODO: 修改音轨宽度
            //f_soundTracksWidth = (win_height - I_SliderHeight) / I_SoundTracks;
            InitData(this.beats, mainRect.width, mainRect.height);
        }
    }
    void LeftView()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(leftViewWidth));
        EditorGUILayout.Space(15);
        // 画按钮
        DrawBeatNodesSetting();
        DrawSongControl(); // draw 1
        EditorGUILayout.Space(15);
        DrawSaveButton();
        EditorGUILayout.Space(15);
        DrawClearButton();

        EventManage(); // draw 2
        //GUILayout.FlexibleSpace();
        //EditorGUILayout.Space(15);

        //DrawSelectNodeType(mainRect);
        DrawSelectNodeType();

        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }
    void RightView()
    {
        DrawRect(GUILayoutUtility.GetRect(rightViewWidth, mainRect.height));
    }

    void DrawBeatNodesSetting()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(100));
        beats.I_BeatPerMinute = EditorGUILayout.IntField("bpm", beats.I_BeatPerMinute);
        EditorGUILayout.Space(10);
        beats.F_BeatStartOffset = EditorGUILayout.FloatField("offset",beats.F_BeatStartOffset);
        EditorGUILayout.Space(10);
        beats.I_SoundTracks = EditorGUILayout.IntField("sound_tracks", beats.I_SoundTracks);
        EditorGUILayout.Space(10);
        EditorGUILayout.EndVertical();
    }
    void DrawSongControl()
    {

        f_curTime = EAudio.GetCurTime();
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(15);
        if (EAudio.IsAudioPlaying())
        {
            if (!b_pause)
            {
                if (GUILayout.Button(PauseButton, GUILayout.Width(I_buttonWidth)))
                {
                    pause();
                }
            }
            else
            {
                if (GUILayout.Button(ReplayButton, GUILayout.Width(I_buttonWidth)))
                {
                    replay();
                }
            }
        }
        else
        {
            if (GUILayout.Button(PlayButton, GUILayout.Width(I_buttonWidth)))
            {
                play();
            }
        }
        EditorGUILayout.Space(15);

        if (GUILayout.Button(StopButton, GUILayout.Width(I_buttonWidth)))
        {
            stop();
        }
        EditorGUILayout.Space(15);
        EditorGUILayout.EndHorizontal();

    }

    void DrawSelectNodeType()
    {
        Rect totalRect = EditorGUILayout.BeginVertical();

        foreach (int i in System.Enum.GetValues(typeof(BeatType)))
        {
            if (i == -1)
                continue;
            BeatType type = (BeatType)i;
            //HighLight
            if (type == e_CurNodeType)
            {
                //GUI.color = Color.grey;
                tempRect = new Rect(totalRect.xMin + 5 - .1f * I_ViewBtnScale, totalRect.yMin + (i * 1.1f + 1 - .1f) * I_ViewBtnScale, I_ViewBtnScale * 1.2f, I_ViewBtnScale * 1.2f);
                GUI.DrawTexture(tempRect, t2_BeatBtn_gray);
            }
            //Draw Tips
            GUI.color = Color.white;
            tempRect = new Rect(totalRect.xMin + 5 + I_ViewBtnScale + 10f, totalRect.yMin + (i * 1.1f + 1) * I_ViewBtnScale, 80, I_ViewBtnScale);
            GUI.Box(tempRect, type.ToString());

            //Draw Btn
            //GUI.color = GetNodeColor(true, type);
            tempRect = new Rect(totalRect.xMin + 5, totalRect.yMin + (i * 1.1f + 1) * I_ViewBtnScale, I_ViewBtnScale, I_ViewBtnScale);
            GUI.DrawTexture(tempRect, BeatBtnDict[type]);
            //Btn Click
            if (tempRect.Contains(v2_mousePos))
            {

                if (e_CurNodeType == type && i_curSelectPos != -1)
                {
                    beats.AdjustNode(i_curSelectPos, e_CurNodeType);
                }
                Debug.LogFormat("[CusaEditor.DrawSelectNodeType] click: {0}", e_CurNodeType.ToString());
                e_CurNodeType = type;
            }
        }

        EditorGUILayout.EndVertical();

    }
    void play()
    {
        Debug.Log("play");
        b_pause = false;
        i_pauseSample = 0;
        EAudio.PlayClip();
    }

    void stop()
    {
        Debug.Log("stop");
        i_curSelectPos = -1;
        b_pause = false;
        i_pauseSample = 0;
        EAudio.StopClip();
        f_beatCheck = 0f;
    }

    void pause()
    {
        Debug.Log("pause");
        i_pauseSample = EAudio.GetCurSample();
        EAudio.PauseClip();
        b_pause = true;
    }

    void replay()
    {
        Debug.Log("replay");
        EAudio.ResumeClip();
        EAudio.SetSamplePosition(i_pauseSample);
        b_pause = false;
    }

    void EventManage()
    {
        if (Event.current.isMouse)
        {
            if (Event.current.type == EventType.MouseDown)
            {
                v2_mousePos = Event.current.mousePosition;
            }
            b_isLeftClick = Event.current.button == 0;

        }
        else if (Event.current.isScrollWheel)
        {
            if (Event.current.delta.y > 0)
            {
                SetSongPos(f_curTime - .5f);
            }
            else if (Event.current.delta.x < 0)
            {
                SetSongPos(f_curTime + .5f);
            }

        }
        // TODO： 快捷按键
    }

    //滚轮到不同位置播放歌曲
    void SetSongPos(float posTime)
    {
        if (EAudio.IsAudioPlaying())
        {
            //Debug.Log("[CusaEditor.SetSongPos] is playing");
            EAudio.SetSamplePosition((int)(EAudio.GetSampleDuration() * (posTime / f_totalTime)));
            i_pauseSample = EAudio.GetCurSample();
        }
        else
        {
            //Debug.Log("[CusaEditor.SetSongPos] not playing");
            play();
            EAudio.SetSamplePosition((int)(EAudio.GetSampleDuration() * (posTime / f_totalTime)));
            pause();

        }
        f_beatCheck = 0;

    }
    void DrawSaveButton()
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(SaveButton, GUILayout.Width(150)))
        {
            SaveToJson();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

    }
    void DrawClearButton()
    {
        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();
        if (GUILayout.Button(ClearButton, GUILayout.Width(150)))
        {
            Clear();
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();

    }

    void DrawRect(Rect mainRect)
    {
        // 方便观察，之后删掉
        //GUI.color = mainRectColor;
        //GUI.Box(mainRect, "");
        Rect upslider = new Rect(mainRect.xMin, mainRect.yMin, mainRect.width, I_SliderHeight);
        Rect viewRect = new Rect(mainRect.xMin, mainRect.yMin + I_SliderHeight, mainRect.width, mainRect.height - I_SliderHeight);

        DrawSider(upslider); // draw 4
        DrawView(viewRect); // draw 3
    }

    void DrawView(Rect totalRect)
    {
        // 背景
        //GUI.color = viewColor;
        //GUI.Box(totalRect, "");

        //Draw Main View
        float f_time = f_curTime - beats.F_BeatStartOffset;
        int i_startPos = Mathf.FloorToInt(f_time / f_beatEach);

        // 该变量用于控制画面随时间的偏移
        float f_timeParam = f_time >= 0 ? (f_time % f_beatEach) / f_beatEach : f_time / f_beatEach;

        for (int j = 0; j < I_SoundTracks; j++)
        {
            GUI.color = lineColor;
            Rect lineRect = new Rect(totalRect.xMin, totalRect.yMin + (totalRect.height / (I_SoundTracks + 1)) * (j + 1) - 2f, totalRect.width, 4f);
            GUI.Box(lineRect, t2_LineTex);
            int soundtrack = j + 1;
            for (int i = 0; i < I_BeatsInView; i++)
            {
                int i_curPos = i_startPos + i;
                bool clickable = true;
                if (i_curPos < 0 || i_curPos > i_totalBeats)
                {
                    clickable = false;
                }
                if (i_curPos == i_curSelectPos && beats.ContainsNode(i_curPos, soundtrack)) // 选中高亮效果
                {

                    tempRect = new Rect(lineRect.xMin + I_BeatCheckLine + I_ViewBtnScale - I_ViewBtnScale * .1f + (i - f_timeParam) * f_viewBeatWidth,
                                    lineRect.yMax - 2f - I_ViewBtnScale / 2 - I_ViewBtnScale * .1f,
                                    I_ViewBtnScale * 1.2f, I_ViewBtnScale * 1.2f);
                    GUI.DrawTexture(tempRect, t2_BeatBtn_gray);

                }
                tempRect = new Rect(lineRect.xMin + I_BeatCheckLine + I_ViewBtnScale + (i - f_timeParam) * f_viewBeatWidth,
                                lineRect.yMax - 2f - I_ViewBtnScale / 2,
                                I_ViewBtnScale, I_ViewBtnScale);


                BeatType type = beats.ContainsNode(i_curPos, soundtrack) ? beats.GetNodeByPos(i_curPos).e_Type : BeatType.Invalid;
                if (!totalRect.Contains(tempRect.position))
                {
                    continue;
                }
                GUI.DrawTexture(tempRect, BeatBtnDict[type], ScaleMode.ScaleAndCrop);

                // mouse click
                if (clickable && v2_mousePos != Vector2.zero && tempRect.Contains(v2_mousePos))
                {
                    Debug.LogFormat("[CusaEditor.DrawView] click: {0}", e_CurNodeType.ToString());

                    if (!b_pause)
                    {
                        pause();
                    }
                    if (b_isLeftClick)
                    {
                        if (beats.GetNodeByPos(i_curPos) == null || i_curSelectPos == i_curPos)
                        {
                            //Debug.Log("[CusaEditor.DrawView] curPos node doesn't exist. Add to List");
                            beats.SetNode(i_curPos, soundtrack, e_CurNodeType);
                        }
                        i_curSelectPos = i_curPos;
                        // 检测是否存在
                        //Debug.LogFormat("[CusaEditor.DrawView] i_curSelectPos {0}", beats.GetNodeByPos(i_curPos).i_BeatPos);

                    }
                    else
                    {
                        if (beats.ContainsNode(i_curPos, soundtrack))
                        {
                            beats.RemoveNode(i_curPos);
                            i_curSelectPos = -1;
                        }

                    }
                }

            }
        }
        DrawViewExtra(totalRect, v2_mousePos);
        //Draw Beat Check Line(Red)
        GUI.color = new Color(1, 0, 0, 1.0f);
        tempRect = new Rect(totalRect.xMin + I_BeatCheckLine, totalRect.yMin, 3f, totalRect.height);
        GUI.DrawTexture(tempRect, t2_LineTex);
        //CheckLine Time/BeatPos
        tempRect = new Rect(totalRect.xMin + I_BeatCheckLine + 20, totalRect.yMax - 120, 120, 120);
        GUI.Label(tempRect, string.Format("Time:{0:000.0}/Pos:{1}", f_curTime, i_startPos >= 0 ? i_startPos : 0));

    }

    void DrawViewExtra(Rect totalRect, Vector2 v2_mousePos)
    {
        //画计数器
        GUI.color = Color.white;
        tempRect = new Rect(totalRect.xMin + I_BeatCheckLine + 5, totalRect.yMin + 5, 120, 20);
        GUI.Box(tempRect, "Beats Set:" + beats.GetNodes().Count.ToString() + "/" + i_totalBeats.ToString());

        // 画时间
        GUI.color = Color.white;
        tempRect = new Rect(totalRect.xMin + I_BeatCheckLine + 5, totalRect.yMin + 30, 120, 20);
        GUI.Box(tempRect, "Clip Length:" + string.Format("{0:000.0}", beats.AC_ClipToPlay.length));

        // 画选择的节点
        if (i_curSelectPos != -1)
        {
            // TODO: 存在Bug1
            Node tempNode = beats.GetNodeByPos(i_curSelectPos);
            if (tempNode == null)
            {
                Debug.LogError("[CusaEditor.DrawViewExtra] selected Node doesn't exist.");
                return;
            }
            List<float> beatsCenter = beats.BeatsCenterWithOffset(tempNode.i_BeatPos, tempNode.e_Type, f_beatEach);
            GUI.color = Color.white;
            // 如果 beats类型是两个小拍或者三个小拍，则会显示多个beats的信息，因此box的大小会有所改变

            tempRect = new Rect(totalRect.xMin + 5, totalRect.yMin + 55, 120, 40 + beatsCenter.Count * 20);
            GUI.Box(tempRect, "");

            tempRect = new Rect(totalRect.xMin + 10, totalRect.yMin + 60, 110, 20);
            GUI.Label(tempRect, "Ind:" + beats.GetNodeIndex(i_curSelectPos).ToString() + "/Pos:" + i_curSelectPos.ToString());
            tempRect = new Rect(totalRect.xMin + 10, totalRect.yMin + 80, 110, 20);
            GUI.Label(tempRect, "Type:" + beats.GetNodeByPos(i_curSelectPos).e_Type.ToString());

            for (int i = 0; i < beatsCenter.Count; i++)
            {
                tempRect = new Rect(totalRect.xMin + 10, totalRect.yMin + 80 + (i + 1) * 20, 110, 20);
                GUI.Label(tempRect, "Beat " + (i + 1).ToString() + ":" + beatsCenter[i].ToString());
            }

        }

    }

    void DrawSider(Rect totalRect)
    {
        GUI.color = upsliderColor;
        GUI.Box(totalRect, "");
        #region Progress Line
        GUI.color = Color.white;
        float curPos = f_curTime / f_totalTime;
        tempRect = new Rect(totalRect.xMin + (F_RectWidth - leftViewWidth) * curPos, totalRect.yMin, 1, totalRect.height);
        GUI.DrawTexture(tempRect, t2_LineTex);
        #endregion

        for (int j = 0; j < I_SoundTracks; ++j)
        {
            int sound_track = j + 1;
            // 画线
            GUI.color = lineColor;
            Rect midBorder = new Rect(totalRect.xMin, totalRect.yMin + I_SliderHeight / (I_SoundTracks + 1) * (j + 1) - 2f, totalRect.width, 2f);
            GUI.DrawTexture(midBorder, t2_LineTex);

            // 画点
            // TODO: 做成可以滑动缩放的
            float startX = totalRect.xMin + I_SliderBeatScale + f_sliderStartOffset;
            for (int i = 0; i < i_totalBeats; i++)
            {
                GUI.color = GetNodeColor(true, beats.ContainsNode(i, sound_track) ? beats.GetNodeByPos(i).e_Type : BeatType.Invalid);

                tempRect = new Rect(startX + i * f_sliderBeatWidth, midBorder.yMax - 1f - I_SliderBeatScale / 2, I_SliderBeatScale / 2, 8);
                GUI.DrawTexture(tempRect, t2_LineTex);
            }

        }

        // 点击后改变的只有f_curTime 
        if (v2_mousePos != Vector2.zero && totalRect.Contains(v2_mousePos))
        {
            // TODO: bug  宽度并不固定
            Debug.LogFormat("[CusaEditor.DrawSider] totalRect.position.x: {0}", totalRect.position.x);
            Debug.LogFormat("[CusaEditor.DrawSider] mouse.position.x: {0}", v2_mousePos.x);
            Vector2 offset = v2_mousePos - totalRect.position;
            //Debug.LogFormat("[CusaEditor.DrawSider] click: offset {0}", offset.x);
            float value = offset.x / (F_RectWidth - leftViewWidth);
            if (value > 1)
            {
                Debug.Log("[CusaEditor.DrawSider] click out of area.");
                return;
            }
            SetSongPos(value * f_totalTime);
        }
    }

    #region ColorSetting
    Color GetNodeColor(bool editable, BeatType type = BeatType.Invalid)
    {
        if (!editable)
        {
            return Color.grey;
        }
        switch (type)
        {
            default:
                return Color.white;
            case BeatType.Single:
                return Color.blue;
            case BeatType.Longkey:
                return Color.red;
            case BeatType.Point:
                return Color.green;
        }
    }
    #endregion

    #region SaveToJson

    void SaveToJson()
    {
        Debug.Log("[CusaEditor.SaveToJson]");

        BeatNodesJson beatjsonObj = new BeatNodesJson();
        beatjsonObj.song_name = "xxx";
        beatjsonObj.path = "xxx_path";
        beatjsonObj.bpm = beats.I_BeatPerMinute;
        beatjsonObj.offset = beats.F_BeatStartOffset;
        beatjsonObj.sound_tracks = beats.I_SoundTracks;
        beatjsonObj.nodes = new List<BeatNodesJson.NodeJson>();

        List<Node> nodes = beats.GetNodes();
        for (int i = 0; i < nodes.Count; ++i)
        {
            beatjsonObj.nodes.Add(new BeatNodesJson.NodeJson(nodes[i].i_BeatPos, nodes[i].i_sound_track, nodes[i].e_Type));
        }
        string jsonstr = JsonUtility.ToJson(beatjsonObj, true);
        Debug.Log(jsonstr);

        string path = null;
#if UNITY_EDITOR
        path = "Assets/Resources/GameJSONData/MusicInfo.json";
#endif

        using (FileStream fs = new FileStream(path, FileMode.Create))
        {
            using (StreamWriter writer = new StreamWriter(fs))
            {
                writer.Write(jsonstr);
            }
        }
#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif
    }
    void Clear()
    {
        beats.Clear();
    }
    #endregion

}
