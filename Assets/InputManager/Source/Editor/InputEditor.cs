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
using UnityEditor.IMGUI.Controls;
using System.Linq;
using System.Collections.Generic;
using Luminosity.IO;
using UnityInputConverter;
using UnityObject = UnityEngine.Object;

namespace LuminosityEditor.IO
{
	public partial class InputEditor : EditorWindow
	{
		#region [Fields]
		[SerializeField]
		private InputManager m_inputManager;
		[SerializeField]
		private List<SearchResult> m_searchResults;
		[SerializeField]
		private Selection m_selection;
		[SerializeField]
		private Vector2 m_hierarchyScrollPos = Vector2.zero;
		[SerializeField]
		private Vector2 m_mainPanelScrollPos = Vector2.zero;
		[SerializeField]
		private float m_hierarchyPanelWidth = MENU_WIDTH * 2;
		[SerializeField]
		private Texture2D m_highlightTexture;
		[SerializeField]
		private string m_searchString = "";
		
		private GUIContent m_gravityInfo;
		private GUIContent m_sensitivityInfo;
		private GUIContent m_snapInfo;
		private GUIContent m_deadZoneInfo;
		private GUIContent m_plusButtonContent;
		private GUIContent m_minusButtonContent;
		private GUIContent m_upButtonContent;
		private GUIContent m_downButtonContent;
		private InputAction m_copySource;
		private SearchField m_searchField;
		private KeyCodeField[] m_keyFields;
		private GUIStyle m_whiteLabel;
		private GUIStyle m_whiteFoldout;
		private GUIStyle m_warningLabel;
		private bool m_isResizingHierarchy = false;
		private bool m_tryedToFindInputManagerInScene = false;
		private bool m_isDisposed = false;
		private string[] m_axisOptions;
		private string[] m_joystickOptions;
		#endregion

		#region [Startup]
		private void OnEnable()
		{
			m_gravityInfo = new GUIContent("Gravity", "The speed(in units/sec) at which a digital axis falls towards neutral.");
			m_sensitivityInfo = new GUIContent("Sensitivity", "The speed(in units/sec) at which an axis moves towards the target value.");
			m_snapInfo = new GUIContent("Snap", "If input switches direction, do we snap to neutral and continue from there? For digital axes only.");
			m_deadZoneInfo = new GUIContent("Dead Zone", "Size of analog dead zone. Values within this range map to neutral.");
			m_plusButtonContent = new GUIContent(EditorToolbox.GetUnityIcon("ol plus"));
			m_minusButtonContent = new GUIContent(EditorToolbox.GetUnityIcon("ol minus"));
			m_upButtonContent = new GUIContent(EditorToolbox.GetCustomIcon("input_editor_arrow_up"));
			m_downButtonContent = new GUIContent(EditorToolbox.GetCustomIcon("input_editor_arrow_down"));

			m_joystickOptions = EditorToolbox.GenerateJoystickNames();
			m_axisOptions = EditorToolbox.GenerateJoystickAxisNames();
			m_searchField = new SearchField();

			CreateKeyFields();

			EditorToolbox.ShowStartupWarning();
			IsOpen = true;

			m_tryedToFindInputManagerInScene = false;
			if(m_inputManager == null)
				m_inputManager = UnityObject.FindObjectOfType<InputManager>();
			if(m_searchResults == null)
				m_searchResults = new List<SearchResult>();
			if(m_selection == null)
				m_selection = new Selection();
			if(m_highlightTexture == null)
				CreateHighlightTexture();

			EditorApplication.playModeStateChanged += HandlePlayModeChanged;
			m_isDisposed = false;
		}

		private void OnDisable()
		{
			Dispose();
		}

		private void OnDestroy()
		{
			Dispose();
		}

		private void Dispose()
		{
			if(!m_isDisposed)
			{
				IsOpen = false;
				Texture2D.DestroyImmediate(m_highlightTexture);
				m_highlightTexture = null;
				m_copySource = null;

				EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
				m_isDisposed = true;
			}
		}
		#endregion

		#region [Menus]
		private void CreateFileMenu(Rect position)
		{
			GenericMenu fileMenu = new GenericMenu();
			fileMenu.AddItem(new GUIContent("Overwrite Project Settings"), false, HandleFileMenuOption, FileMenuOptions.OverwriteProjectSettings);
			fileMenu.AddItem(new GUIContent("Default Control Scheme"), false, HandleFileMenuOption, FileMenuOptions.CreateDefaultInputProfile);
			
			fileMenu.AddSeparator("");
			if(m_inputManager.ControlSchemes.Count > 0)
				fileMenu.AddItem(new GUIContent("Create Snapshot"), false, HandleFileMenuOption, FileMenuOptions.CreateSnapshot);
			else
				fileMenu.AddDisabledItem(new GUIContent("Create Snapshot"));

			if(EditorToolbox.CanLoadSnapshot())
				fileMenu.AddItem(new GUIContent("Restore Snapshot"), false, HandleFileMenuOption, FileMenuOptions.LoadSnapshot);
			else
				fileMenu.AddDisabledItem(new GUIContent("Restore Snapshot"));
			fileMenu.AddSeparator("");

			if(m_inputManager.ControlSchemes.Count > 0)
				fileMenu.AddItem(new GUIContent("Export"), false, HandleFileMenuOption, FileMenuOptions.Export);
			else
				fileMenu.AddDisabledItem(new GUIContent("Export"));

			fileMenu.AddItem(new GUIContent("Import"), false, HandleFileMenuOption, FileMenuOptions.Import);
			
			fileMenu.DropDown(position);
		}

		private void HandleFileMenuOption(object arg)
		{
			FileMenuOptions option = (FileMenuOptions)arg;
			switch(option)
			{
			case FileMenuOptions.OverwriteProjectSettings:
				EditorToolbox.OverwriteProjectSettings();
				break;
			case FileMenuOptions.CreateSnapshot:
				EditorToolbox.CreateSnapshot(m_inputManager);
				break;
			case FileMenuOptions.LoadSnapshot:
				EditorToolbox.LoadSnapshot(m_inputManager);
				break;
			case FileMenuOptions.Export:
				ExportInputProfile();
				break;
			case FileMenuOptions.Import:
				ImportInputProfile();
				break;
			case FileMenuOptions.CreateDefaultInputProfile:
				LoadInputProfileFromResource(EditorToolbox.DEFAULT_INPUT_PROFILE);
				break;
			}
		}

		private void CreateEditMenu(Rect position)
		{
			GenericMenu editMenu = new GenericMenu();
			editMenu.AddItem(new GUIContent("New Control Scheme"), false, HandleEditMenuOption, EditMenuOptions.NewControlScheme);
			if(m_selection.IsControlSchemeSelected)
				editMenu.AddItem(new GUIContent("New Action"), false, HandleEditMenuOption, EditMenuOptions.NewInputAction);
			else
				editMenu.AddDisabledItem(new GUIContent("New Action"));
			editMenu.AddSeparator("");

			if(!m_selection.IsEmpty)
				editMenu.AddItem(new GUIContent("Duplicate"), false, HandleEditMenuOption, EditMenuOptions.Duplicate);
			else
				editMenu.AddDisabledItem(new GUIContent("Duplicate"));

			if(!m_selection.IsEmpty)
				editMenu.AddItem(new GUIContent("Delete"), false, HandleEditMenuOption, EditMenuOptions.Delete);
			else
				editMenu.AddDisabledItem(new GUIContent("Delete"));

			if(m_inputManager.ControlSchemes.Count > 0)
				editMenu.AddItem(new GUIContent("Delete All"), false, HandleEditMenuOption, EditMenuOptions.DeleteAll);
			else
				editMenu.AddDisabledItem(new GUIContent("Delete All"));

			if(m_selection.IsActionSelected)
				editMenu.AddItem(new GUIContent("Copy"), false, HandleEditMenuOption, EditMenuOptions.Copy);
			else
				editMenu.AddDisabledItem(new GUIContent("Copy"));

			if(m_copySource != null && m_selection.IsActionSelected)
				editMenu.AddItem(new GUIContent("Paste"), false, HandleEditMenuOption, EditMenuOptions.Paste);
			else
				editMenu.AddDisabledItem(new GUIContent("Paste"));

			editMenu.AddSeparator("");

			editMenu.AddItem(new GUIContent("Select Target"), false, HandleEditMenuOption, EditMenuOptions.SelectTarget);
			editMenu.AddItem(new GUIContent("Ignore Timescale"), m_inputManager.IgnoreTimescale, HandleEditMenuOption, EditMenuOptions.IgnoreTimescale);
			editMenu.DropDown(position);
		}

		private void HandleEditMenuOption(object arg)
		{
			EditMenuOptions option = (EditMenuOptions)arg;
			switch(option)
			{
			case EditMenuOptions.NewControlScheme:
				CreateNewControlScheme();
				break;
			case EditMenuOptions.NewInputAction:
				CreateNewInputAction();
				break;
			case EditMenuOptions.Duplicate:
				Duplicate();
				break;
			case EditMenuOptions.Delete:
				Delete();
				break;
			case EditMenuOptions.DeleteAll:
				DeleteAll();
				break;
			case EditMenuOptions.SelectTarget:
				UnityEditor.Selection.activeGameObject = m_inputManager.gameObject;
				break;
			case EditMenuOptions.IgnoreTimescale:
				m_inputManager.IgnoreTimescale = !m_inputManager.IgnoreTimescale;
				break;
			case EditMenuOptions.Copy:
				CopyInputAction();
				break;
			case EditMenuOptions.Paste:
				PasteInputAction();
				break;
			}
		}

		private void CreateControlSchemeContextMenu(Rect position)
		{
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("New Action"), false, HandleControlSchemeContextMenuOption, ControlSchemeContextMenuOptions.NewInputAction);
			contextMenu.AddSeparator("");

			contextMenu.AddItem(new GUIContent("Duplicate"), false, HandleControlSchemeContextMenuOption, ControlSchemeContextMenuOptions.Duplicate);
			contextMenu.AddItem(new GUIContent("Delete"), false, HandleControlSchemeContextMenuOption, ControlSchemeContextMenuOptions.Delete);
			contextMenu.AddSeparator("");

			contextMenu.AddItem(new GUIContent("Move Up"), false, HandleControlSchemeContextMenuOption, ControlSchemeContextMenuOptions.MoveUp);
			contextMenu.AddItem(new GUIContent("Move Down"), false, HandleControlSchemeContextMenuOption, ControlSchemeContextMenuOptions.MoveDown);

			contextMenu.DropDown(position);
		}

		private void HandleControlSchemeContextMenuOption(object arg)
		{
			ControlSchemeContextMenuOptions option = (ControlSchemeContextMenuOptions)arg;
			switch(option)
			{
			case ControlSchemeContextMenuOptions.NewInputAction:
				CreateNewInputAction();
				break;
			case ControlSchemeContextMenuOptions.Duplicate:
				Duplicate();
				break;
			case ControlSchemeContextMenuOptions.Delete:
				Delete();
				break;
			case ControlSchemeContextMenuOptions.MoveUp:
				ReorderControlScheme(MoveDirection.Up);
				break;
			case ControlSchemeContextMenuOptions.MoveDown:
				ReorderControlScheme(MoveDirection.Down);
				break;
			}
		}

		private void CreateInputActionContextMenu(Rect position)
		{
			GenericMenu contextMenu = new GenericMenu();
			contextMenu.AddItem(new GUIContent("Duplicate"), false, HandleInputActionContextMenuOption, InputActionContextMenuOptions.Duplicate);
			contextMenu.AddItem(new GUIContent("Delete"), false, HandleInputActionContextMenuOption, InputActionContextMenuOptions.Delete);
			contextMenu.AddItem(new GUIContent("Copy"), false, HandleInputActionContextMenuOption, InputActionContextMenuOptions.Copy);
			contextMenu.AddItem(new GUIContent("Paste"), false, HandleInputActionContextMenuOption, InputActionContextMenuOptions.Paste);
			contextMenu.AddSeparator("");

			contextMenu.AddItem(new GUIContent("Move Up"), false, HandleInputActionContextMenuOption, InputActionContextMenuOptions.MoveUp);
			contextMenu.AddItem(new GUIContent("Move Down"), false, HandleInputActionContextMenuOption, InputActionContextMenuOptions.MoveDown);

			contextMenu.DropDown(position);
		}

		private void HandleInputActionContextMenuOption(object arg)
		{
			InputActionContextMenuOptions option = (InputActionContextMenuOptions)arg;
			switch(option)
			{
			case InputActionContextMenuOptions.Duplicate:
				Duplicate();
				break;
			case InputActionContextMenuOptions.Delete:
				Delete();
				break;
			case InputActionContextMenuOptions.Copy:
				CopyInputAction();
				break;
			case InputActionContextMenuOptions.Paste:
				PasteInputAction();
				break;
			case InputActionContextMenuOptions.MoveUp:
				ReorderInputAction(MoveDirection.Up);
				break;
			case InputActionContextMenuOptions.MoveDown:
				ReorderInputAction(MoveDirection.Down);
				break;
			}
		}

		private void CreateNewControlScheme()
		{
			m_inputManager.ControlSchemes.Add(new ControlScheme());
			m_selection.Reset();
			m_selection.ControlScheme = m_inputManager.ControlSchemes.Count - 1;
			Repaint();
		}

		private void CreateNewInputAction()
		{
			if(m_selection.IsControlSchemeSelected)
			{
				ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];
				scheme.CreateNewAction("New Action");
				scheme.IsExpanded = true;
				m_selection.Action = scheme.Actions.Count - 1;
				ResetKeyFields();
				Repaint();
			}
		}

		private void Duplicate()
		{
			if(m_selection.IsActionSelected)
			{
				DuplicateInputAction();
			}
			else if(m_selection.IsControlSchemeSelected)
			{
				DuplicateControlScheme();
			}
		}

		private void DuplicateInputAction()
		{
			ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];
			InputAction source = scheme.Actions[m_selection.Action];

			scheme.InsertNewAction(m_selection.Action + 1, source.Name + " Copy", source);
			m_selection.Action++;

			if(m_searchString.Length > 0)
			{
				UpdateSearchResults();
			}
			Repaint();
		}

		private void DuplicateControlScheme()
		{
			ControlScheme source = m_inputManager.ControlSchemes[m_selection.ControlScheme];

			m_inputManager.ControlSchemes.Insert(m_selection.ControlScheme + 1, ControlScheme.Duplicate(source.Name + " Copy", source));
			m_selection.ControlScheme++;

			if(m_searchString.Length > 0)
			{
				UpdateSearchResults();
			}
			Repaint();
		}

		private void Delete()
		{
			if(m_selection.IsActionSelected)
			{
				ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];
				scheme.DeleteAction(m_selection.Action);
			}
			else if(m_selection.IsControlSchemeSelected)
			{
				ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];
				if(m_inputManager.PlayerOneDefault == scheme.Name)
					m_inputManager.PlayerOneDefault = null;
				if(m_inputManager.PlayerTwoDefault == scheme.Name)
					m_inputManager.PlayerTwoDefault = null;
				if(m_inputManager.PlayerThreeDefault == scheme.Name)
					m_inputManager.PlayerThreeDefault = null;
				if(m_inputManager.PlayerFourDefault == scheme.Name)
					m_inputManager.PlayerFourDefault = null;

				m_inputManager.ControlSchemes.RemoveAt(m_selection.ControlScheme);
			}

			m_selection.Reset();
			if(m_searchString.Length > 0)
			{
				UpdateSearchResults();
			}
			Repaint();
		}

		private void DeleteAll()
		{
			m_inputManager.ControlSchemes.Clear();
			m_inputManager.PlayerOneDefault = string.Empty;
			m_inputManager.PlayerTwoDefault = string.Empty;
			m_inputManager.PlayerThreeDefault = string.Empty;
			m_inputManager.PlayerFourDefault = string.Empty;

			m_selection.Reset();
			if(m_searchString.Length > 0)
			{
				UpdateSearchResults();
			}
			Repaint();
		}

		private void CopyInputAction()
		{
			ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];
			m_copySource = InputAction.Duplicate(scheme.Actions[m_selection.Action]);
		}

		private void PasteInputAction()
		{
			ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];
			InputAction action = scheme.Actions[m_selection.Action];
			action.Copy(m_copySource);
		}

		private void ReorderControlScheme(MoveDirection dir)
		{
			if(m_selection.IsControlSchemeSelected)
			{
				var index = m_selection.ControlScheme;

				if(dir == MoveDirection.Up && index > 0)
				{
					var temp = m_inputManager.ControlSchemes[index];
					m_inputManager.ControlSchemes[index] = m_inputManager.ControlSchemes[index - 1];
					m_inputManager.ControlSchemes[index - 1] = temp;

					m_selection.Reset();
					m_selection.ControlScheme = index - 1;
				}
				else if(dir == MoveDirection.Down && index < m_inputManager.ControlSchemes.Count - 1)
				{
					var temp = m_inputManager.ControlSchemes[index];
					m_inputManager.ControlSchemes[index] = m_inputManager.ControlSchemes[index + 1];
					m_inputManager.ControlSchemes[index + 1] = temp;

					m_selection.Reset();
					m_selection.ControlScheme = index + 1;
				}
			}
		}

		private void ReorderInputAction(MoveDirection dir)
		{
			if(m_selection.IsActionSelected)
			{
				var scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];
				var schemeIndex = m_selection.ControlScheme;
				var actionIndex = m_selection.Action;

				if(dir == MoveDirection.Up && actionIndex > 0)
				{
					scheme.SwapActions(actionIndex, actionIndex - 1);
					
					m_selection.Reset();
					m_selection.ControlScheme = schemeIndex;
					m_selection.Action = actionIndex - 1;
				}
				else if(dir == MoveDirection.Down && actionIndex < scheme.Actions.Count - 1)
				{
					scheme.SwapActions(actionIndex, actionIndex + 1);

					m_selection.Reset();
					m_selection.ControlScheme = schemeIndex;
					m_selection.Action = actionIndex + 1;
				}
			}
		}

		private void UpdateSearchResults()
		{
			m_searchResults.Clear();

			if(!string.IsNullOrEmpty(m_searchString))
			{
				for(int i = 0; i < m_inputManager.ControlSchemes.Count; i++)
				{
					IEnumerable<int> axes = from a in m_inputManager.ControlSchemes[i].Actions
											where (a.Name.IndexOf(m_searchString, System.StringComparison.InvariantCultureIgnoreCase) >= 0)
											select m_inputManager.ControlSchemes[i].Actions.IndexOf(a);

					if(axes.Count() > 0)
					{
						m_searchResults.Add(new SearchResult(i, axes));
					}
				}
			}
		}
		#endregion

		#region [OnGUI]
		private void OnGUI()
		{
			EnsureGUIStyles();

			if(m_inputManager == null && !m_tryedToFindInputManagerInScene)
				TryToFindInputManagerInScene();

			if(m_inputManager == null)
			{
				DrawMissingInputManagerWarning();
				return;
			}

			ValidateSelection();

			Undo.RecordObject(m_inputManager, "InputManager");
			if(m_selection.IsControlSchemeSelected)
			{
				DrawMainPanel();
			}

			UpdateHierarchyPanelWidth();
			if(m_searchString.Length > 0)
			{
				DrawSearchResults();
			}
			else
			{
				DrawHierarchyPanel();
			}
			DrawMainToolbar();
			if(GUI.changed)
			{
				EditorUtility.SetDirty(m_inputManager);
			}
		}

		private void ValidateSelection()
		{
			if(m_inputManager == null || m_inputManager.ControlSchemes.Count <= 0)
				m_selection.Reset();
			else if(m_selection.IsControlSchemeSelected && m_selection.ControlScheme >= m_inputManager.ControlSchemes.Count)
				m_selection.Reset();
			else if(m_selection.IsActionSelected && m_selection.Action >= m_inputManager.ControlSchemes[m_selection.ControlScheme].Actions.Count)
				m_selection.Reset();
		}

		private void DrawMissingInputManagerWarning()
		{
			Rect warningRect = new Rect(0.0f, 20.0f, position.width, 40.0f);
			Rect buttonRect = new Rect(position.width / 2 - 100.0f, warningRect.yMax, 200.0f, 25.0f);

			EditorGUI.LabelField(warningRect, "Could not find an input manager instance in the scene!", m_warningLabel);
			if(GUI.Button(buttonRect, "Try Again"))
			{
				TryToFindInputManagerInScene();
			}
		}

		private void DrawMainToolbar()
		{
			Rect screenRect = new Rect(0.0f, 0.0f, position.width, TOOLBAR_HEIGHT);
			Rect fileMenuRect = new Rect(0.0f, 0.0f, MENU_WIDTH, screenRect.height);
			Rect editMenuRect = new Rect(fileMenuRect.xMax, 0.0f, MENU_WIDTH, screenRect.height);
			Rect paddingLabelRect = new Rect(editMenuRect.xMax, 0.0f, screenRect.width - MENU_WIDTH * 2, screenRect.height);
			Rect searchFieldRect = new Rect(screenRect.width - (MENU_WIDTH * 1.5f + 5.0f), 2.0f, MENU_WIDTH * 1.5f, screenRect.height - 2.0f);
			int lastSearchStringLength = m_searchString.Length;

			GUI.BeginGroup(screenRect);
			DrawFileMenu(fileMenuRect);
			DrawEditMenu(editMenuRect);
			EditorGUI.LabelField(paddingLabelRect, "", EditorStyles.toolbarButton);
			
			m_searchString = m_searchField.OnToolbarGUI(searchFieldRect, m_searchString);

			GUI.EndGroup();

			if(lastSearchStringLength != m_searchString.Length)
			{
				UpdateSearchResults();
			}
		}

		private void DrawFileMenu(Rect screenRect)
		{
			EditorGUI.LabelField(screenRect, "File", EditorStyles.toolbarDropDown);
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
				screenRect.Contains(Event.current.mousePosition))
			{
				CreateFileMenu(new Rect(screenRect.x, screenRect.yMax, 0.0f, 0.0f));
			}
		}

		private void DrawEditMenu(Rect screenRect)
		{
			EditorGUI.LabelField(screenRect, "Edit", EditorStyles.toolbarDropDown);
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 &&
				screenRect.Contains(Event.current.mousePosition))
			{
				CreateEditMenu(new Rect(screenRect.x, screenRect.yMax, 0.0f, 0.0f));
			}
		}

		private void UpdateHierarchyPanelWidth()
		{
			float cursorRectWidth = m_isResizingHierarchy ? MAX_CURSOR_RECT_WIDTH : MIN_CURSOR_RECT_WIDTH;
			Rect cursorRect = new Rect(m_hierarchyPanelWidth - cursorRectWidth / 2, TOOLBAR_HEIGHT, cursorRectWidth,
										position.height - TOOLBAR_HEIGHT);
			Rect resizeRect = new Rect(m_hierarchyPanelWidth - MIN_CURSOR_RECT_WIDTH / 2, 0.0f,
										MIN_CURSOR_RECT_WIDTH, position.height);

			EditorGUIUtility.AddCursorRect(cursorRect, MouseCursor.ResizeHorizontal);
			switch(Event.current.type)
			{
			case EventType.MouseDown:
				if(Event.current.button == 0 && resizeRect.Contains(Event.current.mousePosition))
				{
					m_isResizingHierarchy = true;
					Event.current.Use();
				}
				break;
			case EventType.MouseUp:
				if(Event.current.button == 0 && m_isResizingHierarchy)
				{
					m_isResizingHierarchy = false;
					Event.current.Use();
				}
				break;
			case EventType.MouseDrag:
				if(m_isResizingHierarchy)
				{
					m_hierarchyPanelWidth = Mathf.Clamp(m_hierarchyPanelWidth + Event.current.delta.x,
													 MIN_HIERARCHY_PANEL_WIDTH, position.width / 2);
					Event.current.Use();
					Repaint();
				}
				break;
			default:
				break;
			}
		}

		private void DrawSearchResults()
		{
			Rect screenRect = new Rect(0.0f, TOOLBAR_HEIGHT - 5.0f, m_hierarchyPanelWidth, position.height - TOOLBAR_HEIGHT + 10.0f);
			Rect scrollView = new Rect(screenRect.x, screenRect.y + 5.0f, screenRect.width, position.height - screenRect.y);
			Rect viewRect = new Rect(0.0f, 0.0f, scrollView.width, CalculateSearchResultViewRectHeight());
			float itemPosY = 0.0f;

			GUI.Box(screenRect, "");
			m_hierarchyScrollPos = GUI.BeginScrollView(scrollView, m_hierarchyScrollPos, viewRect);
			for(int i = 0; i < m_searchResults.Count; i++)
			{
				Rect csRect = new Rect(1.0f, itemPosY, viewRect.width - 2.0f, HIERARCHY_ITEM_HEIGHT);
				int csIndex = m_searchResults[i].ControlScheme;

				DrawHierarchyControlSchemeItem(csRect, csIndex);
				itemPosY += HIERARCHY_ITEM_HEIGHT;

				if(m_inputManager.ControlSchemes[csIndex].IsExpanded)
				{
					for(int j = 0; j < m_searchResults[i].Actions.Count; j++)
					{
						Rect iaRect = new Rect(1.0f, itemPosY, viewRect.width - 2.0f, HIERARCHY_ITEM_HEIGHT);
						int iaIndex = m_searchResults[i].Actions[j];

						DrawHierarchyInputActionItem(iaRect, csIndex, iaIndex);
						itemPosY += HIERARCHY_ITEM_HEIGHT;
					}
				}
			}
			GUI.EndScrollView();
		}

		private void DrawHierarchyPanel()
		{
			Rect screenRect = new Rect(0.0f, TOOLBAR_HEIGHT - 5.0f, m_hierarchyPanelWidth, position.height - TOOLBAR_HEIGHT + 10.0f);
			Rect scrollView = new Rect(screenRect.x, screenRect.y + 5.0f, screenRect.width, position.height - screenRect.y);
			Rect viewRect = new Rect(0.0f, 0.0f, scrollView.width, CalculateHierarchyViewRectHeight());
			float itemPosY = 0.0f;

			GUI.Box(screenRect, "");
			m_hierarchyScrollPos = GUI.BeginScrollView(scrollView, m_hierarchyScrollPos, viewRect);
			for(int i = 0; i < m_inputManager.ControlSchemes.Count; i++)
			{
				Rect csRect = new Rect(1.0f, itemPosY, viewRect.width - 2.0f, HIERARCHY_ITEM_HEIGHT);
				DrawHierarchyControlSchemeItem(csRect, i);
				itemPosY += HIERARCHY_ITEM_HEIGHT;

				if(m_inputManager.ControlSchemes[i].IsExpanded)
				{
					for(int j = 0; j < m_inputManager.ControlSchemes[i].Actions.Count; j++)
					{
						Rect iaRect = new Rect(1.0f, itemPosY, viewRect.width - 2.0f, HIERARCHY_ITEM_HEIGHT);
						DrawHierarchyInputActionItem(iaRect, i, j);
						itemPosY += HIERARCHY_ITEM_HEIGHT;
					}
				}
			}
			GUI.EndScrollView();
		}

		private void DrawHierarchyControlSchemeItem(Rect position, int index)
		{
			ControlScheme scheme = m_inputManager.ControlSchemes[index];
			Rect foldoutRect = new Rect(5.0f, 1.0f, 10, position.height - 1.0f);
			Rect nameRect = new Rect(foldoutRect.xMax + 5.0f, 1.0f, position.width - (foldoutRect.xMax + 5.0f), position.height - 1.0f);

			if(Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 1))
			{
				if(position.Contains(Event.current.mousePosition))
				{
					m_selection.Reset();
					m_selection.ControlScheme = index;
					GUI.FocusControl(null);
					Repaint();

					if(Event.current.button == 1)
					{
						CreateControlSchemeContextMenu(new Rect(Event.current.mousePosition, Vector2.zero));
					}
				}
			}

			GUI.BeginGroup(position);
			if(m_selection.IsControlSchemeSelected && !m_selection.IsActionSelected && m_selection.ControlScheme == index)
			{
				GUI.DrawTexture(new Rect(0, 0, position.width, position.height), m_highlightTexture, ScaleMode.StretchToFill);
				scheme.IsExpanded = EditorGUI.Foldout(foldoutRect, scheme.IsExpanded, GUIContent.none);
				EditorGUI.LabelField(nameRect, scheme.Name, m_whiteLabel);
			}
			else
			{
				scheme.IsExpanded = EditorGUI.Foldout(foldoutRect, scheme.IsExpanded, GUIContent.none);
				EditorGUI.LabelField(nameRect, scheme.Name);
			}
			GUI.EndGroup();
		}

		private void DrawHierarchyInputActionItem(Rect position, int controlSchemeIndex, int index)
		{
			InputAction action = m_inputManager.ControlSchemes[controlSchemeIndex].Actions[index];
			Rect nameRect = new Rect(HIERARCHY_INDENT_SIZE, 1.0f, position.width - HIERARCHY_INDENT_SIZE, position.height - 1.0f);

			if(Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 1))
			{
				if(position.Contains(Event.current.mousePosition))
				{
					m_selection.Reset();
					m_selection.ControlScheme = controlSchemeIndex;
					m_selection.Action = index;
					ResetKeyFields();
					Event.current.Use();
					GUI.FocusControl(null);
					Repaint();

					if(Event.current.button == 1)
					{
						CreateInputActionContextMenu(new Rect(Event.current.mousePosition, Vector2.zero));
					}
				}
			}

			GUI.BeginGroup(position);
			if(m_selection.IsActionSelected && m_selection.ControlScheme == controlSchemeIndex && m_selection.Action == index)
			{
				GUI.DrawTexture(new Rect(0, 0, position.width, position.height), m_highlightTexture, ScaleMode.StretchToFill);
				EditorGUI.LabelField(nameRect, action.Name, m_whiteLabel);
			}
			else
			{
				EditorGUI.LabelField(nameRect, action.Name);
			}
			GUI.EndGroup();
		}

		private void DrawMainPanel()
		{
			Rect position = new Rect(m_hierarchyPanelWidth, TOOLBAR_HEIGHT,
										this.position.width - m_hierarchyPanelWidth,
										this.position.height - TOOLBAR_HEIGHT);
			ControlScheme scheme = m_inputManager.ControlSchemes[m_selection.ControlScheme];

			if(m_selection.IsActionSelected)
			{
				InputAction action = scheme.Actions[m_selection.Action];
				DrawInputActionFields(position, action);
			}
			else
			{
				DrawControlSchemeFields(position, scheme);
			}

			if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				if(position.Contains(Event.current.mousePosition))
				{
					Event.current.Use();
					GUI.FocusControl(null);
					Repaint();
				}
			}
		}

		private void DrawControlSchemeFields(Rect position, ControlScheme controlScheme)
		{
			position.x += 5;
			position.y += 5;
			position.width -= 10;

			GUILayout.BeginArea(position);
			controlScheme.Name = EditorGUILayout.TextField("Name", controlScheme.Name);
			EditorGUILayout.LabelField("Description");
			controlScheme.Description = EditorGUILayout.TextArea(controlScheme.Description, GUILayout.MinHeight(100.0f));

			GUILayout.EndArea();
		}

		private void DrawInputActionFields(Rect position, InputAction action)
		{
			bool collectionChanged = false;
			float viewRectHeight = CalculateInputActionViewRectHeight(action);
			float itemPosY = 0.0f;
			float contentWidth = position.width - 10.0f;
			Rect viewRect = new Rect(-5.0f, -5.0f, position.width - 10.0f, viewRectHeight - 10.0f);
			
			if(viewRect.width < MIN_MAIN_PANEL_WIDTH)
			{
				viewRect.width = MIN_MAIN_PANEL_WIDTH;
				contentWidth = viewRect.width - 10.0f;
			}

			if(viewRectHeight - 10.0f > position.height)
			{
				viewRect.width -= SCROLL_BAR_WIDTH;
				contentWidth -= SCROLL_BAR_WIDTH;
			}

			m_mainPanelScrollPos = GUI.BeginScrollView(position, m_mainPanelScrollPos, viewRect);
			Rect nameRect = new Rect(0.0f, ValuePP(ref itemPosY, INPUT_FIELD_HEIGHT + FIELD_SPACING), contentWidth, INPUT_FIELD_HEIGHT);
			Rect descriptionRect = new Rect(0.0f, ValuePP(ref itemPosY, INPUT_FIELD_HEIGHT + FIELD_SPACING), contentWidth, INPUT_FIELD_HEIGHT);

			string name = EditorGUI.TextField(nameRect, "Name", action.Name);
			if(name != action.Name) action.Name = name;		//	This prevents the warning at runtime

			action.Description = EditorGUI.TextField(descriptionRect, "Description", action.Description);

			if(action.Bindings.Count > 0)
			{
				itemPosY += INPUT_ACTION_SPACING;

				for(int i = 0; i < action.Bindings.Count; i++)
				{
					float bindingRectHeight = CalculateInputBindingViewRectHeight(action.Bindings[i]);
					Rect bindingRect = new Rect(-4.0f, ValuePP(ref itemPosY, bindingRectHeight + INPUT_BINDING_SPACING), contentWidth + 8.0f, bindingRectHeight);

					var res = DrawInputBindingFields(bindingRect, "Binding " + (i + 1).ToString("D2"), action, i);
					if(res == CollectionAction.Add)
					{
						action.InsertNewBinding(i + 1);
						collectionChanged = true;
					}
					else if(res == CollectionAction.Remove)
					{
						action.DeleteBinding(i--);
						collectionChanged = true;
					}
					else if(res == CollectionAction.MoveUp)
					{
						action.SwapBindings(i, i - 1);
						collectionChanged = true;
					}
					else if(res == CollectionAction.MoveDown)
					{
						action.SwapBindings(i, i + 1);
						collectionChanged = true;
					}
				}
			}
			else
			{
				Rect buttonRect = new Rect(contentWidth / 2 - 125.0f, itemPosY + INPUT_ACTION_SPACING, 250.0f, BUTTON_HEIGHT);
				if(GUI.Button(buttonRect, "Add New Binding"))
				{
					action.CreateNewBinding();
					collectionChanged = true;
				}
			}

			GUI.EndScrollView();

			if(collectionChanged)
			{
				Repaint();
			}
		}

		private CollectionAction DrawInputBindingFields(Rect position, string label, InputAction action, int bindingIndex)
		{
			Rect headerRect = new Rect(position.x + 5.0f, position.y, position.width, INPUT_FIELD_HEIGHT);
			Rect removeButtonRect = new Rect(position.width - 25.0f, position.y + 2, 20.0f, 20.0f);
			Rect addButtonRect = new Rect(removeButtonRect.x - 20.0f, position.y + 2, 20.0f, 20.0f);
			Rect downButtonRect = new Rect(addButtonRect.x - 20.0f, position.y + 2, 20.0f, 20.0f);
			Rect upButtonRect = new Rect(downButtonRect.x - 20.0f, position.y + 2, 20.0f, 20.0f);
			Rect layoutArea = new Rect(position.x + 10.0f, position.y + INPUT_FIELD_HEIGHT + FIELD_SPACING + 5.0f, position.width - 12.5f, position.height - (INPUT_FIELD_HEIGHT + FIELD_SPACING + 5.0f));
			InputBinding binding = action.Bindings[bindingIndex];
			KeyCode positive = binding.Positive, negative = binding.Negative;
			CollectionAction result = CollectionAction.None;

			//GUI.Box(position, "");
			EditorGUI.LabelField(headerRect, label, EditorStyles.boldLabel);
			
			GUILayout.BeginArea(layoutArea);
			binding.Type = (InputType)EditorGUILayout.EnumPopup("Type", binding.Type);

			if(binding.Type == InputType.Button || binding.Type == InputType.DigitalAxis)
				DrawKeyCodeField(action, bindingIndex, KeyType.Positive);

			if(binding.Type == InputType.DigitalAxis)
				DrawKeyCodeField(action, bindingIndex, KeyType.Negative);

			if(binding.Type == InputType.AnalogAxis || binding.Type == InputType.AnalogButton ||
				binding.Type == InputType.MouseAxis)
			{
				binding.Axis = EditorGUILayout.Popup("Axis", binding.Axis, m_axisOptions);
			}

			if(binding.Type == InputType.AnalogAxis || binding.Type == InputType.AnalogButton)
				binding.Joystick = EditorGUILayout.Popup("Joystick", binding.Joystick, m_joystickOptions);

			if(binding.Type == InputType.GamepadButton)
				binding.GamepadButton = (GamepadButton)EditorGUILayout.EnumPopup("Button", binding.GamepadButton);

			if(binding.Type == InputType.GamepadAnalogButton || binding.Type == InputType.GamepadAxis)
				binding.GamepadAxis = (GamepadAxis)EditorGUILayout.EnumPopup("Axis", binding.GamepadAxis);

			if(binding.Type == InputType.GamepadButton || binding.Type == InputType.GamepadAnalogButton ||
				binding.Type == InputType.GamepadAxis)
			{
				binding.GamepadIndex = (GamepadIndex)EditorGUILayout.EnumPopup("Gamepad", binding.GamepadIndex);
			}

			if(binding.Type == InputType.DigitalAxis)
				binding.Gravity = EditorGUILayout.FloatField(m_gravityInfo, binding.Gravity);

			if(binding.Type == InputType.DigitalAxis || binding.Type == InputType.AnalogAxis ||
				binding.Type == InputType.MouseAxis || binding.Type == InputType.GamepadAxis)
			{
				binding.Sensitivity = EditorGUILayout.FloatField(m_sensitivityInfo, binding.Sensitivity);
			}

			if(binding.Type == InputType.AnalogAxis || binding.Type == InputType.GamepadAxis ||
				binding.Type == InputType.AnalogButton || binding.Type == InputType.GamepadAnalogButton)
			{
				binding.DeadZone = EditorGUILayout.FloatField(m_deadZoneInfo, binding.DeadZone);
			}

			if(binding.Type == InputType.DigitalAxis)
				binding.Snap = EditorGUILayout.Toggle(m_snapInfo, binding.Snap);

			if(binding.Type == InputType.DigitalAxis || binding.Type == InputType.AnalogAxis ||
				binding.Type == InputType.MouseAxis || binding.Type == InputType.RemoteAxis ||
				binding.Type == InputType.AnalogButton || binding.Type == InputType.GamepadAnalogButton ||
				binding.Type == InputType.GamepadAxis)
			{
				binding.Invert = EditorGUILayout.Toggle("Invert", binding.Invert);
			}

			if(binding.Type == InputType.Button && (Event.current == null || Event.current.type != EventType.KeyUp))
			{
				if(IsGenericJoystickButton(binding.Positive))
				{
					GUILayout.Space(JOYSTICK_WARNING_SPACING);
					DrawGenericJoystickButtonWarning(binding.Positive, binding.Joystick);
				}
			}

			GUILayout.EndArea();

			if(action.Bindings.Count < InputAction.MAX_BINDINGS)
			{
				if(GUI.Button(addButtonRect, m_plusButtonContent, EditorStyles.label))
				{
					result = CollectionAction.Add;
				}
			}

			if(GUI.Button(removeButtonRect, m_minusButtonContent, EditorStyles.label))
			{
				result = CollectionAction.Remove;
			}

			if(GUI.Button(upButtonRect, m_upButtonContent, EditorStyles.label))
			{
				result = CollectionAction.MoveUp;
			}

			if(GUI.Button(downButtonRect, m_downButtonContent, EditorStyles.label))
			{
				result = CollectionAction.MoveDown;
			}

			return result;
		}

		private void DrawKeyCodeField(InputAction action, int bindingIndex, KeyType keyType)
		{
			InputBinding binding = action.Bindings[bindingIndex];
			int kfIndex = bindingIndex * 2;

			if(keyType == KeyType.Positive)
			{
				binding.Positive = m_keyFields[kfIndex].OnGUI("Positive", binding.Positive);
			}
			else
			{
				binding.Negative = m_keyFields[kfIndex + 1].OnGUI("Negative", binding.Negative);
			}
		}
		#endregion

		#region [Utility]
		private void CreateKeyFields()
		{
			m_keyFields = new KeyCodeField[InputAction.MAX_BINDINGS * 2];
			for(int i = 0; i < m_keyFields.Length; i++)
			{
				m_keyFields[i] = new KeyCodeField();
			}
		}

		private void ResetKeyFields()
		{
			for(int i = 0; i < m_keyFields.Length; i++)
			{
				m_keyFields[i].Reset();
			}
		}

		private void CreateHighlightTexture()
		{
			m_highlightTexture = new Texture2D(1, 1);
			m_highlightTexture.SetPixel(0, 0, HIGHLIGHT_COLOR);
			m_highlightTexture.Apply();
		}

		private void EnsureGUIStyles()
		{
			if(m_highlightTexture == null)
			{
				CreateHighlightTexture();
			}
			if(m_whiteLabel == null)
			{
				m_whiteLabel = new GUIStyle(EditorStyles.label);
				m_whiteLabel.normal.textColor = Color.white;
			}
			if(m_whiteFoldout == null)
			{
				m_whiteFoldout = new GUIStyle(EditorStyles.foldout);
				m_whiteFoldout.normal.textColor = Color.white;
				m_whiteFoldout.onNormal.textColor = Color.white;
				m_whiteFoldout.active.textColor = Color.white;
				m_whiteFoldout.onActive.textColor = Color.white;
				m_whiteFoldout.focused.textColor = Color.white;
				m_whiteFoldout.onFocused.textColor = Color.white;
			}
			if(m_warningLabel == null)
			{
				m_warningLabel = new GUIStyle(EditorStyles.largeLabel)
				{
					alignment = TextAnchor.MiddleCenter,
					fontStyle = FontStyle.Bold,
					fontSize = 14
				};
			}
		}

		private void ExportInputProfile()
		{
			string file = EditorUtility.SaveFilePanel("Export input profile", "", "profile.xml", "xml");
			if(string.IsNullOrEmpty(file))
				return;

			InputSaverXML inputSaver = new InputSaverXML(file);
			inputSaver.Save(m_inputManager.GetSaveData());
			if(file.StartsWith(Application.dataPath))
			{
				AssetDatabase.Refresh();
			}
		}

		private void ImportInputProfile()
		{
			string file = EditorUtility.OpenFilePanel("Import input profile", "", "xml");
			if(string.IsNullOrEmpty(file))
				return;

			bool replace = EditorUtility.DisplayDialog("Replace or Append", "Do you want to replace the current control schemes?", "Replace", "Append");
			if(replace)
			{
				InputLoaderXML inputLoader = new InputLoaderXML(file);
				m_inputManager.SetSaveData(inputLoader.Load());
				m_selection.Reset();
			}
			else
			{
				InputLoaderXML inputLoader = new InputLoaderXML(file);
				var saveData = inputLoader.Load();
				if(saveData.ControlSchemes != null && saveData.ControlSchemes.Count > 0)
				{
					foreach(var scheme in saveData.ControlSchemes)
					{
						m_inputManager.ControlSchemes.Add(scheme);
					}
				}
			}

			if(m_searchString.Length > 0)
			{
				UpdateSearchResults();
			}
			Repaint();
		}

		private void LoadInputProfileFromResource(string resourcePath)
		{
			if(m_inputManager.ControlSchemes.Count > 0)
			{
				bool cont = EditorUtility.DisplayDialog("Warning", "This operation will replace the current control schemes!\nDo you want to continue?", "Yes", "No");
				if(!cont) return;
			}

			TextAsset textAsset = Resources.Load<TextAsset>(resourcePath);
			if(textAsset != null)
			{
				using(System.IO.StringReader reader = new System.IO.StringReader(textAsset.text))
				{
					InputLoaderXML inputLoader = new InputLoaderXML(reader);
					m_inputManager.SetSaveData(inputLoader.Load());
					m_selection.Reset();
				}
			}
			else
			{
				EditorUtility.DisplayDialog("Error", "Failed to load input profile. The resource file might have been deleted or renamed.", "OK");
			}
		}

		private void TryToFindInputManagerInScene()
		{
			m_inputManager = UnityObject.FindObjectOfType<InputManager>();
			m_tryedToFindInputManagerInScene = true;
		}

		private void HandlePlayModeChanged(PlayModeStateChange state)
		{
			if(state == PlayModeStateChange.EnteredEditMode || state == PlayModeStateChange.EnteredPlayMode)
			{
				TryToFindInputManagerInScene();
			}
		}

		private float CalculateHierarchyViewRectHeight()
		{
			float height = 0.0f;
			foreach(var scheme in m_inputManager.ControlSchemes)
			{
				height += HIERARCHY_ITEM_HEIGHT;
				if(scheme.IsExpanded)
				{
					height += scheme.Actions.Count * HIERARCHY_ITEM_HEIGHT;
				}
			}

			return height;
		}

		private float CalculateSearchResultViewRectHeight()
		{
			float height = 0.0f;
			foreach(var result in m_searchResults)
			{
				height += HIERARCHY_ITEM_HEIGHT;
				if(m_inputManager.ControlSchemes[result.ControlScheme].IsExpanded)
				{
					height += result.Actions.Count * HIERARCHY_ITEM_HEIGHT;
				}
			}

			return height;
		}

		private float CalculateInputActionViewRectHeight(InputAction action)
		{
			float height = INPUT_FIELD_HEIGHT * 2 + FIELD_SPACING * 2 + INPUT_ACTION_SPACING;
			if(action.Bindings.Count > 0)
			{
				foreach(var binding in action.Bindings)
				{
					height += CalculateInputBindingViewRectHeight(binding) + INPUT_BINDING_SPACING;
				}

				height += 15.0f;
			}
			else
			{
				height += BUTTON_HEIGHT;
			}

			return height;
		}

		private float CalculateInputBindingViewRectHeight(InputBinding binding)
		{
			int numberOfFields = 12;
			switch(binding.Type)
			{
			case InputType.Button:
				numberOfFields = 1;
				break;
			case InputType.MouseAxis:
				numberOfFields = 3;
				break;
			case InputType.DigitalAxis:
				numberOfFields = 6;
				break;
			case InputType.RemoteButton:
				numberOfFields = 0;
				break;
			case InputType.RemoteAxis:
				numberOfFields = 1;
				break;
			case InputType.AnalogButton:
				numberOfFields = 4;
				break;
			case InputType.AnalogAxis:
				numberOfFields = 5;
				break;
			case InputType.GamepadButton:
				numberOfFields = 2;
				break;
			case InputType.GamepadAnalogButton:
				numberOfFields = 4;
				break;
			case InputType.GamepadAxis:
				numberOfFields = 5;
				break;
			}

			numberOfFields += 2;    //	Header and type

			float height = INPUT_FIELD_HEIGHT * numberOfFields + FIELD_SPACING * numberOfFields + 10.0f;
			if(binding.Type == InputType.Button && (Event.current == null || Event.current.type != EventType.KeyUp))
			{
				if(IsGenericJoystickButton(binding.Positive))
					height += JOYSTICK_WARNING_SPACING + JOYSTICK_WARNING_HEIGHT;
			}

			return height;
		}

		private float ValuePP(ref float height, float amount)
		{
			float value = height;
			height += amount;

			return value;
		}

		private bool IsGenericJoystickButton(KeyCode keyCode)
		{
			return (int)keyCode >= (int)KeyCode.JoystickButton0 && (int)keyCode <= (int)KeyCode.JoystickButton19;
		}

		private void DrawGenericJoystickButtonWarning(KeyCode button, int joystick)
		{
			if(joystick < 8)
			{
				KeyCode correctButton = (KeyCode)((int)KeyCode.Joystick1Button0 + joystick * 20 + ((int)button - (int)KeyCode.JoystickButton0));
				string warning = string.Format("'{0}' will receive input from all joysticks. Use '{1}' to receive input only from 'Joystick {2}'.", KeyCodeConverter.KeyToString(button), KeyCodeConverter.KeyToString(correctButton), joystick + 1);
				EditorGUILayout.HelpBox(warning, MessageType.Warning);
			}
			else
			{
				string warning = string.Format("'{0}' will receive input from all joysticks.", KeyCodeConverter.KeyToString(button));
				EditorGUILayout.HelpBox(warning, MessageType.Warning);
			}
		}
		#endregion

		#region [Static Interface]
		public static bool IsOpen { get; private set; }

		[MenuItem("Luminosity/Input Manager/Open Input Editor", false, 0)]
		public static void OpenWindow()
		{
			if(!IsOpen)
			{
				if(UnityObject.FindObjectOfType(typeof(InputManager)) == null)
				{
					bool create = EditorUtility.DisplayDialog("Warning", "There is no InputManager instance in the scene. Do you want to create one?", "Yes", "No");
					if(create)
					{
						GameObject gameObject = new GameObject("InputManager");
						gameObject.AddComponent<InputManager>();
					}
					else
					{
						return;
					}
				}
				var window = EditorWindow.GetWindow<InputEditor>("Input Editor");
				window.minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
			}
		}

		public static void OpenWindow(InputManager target)
		{
			if(!IsOpen)
			{
				var window = EditorWindow.GetWindow<InputEditor>("Input Editor");
				window.m_inputManager = target;
				window.minSize = new Vector2(MIN_WINDOW_WIDTH, MIN_WINDOW_HEIGHT);
			}
		}

		public static void CloseWindow()
		{
			if(IsOpen)
			{
				var window = EditorWindow.GetWindow<InputEditor>("Input Editor");
				window.Close();
			}
		}
		#endregion
	}
}
