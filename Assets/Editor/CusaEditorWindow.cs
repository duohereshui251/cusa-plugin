#define UseLittleBeat
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine.UIElements;
using UnityEditor.UI;
using System.IO;
using NUnit.Framework.Constraints;
using System;
using UnityEditorInternal;
using Object = UnityEngine.Object;
using NUnit.Framework;
using JetBrains.Annotations;

public class CusaEditorWindow : EditorWindow, IHasCustomMenu
{
    // lock
    [System.NonSerialized]
    private GUIStyle lockButtonStyle;
    [System.NonSerialized]
    private bool locked = false;
    [SerializeField] private int m_LastSelectedObjectID;

    // button string
    string PlayButton = "play";
    string StopButton = "stop";
    string PauseButton = "Pause";
    string ReplayButton = "Resume";
    string SaveButton = "Save Data";
    string ClearButton = "Clear";

    // MainView & upsidebar data
    float F_RectWidth; // 整体宽度不变
    float f_SliderHeight;
    int I_BeatsInView; // 固定显示的节拍数量
    /// <summary>
    /// 小节
    /// </summary>
    int little_beats;
    /// <summary>
    /// 小节选项
    /// </summary>
    string[] little_beats_options = new string[] { "2", "3", "4", "6", "8", "12", "16", "24", "32" };
    List<int> little_beats_values = new List<int>();
    int little_beats_index = 0;

    /// <summary>
    /// 线的宽度， 默认为4
    /// </summary>
    int I_ViewLineWidth; // 节拍大小调整
    /// <summary>
    /// 线的高度
    /// </summary>
    int I_ViewLineHeight;
    /// <summary>
    /// 高亮的扩大比例
    /// </summary>
    float f_ViewBtnHighLightScale;

    int I_SliderBeatScale;
    /// <summary>
    /// 判定线位置， ，默认50
    /// </summary>
    int I_BeatCheckLine;
    BeatType e_CurNoteType;
    int I_buttonWidth;
    int I_SoundTracks;

    // 节拍的数值信息和样式
    float f_soundTracksWidth;
    /// <summary>
    /// 每根线的间隔距离， 默认40.0f
    /// </summary>
    float f_viewIntervalWidth;
    /// <summary>
    /// 每个note的宽度, 默认12
    /// </summary>
    float f_viewNoteWidth;
    float f_sliderBeatWidth;
    int I_sliderBlockWidth;
    float f_sliderStartOffset;
    int i_totalBeats;
    int i_totalLittleBeats;

    /// <summary>
    /// 预览每条轨道的高度
    /// </summary>
    float PerTrackHeight;
    /// <summary>
    /// 预览每条轨道的block数量
    /// </summary>
    int blocks;
    /// <summary>
    /// 预览每个block表达多少个小节
    /// </summary>
    int PerBlockBeats;

    /// <summary>
    /// 绿色粗线
    /// </summary>
    Texture2D t2_Thickline;
    /// <summary>
    /// 蓝色细线
    /// </summary>
    Texture2D t2_Thinline;
    /// <summary>
    /// note
    /// </summary>
    Texture2D t2_Note;
    Texture2D t2_Noodle_in;
    Texture2D t2_Noodle_out;
    Texture2D t2_LineTex;


    // 歌曲控制信息

    public BeatNotes beats;
    bool b_isLoadBeats;
    bool b_pause;
    int i_pauseSample;
    float f_curTime;
    float f_totalTime;
    float f_beatEach;
    float f_littleBeatEach;
    int i_curSelectPos;
    /// <summary>
    /// 面条起点
    /// </summary>
    int NoodleStartPos;
    /// <summary>
    /// 面条终点
    /// </summary>
    int NoodleEndPos;

    //float f_beatToScale;
    //float f_beatCheck;

    float lastWidth;
    float lastHeight;
    bool b_stay = true;
    //EditorCoroutineRunner co;

    // 颜色信息
    Color mainRectColor = Color.green;
    Color upsliderColor = Color.black;
    Color viewColor = Color.gray;
    Color lineColor = Color.gray;
    // 布局数值信息
    float leftViewWidth;
    float rightViewWidth;

    /// <summary>
    /// 预览长度, 不会随着窗口的改变而变长，而会随着鼠标滚轮放大缩小变长
    /// </summary>
    float f_sliderDisplayWidth;
    bool b_isSetSliderDisplayWidth = false;

    // 鼠标位置信息
    Vector2 v2_mousePos = Vector2.zero;
    bool b_isLeftClick = false;
    bool b_NoodleStart = false;
    int i_NoodleStartPos = -1;
    int i_NoodleEndPos = -1;

    Rect tempRect;
    // 保存各个组件的Rect信息
    Rect LeftRect;
    Rect MusicInfoRect;
    Rect NoteTypesRect;
    Rect TypeEditRect;
    Rect MainRect;
    Rect SliderRect;
    Rect SliderLineRect;
    Rect SliderBarRect;
    Rect MainViewRect;
    Rect NoteSettingRect;


    // 选中的类型名称
    string S_SelectedNoteTypeName;
    bool b_TypeSelected = false;
    NoteType CurNoteType;
    bool Editable
    {
        get
        {
            return beats.AC_ClipToPlay != null;
        }
    }

    public CusaEditorWindow()
    {
        Debug.Log("[CusaEditorWindow.construct]");
        this.titleContent = new GUIContent("Cusa Editor");
    }
    private void Awake()
    {
        Debug.Log("[CusaEditorWindow.Awake]");
        F_RectWidth = 1000;
        I_BeatCheckLine = 50;
        f_SliderHeight = 80;
        I_BeatsInView = 16;
        I_ViewLineWidth = 8;
        I_ViewLineHeight = 60;
        f_ViewBtnHighLightScale = 2f;
        I_SliderBeatScale = 2;
        I_buttonWidth = 90;
        //f_tipWidth = 80;

        // TODO8
        //little_beats_options = ConvertSlashToUnicodeSlash(little_beats_options);
        e_CurNoteType = BeatType.Single;
        Event.current = new Event();

    }
    [OnOpenAsset]
    static bool OnOpenAsset(int instanceID, int line)
    {
        Debug.Log("[CusaEditorWindow.OnOpenAsset]");
        var nodesInfo = EditorUtility.InstanceIDToObject(instanceID) as BeatNotes;
        if (nodesInfo)
        {
            EditorWindow.GetWindow<CusaEditorWindow>();
            return true;
        }
        return false;
    }
    private void OnEnable()
    {
        // 由于CusaInspector里window.InitData(beats)调用会比较晚，所以这里一开始基本都会返回错误，但没有影响
        Debug.Log("[CusaEditorWindow.OnEnable]");
        OnSelectionChange();
        if (!Editable)
        {
            Debug.Log("[CusaEditorWindow.OnEnable] not Editable.");
            return;
        }
        b_stay = true;
        this.minSize = new Vector2(800, 400);

    }
    void IHasCustomMenu.AddItemsToMenu(GenericMenu menu)
    {
        menu.AddItem(new GUIContent("Lock"), this.locked, () =>
        {
            this.locked = !this.locked;
        });
    }
    private void ShowButton(Rect position)
    {
        if (this.lockButtonStyle == null)
        {
            this.lockButtonStyle = "IN LockButton";
        }
        this.locked = GUI.Toggle(position, this.locked, GUIContent.none, this.lockButtonStyle);
    }

    private void OnSelectionChange()
    {
        Object activeObject = Selection.activeObject;

        if (locked)
        {
            activeObject = EditorUtility.InstanceIDToObject(m_LastSelectedObjectID);
        }
        BeatNotes tempBeats = activeObject as BeatNotes;


        if (tempBeats != null)
        {
            beats = tempBeats;
            Debug.Log("[CusaEditorWindow.OnSelectionChange] change BeatNotes file");
            InitData(1180, 400);
        }
        EditorCoroutineRunner.StartEditorCoroutine(CheckForResize());

    }

    public void InitData(float win_width, float win_height)
    {
        //this.beats = beats;

        var x = (Screen.currentResolution.width - win_width) / 2;
        var y = (Screen.currentResolution.height - win_height) / 2;


        this.position = new Rect(x, y, win_width, win_height);

        if (!Editable)
        {
            Debug.Log("[CusaEditorWindow.InitData] not Editable.");
            return;
        }
        // init data
        beats.InitType();

        EAudio.AttachClipTo(beats.AC_ClipToPlay);

        f_totalTime = beats.AC_ClipToPlay.length;

        // TODO8：初始化小节
        little_beats = beats.I_LittleBeats;
        little_beats_values = new List<int> { 2, 3, 4, 6, 8, 12, 16, 24, 32 };
        little_beats_index = little_beats_values.FindIndex(xx => xx == beats.I_LittleBeats);

        f_beatEach = 60f / beats.I_BeatPerMinute;
        f_littleBeatEach = f_beatEach / little_beats;
        Debug.Log("[CusaEditorWindow.InitData] f_littleBeatEac: " + f_littleBeatEach.ToString());
        f_viewIntervalWidth = 40.0f;
        f_viewNoteWidth = 12.0f;
        i_totalBeats = (int)(f_totalTime / f_beatEach);
        i_totalLittleBeats = (int)(f_totalTime / f_littleBeatEach);
        Debug.Log("[CusaEditorWindow.InitData] i_totalLittleBeats: " + i_totalLittleBeats.ToString());

        F_RectWidth = win_width;

        leftViewWidth = 180.0f;
        rightViewWidth = F_RectWidth - leftViewWidth;

        if (!b_isSetSliderDisplayWidth)
        {
            f_sliderDisplayWidth = rightViewWidth;
            b_isSetSliderDisplayWidth = true;
        }

        I_BeatsInView = Mathf.FloorToInt((rightViewWidth - I_BeatCheckLine) / f_viewIntervalWidth) - 2;

        f_sliderStartOffset = beats.F_BeatStartOffset > 0 ? rightViewWidth * (beats.F_BeatStartOffset / f_totalTime) : 0;

        f_sliderBeatWidth = (f_sliderDisplayWidth - f_sliderStartOffset) / i_totalBeats;

        I_sliderBlockWidth = 4;

        I_SoundTracks = beats.I_SoundTracks;

        f_SliderHeight = win_height / 3;

        blocks = (int)f_sliderDisplayWidth / I_sliderBlockWidth;
        PerBlockBeats = (i_totalLittleBeats / (blocks - 1)) + 1;
        Debug.LogFormat("[CusaEditorWindow.InitData] blocks: {0}, PerBlockBeats: {1}", blocks, PerBlockBeats);
        if (I_SoundTracks > 0)
        {
            f_soundTracksWidth = (win_height - f_SliderHeight) / I_SoundTracks;

        }
        if (I_SoundTracks > 2)
        {
            f_SliderHeight = win_height / 3 + (I_SoundTracks - 2) * 20;
        }

        t2_Thickline = Resources.Load<Texture2D>("Texture/Editor/thick_line");
        t2_Thinline = Resources.Load<Texture2D>("Texture/Editor/thin_line_blue");
        t2_Note = Resources.Load<Texture2D>("Texture/Editor/note");
        t2_Noodle_in = Resources.Load<Texture2D>("Texture/Editor/noodle2");
        t2_Noodle_out = Resources.Load<Texture2D>("Texture/Editor/noodle1");

        if (t2_Thickline == null)
            Debug.LogError("[CusaEditorWindow.OnEnable] button load failed.");
        t2_LineTex = new Texture2D(1, 1);
        b_pause = false;
        i_pauseSample = 0;
        i_curSelectPos = -1;
        EditorApplication.update += Update;
    }

    private void OnDisable()
    {
        Debug.Log("[CusaEditorWindow.OnDisable]");
        EditorApplication.update -= Update;
        i_pauseSample = 0;
        EAudio.StopClip();
        //EditorUtility.SetDirty(target);
        beats.ForceSort();
    }

    private void Update()
    {
        // TODO8
        //PlayPressKey();
        Repaint();
    }
    [MenuItem("Tool/CusaEditorWindow")]
    public static void Init()
    {

        Debug.Log("[CusaEditorWindow.Init]");
        EditorWindow.GetWindow<CusaEditorWindow>();

    }

    private void OnGUI()
    {
        //GUIUtility.keyboardControl = 0;
        if (!Editable)
        {
            return;
        }
        Rect tempRect;

        // 1: 整个矩形
        tempRect = EditorGUILayout.BeginHorizontal();
        if (tempRect.width > 0.0f)
        {
            MainRect = tempRect;
        }
        LeftView();
        RightView();

        EditorGUILayout.EndHorizontal();


        v2_mousePos = Vector2.zero;
    }
    void LeftView()
    {
        // 高度指定必须是position.height， 如果是MainRect.height， 高度则不会改变
        LeftRect = EditorGUILayout.BeginVertical(GUILayout.Width(leftViewWidth), GUILayout.Height(position.height));

        Rect sidelineRect = new Rect(tempRect.xMax - 1, tempRect.yMin, 1, tempRect.height);
        GUI.color = Color.gray;
        GUI.Box(sidelineRect, t2_LineTex);
        GUI.color = Color.white;

        // 画按钮
        LeftUpView();

        //NoteTypesRect = new Rect(LeftRect.xMin, LeftRect.yMin + MusicInfoRect.height, MusicInfoRect.width, LeftRect.height - MusicInfoRect.height - 80);
        //TypeEditRect = new Rect(LeftRect.xMin, LeftRect.yMax - 80, MusicInfoRect.width, 80);

        //GUI.color = Color.blue;
        //GUI.Box(NoteTypesRect, "");
        //GUI.color = Color.green;
        //GUI.Box(TypeEditRect, "");
        EventManage(); // draw 2
        DrawSelectNoteType_2();


        EditorGUILayout.EndVertical();
    }
    void RightView()
    {
        DrawRect(GUILayoutUtility.GetRect(rightViewWidth, position.height));
    }

    // 画文件名，歌曲信息，歌曲控制按钮
    void LeftUpView()
    {
        MusicInfoRect = EditorGUILayout.BeginVertical();
        EditorGUILayout.Space(15);
        //GUI.color = Color.red;
        //GUI.Box(MusicInfoRect,"");
        DrawFileName();
        EditorGUILayout.Space(15);
        DrawBeatNotesSetting();
        EditorGUILayout.Space(15);

        DrawSongControl(); // draw 1
        EditorGUILayout.Space(15);
        DrawSaveAndClearButton();
        EditorGUILayout.Space(15);

        EditorGUILayout.Space(15);

        EditorGUILayout.EndVertical();

    }

    void DrawFileName()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(15);
        Rect t = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(150));
        tempRect = new Rect(t.xMin, t.yMax - 2, t.width, 2);
        GUI.color = lineColor;
        GUI.Box(tempRect, t2_LineTex);
        GUI.color = Color.white;
        EditorGUILayout.LabelField(beats.name);
        //EditorGUILayout.Space(10);

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(15);
        EditorGUILayout.EndHorizontal();

    }
    void DrawBeatNotesSetting()
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(15);

        NoteSettingRect = EditorGUILayout.BeginVertical(GUILayout.MaxWidth(150));
        GUI.color = Color.white;
        GUI.Box(NoteSettingRect, "");
        GUILayout.BeginHorizontal();
        GUILayout.Label("song name", GUILayout.Width(75));
        GUILayout.FlexibleSpace();
        beats.S_SongName = EditorGUILayout.TextField(beats.S_SongName, GUILayout.MinWidth(100));
        GUILayout.EndHorizontal();

        EditorGUILayout.Space(10);
        beats.I_BeatPerMinute = EditorGUILayout.IntField("bpm", beats.I_BeatPerMinute);
        EditorGUILayout.Space(10);
        beats.F_BeatStartOffset = EditorGUILayout.FloatField("offset", beats.F_BeatStartOffset);
        EditorGUILayout.Space(10);
        beats.I_SoundTracks = EditorGUILayout.IntField("sound_tracks", beats.I_SoundTracks);
        EditorGUILayout.Space(10);

        // TODO8
        little_beats_index = EditorGUILayout.Popup("节奏", little_beats_index, little_beats_options);
        beats.I_LittleBeats = little_beats_values[little_beats_index];
        EditorGUILayout.Space(10);

        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(15);
        EditorGUILayout.EndHorizontal();

        if (EditorGUIUtility.editingTextField == false)
        {
            GUI.FocusControl(null);
        }
    }
    void DrawSongControl()
    {
        f_curTime = EAudio.GetCurTime();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(15);

        Rect t = EditorGUILayout.BeginHorizontal();
        //GUI.color = viewColor;
        //GUI.Box(t, "");

        if (EAudio.IsAudioPlaying())
        {
            if (!b_pause)
            {
                if (GUILayout.Button(PauseButton, GUILayout.Width(I_buttonWidth)))
                {
                    //todo9
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

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(StopButton, GUILayout.Width(I_buttonWidth)))
        {
            stop();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(15);
        EditorGUILayout.EndHorizontal();

    }


    // TODO1: 修改BeatNote的类型，把枚举的跟换成自己自定义的列表
    void DrawSelectNoteTypeButton()
    {
        EditorGUILayout.BeginVertical();
        // 画添加，删除按钮
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Add", GUILayout.Width(I_buttonWidth)))
        {
            Debug.Log("[CusaEditorWindows] Add Note type");
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Delete", GUILayout.Width(I_buttonWidth)))
        {
            Debug.Log("[CusaEditorWindows] Delete Note type");
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(15);

        // 画修改颜色， 修改名称按钮
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Change color", GUILayout.Width(I_buttonWidth)))
        {
            Debug.Log("[CusaEditorWindows] Change Color");
        }
        GUILayout.FlexibleSpace();

        if (GUILayout.Button("Change Name", GUILayout.Width(I_buttonWidth)))
        {
            Debug.Log("[CusaEditorWindows] Change Name");
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    void AddNoteType()
    {
        Debug.Log("[CusaEditorWindows.AddNoteType] Add Note Type.");
        if (beats.AddNoteType(BeatNotes.default_name, Color.white))
        {
            beats.PrintAllNoteType();
        }
        else
        {
            Debug.Log("Add Note Type faild");
        }
    }
    void DeleteNoteType(string selectName)
    {
        Debug.Log("[CusaEditorWindows.AddNoteType] Delete Note Type.");
        // 获取选中的type类型名字

        if (!beats.DeleteNoteType(selectName))
        {
            Debug.LogError("Delete Note Type faild");
        }
    }

    void DrawTypeControlButton()
    {
        Rect buttonRect = EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add", GUILayout.Width(I_buttonWidth)))
        {
            AddNoteType();
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Delete", GUILayout.Width(I_buttonWidth)))
        {
            if (b_TypeSelected)
            {
                DeleteNoteType(S_SelectedNoteTypeName);
                S_SelectedNoteTypeName = "";
                b_TypeSelected = false;

            }
        }
        EditorGUILayout.EndHorizontal();
    }
    void DrawAllNoteTypes()
    {
        Rect tempRect = EditorGUILayout.BeginVertical();
        GUI.color = Color.gray;
        GUI.Box(tempRect, "");
        GUI.color = Color.white;

        int numbers_one_row = 2;
        for (int i = 0; i <= beats.GetTypeCount() / numbers_one_row; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < numbers_one_row; ++j)
            {
                int index = i * numbers_one_row + j;
                if (index < beats.GetTypeCount())
                {
                    NoteType node = beats.GetTypeByIndex(index);
                    GUI.color = node.NoteColor;
                    string buttonname = "xxxxx";
                    if (GUILayout.Button(node.S_TypeName, EditorStyles.toolbarButton, GUILayout.ExpandWidth(true)))
                    {
                        GUI.SetNextControlName(buttonname);
                        GUI.FocusControl(buttonname);
                        b_TypeSelected = true;
                        S_SelectedNoteTypeName = node.S_TypeName;
                        Debug.LogFormat("current selected: {0}", S_SelectedNoteTypeName);
                    }
                    GUI.color = Color.white;
                    //GUILayout.FlexibleSpace();
                }
            }
            EditorGUILayout.EndHorizontal();
            //GUILayout.Box(NoteType.t2_Thickline, GUILayout.Width(IconScale), GUILayout.Height(IconScale));
            //GUILayout.Label(node.S_TypeName, GUILayout.MaxWidth(IconScale * 1.5f), GUILayout.MaxHeight(IconScale));
        }

        GUILayout.FlexibleSpace();
        //EditorGUILayout.EndScrollView();
        DrawTypeControlButton();
        EditorGUILayout.EndVertical();
    }

    void DrawTypeEditor()
    {
        Rect bottomRect = EditorGUILayout.BeginVertical(GUILayout.Height(100));

        Rect lineRect = new Rect(bottomRect.xMin, bottomRect.yMin, bottomRect.width, 1);
        GUI.color = Color.gray;
        GUI.Box(lineRect, t2_LineTex);

        lineRect = new Rect(bottomRect.xMin, bottomRect.yMin + 25, bottomRect.width, 1);
        GUI.color = Color.gray;
        GUI.Box(lineRect, t2_LineTex);

        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        GUI.color = Color.white;
        EditorGUILayout.LabelField("Edit", style, GUILayout.ExpandWidth(true), GUILayout.Height(20));
        EditorGUILayout.Space(5);
        if (b_TypeSelected)
        {
            NoteType CurrentNoteType = beats.GetTypeByName(S_SelectedNoteTypeName);

            if (CurrentNoteType != null)
            {
                EditorGUILayout.LabelField("Color");
                EditorGUILayout.Space(2);
                Color new_color = EditorGUILayout.ColorField(CurrentNoteType.NoteColor);
                //EditorGUILayout.EndHorizontal();
                EditorGUILayout.Space(5);
                //EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Typename");
                EditorGUILayout.Space(2);
                //string new_name = EditorGUILayout.TextField(CurrentNoteType.S_TypeName);
                string new_name = EditorGUILayout.TextField(CurrentNoteType.S_TypeName);
                //GUI.SetNextControlName(buttonName);
                if (new_name != S_SelectedNoteTypeName)
                {
                    S_SelectedNoteTypeName = new_name;
                }
                beats.UpdateType(CurrentNoteType.S_TypeName, CurrentNoteType.NoteColor, new_name, new_color);
            }
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndVertical();
    }
    void DrawSelectNoteType_2()
    {
        tempRect = EditorGUILayout.BeginHorizontal();

        EditorGUILayout.Space(15);
        EditorGUILayout.BeginVertical();

        // draw a line
        Rect lineRect = new Rect(tempRect.xMin, tempRect.yMin + 1, tempRect.width, 1);
        GUI.color = Color.gray;
        GUI.Box(lineRect, t2_LineTex);

        lineRect = new Rect(tempRect.xMin, tempRect.yMin + 25, tempRect.width, 1);
        GUI.color = Color.gray;
        GUI.Box(lineRect, t2_LineTex);

        // lable
        var style = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter };
        GUI.color = Color.white;
        EditorGUILayout.LabelField("Beat Note Types Edit", style, GUILayout.ExpandWidth(true), GUILayout.Height(20));

        GUI.color = Color.white;

        EditorGUILayout.Space(5);

        DrawAllNoteTypes();

        EditorGUILayout.Space(15);

        DrawTypeEditor();

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space(15);
        EditorGUILayout.EndHorizontal();
    }

    /// <summary>
    /// 回车添加note
    /// </summary>
    void AddNotePressKey()
    {
        float f_time = f_curTime - beats.F_BeatStartOffset;
        int CurPos = Mathf.FloorToInt(f_time / f_littleBeatEach);
        Debug.LogFormat("[CusaEditor.AddNotePressKey] Add note to CurPos {0}", CurPos);
        // TODO12
        beats.SetNote(CurPos, 1, S_SelectedNoteTypeName);
    }

    /// <summary>
    /// 空格控制播放暂停
    /// </summary>
    void PlayPressKey()
    {
        //Debug.Log("[CusaEditorWindow.PlayPressKey] press key space");
        if (EAudio.IsAudioPlaying())
        {
            if (!b_pause)
            {
                pause();
            }
            else
            {
                replay();
            }
        }
        else
        {
            play();
        }
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
        //SetSongPos(beats.F_BeatStartOffset);

        //f_beatCheck = 0f;
    }

    void pause()
    {
        Debug.Log("pause");
        i_pauseSample = EAudio.GetCurSample();
        EAudio.PauseClip();
        b_pause = true;
        SetSongPos(f_curTime + (f_littleBeatEach - (f_curTime - beats.F_BeatStartOffset) % f_littleBeatEach));
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

        if (Event.current.type == EventType.KeyDown)
        {
            if (Event.current.keyCode == (KeyCode.Space))
            {
                PlayPressKey();
            }
            if (Event.current.keyCode == (KeyCode.Return))
            {
                AddNotePressKey();
            }

        }
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
            if (MainViewRect.Contains(v2_mousePos) || SliderRect.Contains(v2_mousePos))
            {
                float current_time_fixed = f_curTime + (f_littleBeatEach - (f_curTime - beats.F_BeatStartOffset) % f_littleBeatEach);

                if (Event.current.delta.y > 0)
                {
                    SetSongPos(current_time_fixed - f_littleBeatEach);
                }
                else if (Event.current.delta.y < 0)
                {
                    SetSongPos(current_time_fixed + f_littleBeatEach);
                }

            }

        }
        if (Event.current.type == EventType.MouseDrag)
        {
            if (Event.current.button == 0)
            {
                float current_time_fixed = f_curTime + (f_littleBeatEach - (f_curTime - beats.F_BeatStartOffset) % f_littleBeatEach);
                if (MainViewRect.Contains(v2_mousePos) || SliderRect.Contains(v2_mousePos))
                {
                    if (Event.current.delta.x > 0)
                    {
                        SetSongPos(current_time_fixed - f_littleBeatEach);
                    }
                    else if (Event.current.delta.x < 0)
                    {
                        SetSongPos(current_time_fixed + f_littleBeatEach);
                    }
                }

            }
        }
    }

    //滚轮到不同位置播放歌曲
    void SetSongPos(float posTime)
    {
        if (EAudio.IsAudioPlaying())
        {
            //Debug.Log("[CusaEditorWindow.SetSongPos] is playing");
            EAudio.SetSamplePosition((int)(EAudio.GetSampleDuration() * (posTime / f_totalTime)));
            i_pauseSample = EAudio.GetCurSample();
        }
        else
        {
            //Debug.Log("[CusaEditorWindow.SetSongPos] not playing");
            play();
            EAudio.SetSamplePosition((int)(EAudio.GetSampleDuration() * (posTime / f_totalTime)));
            pause();

        }
        //f_beatCheck = 0;

    }
    void DrawSaveAndClearButton()
    {
        f_curTime = EAudio.GetCurTime();

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.Space(15);

        Rect t = EditorGUILayout.BeginHorizontal();
        //GUI.color = viewColor;
        //GUI.Box(t, "");
        if (GUILayout.Button(SaveButton, GUILayout.Width(I_buttonWidth)))
        {
            SaveToJson();
        }

        GUILayout.FlexibleSpace();

        if (GUILayout.Button(ClearButton, GUILayout.Width(I_buttonWidth)))
        {
            Clear();
        }

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.Space(15);
        EditorGUILayout.EndHorizontal();

    }


    void DrawRect(Rect MainRect)
    {
        // 方便观察，之后删掉

        Rect tempSliderRect = new Rect(MainRect.xMin, MainRect.yMin, MainRect.width, f_SliderHeight);
        SliderRect = tempSliderRect;
        Rect tempMainViewRect = new Rect(MainRect.xMin, MainRect.yMin + f_SliderHeight, MainRect.width, MainRect.height - f_SliderHeight);
        MainViewRect = tempMainViewRect;

        DrawSlider(tempSliderRect); // draw 4
        DrawView(tempMainViewRect); // draw 3
    }

    void DrawView(Rect totalRect)
    {
        //Draw Main View    
        // TODO8
        float f_time = f_curTime;
        // Pos以小节为单位
        int i_startPos = Mathf.FloorToInt(f_time / f_littleBeatEach);

        float f_timeParam = f_time >= 0 ? (f_time % f_littleBeatEach) / f_littleBeatEach : 0;
        for (int j = 0; j < I_SoundTracks; j++)
        {
            // 画上下两条轨道的线
            GUI.color = lineColor;
            float up = (totalRect.yMin + (totalRect.height / (I_SoundTracks + 1)) * (j + 1) - 2f) - I_ViewLineHeight / 2;
            float down = (totalRect.yMin + (totalRect.height / (I_SoundTracks + 1)) * (j + 1) - 2f) + I_ViewLineHeight / 2;

            Rect lineRectUp = new Rect(totalRect.xMin, up, totalRect.width, 2f);
            GUI.Box(lineRectUp, t2_LineTex);

            Rect lineRectDown = new Rect(totalRect.xMin, down, totalRect.width, 2f);
            GUI.Box(lineRectDown, t2_LineTex);

            int soundtrack = j + 1;
            //GUI.Box(lineRect, t2_LineTex);
            Rect lineRect = lineRectDown;

#if (UseLittleBeat)

            for (int i = 0; i <= I_BeatsInView; ++i)
            {
                int i_curPos = i_startPos + i;
                bool clickable = true;
                // 如果当前为止在offset前面
                if (i_curPos <= 0 || i_curPos > i_totalLittleBeats)
                {
                    // 不可以点击
                    clickable = false;
                }
                #region DrawNotes

                if (i_curPos % little_beats == 0)
                {
                    //画数字
                    if (j == 0)
                    {
                        int i_beatPos = (i_curPos / little_beats) + 1;
                        float posX = lineRectUp.xMin + I_BeatCheckLine + (i - f_timeParam) * f_viewIntervalWidth + little_beats * f_viewIntervalWidth / 2;
                        float posY = lineRectUp.yMin - 23;
                        tempRect = new Rect(posX, posY, 20, 20);
                        GUI.Label(tempRect, i_beatPos.ToString());
                    }
                    //画长画粗 
                    tempRect = new Rect(lineRect.xMin + I_BeatCheckLine + (i - f_timeParam) * f_viewIntervalWidth - 0.1f,
                        lineRect.yMax - 2f - I_ViewLineHeight - 0.2f,
                        I_ViewLineWidth + 0.2f, I_ViewLineHeight + 0.2f);

                    GUI.DrawTexture(tempRect, t2_Thickline);
                }
                else
                {
                    tempRect = new Rect(lineRect.xMin + I_BeatCheckLine + (i - f_timeParam) * f_viewIntervalWidth,
                        lineRect.yMax - 2f - I_ViewLineHeight,
                        I_ViewLineWidth, I_ViewLineHeight);
                    GUI.DrawTexture(tempRect, t2_Thinline);
                }
                //BeatType type = beats.ContainsNote(i_curPos, soundtrack) ? beats.GetNoteByPos(i_curPos).e_Type : BeatType.Invalid;
                string nType = beats.ContainsNote(i_curPos, soundtrack) ? beats.GetNoteByPos(i_curPos).c_Type : NoteType.Invalid;

                // 超出范围的就不画
                if (!totalRect.Contains(tempRect.position))
                {
                    //Debug.Log(i_curPos);
                    continue;
                }

                if (!clickable)
                    GUI.color = Color.black;

                // 画note
                if (nType != NoteType.Invalid)
                {
                    if (nType == NoteType.Noodle)
                    {
                        // 画面条
                        // 区分出设置一半的和设置完成的
                        var note = beats.GetNoteByPos(i_curPos);

                        if (note.isEnd == true)
                        {
                      
                            float img_width = (note.i_EndPos - note.i_StartPos) * f_viewIntervalWidth;
                            Rect tempNoodleRect = new Rect(tempRect.xMin, tempRect.yMin, img_width, tempRect.height);
                            GUI.DrawTexture(tempNoodleRect, t2_Noodle_in);

                        }
                        else if(note.i_EndPos == note.i_StartPos)
                        { 
                            // 只画了一半的情况                           
                            GUI.color = Color.yellow;
                            Rect tempNoteRect = new Rect(tempRect.xMin + tempRect.width, tempRect.yMin, f_viewNoteWidth, tempRect.height);
                            GUI.DrawTexture(tempNoteRect, t2_Note);
                        }

                    }
                    else
                    {
                        GUI.color = beats.GetTypeColor(nType);
                        Rect tempNoteRect = new Rect(tempRect.xMin + tempRect.width, tempRect.yMin, f_viewNoteWidth, tempRect.height);
                        GUI.DrawTexture(tempNoteRect, t2_Note);
                    }

                }
                GUI.color = Color.white;
                #endregion

                #region mouseClick

                Rect noteIntervalRect = new Rect(tempRect.xMin, tempRect.yMin, tempRect.width + f_viewIntervalWidth - 5, tempRect.height);

                if (clickable && v2_mousePos != Vector2.zero && noteIntervalRect.Contains(v2_mousePos))
                {
                    if (!b_pause)
                    {
                        pause();
                    }
                    if (b_isLeftClick)
                    {

                        if (beats.GetNoteByPos(i_curPos) == null || i_curSelectPos == i_curPos)
                        {
                            Debug.LogFormat("[CusaEditor.DrawView] clicked node's type will be {0}", S_SelectedNoteTypeName);

                            if (S_SelectedNoteTypeName == NoteType.Noodle)
                            {
                                if (!b_NoodleStart)
                                {
                                    b_NoodleStart = true;
                                    i_NoodleStartPos = i_curPos;
                                    beats.SetNoodle(i_curPos, soundtrack, i_NoodleStartPos, i_NoodleStartPos, false);
                                }
                                else
                                {

                                    i_NoodleEndPos = i_curPos;
                                    var StartNote = beats.GetNoteByPos(i_NoodleStartPos);
                                    StartNote.i_EndPos = i_NoodleEndPos;
                                    StartNote.isEnd = true;
                                    for (int ii = i_NoodleStartPos + 1; ii <= i_NoodleEndPos; ii++)
                                    {
                                        beats.SetNoodle(ii, soundtrack, i_NoodleStartPos, i_NoodleEndPos, true);
                                    }


                                    // 打印面条信息
                                    for (int ii = i_NoodleStartPos; ii <= i_NoodleEndPos; ii++)
                                    {
                                        var nnn = beats.GetNoteByPos(ii);
                                        Debug.LogFormat("[Print Noodles] note[{0}] startPos: {1}, endPos: {2}, isEnd: {3}", nnn.i_LittleBeatPos, nnn.i_StartPos, nnn.i_EndPos, nnn.isEnd);
                                    }
                                    i_NoodleStartPos = -1;
                                    i_NoodleEndPos = -1;
                                    b_NoodleStart = false;

                                }
                            }
                            else
                            {
                                beats.SetNote(i_curPos, soundtrack, S_SelectedNoteTypeName);
                            }

                        }
                        i_curSelectPos = i_curPos;
                        // 检测是否存在
                        //Debug.LogFormat("[CusaEditorWindow.DrawView] i_curSelectPos {0}", beats.GetNoteByPos(i_curPos).i_BeatPos);

                    }
                    else
                    {
                        if (beats.ContainsNote(i_curPos, soundtrack))
                        {
                            beats.RemoveNote(i_curPos);
                            i_curSelectPos = -1;
                        }
                    }
                }
                #endregion


            }
#else
            for (int i = 0; i < I_BeatsInView; i++)
            {
                int i_curPos = i_startPos + i;
                bool clickable = true;

                // 如果当前为止在offset前面
                if (i_curPos < 0 || i_curPos > i_totalBeats)
                {
                    // 不可以点击
                    clickable = false;
                }
                // TODO8.3
                if (i_curPos == i_curSelectPos && beats.ContainsNote(i_curPos, soundtrack)) // 选中高亮效果
                {
                    tempRect = new Rect(lineRect.xMin + I_BeatCheckLine + I_ViewLineWidth - I_ViewLineWidth * .1f + (i - f_timeParam) * f_viewIntervalWidth,
                                    lineRect.yMax - 2f - I_ViewLineHeight / 2 - I_ViewLineHeight * .1f,
                                    I_ViewLineWidth * 1.2f, I_ViewLineHeight * 1.2f);
                    GUI.DrawTexture(tempRect, t2_Thinline);
                }
                
                // TODO 8.2 
                tempRect = new Rect(lineRect.xMin + I_BeatCheckLine + I_ViewLineWidth + (i - f_timeParam) * f_viewIntervalWidth,
                                lineRect.yMax - 2f - I_ViewLineHeight / 2,
                                I_ViewLineWidth, I_ViewLineHeight);

                BeatType type = beats.ContainsNote(i_curPos, soundtrack) ? beats.GetNoteByPos(i_curPos).e_Type : BeatType.Invalid;
                string nType = beats.ContainsNote(i_curPos, soundtrack) ? beats.GetNoteByPos(i_curPos).c_Type : NoteType.Invalid;
                if (!totalRect.Contains(tempRect.position))
                {
                    continue;
                }

                if (!clickable)
                    GUI.color = Color.black;
                else
                    GUI.color = beats.GetTypeColor(nType);
                GUI.DrawTexture(tempRect, t2_Thinline, ScaleMode.ScaleAndCrop);

                GUI.color = Color.white;

                // mouse click
                if (clickable && v2_mousePos != Vector2.zero && tempRect.Contains(v2_mousePos))
                {
                    //Debug.LogFormat("[CusaEditorWindow.DrawView] click: {0}", e_CurNoteType.ToString());

                    if (!b_pause)
                    {
                        pause();
                    }
                    if (b_isLeftClick)
                    {
                        if (beats.GetNoteByPos(i_curPos) == null || i_curSelectPos == i_curPos)
                        {


                            Debug.LogFormat("[CusaEditor.DrawView] clicked node's type will be {0}", S_SelectedNoteTypeName);
                            // TODO8
                            beats.SetNote(i_curPos, 3, soundtrack, S_SelectedNoteTypeName);

                        }
                        i_curSelectPos = i_curPos;
                        // 检测是否存在
                        //Debug.LogFormat("[CusaEditorWindow.DrawView] i_curSelectPos {0}", beats.GetNoteByPos(i_curPos).i_BeatPos);

                    }
                    else
                    {
                        if (beats.ContainsNote(i_curPos, soundtrack))
                        {
                            beats.RemoveNote(i_curPos);
                            i_curSelectPos = -1;
                        }
                    }
                }

            }
#endif
        }
        DrawViewExtra(totalRect, v2_mousePos);
        //Draw Beat Check Line(Red)
        GUI.color = lineColor;
        tempRect = new Rect(totalRect.xMin + I_BeatCheckLine, totalRect.yMin, 3f, totalRect.height);
        GUI.DrawTexture(tempRect, t2_LineTex);
        //CheckLine Time/BeatPos
        GUI.color = Color.white;
        tempRect = new Rect(totalRect.xMin + I_BeatCheckLine + 5, totalRect.yMax - 30, 120, 20);
        GUI.Box(tempRect, string.Format("Time:{0:000.0}/Pos:{1}", f_curTime, i_startPos >= 0 ? i_startPos : 0));

    }

    void DrawViewExtra(Rect totalRect, Vector2 v2_mousePos)
    {
        //画计数器
        float temp_width = 120.0f;
        float temp_height = 20.0f;
        float startX = totalRect.xMin + I_BeatCheckLine + 5;
        GUI.color = Color.white;
        tempRect = new Rect(startX, totalRect.yMin + 5, temp_width, temp_height);
        // TODO: 没必要，可以删掉
        GUI.Box(tempRect, "Beats Set:" + beats.GetNotes().Count.ToString() + "/" + i_totalBeats.ToString());

        // 画时间
        GUI.color = Color.white;
        tempRect = new Rect(startX, totalRect.yMin + 30, temp_width, temp_height);
        GUI.Box(tempRect, "Clip Length:" + string.Format("{0:000.0}", beats.AC_ClipToPlay.length));

        // 画选择的节点
        if (i_curSelectPos != -1)
        {
            Note tempNote = beats.GetNoteByPos(i_curSelectPos);
            if (tempNote == null)
            {
                //Debug.LogError("[CusaEditorWindow.DrawViewExtra] selected Note doesn't exist.");
                return;
            }

            List<float> beatsCenter = beats.BeatsCenterWithOffset(tempNote.i_LittleBeatPos, tempNote.e_Type, f_beatEach);
            GUI.color = Color.white;
            // 如果 beats类型是两个小拍或者三个小拍，则会显示多个beats的信息，因此box的大小会有所改变

            tempRect = new Rect(startX + temp_width + 30, totalRect.yMin + 5, 120, 40 + beatsCenter.Count * 20);
            GUI.Box(tempRect, "");

            tempRect = new Rect(startX + temp_width + 30, totalRect.yMin + 5, 110, 20);
            GUI.Label(tempRect, "Ind:" + beats.GetNoteIndex(i_curSelectPos).ToString() + "/Pos:" + i_curSelectPos.ToString());
            tempRect = new Rect(startX + temp_width + 30, totalRect.yMin + 25, 110, 20);
            GUI.Label(tempRect, "Type:" + beats.GetNoteByPos(i_curSelectPos).e_Type.ToString());

            for (int i = 0; i < beatsCenter.Count; i++)
            {
                tempRect = new Rect(startX + temp_width + 30, totalRect.yMin + 25 + (i + 1) * 20, 110, 20);
                GUI.Label(tempRect, "Beat " + (i + 1).ToString() + ":" + beatsCenter[i].ToString());
            }

        }

    }

    /// <summary>
    /// 滑动条
    /// </summary>
    void DrawSliderLine()
    {
        float curPos = f_curTime / f_totalTime;
        float oldPos = curPos;
        GUI.color = Color.white;
        SliderBarRect = new Rect(SliderRect.xMin, SliderRect.yMax - 10, f_sliderDisplayWidth, 20);

        curPos = GUI.HorizontalSlider(SliderBarRect, curPos, 0, 1);

        SliderLineRect = new Rect(SliderBarRect.xMin + f_sliderDisplayWidth * curPos, SliderRect.yMin, 1, SliderRect.height);
        GUI.DrawTexture(SliderLineRect, t2_LineTex);
        if (oldPos != curPos)
        {
            float curTime = curPos * f_totalTime;
            curTime = curTime + (f_littleBeatEach - ((curTime - beats.F_BeatStartOffset) % f_littleBeatEach));
            curPos = curTime / f_totalTime;
            SetSongPos(curTime);
        }
    }

    void DrawSlider(Rect totalRect)
    {
        GUI.color = upsliderColor;
        GUI.Box(totalRect, "");

        PerTrackHeight = (totalRect.height - 3 * I_SoundTracks) / I_SoundTracks;
        for (int i = 1; i <= I_SoundTracks - 1; ++i)
        {
            Rect templineRect = new Rect(totalRect.xMin, totalRect.yMin + i * (PerTrackHeight + 3) - 3, f_sliderDisplayWidth, 3);
            GUI.color = lineColor;
            GUI.DrawTexture(templineRect, t2_LineTex);
        }
        float startX = totalRect.xMin;
        float endY = totalRect.yMin;

        for (int i = 0; i < I_SoundTracks; ++i)
        {
            startX = totalRect.xMin;
            endY = totalRect.yMin + (i + 1) * (PerTrackHeight + 3) - 3;
            for (int j = 0; j < blocks; ++j)
            {
                int blockNotes = beats.GetPerBlockNotes(j, PerBlockBeats, i + 1);
                // 计算block note数量的函数
                //float blockHeight = PerTrackHeight;
                float blockHeight = PerTrackHeight * blockNotes / PerBlockBeats;
                if (justForDebug.on)
                {
                    Debug.LogFormat("[BeatNodes.GetPerBlockNotes] track: {0}, blockNotes: {1}, blockHeight: {2}", i + 1, blockNotes, blockHeight);
                    justForDebug.on = false;
                }
                Rect tempBlock = new Rect(startX + j * I_sliderBlockWidth, endY - blockHeight, I_sliderBlockWidth - 1, blockHeight);
                GUI.color = Color.red;
                GUI.DrawTexture(tempBlock, t2_LineTex);
            }
        }
        DrawSliderLine();
        //for (int j = 0; j < I_SoundTracks; ++j)
        //{
        //    int sound_track = j + 1;
        //    // 画线
        //    GUI.color = lineColor;
        //    Rect midBorder = new Rect(totalRect.xMin, totalRect.yMin + f_SliderHeight / (I_SoundTracks + 1) * (j + 1) - 2f, totalRect.width, 2f);
        //    GUI.DrawTexture(midBorder, t2_LineTex);

        //    // 画点
        //    float startX = totalRect.xMin + I_SliderBeatScale + f_sliderStartOffset;
        //    // TODO8： 重新考虑Slider的绘画策略
        //    for (int i = 0; i < i_totalBeats; i++)
        //    {
        //        string nType = beats.ContainsNote(i, sound_track) ? beats.GetNoteByPos(i).c_Type : NoteType.Invalid;
        //        GUI.color = beats.GetTypeColor(nType);
        //        tempRect = new Rect(startX + i * f_sliderBeatWidth, midBorder.yMax - 1f - I_SliderBeatScale / 2, I_SliderBeatScale / 2, 8);
        //        GUI.DrawTexture(tempRect, t2_LineTex);
        //    }
        //}
    }
    #region ColorSetting
    Color GetNoteColor(bool editable, BeatType type = BeatType.Invalid)
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
        Debug.Log("[CusaEditorWindow.SaveToJson]");

        BeatNotesJson beatjsonObj = new BeatNotesJson();
        beatjsonObj.song_name = beats.name;
        beatjsonObj.path = "xxx_path";
        beatjsonObj.bpm = beats.I_BeatPerMinute;
        beatjsonObj.offset = beats.F_BeatStartOffset;
        beatjsonObj.sound_tracks = beats.I_SoundTracks;
        beatjsonObj.little_beats = beats.I_LittleBeats;
        beatjsonObj.nodes = new List<BeatNotesJson.NoteJson>();

        List<Note> nodes = beats.GetNotes();
        for (int i = 0; i < nodes.Count; ++i)
        {
            beatjsonObj.nodes.Add(new BeatNotesJson.NoteJson(i, nodes[i].i_LittleBeatPos, nodes[i].i_sound_track, nodes[i].e_Type, nodes[i].i_StartPos, nodes[i].i_EndPos));
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
        i_curSelectPos = -1;
        beats.Clear();
    }
    #endregion

    // 检测窗口大小位置发生改变
    IEnumerator CheckForResize()
    {
        lastWidth = position.width;
        lastHeight = position.height;

        while (b_stay)
        {
            if (lastWidth != position.width || lastHeight != position.height)
            {
                Debug.Log("[CusaEditorWindow.CheckForResize] editor window's size changed. ");
                CalculateRects();
                lastWidth = position.width;
                lastHeight = position.height;
            }
            yield return new WaitForSeconds(0.3f);
        }
    }

    void CalculateRects()
    {
        F_RectWidth = position.width;
        rightViewWidth = F_RectWidth - leftViewWidth;
        I_BeatsInView = Mathf.FloorToInt((rightViewWidth - I_BeatCheckLine) / f_viewIntervalWidth) - 2;
        Debug.LogFormat("[CusaEditorWindow.CalculateRects] editor window's width: {0}", F_RectWidth);
        Debug.LogFormat("[CusaEditorWindow.CalculateRects] I_BeatsInView: {0}", I_BeatsInView);
    }

    public static string[] ConvertSlashToUnicodeSlash(string[] texts_)
    {
        for (int i = 0; i < 9; ++i)
        {
            texts_[i].Replace('/', '\u2215');
        }
        return texts_;

    }

    public static string[] ConvertUnicodeSlashToSlash(string[] texts_)
    {
        for (int i = 0; i < 9; ++i)
        {
            texts_[i].Replace('\u2215', '/');
        }
        return texts_;
    }


}
