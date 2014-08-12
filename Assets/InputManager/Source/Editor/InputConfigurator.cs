#region [Copyright (c) 2014 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2014 Cristian Alexandru Geambasu
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using TeamUtility.Editor.IO.InputManager;

namespace TeamUtility.Editor
{
	public sealed class InputConfigurator : EditorWindow
	{
		#region [SearchResult]
		[Serializable]
		private class SearchResult
		{
			public int configuration;
			public List<int> axes;
			
			public SearchResult()
			{
				configuration = 0;
				axes = new List<int>();
			}
			
			public SearchResult(int configuration, IEnumerable<int> axes)
			{
				this.configuration = configuration;
				this.axes = new List<int>(axes);
			}
		}
		#endregion
		
		#region [Fields]
		[SerializeField] private InputManager _inputManager;
		[SerializeField] private List<SearchResult> _searchResults;
		[SerializeField] private List<bool> _configurationFoldouts;
		[SerializeField] private List<int> _selectionPath;
		[SerializeField] private Vector2 _hierarchiScrollPos = Vector2.zero;
		[SerializeField] private Vector2 _mainPanelScrollPos = Vector2.zero;
		[SerializeField] private float _hierarchyPanelWidth = _menuWidth * 2;
		[SerializeField] private Texture2D _highlightTexture;
		[SerializeField] private string _searchString = "";
		[SerializeField] private string _keyString = string.Empty;
		
		private GUIStyle _whiteLabel;
		private GUIStyle _whiteFoldout;
		private float _minCursorRectWidth = 10.0f;
		private float _maxCursorRectWidth = 50.0f;
		private float _toolbarHeight = 18.0f;
		private bool _isResizingHierarchy;
		private bool _editingPositiveKey = false;
		private bool _editingAltPositiveKey = false;
		private bool _editingNegativeKey = false;
		private bool _editingAltNegativeKey = false;
		private string[] _axisOptions = new string[] { "X", "Y", "3rd(Scrollwheel)", "4th", "5th", "6th", "7th", "8th", "9th", "10th" };
		private string[] _joystickOptions = new string[] { "Joystick 1", "Joystick 2", "Joystick 3", "Joystick 4" };
		
		private const float _menuWidth = 100.0f;
		private const float _minHierarchyPanelWidth = 150.0f;
		#endregion
		
		private void OnEnable()
		{
			EditorToolbox.ShowStartupWarning();
			CreateGUIStyles();
			IsOpen = true;
			
			if(_inputManager == null)
			{
				_inputManager = UnityEngine.Object.FindObjectOfType(typeof(InputManager)) as InputManager;
			}
			if(_configurationFoldouts == null)
			{
				_configurationFoldouts = new List<bool>();
				UpdateFoldouts();
			}
			if(_selectionPath == null)
			{
				_selectionPath = new List<int>();
			}
			if(_highlightTexture == null)
			{
				CreateHighlightTexture();
			}
			if(_searchResults == null)
			{
				_searchResults = new List<SearchResult>();
			}
		}

		private void UpdateFoldouts()
		{
			_configurationFoldouts.Clear();
			for(int i = 0; i < _inputManager.inputConfigurations.Count; i++)
			{
				_configurationFoldouts.Add(false);
			}
		}
		
		private void CreateHighlightTexture()
		{
			_highlightTexture = new Texture2D(1, 1);
			_highlightTexture.SetPixel(0, 0, new Color32(50, 125, 255, 255));
			_highlightTexture.Apply();
		}
		
		private void CreateGUIStyles()
		{
			if(_whiteLabel == null)
			{
				_whiteLabel = new GUIStyle(EditorStyles.label);
				_whiteLabel.normal.textColor = Color.white;
			}
			if(_whiteFoldout == null)
			{
				_whiteFoldout = new GUIStyle(EditorStyles.foldout);
				_whiteFoldout.normal.textColor = Color.white;
				_whiteFoldout.onNormal.textColor = Color.white;
				_whiteFoldout.active.textColor = Color.white;
				_whiteFoldout.onActive.textColor = Color.white;
				_whiteFoldout.focused.textColor = Color.white;
				_whiteFoldout.onFocused.textColor = Color.white;
			}
		}
		
		private void OnDestroy()
		{
			IsOpen = false;
			UnityEngine.Object.DestroyImmediate(_highlightTexture);
			_highlightTexture = null;
		}
		
		#region [Menus]
		private void CreateFileMenu(Rect position)
		{
			GenericMenu fileMenu = new GenericMenu();
			fileMenu.AddItem(new GUIContent("Overwrite Input Settings"), false, HandleFileMenuOption, 0);
			fileMenu.AddSeparator("");
			if(_inputManager.inputConfigurations.Count > 0)
			{
				fileMenu.AddItem(new GUIContent("Create Snapshot"), false, HandleFileMenuOption, 1);
			}
			else
			{
				fileMenu.AddDisabledItem(new GUIContent("Create Snapshot"));
			}
			if(EditorToolbox.CanLoadSnapshot())
			{
				fileMenu.AddItem(new GUIContent("Load Snapshot"), false, HandleFileMenuOption, 2);
			}
			else
			{
				fileMenu.AddDisabledItem(new GUIContent("Load Snapshot"));
			}
			fileMenu.AddSeparator("");
			
			fileMenu.AddItem(new GUIContent("Forum"), false, HandleFileMenuOption, 3);
			fileMenu.AddItem(new GUIContent("About"), false, HandleFileMenuOption, 4);
			fileMenu.DropDown(position);
		}
		
		private void HandleFileMenuOption(object arg)
		{
			int option = (int)arg;
			switch(option)
			{
			case 0:
				EditorToolbox.OverwriteInputSettings();
				break;
			case 1:
				EditorToolbox.CreateSnapshot(_inputManager);
				break;
			case 2:
				EditorToolbox.LoadSnapshot(_inputManager);
				break;
			case 3:
				MenuCommands.OpenForumPage();
				break;
			case 4:
				MenuCommands.OpenAboutDialog();
				break;
			}
		}
		
		private void CreateEditMenu(Rect position)
		{
			GenericMenu editMenu = new GenericMenu();
			editMenu.AddItem(new GUIContent("New Configuration"), false, HandleEditMenuOption, 0); 
			if(_selectionPath.Count >= 1)
			{
				editMenu.AddItem(new GUIContent("New Axis"), false, HandleEditMenuOption, 1);
			}
			else
			{
				editMenu.AddDisabledItem(new GUIContent("New Axis"));
			}
			editMenu.AddSeparator("");
			
			if(_selectionPath.Count > 0)
			{
				editMenu.AddItem(new GUIContent("Duplicate          Shift+D"), false, HandleEditMenuOption, 2);
			}
			else
			{
				editMenu.AddDisabledItem(new GUIContent("Duplicate          Shift+D"));
			}
			if(_selectionPath.Count > 0)
			{
				editMenu.AddItem(new GUIContent("Delete                Del"), false, HandleEditMenuOption, 3);
			}
			else
			{
				editMenu.AddDisabledItem(new GUIContent("Delete                Del"));
			}
			editMenu.AddSeparator("");
			
			editMenu.AddItem(new GUIContent("Select Target"), false, HandleEditMenuOption, 4);
			editMenu.AddItem(new GUIContent("Dont Destroy On Load"), _inputManager.dontDestroyOnLoad, HandleEditMenuOption, 5);
			editMenu.DropDown(position);
		}
		
		private void HandleEditMenuOption(object arg)
		{
			int option = (int)arg;
			switch(option)
			{
			case 0:
				CreateNewInputConfiguration();
				break;
			case 1:
				CreateNewAxisConfiguration();
				break;
			case 2:
				Duplicate();
				break;
			case 3:
				Delete();
				break;
			case 4:
				Selection.activeGameObject = _inputManager.gameObject;
				break;
			case 5:
				_inputManager.dontDestroyOnLoad = !_inputManager.dontDestroyOnLoad;
				break;
			}
		}
		
		private void CreateNewInputConfiguration()
		{
			_inputManager.inputConfigurations.Add(new InputConfiguration());
			_configurationFoldouts.Add(false);
			_selectionPath.Clear();
			_selectionPath.Add(_inputManager.inputConfigurations.Count - 1);
			Repaint();
		}
		
		private void CreateNewAxisConfiguration()
		{
			if(_selectionPath.Count >= 1)
			{
				InputConfiguration inputConfig = _inputManager.inputConfigurations[_selectionPath[0]];
				inputConfig.axes.Add(new AxisConfiguration());
				_configurationFoldouts[_selectionPath[0]] = true;
				if(_selectionPath.Count == 2) {
					_selectionPath[1] = inputConfig.axes.Count - 1;
				}
				else {
					_selectionPath.Add(inputConfig.axes.Count - 1);
				}
				Repaint();
			}
		}
		
		private void Duplicate()
		{
			if(_selectionPath.Count == 1) {
				DuplicateInputConfiguration();
			}
			else if(_selectionPath.Count == 2) {
				DuplicateAxisConfiguration();
			}
		}
		
		private void DuplicateAxisConfiguration()
		{
			InputConfiguration inputConfig = _inputManager.inputConfigurations[_selectionPath[0]];
			AxisConfiguration source = inputConfig.axes[_selectionPath[1]];
			AxisConfiguration axisConfig = AxisConfiguration.Duplicate(source);
			if(_selectionPath[1] < inputConfig.axes.Count - 1)
			{
				inputConfig.axes.Insert(_selectionPath[1], axisConfig);
				_selectionPath[1]++;
			}
			else
			{
				inputConfig.axes.Add(axisConfig);
				_selectionPath[1] = inputConfig.axes.Count - 1;
			}
			Repaint();
		}
		
		private void DuplicateInputConfiguration()
		{
			InputConfiguration source = _inputManager.inputConfigurations[_selectionPath[0]];
			InputConfiguration inputConfig = InputConfiguration.Duplicate(source);
			if(_selectionPath[0] < _inputManager.inputConfigurations.Count - 1)
			{
				_inputManager.inputConfigurations.Insert(_selectionPath[0] + 1, inputConfig);
			_configurationFoldouts.Insert(_selectionPath[0] + 1, false);
				
				_selectionPath[0]++;
			}
			else
			{
				_inputManager.inputConfigurations.Add(inputConfig);
				_configurationFoldouts.Add(false);
				_selectionPath[0] = _inputManager.inputConfigurations.Count - 1;
			}
			Repaint();
		}
		
		private void Delete()
		{
			if(_selectionPath.Count == 1)
			{
				_inputManager.inputConfigurations.RemoveAt(_selectionPath[0]);
				_configurationFoldouts.RemoveAt(_selectionPath[0]);
				Repaint();
			}
			else if(_selectionPath.Count == 2)
			{
				_inputManager.inputConfigurations[_selectionPath[0]].axes.RemoveAt(_selectionPath[1]);
				Repaint();
			}
			_selectionPath.Clear();
		}
		
		
		
		private void UpdateSearchResults()
		{
			_searchResults.Clear();
			
			for(int i = 0; i < _inputManager.inputConfigurations.Count; i++)
			{
				IEnumerable<int> axes = from a in _inputManager.inputConfigurations[i].axes
										where a.name.StartsWith(_searchString, StringComparison.InvariantCultureIgnoreCase)
										select _inputManager.inputConfigurations[i].axes.IndexOf(a);
				
				if(axes.Count() > 0)
				{
					_searchResults.Add(new SearchResult(i, axes));
				}
			}
		}
		#endregion
		
		#region [OnGUI]
		private void OnGUI()
		{
			if(_inputManager == null)
				return;
			
			if(_configurationFoldouts.Count != _inputManager.inputConfigurations.Count)
			{
				UpdateFoldouts();
			}
			
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			Undo.SetSnapshotTarget(_inputManager, "InputManager");
			Undo.CreateSnapshot();
#else
			Undo.RecordObject(_inputManager, "InputManager");
#endif
			UpdateHierarchyPanelWidth();
			if(_searchString.Length > 0)
			{
				DisplaySearchResults();
			}
			else
			{
				DisplayHierarchyPanel();
			}
			if(_selectionPath.Count >= 1)
			{
				DisplayMainPanel();
			}
			DisplayMainToolbar();
#if UNITY_3_4 || UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
			if(GUI.changed)
			{
				Undo.RegisterSnapshot();
				EditorUtility.SetDirty(_inputManager);
			}
			Undo.ClearSnapshotTarget();
#else
			if(GUI.changed)
			{
				EditorUtility.SetDirty(_inputManager);
			}
#endif
			Repaint();
		}
		
		private void DisplayMainToolbar()
		{
			Rect screenRect = new Rect(0.0f, 0.0f, position.width, _toolbarHeight);
			Rect fileMenuRect = new Rect(0.0f, 0.0f, _menuWidth, screenRect.height);
			Rect editMenuRect = new Rect(fileMenuRect.xMax, 0.0f, _menuWidth, screenRect.height);
			Rect paddingLabelRect = new Rect(editMenuRect.xMax, 0.0f, screenRect.width - _menuWidth * 2, screenRect.height);
			Rect searchFieldRect = new Rect(screenRect.width - (_menuWidth * 1.5f + 5.0f), 2.0f, _menuWidth * 1.5f, screenRect.height - 2.0f);
			int lastSearchStringLength = _searchString.Length;
			
			GUI.BeginGroup(screenRect);
			DisplayFileMenu(fileMenuRect);
			DisplayEditMenu(editMenuRect);
			EditorGUI.LabelField(paddingLabelRect, "", EditorStyles.toolbarButton);
			
			GUILayout.BeginArea(searchFieldRect);
			_searchString = EditorToolbox.SearchField(_searchString);
			GUILayout.EndArea();
			
			GUI.EndGroup();
			
			if(lastSearchStringLength != _searchString.Length)
			{
				UpdateSearchResults();
			}
		}
		
		private void DisplayFileMenu(Rect screenRect)
		{
			EditorGUI.LabelField(screenRect, "File", EditorStyles.toolbarDropDown);
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && 
				screenRect.Contains(Event.current.mousePosition))
			{
				CreateFileMenu(new Rect(screenRect.x, screenRect.yMax, 0.0f, 0.0f));
			}
			
			if(Event.current.type == EventType.KeyDown)
			{
				if(Event.current.keyCode == KeyCode.Q && (Event.current.control || Event.current.command))
				{
					Close();
					Event.current.Use();
				}
			}
		}
		
		private void DisplayEditMenu(Rect screenRect)
		{
			EditorGUI.LabelField(screenRect, "Edit", EditorStyles.toolbarDropDown);
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && 
				screenRect.Contains(Event.current.mousePosition))
			{
				CreateEditMenu(new Rect(screenRect.x, screenRect.yMax, 0.0f, 0.0f));
			}
			
			if(Event.current.type == EventType.KeyDown)
			{
				if(Event.current.keyCode == KeyCode.D && Event.current.shift)
				{
					Duplicate();
					Event.current.Use();
				}
				if(Event.current.keyCode == KeyCode.Delete)
				{
					Delete();
					Event.current.Use();
				}
			}
		}
		
		private void UpdateHierarchyPanelWidth()
		{
			float cursorRectWidth = _isResizingHierarchy ? _maxCursorRectWidth : _minCursorRectWidth;
			Rect cursorRect = new Rect(_hierarchyPanelWidth - cursorRectWidth / 2, _toolbarHeight, cursorRectWidth, 
										position.height - _toolbarHeight);
			Rect resizeRect = new Rect(_hierarchyPanelWidth - _minCursorRectWidth / 2, 0.0f, 
										_minCursorRectWidth, position.height);
			
			EditorGUIUtility.AddCursorRect(cursorRect, MouseCursor.ResizeHorizontal);
			switch(Event.current.type)
			{
			case EventType.MouseDown:
				if(Event.current.button == 0 && resizeRect.Contains(Event.current.mousePosition))
				{
					_isResizingHierarchy = true;
					Event.current.Use();
				}
				break;
			case EventType.MouseUp:
				if(Event.current.button == 0 && _isResizingHierarchy)
				{
					_isResizingHierarchy = false;
					Event.current.Use();
				}
				break;
			case EventType.MouseDrag:
				if(_isResizingHierarchy)
				{
					_hierarchyPanelWidth = Mathf.Clamp(_hierarchyPanelWidth + Event.current.delta.x, 
													 _minHierarchyPanelWidth, position.width / 2);
					Event.current.Use();
					Repaint();
				}
				break;
			default:
				break;
			}
		}
		
		private void DisplaySearchResults()
		{
			Rect screenRect = new Rect(0.0f, _toolbarHeight - 5.0f, _hierarchyPanelWidth, position.height - _toolbarHeight + 10.0f);
			GUI.Box(screenRect, "");
			
			if(_searchResults.Count > 0)
			{
				Rect scrollView = new Rect(screenRect.x, screenRect.y + 5.0f, screenRect.width, position.height - screenRect.y);
			
				GUILayout.BeginArea(scrollView);
				_hierarchiScrollPos = EditorGUILayout.BeginScrollView(_hierarchiScrollPos);
				GUILayout.Space(5.0f);
				for(int i = 0; i < _searchResults.Count; i++)
				{
					DisplaySearchResult(screenRect, _searchResults[i]);
				}
				GUILayout.Space(5.0f);
				EditorGUILayout.EndScrollView();
				GUILayout.EndArea();
			}
		}
		
		private void DisplaySearchResult(Rect screenRect, SearchResult result)
		{
			DisplayHierarchyInputConfigItem(screenRect, result.configuration, 
											_inputManager.inputConfigurations[result.configuration].name);
				
			if(_configurationFoldouts[result.configuration])
			{
				for(int i = 0; i < result.axes.Count; i++)
				{
					DisplayHierarchiAxisConfigItem(screenRect, result.configuration, result.axes[i], 
												  _inputManager.inputConfigurations[result.configuration].axes[result.axes[i]].name);
				}
			}
		}
		
		private void DisplayHierarchyPanel()
		{
			Rect screenRect = new Rect(0.0f, _toolbarHeight - 5.0f, _hierarchyPanelWidth, position.height - _toolbarHeight + 10.0f);
			Rect scrollView = new Rect(screenRect.x, screenRect.y + 5.0f, screenRect.width, position.height - screenRect.y);
			
			GUI.Box(screenRect, "");
			GUILayout.BeginArea(scrollView);
			_hierarchiScrollPos = EditorGUILayout.BeginScrollView(_hierarchiScrollPos);
			GUILayout.Space(5.0f);
			for(int i = 0; i < _inputManager.inputConfigurations.Count; i++)
			{
				DisplayHierarchyInputConfigItem(screenRect, i, _inputManager.inputConfigurations[i].name);
				
				if(_configurationFoldouts[i])
				{
					for(int j = 0; j < _inputManager.inputConfigurations[i].axes.Count; j++)
					{
						DisplayHierarchiAxisConfigItem(screenRect, i, j, _inputManager.inputConfigurations[i].axes[j].name);
					}
				}
			}
			GUILayout.Space(5.0f);
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
		}
		
		private void DisplayHierarchyInputConfigItem(Rect screenRect, int index, string name)
		{
			Rect configPos = GUILayoutUtility.GetRect(new GUIContent(name), EditorStyles.foldout, GUILayout.Height(15.0f));
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				if(configPos.Contains(Event.current.mousePosition))
				{
					_selectionPath.Clear();
					_selectionPath.Add(index);
					Repaint();
				}
				else if(screenRect.Contains(Event.current.mousePosition))
				{
					_selectionPath.Clear();
					Repaint();
				}
			}
			
			if(_selectionPath.Count == 1 && _selectionPath[0] == index)
			{
				if(_highlightTexture == null) {
					CreateHighlightTexture();
				}
				GUI.DrawTexture(configPos, _highlightTexture, ScaleMode.StretchToFill);
				_configurationFoldouts[index] = EditorGUI.Foldout(configPos, _configurationFoldouts[index], name, _whiteFoldout);
			}
			else
			{
				_configurationFoldouts[index] = EditorGUI.Foldout(configPos, _configurationFoldouts[index], name);
			}
		}
		
		private void DisplayHierarchiAxisConfigItem(Rect screenRect, int inputConfigIndex, int index, string name)
		{
			Rect configPos = GUILayoutUtility.GetRect(new GUIContent(name), EditorStyles.label, GUILayout.Height(15.0f));
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				if(configPos.Contains(Event.current.mousePosition))
				{
					_editingPositiveKey = false;
					_editingPositiveKey = false;
					_editingAltPositiveKey = false;
					_editingAltNegativeKey = false;
					_keyString = string.Empty;
					_selectionPath.Clear();
					_selectionPath.Add(inputConfigIndex);
					_selectionPath.Add(index);
					Event.current.Use();
					Repaint();
				}
				else if(screenRect.Contains(Event.current.mousePosition))
				{
					_selectionPath.Clear();
					Repaint();
				}
			}
			
			if(_selectionPath.Count == 2 && _selectionPath[0] == inputConfigIndex &&
				_selectionPath[1] == index)
			{
				if(_highlightTexture == null) {
					CreateHighlightTexture();
				}
				GUI.DrawTexture(configPos, _highlightTexture, ScaleMode.StretchToFill);
				
				configPos.x += 20.0f;
				EditorGUI.LabelField(configPos, name, _whiteLabel);
			}
			else
			{
				configPos.x += 20.0f;
				EditorGUI.LabelField(configPos, name);
			} 
		}
		
		private void DisplayMainPanel()
		{
			Rect screenRect = new Rect(_hierarchyPanelWidth + 5.0f, _toolbarHeight + 5, 
										position.width - (_hierarchyPanelWidth + 5.0f), 
										position.height - _toolbarHeight - 5.0f);
			InputConfiguration inputConfig = _inputManager.inputConfigurations[_selectionPath[0]];
			
			if(_selectionPath.Count < 2)
			{
				DisplayInputConfigurationFields(inputConfig, screenRect);
			}
			else
			{
				AxisConfiguration axisConfig = inputConfig.axes[_selectionPath[1]];
				DisplayAxisConfigurationFields(inputConfig, axisConfig, screenRect);
			}
		}
		
		private void DisplayInputConfigurationFields(InputConfiguration inputConfig, Rect screenRect)
		{
			GUILayout.BeginArea(screenRect);
			_mainPanelScrollPos = EditorGUILayout.BeginScrollView(_mainPanelScrollPos);
			inputConfig.name = EditorGUILayout.TextField("Name", inputConfig.name);
			EditorGUILayout.Space();
			EditorGUILayout.BeginHorizontal();
			GUI.enabled = (!EditorApplication.isPlaying && _inputManager.defaultConfiguration != inputConfig.name);
			if(GUILayout.Button("Make Default", GUILayout.Width(135.0f), GUILayout.Height(20.0f)))
			{
				_inputManager.defaultConfiguration = inputConfig.name;
			}
			GUI.enabled = (EditorApplication.isPlaying && InputManager.CurrentConfiguration.name != inputConfig.name);
			if(GUILayout.Button("Switch To", GUILayout.Width(135.0f), GUILayout.Height(20.0f)))
			{
				InputManager.SetConfiguration(inputConfig.name);
			}
			GUI.enabled = true;
			
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
		}
		
		private void DisplayAxisConfigurationFields(InputConfiguration inputConfig, AxisConfiguration axisConfig, Rect screenRect)
		{
			GUIContent gravityInfo = new GUIContent("Gravity", "The speed(in units/sec) at which a digital axis falls towards neutral.");
			GUIContent sensitivityInfo = new GUIContent("Sensitivity", "The speed(in units/sec) at which an axis moves towards the target value.");
			GUIContent snapInfo = new GUIContent("Snap", "If input switches direction, do we snap to neutral and continue from there? For digital axes only.");
			GUIContent deadZoneInfo = new GUIContent("Dead Zone", "Size of analog dead zone. Values within this range map to neutral.");
			
			GUILayout.BeginArea(screenRect);
			_mainPanelScrollPos = GUILayout.BeginScrollView(_mainPanelScrollPos);
			axisConfig.name = EditorGUILayout.TextField("Name", axisConfig.name);
			axisConfig.description = EditorGUILayout.TextField("Description", axisConfig.description);
			
			//	Positive Key
			EditorToolbox.KeyCodeField(ref _keyString, ref _editingPositiveKey, "Positive", 
									   "editor_positive_key", axisConfig.positive);
			ProcessKeyString(ref axisConfig.positive, ref _editingPositiveKey);
			//	Negative Key
			EditorToolbox.KeyCodeField(ref _keyString, ref _editingNegativeKey, "Negative", 
									   "editor_negative_key", axisConfig.negative);
			ProcessKeyString(ref axisConfig.negative, ref _editingNegativeKey);
			//	Alt Positive Key
			EditorToolbox.KeyCodeField(ref _keyString, ref _editingAltPositiveKey, "Alt Positive", 
									   "editor_alt_positive_key", axisConfig.altPositive);
			ProcessKeyString(ref axisConfig.altPositive, ref _editingAltPositiveKey);
			//	Alt Negative key
			EditorToolbox.KeyCodeField(ref _keyString, ref _editingAltNegativeKey, "Alt Negative", 
									   "editor_alt_negative_key", axisConfig.altNegative);
			ProcessKeyString(ref axisConfig.altNegative, ref _editingAltNegativeKey);
			
			axisConfig.gravity = EditorGUILayout.FloatField(gravityInfo, axisConfig.gravity);
			axisConfig.deadZone = EditorGUILayout.FloatField(deadZoneInfo, axisConfig.deadZone);
			axisConfig.sensitivity = EditorGUILayout.FloatField(sensitivityInfo, axisConfig.sensitivity);
			axisConfig.snap = EditorGUILayout.Toggle(snapInfo, axisConfig.snap);
			axisConfig.invert = EditorGUILayout.Toggle("Invert", axisConfig.invert);
			axisConfig.type = (InputType)EditorGUILayout.EnumPopup("Type", axisConfig.type);
			axisConfig.axis = EditorGUILayout.Popup("Axis", axisConfig.axis, _axisOptions);
			axisConfig.joystick = EditorGUILayout.Popup("Joystick", axisConfig.joystick, _joystickOptions);
			
			GUILayout.EndScrollView();
			GUILayout.EndArea();
		}
		
		private void ProcessKeyString(ref KeyCode key, ref bool isEditing)
		{
			if(isEditing && Event.current.type == EventType.KeyUp)
			{
				key = AxisConfiguration.StringToKey(_keyString);
				if(key == KeyCode.None)
				{
					_keyString = string.Empty;
				}
				else
				{
					_keyString = key.ToString();
				}
				isEditing = false;
			}
		}
		
		#endregion
		
		#region [Static Interface]
		public static bool IsOpen { get; private set; }
		
		[MenuItem("Team Utility/Input Manager/Open Advanced Editor", false, 0)]
		public static void OpenWindow()
		{
			if(!IsOpen)
			{
				if(UnityEngine.Object.FindObjectOfType(typeof(InputManager)) == null)
				{
					GameObject gameObject = new GameObject("InputManager");
					gameObject.AddComponent<InputManager>();
				}
				EditorWindow.GetWindow<InputConfigurator>("Input");
			}
		}
		
		public static void OpenWindow(InputManager target)
		{
			if(!IsOpen)
			{
				var window = EditorWindow.GetWindow<InputConfigurator>("Input");
				window._inputManager = target;
			}
		}
		
		public static void CloseWindow()
		{
			if(IsOpen)
			{
				var window = EditorWindow.GetWindow<InputConfigurator>("Input");
				window.Close();
			}
		}
		#endregion
	}
}
