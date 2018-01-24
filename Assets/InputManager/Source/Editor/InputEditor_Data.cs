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
using System.Collections.Generic;
using UnityInputConverter;

namespace LuminosityEditor.IO
{
	public partial class InputEditor : EditorWindow
	{
		private static readonly Color32 HIGHLIGHT_COLOR = new Color32(62, 125, 231, 200);
		private const float MIN_WINDOW_WIDTH = 600.0f;
		private const float MIN_WINDOW_HEIGHT = 200.0f;
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

		private enum MoveDirection
		{
			Up, Down
		}

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
			NewInputAction = 0, Duplicate, Delete, MoveUp, MoveDown
		}

		private enum InputActionContextMenuOptions
		{
			Duplicate, Delete, Copy, Paste, MoveUp, MoveDown
		}

		private enum CollectionAction
		{
			None, Remove, Add, MoveUp, MoveDown
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
	}
}
