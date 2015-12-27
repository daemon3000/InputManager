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
using System.IO;
using System.Xml;
using System.Collections.Generic;
using TeamUtility.IO;
using TeamUtilityEditor.IO.InputManager;

namespace TeamUtilityEditor.IO
{
	public class MappingExplorer : EditorWindow
	{
		[SerializeField] private List<JoystickMapping> _mappings;
		[SerializeField] private Texture2D _highlightTexture;
		[SerializeField] private Vector2 _mappingListScrollPos = Vector2.zero;
		[SerializeField] private Vector2 _mappingContentsScrollPos = Vector2.zero;
		[SerializeField] private float _mappingListWidth = 200.0f;
		[SerializeField] private string _searchString = string.Empty;

		private const int SELECTION_EMPTY = -1;

		private List<int> _searchResults;
		private GUIStyle _whiteLabel;
		private GUIStyle _bigLabel;
		private float _toolbarHeight = 18.0f;
		private float _searchFieldWidth = 150.0f;
		private float _minCursorRectWidth = 10.0f;
		private float _maxCursorRectWidth = 50.0f;
		private int _selection = SELECTION_EMPTY;
		private bool _isResizingMappingListWidth = false;
		private string[] _axisDisplayNames = new string[] { "X", "Y", "3rd(Scrollwheel)", "4th", "5th", "6th", "7th", "8th", "9th", "10th" };

		private void OnEnable()
		{
			if(_mappings == null)
			{
				_mappings = new List<JoystickMapping>();
				LoadMappings();
			}
			if(_highlightTexture == null)
			{
				CreateHighlightTexture();
			}
			if(_searchResults == null)
			{
				_searchResults = new List<int>();
			}
			if(_whiteLabel == null)
			{
				_whiteLabel = new GUIStyle(EditorStyles.label);
				_whiteLabel.normal.textColor = Color.white;
			}
			if(_bigLabel == null)
			{
				_bigLabel = new GUIStyle(EditorStyles.largeLabel);
			}
		}

		private void LoadMappings()
		{
			if(_mappings.Count > 0)
				_mappings.Clear();

			LoadBuiltInMappings();
			LoadExternalMappings();
		}

		private void LoadBuiltInMappings()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("joystick_mapping_index");
			if(textAsset == null) {
				Debug.LogError("Failed to load built-in joystick mappings. Index file is missing or corrupted.");
				return;
			}

			try {
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(textAsset.text);

				foreach(XmlNode item in doc.DocumentElement)
				{
					JoystickMapping mapping = new JoystickMapping();
					mapping.LoadFromResources(item.Attributes["path"].InnerText);
					if(mapping.AxisCount > 0)
					{
						_mappings.Add(mapping);
					}
					else
					{
						Debug.LogError("Failed to load mapping from Resources folder at path: " + item.Attributes["path"].InnerText);
					}
				}
			}
			catch(System.Exception ex) {
				Debug.LogException(ex);
				Debug.LogError("Failed to load built-in joystick mappings. File format is invalid.");
			}

			Resources.UnloadAsset(textAsset);
		}

		private void LoadExternalMappings()
		{
			string folder = Application.dataPath.Substring(0, Application.dataPath.Length - 6) + "JoystickMappings/";
			if(!Directory.Exists(folder))
				return;

			string[] files = Directory.GetFiles(folder, "*.xml");
			foreach(string file in files)
			{
				JoystickMapping mapping = new JoystickMapping();
				mapping.Load(file);
				if(mapping.AxisCount > 0)
				{
					_mappings.Add(mapping);
				}
				else
				{
					Debug.LogError("Failed to load mapping from: " + file);
				}
			}
		}

		private void CreateHighlightTexture()
		{
			_highlightTexture = new Texture2D(1, 1);
			_highlightTexture.SetPixel(0, 0, new Color32(50, 125, 255, 255));
			_highlightTexture.Apply();
		}

		private void OnDestroy()
		{
			Texture2D.DestroyImmediate(_highlightTexture);
			_highlightTexture = null;
		}

		private void OnGUI()
		{
			Rect toolbarPosition = new Rect(0.0f, 0.0f, this.position.width, _toolbarHeight);
			Rect mappingListPosition = new Rect(0.0f, _toolbarHeight, _mappingListWidth, this.position.height - _toolbarHeight);
			Rect mappingContentsPosition = new Rect(_mappingListWidth, _toolbarHeight, this.position.width - _mappingListWidth, this.position.height - _toolbarHeight);

			UpdateMappingListWidth();
			DisplaySelectedMappingContent(mappingContentsPosition);
			DisplayMappingList(mappingListPosition);
			DisplayToolbar(toolbarPosition);
		}

		private void DisplayToolbar(Rect screenRect)
		{
			Rect searchFieldRect = new Rect(screenRect.width - (_searchFieldWidth + 5.0f), 2.0f, _searchFieldWidth, screenRect.height - 2.0f);
			int lastSearchStringLength = _searchString.Length;

			EditorGUI.LabelField(screenRect, "", EditorStyles.toolbarButton);

			GUILayout.BeginArea(searchFieldRect);
			_searchString = EditorToolbox.SearchField(_searchString);
			GUILayout.EndArea();
			
			if(lastSearchStringLength != _searchString.Length)
			{
				UpdateSearchResults();
			}
		}

		private void UpdateMappingListWidth()
		{
			float cursorRectWidth = _isResizingMappingListWidth ? _maxCursorRectWidth : _minCursorRectWidth;
			Rect cursorRect = new Rect(_mappingListWidth - cursorRectWidth / 2, _toolbarHeight, cursorRectWidth, 
			                           position.height - _toolbarHeight);
			Rect resizeRect = new Rect(_mappingListWidth - _minCursorRectWidth / 2, 0.0f, 
			                           _minCursorRectWidth, position.height);
			
			EditorGUIUtility.AddCursorRect(cursorRect, MouseCursor.ResizeHorizontal);
			switch(Event.current.type)
			{
			case EventType.MouseDown:
				if(Event.current.button == 0 && resizeRect.Contains(Event.current.mousePosition))
				{
					_isResizingMappingListWidth = true;
					Event.current.Use();
				}
				break;
			case EventType.MouseUp:
				if(Event.current.button == 0 && _isResizingMappingListWidth)
				{
					_isResizingMappingListWidth = false;
					Event.current.Use();
				}
				break;
			case EventType.MouseDrag:
				if(_isResizingMappingListWidth)
				{
					_mappingListWidth = Mathf.Clamp(_mappingListWidth + Event.current.delta.x, 200.0f, position.width / 2);
					Event.current.Use();
					Repaint();
				}
				break;
			default:
				break;
			}
		}

		private void DisplayMappingList(Rect screenRect)
		{
			Rect bgPosition = new Rect(0.0f, _toolbarHeight - 5.0f, _mappingListWidth, position.height - _toolbarHeight + 10.0f);
			GUI.Box(bgPosition, "");

			Rect scrollView = new Rect(screenRect.x, screenRect.y, screenRect.width, position.height - screenRect.y);
			GUILayout.BeginArea(scrollView);
			_mappingListScrollPos = EditorGUILayout.BeginScrollView(_mappingListScrollPos);
			GUILayout.Space(5.0f);
			if(_searchString.Length > 0)
			{
				for(int i = 0; i < _searchResults.Count; i++)
				{
					DisplayMapping(screenRect, _searchResults[i]);
				}
			}
			else
			{
				for(int i = 0; i < _mappings.Count; i++)
				{
					DisplayMapping(screenRect, i);
				}
			}
			GUILayout.Space(5.0f);
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		private void DisplayMapping(Rect screenRect, int index)
		{
			Rect configPos = GUILayoutUtility.GetRect(new GUIContent(name), EditorStyles.label, GUILayout.Height(15.0f));
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				if(configPos.Contains(Event.current.mousePosition))
				{
					_selection = index;
					_mappingContentsScrollPos = Vector2.zero;
					Repaint();
				}
			}
			
			if(_selection == index)
			{
				if(_highlightTexture == null) {
					CreateHighlightTexture();
				}
				GUI.DrawTexture(configPos, _highlightTexture, ScaleMode.StretchToFill);
				EditorGUI.LabelField(configPos, _mappings[index].Name, _whiteLabel);
			}
			else
			{
				EditorGUI.LabelField(configPos, _mappings[index].Name);
			}
		}

		private void DisplaySelectedMappingContent(Rect screenRect)
		{
			if(_selection == SELECTION_EMPTY)
				return;

			Rect scrollView = new Rect(screenRect.x, screenRect.y, screenRect.width, position.height - screenRect.y);
			GUILayout.BeginArea(scrollView);
			_mappingContentsScrollPos = EditorGUILayout.BeginScrollView(_mappingContentsScrollPos);
			GUILayout.Space(5.0f);

			foreach(AxisMapping axisMapping in _mappings[_selection])
			{
				if(axisMapping.ScanType == MappingWizard.ScanType.Button)
				{
					EditorGUILayout.SelectableLabel(string.Format("{0} - {1}", axisMapping.Name, axisMapping.Key), _bigLabel);
				}
				else
				{
					EditorGUILayout.SelectableLabel(string.Format("{0} - {1}", axisMapping.Name, _axisDisplayNames[axisMapping.JoystickAxis]), _bigLabel);
				}
			}

			GUILayout.Space(5.0f);
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		private void UpdateSearchResults()
		{
			_searchResults.Clear();
			for(int i = 0; i < _mappings.Count; i++)
			{
				if(_mappings[i].Name.IndexOf(_searchString, System.StringComparison.InvariantCultureIgnoreCase) >= 0)
				{
					_searchResults.Add(i);
				}
			}

			_selection = SELECTION_EMPTY;
		}

		[MenuItem("Team Utility/Input Manager/View Joystick Mappings", false, 1)]
		public static void Open()
		{
			EditorWindow.GetWindow<MappingExplorer>("Mapping Explorer");
		}
	}
}
