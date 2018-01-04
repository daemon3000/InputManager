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
using UnityEngine;
using UnityEditor;
using System;
using Luminosity.IO;
using Luminosity.IO.Events;

namespace LuminosityEditor.IO
{
	[CustomEditor(typeof(InputEventManager))]
	public class InputEventManagerInspector : Editor
	{
		private enum CollectionAction
		{
			None, Remove, Add
		}

		private SerializedProperty m_inputEvents;
		private KeyCodeField m_keyCodeField;
		private GUIStyle m_headerStyle;
		private GUIStyle m_footerButtonStyle;
		private GUIContent m_plusButtonContent;
		private GUIContent m_minusButtonContent;
		private InputEventManager m_eventManager;

		private void OnEnable()
		{
			m_inputEvents = serializedObject.FindProperty("m_inputEvents");
			m_plusButtonContent = new GUIContent(EditorGUIUtility.Load("ol plus.png") as Texture, "Insert a new event after this one.");
			m_minusButtonContent = new GUIContent(EditorGUIUtility.Load("ol minus.png") as Texture, "Delete this event.");
			m_eventManager = (InputEventManager)target;
			m_keyCodeField = new KeyCodeField();
		}

		public override void OnInspectorGUI()
		{
			EnsureGUIStyles();

			serializedObject.Update();

			EditorGUILayout.Space();
			if(m_inputEvents.arraySize > 0)
			{
				for(int i = 0; i < m_inputEvents.arraySize; i++)
				{
					var action = DisplayInputEvent(i);
					if(action == CollectionAction.Add)
					{
						m_inputEvents.InsertArrayElementAtIndex(i);
                        break;
					}
					else if(action == CollectionAction.Remove)
					{
						m_inputEvents.DeleteArrayElementAtIndex(i--);
					}
				}
			}
			else
			{
				if(GUILayout.Button("Add Event", GUILayout.Height(24.0f)))
					m_inputEvents.InsertArrayElementAtIndex(0);
			}

			serializedObject.ApplyModifiedProperties();
		}

		private CollectionAction DisplayInputEvent(int index)
		{
			SerializedProperty inputEvent = m_inputEvents.GetArrayElementAtIndex(index);
			SerializedProperty eventName = inputEvent.FindPropertyRelative("m_name");
			SerializedProperty actionName = inputEvent.FindPropertyRelative("m_actionName");
			SerializedProperty keyCode = inputEvent.FindPropertyRelative("m_keyCode");
			SerializedProperty eventType = inputEvent.FindPropertyRelative("m_eventType");
			SerializedProperty inputState = inputEvent.FindPropertyRelative("m_inputState");
            SerializedProperty playerID = inputEvent.FindPropertyRelative("m_playerID");
			SerializedProperty actionEvent = inputEvent.FindPropertyRelative("m_onAction");
			SerializedProperty axisEvent = inputEvent.FindPropertyRelative("m_onAxis");
            InputEvent evt = m_eventManager.GetEvent(index);
            CollectionAction evtAction = CollectionAction.None;

            string label = string.IsNullOrEmpty(evt.Name) ? "Event" : evt.Name;
            if (inputEvent.isExpanded)
                label += " (Click to collapse)";
            else
                label += " (Click to expand)";

            if (GUILayout.Button(label, m_headerStyle, GUILayout.ExpandWidth(true)))
                inputEvent.isExpanded = !inputEvent.isExpanded;

            if (inputEvent.isExpanded)
            {
				Rect bgRect = GUILayoutUtility.GetLastRect();
				bgRect.y += 18;
				bgRect.height = CalculateBackgroundHeight(evt);
				GUI.Box(bgRect, "", (GUIStyle)"RL Background");

				EditorGUILayout.BeginHorizontal();
				GUILayout.Space(4.0f);
				EditorGUILayout.BeginVertical();

				EditorGUILayout.PropertyField(eventName);
				EditorGUILayout.PropertyField(eventType);
				if(evt.EventType == InputEventType.Axis)
				{
                    EditorGUILayout.PropertyField(playerID);
                    EditorGUILayout.PropertyField(actionName);
					EditorGUILayout.PropertyField(axisEvent);
				}
				else if(evt.EventType == InputEventType.Button)
				{
                    EditorGUILayout.PropertyField(playerID);
                    EditorGUILayout.PropertyField(actionName);
					EditorGUILayout.PropertyField(inputState);
					EditorGUILayout.PropertyField(actionEvent);
				}
				else
				{
					var keyName = keyCode.enumNames[keyCode.enumValueIndex];
					KeyCode key = InputBinding.StringToKey(keyName);

					key = m_keyCodeField.OnGUI("Key", key);
					keyCode.enumValueIndex = Array.IndexOf<string>(keyCode.enumNames, key.ToString());

					EditorGUILayout.PropertyField(inputState);
					EditorGUILayout.PropertyField(actionEvent);
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
			if(GUI.Button(new Rect(lastRect.x, lastRect.y, lastRect.width / 2, lastRect.height), m_plusButtonContent, m_footerButtonStyle))
			{
				evtAction = CollectionAction.Add;
			}
			if(GUI.Button(new Rect(lastRect.center.x, lastRect.y, lastRect.width / 2, lastRect.height), m_minusButtonContent, m_footerButtonStyle))
			{
				evtAction = CollectionAction.Remove;
			}

			EditorGUILayout.EndHorizontal();
			return evtAction;
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

		private float CalculateBackgroundHeight(InputEvent evt)
		{
			int fieldCount = evt.EventType == InputEventType.Button ? 5 : 4;
			int eventCount = evt.EventType == InputEventType.Axis ? evt.OnAxis.GetPersistentEventCount() : evt.OnAction.GetPersistentEventCount();
			float fieldHeight = 18.0f;
			float eventBorderHeight = 95.0f;
			float eventHeight = 43.0f;

			return fieldCount * fieldHeight + eventBorderHeight + Math.Max(eventCount - 1, 0) * eventHeight;
		}
	}
}
