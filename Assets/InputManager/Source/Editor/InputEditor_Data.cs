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
using System.Collections.Generic;
using UnityInputConverter;

namespace TeamUtilityEditor.IO
{
	public partial class InputEditor : EditorWindow
	{
		private static readonly Color32 HIGHLIGHT_COLOR = new Color32(62, 125, 231, 200);
		private const float MENU_WIDTH = 100.0f;
		private const float MIN_HIERARCHY_PANEL_WIDTH = 150.0f;
		private const float MIN_CURSOR_RECT_WIDTH = 10.0f;
		private const float MAX_CURSOR_RECT_WIDTH = 50.0f;
		private const float TOOLBAR_HEIGHT = 18.0f;
		private const float HIERARCHY_ITEM_HEIGHT = 18.0f;
		private const float HIERARCHY_INDENT_SIZE = 30.0f;
		private const float INPUT_FIELD_HEIGHT = 16.0f;
		private const float FIELD_SPACING = 2.0f;
		private const float BUTTON_HEIGHT = 24.0f;
		private const float INPUT_ACTION_SPACING = 20.0f;
		private const float INPUT_BINDING_SPACING = 10.0f;
		private const float SCROLL_BAR_WIDTH = 15.0f;
		private const float MIN_MAIN_PANEL_WIDTH = 300.0f;
		private const float JOYSTICK_WARNING_SPACING = 10.0f;
		private const float JOYSTICK_WARNING_HEIGHT = 40.0f;

		private enum FileMenuOptions
		{
			OverwriteProjectSettings = 0, CreateSnapshot, LoadSnapshot, Export, Import, CreateDefaultInputProfile
		}

		private enum EditMenuOptions
		{
			NewControlScheme = 0, NewInputAction, Duplicate, Delete, DeleteAll, SelectTarget, IgnoreTimescale, Copy, Paste
		}

		private enum ControlSchemeContextMenuOptions
		{
			NewInputAction = 0, Duplicate, Delete
		}

		private enum InputActionContextMenuOptions
		{
			Duplicate, Delete, Copy, Paste
		}

		private enum CollectionAction
		{
			None, Remove, Add
		}

		private enum KeyType
		{
			Positive = 0, Negative
		}

		[Serializable]
		private class SearchResult
		{
			public int ControlScheme;
			public List<int> Actions;

			public SearchResult()
			{
				ControlScheme = 0;
				Actions = new List<int>();
			}

			public SearchResult(int controlScheme, IEnumerable<int> actions)
			{
				ControlScheme = controlScheme;
				Actions = new List<int>(actions);
			}
		}
		
		[SerializeField]
		private class Selection
		{
			public const int NONE = -1;

			public int ControlScheme;
			public int Action;

			public bool IsEmpty
			{
				get { return ControlScheme == NONE && Action == NONE; }
			}

			public bool IsControlSchemeSelected
			{
				get { return ControlScheme != NONE; }
			}

			public bool IsActionSelected
			{
				get { return Action != NONE; }
			}

			public Selection()
			{
				ControlScheme = Action = NONE;
			}

			public void Reset()
			{
				ControlScheme = Action = NONE;
			}
		}

		private class KeyCodeField
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
}
