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
using UnityEditorInternal;
using Luminosity.IO.Events;

namespace LuminosityEditor.IO
{
	[CustomEditor(typeof(InputEventGroup))]
	public class InputEventGroupInspector : Editor
	{
		private SerializedProperty m_receiveInput;
		private SerializedProperty m_inputEventGroups;
		private SerializedProperty m_inputEventManagers;
		private ReorderableList m_inputEventGroupList;
		private ReorderableList m_inputEventManagerList;

		private void OnEnable()
		{
			m_receiveInput = serializedObject.FindProperty("m_receiveInput");
			m_inputEventGroups = serializedObject.FindProperty("m_inputEventGroups");
			m_inputEventManagers = serializedObject.FindProperty("m_inputEventManagers");

			m_inputEventGroupList = new ReorderableList(serializedObject, m_inputEventGroups, true, true, true, true);
			m_inputEventGroupList.drawHeaderCallback += rect =>
			{
				EditorGUI.LabelField(rect, "Groups");
			};
			m_inputEventGroupList.drawElementCallback += (rect, index, isActive, isFocused) =>
			{
				SerializedProperty item = m_inputEventGroups.GetArrayElementAtIndex(index);

				rect.y += 2;
				rect.height = 16;
				EditorGUI.PropertyField(rect, item, GUIContent.none);
			};

			m_inputEventManagerList = new ReorderableList(serializedObject, m_inputEventManagers, true, true, true, true);
			m_inputEventManagerList.drawHeaderCallback += rect =>
			{
				EditorGUI.LabelField(rect, "Events");
			};
			m_inputEventManagerList.drawElementCallback += (rect, index, isActive, isFocused) =>
			{
				SerializedProperty item = m_inputEventManagers.GetArrayElementAtIndex(index);

				rect.y += 2;
				rect.height = 16;
				EditorGUI.PropertyField(rect, item, GUIContent.none);
			};
		}

		public override void OnInspectorGUI()
		{
			serializedObject.Update();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
			EditorGUILayout.PropertyField(m_receiveInput);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Groups", EditorStyles.boldLabel);
			m_inputEventGroupList.DoLayoutList();

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Events", EditorStyles.boldLabel);
			m_inputEventManagerList.DoLayoutList();

			EditorGUILayout.Space();
			if(GUILayout.Button("Find Children", GUILayout.Height(24)))
			{
				InputEventGroup ieg = target as InputEventGroup;
				ieg.FindChildren();
			}

			serializedObject.ApplyModifiedProperties();
		}
	}
}
