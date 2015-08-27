using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using TeamUtility.IO;
using TeamUtilityEditor.IO.InputManager;

namespace TeamUtilityEditor.IO
{
	public class MappingImportWindow : EditorWindow
	{
		[SerializeField] private List<JoystickMapping> _mappings;
		[SerializeField] private AdvancedInputEditor _configurator;
		[SerializeField] private Texture2D _highlightTexture;
		[SerializeField] private Vector2 _mappingListScrollPos = Vector2.zero;
		[SerializeField] private string _searchString = string.Empty;

		private const int SELECTION_EMPTY = -1;
		
		private List<int> _searchResults;
		private GUIStyle _whiteLabel;
		private float _toolbarHeight = 18.0f;
		private float _searchFieldWidth = 150.0f;
		private int _selection = SELECTION_EMPTY;

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
			float importButtonHeight = 24.0f;
			Rect toolbarPosition = new Rect(0.0f, 0.0f, this.position.width, _toolbarHeight);
			Rect mappingListPosition = new Rect(0.0f, _toolbarHeight, this.position.width, this.position.height - (_toolbarHeight + importButtonHeight + 10.0f));
			Rect importButtonPosition = new Rect(this.position.width / 2 - 100.0f, this.position.height - importButtonHeight - 5.0f, 200.0f, importButtonHeight);

			DisplayMappingList(mappingListPosition);
			DisplayToolbar(toolbarPosition);

			GUI.enabled = _selection != SELECTION_EMPTY;
			if(GUI.Button(importButtonPosition, "Import"))
			{
				ImportSelectedMapping();
				Close();
			}
			GUI.enabled = true;
		}

		private void DisplayMappingList(Rect screenRect)
		{
			GUILayout.BeginArea(screenRect);
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

		private void ImportSelectedMapping()
		{
			if(_configurator == null)
			{
				EditorUtility.DisplayDialog("Error", "Unable to import joystick mapping. Did you close the advanced input editor?", "Close");
				return;
			}

			InputConfiguration inputConfig = new InputConfiguration(_mappings[_selection].Name.Replace(' ', '_'));
			foreach(AxisMapping am in _mappings[_selection])
			{
				if(am.ScanType == MappingWizard.ScanType.Button)
				{
					AxisConfiguration axisConfig = new AxisConfiguration(am.Name);
					axisConfig.type = InputType.Button;
					axisConfig.positive = am.Key;
					inputConfig.axes.Add(axisConfig);
				}
				else
				{
					if(am.JoystickAxis < 0 || am.JoystickAxis >= AxisConfiguration.MaxJoystickAxes)
					{
						Debug.LogError("Joystick axis is out of range. Cannot import axis configuration: " + am.Name);
						continue;
					}
					
					AxisConfiguration axisConfig = new AxisConfiguration(am.Name);
					axisConfig.type = InputType.AnalogAxis;
					axisConfig.axis = am.JoystickAxis;
					axisConfig.joystick = 0;
					axisConfig.deadZone = 0.0f;
					axisConfig.sensitivity = 1.0f;
					inputConfig.axes.Add(axisConfig);
				}
			}

			_configurator.AddInputConfiguration(inputConfig);
		}

		public static void Open(AdvancedInputEditor configurator)
		{
			var window = EditorWindow.GetWindow<MappingImportWindow>("Mapping Importer");
			window._configurator = configurator;
			window.minSize = new Vector2(300.0f, 150.0f);
		}
	}
}