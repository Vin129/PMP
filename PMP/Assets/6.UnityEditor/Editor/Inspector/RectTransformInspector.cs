// /****************************************************************************
//  * Copyright (c) 2018 ZhongShan KPP Technology Co
//  * Copyright (c) 2018 Karsion
//  * 
//  * https://github.com/karsion
//  * Date: 2018-02-28 15:58
//  *
//  * Permission is hereby granted, free of charge, to any person obtaining a copy
//  * of this software and associated documentation files (the "Software"), to deal
//  * in the Software without restriction, including without limitation the rights
//  * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  * copies of the Software, and to permit persons to whom the Software is
//  * furnished to do so, subject to the following conditions:
//  * 
//  * The above copyright notice and this permission notice shall be included in
//  * all copies or substantial portions of the Software.
//  * 
//  * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  * THE SOFTWARE.
//  ****************************************************************************/

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace PMP.Extension
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(RectTransform))]
    public class RectTransformInspector : CustomCustomEditor
    {
        private const string strButtonLeft = "ButtonLeft";
        private const string strButtonMid = "ButtonMid";
        private const string strButtonRight = "ButtonRight";

        private static GUIStyle styleMove;
        private static GUIStyle stylePivotSetup;
        private bool autoSetNativeSize;
        private Image image;

        private float scaleAll = 1;
        private SerializedProperty spAnchoredPosition;
        private SerializedProperty spLocalRotation;
        private SerializedProperty spLocalScale;
        private SerializedProperty spPivot;
        private SerializedProperty spSizeDelta;

        public RectTransformInspector()
            : base("RectTransformEditor")
        {
        }

        private void MoveTargetAnchoredPosition(Vector2 v2Unit)
        {
            foreach (Object item in targets)
            {
                RectTransform rtf = item as RectTransform;
                rtf.anchoredPosition += v2Unit;
            }
        }

        private void OnEnable()
        {
            spAnchoredPosition = serializedObject.FindProperty("m_AnchoredPosition");
            spSizeDelta = serializedObject.FindProperty("m_SizeDelta");
            spLocalRotation = serializedObject.FindProperty("m_LocalRotation");
            spLocalScale = serializedObject.FindProperty("m_LocalScale");
            spPivot = serializedObject.FindProperty("m_Pivot");
            scaleAll = spLocalScale.FindPropertyRelative("x").floatValue;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (s_Contents == null)
            {
                s_Contents = new Contents();
            }

            serializedObject.Update();
            Event e = Event.current;
    
            const float fButtonWidth = 21;
            if (stylePivotSetup == null)
            {
                stylePivotSetup = new GUIStyle("PreButton")
                                  {
                                      normal = new GUIStyle("CN Box").normal,
                                      active = new GUIStyle("AppToolbar").normal,
                                      overflow = new RectOffset(),
                                      padding = new RectOffset(0, 0, -1, 0),
                                      fixedWidth = 19
                                  };

                styleMove = new GUIStyle(stylePivotSetup)
                            {
                                padding = new RectOffset(0, 0, -2, 0)
                            };
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.BeginVertical();
                {
                    #region Tools
                    GUILayout.BeginHorizontal();
                    {
                        EditorGUIUtility.labelWidth = 64;
                        float newScale = EditorGUILayout.FloatField(s_Contents.scaleContent, scaleAll);
                        if (!Mathf.Approximately(scaleAll, newScale))
                        {
                            scaleAll = newScale;
                            spLocalScale.vector3Value = Vector3.one*scaleAll;
                        }
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button(s_Contents.anchoredPosition0Content, strButtonLeft, GUILayout.Width(fButtonWidth)))
                        {
                            foreach (Object item in targets)
                            {
                                RectTransform rtf = item as RectTransform;
                                rtf.LocalPositionIdentity();
                            }
                        }

                        if (GUILayout.Button(s_Contents.rotation0Content, strButtonMid, GUILayout.Width(fButtonWidth)))
                        {
                            spLocalRotation.quaternionValue = Quaternion.identity;
                        }

                        if (GUILayout.Button(s_Contents.scale0Content, strButtonRight, GUILayout.Width(fButtonWidth)))
                        {
                            spLocalScale.vector3Value = Vector3.one;
                            scaleAll = spLocalScale.FindPropertyRelative("x").floatValue;
                        }


                        if (GUILayout.Button(s_Contents.roundContent,GUILayout.Width(fButtonWidth*3)))
                        {
                            Vector2 v2 = spAnchoredPosition.vector2Value;
                            spAnchoredPosition.vector2Value = new Vector2(Mathf.Round(v2.x), Mathf.Round(v2.y));
                            v2 = spSizeDelta.vector2Value;
                            spSizeDelta.vector2Value = new Vector2(Mathf.Round(v2.x), Mathf.Round(v2.y));
                        }
                    }
                    GUILayout.EndHorizontal();
                    #endregion

                    #region Copy Paste
                    GUILayout.BeginHorizontal();
                    Color c = GUI.color;
                    GUI.color = new Color(1f, 1f, 0.5f, 1f);
                    if (GUILayout.Button(s_Contents.copyContent, strButtonLeft, GUILayout.Width(fButtonWidth)))
                    {
                        ComponentUtility.CopyComponent(target as RectTransform);
                    }

                    GUI.color = new Color(1f, 0.5f, 0.5f, 1f);
                    if (GUILayout.Button(s_Contents.pasteContent, strButtonMid, GUILayout.Width(fButtonWidth)))
                    {
                        foreach (Object item in targets)
                        {
                            ComponentUtility.PasteComponentValues(item as RectTransform);
                        }
                    }

                    GUI.color = c;
                    if (GUILayout.Button(s_Contents.fillParentContent, strButtonMid, GUILayout.Width(fButtonWidth)))
                    {
                        Undo.RecordObjects(targets, "F");
                        foreach (Object item in targets)
                        {
                            RectTransform rtf = item as RectTransform;
                            rtf.anchorMax = Vector2.one;
                            rtf.anchorMin = Vector2.zero;
                            rtf.offsetMax = Vector2.zero;
                            rtf.offsetMin = Vector2.zero;
                        }
                    }

                    if (GUILayout.Button(s_Contents.normalSizeDeltaContent, strButtonRight, GUILayout.Width(fButtonWidth)))
                    {
                        Undo.RecordObjects(targets, "N");
                        foreach (Object item in targets)
                        {
                            RectTransform rtf = item as RectTransform;
                            Rect rect = rtf.rect;
                            rtf.anchorMax = new Vector2(0.5f, 0.5f);
                            rtf.anchorMin = new Vector2(0.5f, 0.5f);
                            rtf.sizeDelta = rect.size;
                        }
                    }




                    GUILayout.EndHorizontal();
                    #endregion
                }
                GUILayout.EndVertical();
            

                GUILayout.BeginVertical();
                {
                    #region Pivot
                    GUILayout.Label("Pivot", "ProfilerPaneSubLabel"); //┌─┐
                    GUILayout.BeginHorizontal(); //│┼│
                    {
                        //└─┘
                        if (GUILayout.Button("◤", stylePivotSetup))
                        {
                            spPivot.vector2Value = new Vector2(0, 1);
                        }

                        if (GUILayout.Button("", stylePivotSetup))
                        {
                            spPivot.vector2Value = new Vector2(0.5f, 1);
                        }

                        if (GUILayout.Button("◥", stylePivotSetup))
                        {
                            spPivot.vector2Value = new Vector2(1, 1);
                        }
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("", stylePivotSetup))
                        {
                            spPivot.vector2Value = new Vector2(0, 0.5f);
                        }

                        if (GUILayout.Button("+", styleMove))
                        {
                            spPivot.vector2Value = new Vector2(0.5f, 0.5f);
                        }

                        if (GUILayout.Button("", stylePivotSetup))
                        {
                            spPivot.vector2Value = new Vector2(1, 0.5f);
                        }
                    }

                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("◣", stylePivotSetup))
                        {
                            spPivot.vector2Value = new Vector2(0, 0);
                        }

                        if (GUILayout.Button("", stylePivotSetup))
                        {
                            spPivot.vector2Value = new Vector2(0.5f, 0);
                        }

                        if (GUILayout.Button("◢", stylePivotSetup))
                        {
                            spPivot.vector2Value = new Vector2(1, 0);
                        }
                    }

                    GUILayout.EndHorizontal();
                }
                #endregion

                GUILayout.EndVertical();
            }
            GUILayout.EndHorizontal();

            TransformInspector.DrawBottomPanel(target, targets);
            serializedObject.ApplyModifiedProperties();
        }

        private static Contents s_Contents;

        private class Contents
        {
            public readonly GUIContent anchoredPosition0Content = new GUIContent("P", "AnchoredPosition 0");
            public readonly GUIContent scaleContent = new GUIContent("Scale …", "Scale all axis");
            public readonly GUIContent scale0Content = new GUIContent("S", "Scale 1");
            public readonly GUIContent rotation0Content = new GUIContent("R", "Rotation 0");
            public readonly GUIContent roundContent = new GUIContent("Round", "AnchoredPosition DeltaSize round to int");
            public readonly GUIContent copyContent = new GUIContent("C", "Copy component value");
            public readonly GUIContent pasteContent = new GUIContent("P", "Paste component value");
            public readonly GUIContent fillParentContent = new GUIContent("F", "Fill the parent RectTransform");
            public readonly GUIContent normalSizeDeltaContent = new GUIContent("N", "Change to normal sizeDelta mode");
        }
    }
}