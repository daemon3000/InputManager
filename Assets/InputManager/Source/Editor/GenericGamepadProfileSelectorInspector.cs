#region [Copyright (c) 2018 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2018 Cristian Alexandru Geambasu
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy of this software 
//	and associated documentation files (the "Software"), to deal in the Software without restriction, 
//	including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, 
//	and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, 
//	subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in all copies or substantial 
//	portions of the Software.
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
//	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
//	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, 
//	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
#endregion
using System;
using UnityEngine;
using UnityEditor;
using Luminosity.IO;

namespace LuminosityEditor.IO
{
    [CustomEditor(typeof(GenericGamepadProfileSelector))]
    public class GenericGamepadProfileSelectorInspector : Editor
    {
        private enum CollectionAction
		{
			None, Remove, Add
		}

        private SerializedProperty m_defaultProfile;
        private SerializedProperty m_profiles;
		private GUIStyle m_headerStyle;
		private GUIStyle m_footerButtonStyle;
		private GUIContent m_profilePlusButtonContent;
		private GUIContent m_profileMinusButtonContent;
        private GUIContent m_constraintPlusButtonContent;
		private GUIContent m_constraintMinusButtonContent;
		private GenericGamepadProfileSelector m_selector;

        private void OnEnable()
        {
            m_defaultProfile = serializedObject.FindProperty("m_defaultProfile");
            m_profiles = serializedObject.FindProperty("m_profiles");
            m_profilePlusButtonContent = new GUIContent(EditorGUIUtility.Load("ol plus.png") as Texture, "Insert a new profile after this one.");
			m_profileMinusButtonContent = new GUIContent(EditorGUIUtility.Load("ol minus.png") as Texture, "Delete this profile.");
            m_constraintPlusButtonContent = new GUIContent(EditorGUIUtility.Load("ol plus.png") as Texture, "Insert a new constraint after this one.");
			m_constraintMinusButtonContent = new GUIContent(EditorGUIUtility.Load("ol minus.png") as Texture, "Delete this constraint.");
            m_selector = target as GenericGamepadProfileSelector;
        }

        public override void OnInspectorGUI()
        {
            EnsureGUIStyles();

            serializedObject.Update();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("GENERAL", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(m_defaultProfile);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("PROFILES", EditorStyles.boldLabel);
            if(m_profiles.arraySize > 0)
			{
				for(int i = 0; i < m_profiles.arraySize; i++)
				{
					var action = DisplayEntry(i);
					if(action == CollectionAction.Add)
					{
						m_profiles.InsertArrayElementAtIndex(i);
                        break;
					}
					else if(action == CollectionAction.Remove)
					{
						m_profiles.DeleteArrayElementAtIndex(i--);
					}
				}
			}
			else
			{
				if(GUILayout.Button("Add Profile", GUILayout.Height(24.0f)))
					m_profiles.InsertArrayElementAtIndex(0);
			}

            serializedObject.ApplyModifiedProperties();
        }

        private CollectionAction DisplayEntry(int index)
		{
            GenericGamepadProfileSelector.Profile profile = m_selector.GetProfile(index);
			SerializedProperty profileSP = m_profiles.GetArrayElementAtIndex(index);
            SerializedProperty isExpanded = profileSP.FindPropertyRelative("IsExpanded");
			SerializedProperty gamepadProfile = profileSP.FindPropertyRelative("GamepadProfile");
			SerializedProperty constraints = profileSP.FindPropertyRelative("Constraints");
            CollectionAction entryAction = CollectionAction.None;

            string label = "Profile " + (index + 1);
            if(isExpanded.boolValue)
                label += " (Click to collapse)";
            else
                label += " (Click to expand)";

            if(GUILayout.Button(label, m_headerStyle, GUILayout.ExpandWidth(true)))
                isExpanded.boolValue = !isExpanded.boolValue;

            if(isExpanded.boolValue)
            {
                Rect bgRect = GUILayoutUtility.GetLastRect();
                bgRect.y += 18;
                bgRect.height = CalculateBackgroundHeight(profile);
                GUI.Box(bgRect, "", (GUIStyle)"RL Background");

                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(4.0f);
                EditorGUILayout.BeginVertical();

                EditorGUILayout.PropertyField(gamepadProfile);
                GUILayout.Space(4.0f);
                EditorGUILayout.LabelField("Constraints", EditorStyles.boldLabel);
                if(constraints.arraySize > 0)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(14.0f);
                    GUILayout.BeginVertical();

                    for(int i = 0; i < constraints.arraySize; i++)
                    {
                        SerializedProperty constraint = constraints.GetArrayElementAtIndex(i);
                        SerializedProperty type = constraint.FindPropertyRelative("Type");
                        SerializedProperty content = constraint.FindPropertyRelative("Content");

                        GUILayout.BeginHorizontal();
                        EditorGUILayout.PropertyField(type, GUIContent.none);
                        EditorGUILayout.PropertyField(content, GUIContent.none);
                        GUILayout.Space(40.0f);
                        EditorGUILayout.EndHorizontal();

                        Rect constraintRect = GUILayoutUtility.GetLastRect();
                        Rect plusRect = new Rect(constraintRect.xMax - 36, constraintRect.y, 16, constraintRect.height);
                        Rect minusRect = new Rect(constraintRect.xMax - 16, constraintRect.y, 16, constraintRect.height);
                        if(GUI.Button(plusRect, m_constraintPlusButtonContent, m_footerButtonStyle))
                        {
                            constraints.InsertArrayElementAtIndex(i);
                            break;
                        }
                        if(GUI.Button(minusRect, m_constraintMinusButtonContent, m_footerButtonStyle))
                        {
                            constraints.DeleteArrayElementAtIndex(i--);
                        }
                    }

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(14.0f);
                    EditorGUILayout.EndHorizontal();
                }
                else
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(8.0f);
                    GUILayout.BeginVertical();

                    if(GUILayout.Button("Add Constraint", GUILayout.Height(24.0f)))
					    constraints.InsertArrayElementAtIndex(0);

                    EditorGUILayout.EndVertical();
                    GUILayout.Space(8.0f);
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Space(5.0f);
                EditorGUILayout.EndVertical();
                GUILayout.Space(4.0f);
                EditorGUILayout.EndHorizontal();
                GUILayout.Space(2.0f);
            }
			else
			{
				GUILayout.Box("", (GUIStyle)"RL Background", GUILayout.ExpandWidth(true), GUILayout.Height(10));
				GUILayout.Space(-3.0f);
			}

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			GUILayout.Label("", (GUIStyle)"RL Background", GUILayout.Width(60.0f), GUILayout.Height(20));

			Rect lastRect = GUILayoutUtility.GetLastRect();
			if(GUI.Button(new Rect(lastRect.x, lastRect.y, lastRect.width / 2, lastRect.height), m_profilePlusButtonContent, m_footerButtonStyle))
			{
				entryAction = CollectionAction.Add;
			}
			if(GUI.Button(new Rect(lastRect.center.x, lastRect.y, lastRect.width / 2, lastRect.height), m_profileMinusButtonContent, m_footerButtonStyle))
			{
				entryAction = CollectionAction.Remove;
			}

			EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);
			return entryAction;
		}

        private float CalculateBackgroundHeight(GenericGamepadProfileSelector.Profile profile)
		{
            int constraintCount = profile.Constraints.Count;
			int fieldCount = 2;
			float fieldHeight = 18.0f;
			float constraintHeight = 18.0f;

            if(constraintCount > 0)
                return fieldCount * fieldHeight + Math.Max(constraintCount - 1, 0) * constraintHeight + 37.0f;
            else
                return fieldCount * fieldHeight + 45.0f;
		}

        private void EnsureGUIStyles()
		{
			if(m_headerStyle == null)
			{
				m_headerStyle = new GUIStyle(Array.Find<GUIStyle>(GUI.skin.customStyles, obj => obj.name == "RL Header"));
				m_headerStyle.normal.textColor = Color.black;
				m_headerStyle.alignment = TextAnchor.MiddleLeft;
				m_headerStyle.contentOffset = new Vector2(10, 0);
				m_headerStyle.fontSize = 11;
			}
			if(m_footerButtonStyle == null)
			{
				m_footerButtonStyle = new GUIStyle(Array.Find<GUIStyle>(GUI.skin.customStyles, obj => obj.name == "RL FooterButton"))
				{
					alignment = TextAnchor.MiddleCenter
				};
			}
		}
    }
}
