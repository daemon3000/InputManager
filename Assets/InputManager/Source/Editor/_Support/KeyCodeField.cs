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
using UnityInputConverter;

namespace LuminosityEditor.IO
{
	public class KeyCodeField
	{
		private string m_controlName;
		private string m_keyString;
		private bool m_isEditing;

		public KeyCodeField()
		{
			m_controlName = Guid.NewGuid().ToString("N");
			m_keyString = "";
			m_isEditing = false;
		}

		public KeyCode OnGUI(string label, KeyCode key)
		{
			GUI.SetNextControlName(m_controlName);
			bool hasFocus = (GUI.GetNameOfFocusedControl() == m_controlName);
			if(!m_isEditing && hasFocus)
			{
				m_keyString = key == KeyCode.None ? "" : KeyCodeConverter.KeyToString(key);
			}

			m_isEditing = hasFocus;
			if(m_isEditing)
			{
				m_keyString = EditorGUILayout.TextField(label, m_keyString);
			}
			else
			{
				EditorGUILayout.TextField(label, key == KeyCode.None ? "" : KeyCodeConverter.KeyToString(key));
			}

			if(m_isEditing && Event.current.type == EventType.KeyUp)
			{
				key = KeyCodeConverter.StringToKey(m_keyString);
				if(key == KeyCode.None)
				{
					m_keyString = "";
				}
				else
				{
					m_keyString = KeyCodeConverter.KeyToString(key);
				}
				m_isEditing = false;
			}

			return key;
		}

		public KeyCode OnGUI(Rect position, string label, KeyCode key)
		{
			GUI.SetNextControlName(m_controlName);
			bool hasFocus = (GUI.GetNameOfFocusedControl() == m_controlName);
			if(!m_isEditing && hasFocus)
			{
				m_keyString = key == KeyCode.None ? "" : KeyCodeConverter.KeyToString(key);
			}

			m_isEditing = hasFocus;
			if(m_isEditing)
			{
				m_keyString = EditorGUI.TextField(position, label, m_keyString);
			}
			else
			{
				EditorGUI.TextField(position, label, key == KeyCode.None ? "" : KeyCodeConverter.KeyToString(key));
			}

			if(m_isEditing && Event.current.type == EventType.KeyUp)
			{
				key = KeyCodeConverter.StringToKey(m_keyString);
				if(key == KeyCode.None)
				{
					m_keyString = "";
				}
				else
				{
					m_keyString = KeyCodeConverter.KeyToString(key);
				}
				m_isEditing = false;
			}

			return key;
		}

		public void Reset()
		{
			m_keyString = "";
			m_isEditing = false;
		}
	}
}
