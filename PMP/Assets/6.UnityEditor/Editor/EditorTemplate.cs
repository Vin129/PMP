using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using W = UnityEngine.Debug;

#region  生命周期
public class LifecycleEditorWindow :EditorWindow
{
	// [MenuItem("PMP/6.UnityEditor/LifecycleEditorWindow %&Q")]
	// private static void ShowWindow() {
	// 	var window = GetWindow<LifecycleEditorWindow>(false,"LifecycleEditorWindow");
	// 	window.Show();
	// }

	// 打开界面
	private void OnEnable() {
		W.Log("OnEnable");
	}
	// 被聚焦时
	private void OnFocus() {
		W.Log("OnFocus");
	}
	// 属性界面更新时，几乎一直更新
	private void OnInspectorUpdate() {
		W.Log("OnInspectorUpdate");
	}
	//当项目发生更改时
	private void OnProjectChange() {
		W.Log("OnProjectChange");
	}
	//当选择发生更改时
	private void OnSelectionChange() {
		W.Log("OnSelectionChange");
	}
	//场景层次发生改变时
	private void OnHierarchyChange() {
		W.Log("OnHierarchyChange");
	}
	//当渲染UI时
	private void OnGUI() {
		W.Log("OnGUI");
	}
	//丢失聚焦时
	private void OnLostFocus() {
		W.Log("OnLostFocus");
	}
	//隐藏时
	private void OnDisable() {
		W.Log("OnDisable");
	}
	//关闭时
	private void OnDestroy() {
		W.Log("OnDestroy");
	}
}
#endregion
#region GUIStyle 
public class GUIStyleViewer : EditorWindow
{
    private Vector2 scrollVector2 = Vector2.zero;
    private string search = "";

    [MenuItem("PMP/6.UnityEditor/GUIStyle")]
    public static void InitWindow()
    {
        EditorWindow.GetWindow(typeof(GUIStyleViewer));
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal("HelpBox");
        GUILayout.Space(30);
        search = EditorGUILayout.TextField("", search, "SearchTextField", GUILayout.MaxWidth(position.size.x / 2));
        GUILayout.Label("", "SearchCancelButtonEmpty");
        GUILayout.EndHorizontal();
        scrollVector2 = GUILayout.BeginScrollView(scrollVector2);
        foreach (GUIStyle style in GUI.skin.customStyles)
        {
            if (style.name.ToLower().Contains(search.ToLower()))
            {
                DrawStyleItem(style);
            }
        }
        GUILayout.EndScrollView();
    }

    void DrawStyleItem(GUIStyle style)
    {
        GUILayout.BeginHorizontal("box");
        GUILayout.Space(40);
        EditorGUILayout.SelectableLabel(style.name);
        GUILayout.FlexibleSpace();
        EditorGUILayout.SelectableLabel(style.name, style);
        GUILayout.Space(40);
        EditorGUILayout.SelectableLabel("", style, GUILayout.Height(40), GUILayout.Width(40));
        GUILayout.Space(50);
        if (GUILayout.Button("复制到剪贴板"))
        {
            TextEditor textEditor = new TextEditor();
            textEditor.text = style.name;
            textEditor.OnFocus();
            textEditor.Copy();
        }
        GUILayout.EndHorizontal();
        GUILayout.Space(10);
    }
}
#endregion



public class EditorTemplate : EditorWindow {
    string fieldText;
	bool buttonEnabled;
	bool button2;
	bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;
	int selectionIndex = 0;
	private Vector2 scrollVector2 = Vector2.zero;
	[Flags]
	enum editorEnum
	{
		A,
		B,
		C,
	}

	editorEnum e = editorEnum.A;
	editorEnum ee = editorEnum.A;

	[MenuItem("PMP/6.UnityEditor/EditorTemplate %&Z")]
	private static void ShowWindow() {
		// 是否浮动  title
		var window = GetWindow<EditorTemplate>(false,"EditorTemplate");
		// window.titleContent = new GUIContent("EditorTemplate");
		window.minSize = new Vector2(700, 300);
		// window.position = new Rect(window.position.position,new Vector2(800,800));
		window.Show();
	}

	private void OnGUI() {
		GUI.color = Color.white;
		GUI.backgroundColor = Color.white;
		GUI.skin.label.richText = true;
		var offset = Vector2.one;
		var rect = Rect.MinMaxRect(offset.x,offset.y,position.width - offset.x,position.height - offset.y);
		GUI.Box(rect,"WindowBox");
		EditorUtil.DrawGrid(rect);
		
		GUILayout.Space(5);
		//"EditorWindow属性"
		GUILayout.BeginHorizontal(GUILayout.MaxHeight(100));
		GUILayout.BeginVertical("box");
		focuseName = EditorWindow.focusedWindow != null?EditorWindow.focusedWindow.ToString():"空";
		mouseOverName = EditorWindow.mouseOverWindow != null?EditorWindow.mouseOverWindow.ToString():"空";
		EditorGUILayout.LabelField(string.Format("当前聚焦的窗口：{0}",focuseName));
		GUILayout.FlexibleSpace();
		EditorGUILayout.LabelField(string.Format("鼠标停留在窗口：{0}",mouseOverName));
		GUILayout.FlexibleSpace();
		this.autoRepaintOnSceneChange = EditorGUILayout.Toggle("视窗改变重新绘制：",this.autoRepaintOnSceneChange,GUILayout.MinWidth(100));
		GUILayout.EndVertical();
		GUILayout.EndHorizontal();

		//GUILayout
		GUILayout.BeginHorizontal();
		GUILayout.BeginVertical("box");
		//Lable
		GUILayout.Label("You Name:", EditorStyles.boldLabel);
        fieldText = GUILayout.TextField(fieldText);
		GUILayout.Label(string.Format("Hellow {0}",fieldText));

		//Button
		if(GUILayout.Button("Button"))
		{
			buttonEnabled = true;
		}
		if(buttonEnabled)
		{
			if(GUILayout.Button("Show HelpBox"))
			{
				button2 = true;
			}
		}
		if(button2)
		{
			EditorGUILayout.HelpBox("MessageType.Info",MessageType.Info);
			EditorGUILayout.HelpBox("MessageType.Error",MessageType.Error);
			EditorGUILayout.HelpBox("MessageType.None",MessageType.None);
			EditorGUILayout.HelpBox("MessageType.Warning",MessageType.Warning);
		}

		//Toggle
		GUILayout.BeginVertical("box");
		groupEnabled = EditorGUILayout.BeginToggleGroup("ToggleGroup", groupEnabled);

        myBool = EditorGUILayout.Toggle("Toggle", myBool);
        myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);

        EditorGUILayout.EndToggleGroup();
		GUILayout.EndVertical();
		
		GUILayout.BeginVertical("box");
		//Enum
		e = (editorEnum)EditorGUILayout.EnumPopup("EnumPopup",e);
		ee = (editorEnum)EditorGUILayout.EnumFlagsField("EnumFlagsField",ee);

		GUILayout.Label(string.Format("EnumPopup:{0}",e.ToString()));
		GUILayout.Label(string.Format("EnumFlagsField:{0}",ee.ToString()));

		var selections = new string[10];
		for(int i = 0;i< selections.Length;i++)
			selections[i] = "Selet  "+ i; 
		selectionIndex = GUILayout.SelectionGrid(selectionIndex,selections,selections.Length);

		GUILayout.EndVertical();

		GUILayout.BeginVertical("box",GUILayout.MaxHeight(100));
		scrollVector2 = GUILayout.BeginScrollView(scrollVector2);
		foreach (var item in selections)
		{
			GUILayout.Label(item,EditorStyles.boldLabel);
			GUILayout.Space(5);
		}
		GUILayout.EndScrollView();
		GUILayout.EndVertical();

		GUILayout.EndVertical();
		GUILayout.EndHorizontal();
	}


	string focuseName;
	string mouseOverName;
	private void OnInspectorUpdate() {
		
	}
}



