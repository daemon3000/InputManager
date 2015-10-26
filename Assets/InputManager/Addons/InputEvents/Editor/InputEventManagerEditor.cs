#region [Copyright (c) 2015 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2015 Cristian Alexandru Geambasu
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
using System.Collections;
using TeamUtility.IO;

namespace TeamUtilityEditor.IO
{
	[CustomEditor(typeof(InputEventManager))]
	public class InputEventManagerEditor : UnityEditor.Editor
	{
		private enum Action
		{
			None, Remove, Add
		}

		private SerializedProperty _inputEvents;
		private GUIStyle _headerStyle;
		private GUIStyle _footerButtonStyle;
		private GUIContent _plusButtonContent;
		private GUIContent _minusButtonContent;
		private InputEventManager _eventManager;

		private void OnEnable()
		{
			_inputEvents = serializedObject.FindProperty("_inputEvents");
			_plusButtonContent = new GUIContent(EditorGUIUtility.Load("ol plus.png") as Texture, "Insert a new event after this one.");
			_minusButtonContent = new GUIContent(EditorGUIUtility.Load("ol minus.png") as Texture, "Delete this event.");
			_eventManager = (InputEventManager)target;
		}

		public override void OnInspectorGUI()
		{
			EnsureGUIStyles();

			serializedObject.Update();

			EditorGUILayout.Space();
			if(_inputEvents.arraySize > 0)
			{
				for(int i = 0; i < _inputEvents.arraySize; i++)
				{
					var action = DisplayInputEvent(i);
					if(action == Action.Add)
					{
						_inputEvents.InsertArrayElementAtIndex(i);
                        break;
					}
					else if(action == Action.Remove)
					{
						_inputEvents.DeleteArrayElementAtIndex(i--);
					}
				}
			}
			else
			{
				if(GUILayout.Button("Add Event", GUILayout.Height(24.0f)))
					_inputEvents.InsertArrayElementAtIndex(0);
			}

			serializedObject.ApplyModifiedProperties();
		}

		private Action DisplayInputEvent(int index)
		{
			SerializedProperty inputEvent = _inputEvents.GetArrayElementAtIndex(index);
			SerializedProperty eventName = inputEvent.FindPropertyRelative("name");
			SerializedProperty axisName = inputEvent.FindPropertyRelative("axisName");
			SerializedProperty buttonName = inputEvent.FindPropertyRelative("buttonName");
			SerializedProperty keyCode = inputEvent.FindPropertyRelative("keyCode");
			SerializedProperty eventType = inputEvent.FindPropertyRelative("eventType");
			SerializedProperty inputState = inputEvent.FindPropertyRelative("inputState");
			SerializedProperty actionEvent = inputEvent.FindPropertyRelative("onAction");
			SerializedProperty axisEvent = inputEvent.FindPropertyRelative("onAxis");
            InputEvent evt = _eventManager.GetEvent(index);
            Action evtAction = Action.None;

            string label = string.IsNullOrEmpty(evt.name) ? "Event" : evt.name;
            if (inputEvent.isExpanded)
                label += " (Click to collapse)";
            else
                label += " (Click to expand)";

            if (GUILayout.Button(label, _headerStyle, GUILayout.ExpandWidth(true)))
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
				if(evt.eventType == InputEventType.Axis)
				{
					EditorGUILayout.PropertyField(axisName);
					EditorGUILayout.PropertyField(axisEvent);
				}
				else if(evt.eventType == InputEventType.Button)
				{
					EditorGUILayout.PropertyField(buttonName);
					EditorGUILayout.PropertyField(inputState);
					EditorGUILayout.PropertyField(actionEvent);
				}
				else
				{
					EditorGUILayout.PropertyField(keyCode);
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
			if(GUI.Button(new Rect(lastRect.x, lastRect.y, lastRect.width / 2, lastRect.height), _plusButtonContent, _footerButtonStyle))
			{
				evtAction = Action.Add;
			}
			if(GUI.Button(new Rect(lastRect.center.x, lastRect.y, lastRect.width / 2, lastRect.height), _minusButtonContent, _footerButtonStyle))
			{
				evtAction = Action.Remove;
			}

			EditorGUILayout.EndHorizontal();
			return evtAction;
		}

		private void EnsureGUIStyles()
		{
			if(_headerStyle == null)
			{
				_headerStyle = new GUIStyle(Array.Find<GUIStyle>(GUI.skin.customStyles, obj => obj.name == "RL Header"));
				_headerStyle.normal.textColor = Color.black;
				_headerStyle.alignment = TextAnchor.MiddleLeft;
				_headerStyle.contentOffset = new Vector2(10, 0);
				_headerStyle.fontSize = 11;
			}
			if(_footerButtonStyle == null)
			{
				_footerButtonStyle = new GUIStyle(Array.Find<GUIStyle>(GUI.skin.customStyles, obj => obj.name == "RL FooterButton"));
				_footerButtonStyle.alignment = TextAnchor.MiddleCenter;
			}
		}

		private float CalculateBackgroundHeight(InputEvent evt)
		{
			int fieldCount = evt.eventType == InputEventType.Axis ? 3 : 4;
			int eventCount = evt.eventType == InputEventType.Axis ? evt.onAxis.GetPersistentEventCount() : evt.onAction.GetPersistentEventCount();
			float fieldHeight = 18.0f;
			float eventBorderHeight = 95.0f;
			float eventHeight = 43.0f;

			return fieldCount * fieldHeight + eventBorderHeight + Math.Max(eventCount - 1, 0) * eventHeight;
		}
	}
}
