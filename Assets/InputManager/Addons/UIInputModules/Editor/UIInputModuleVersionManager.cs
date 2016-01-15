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
using System.Xml;
using System.Collections.Generic;

namespace TeamUtilityEditor.IO
{
	public class UIInputModuleVersionManager : EditorWindow
	{
		[SerializeField] private Texture2D _highlightTexture;
		[SerializeField] private Vector2 _mappingListScrollPos = Vector2.zero;
		
		private List<string> _moduleNames;
		private List<string> _modulePaths;
		private GUIStyle _whiteLabel;
		private int _selection = SELECTION_EMPTY;
		private const int SELECTION_EMPTY = -1;

		private void OnEnable()
		{
			_selection = SELECTION_EMPTY;
			if(_highlightTexture == null)
			{
				CreateHighlightTexture();
			}
			if(_whiteLabel == null)
			{
				_whiteLabel = new GUIStyle(EditorStyles.label);
				_whiteLabel.normal.textColor = Color.white;
			}
			_moduleNames = new List<string>();
			_modulePaths = new List<string>();

			LoadInputModules();
		}

		private void CreateHighlightTexture()
		{
			_highlightTexture = new Texture2D(1, 1);
			_highlightTexture.SetPixel(0, 0, new Color32(50, 125, 255, 255));
			_highlightTexture.Apply();
		}

		private void LoadInputModules()
		{
			TextAsset textAsset = Resources.Load<TextAsset>("ui_input_module_index");
			if(textAsset == null) 
			{
				Debug.LogError("Failed to load UI input modules. Index file is missing or corrupted.");
				return;
			}
			
			try 
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(textAsset.text);
				foreach(XmlNode item in doc.DocumentElement)
				{
					_moduleNames.Add(item.Attributes["name"].InnerText);
					_modulePaths.Add(item.Attributes["path"].InnerText);
				}
			}
			catch(System.Exception ex) 
			{
				_moduleNames.Clear();
				_modulePaths.Clear();
				Debug.LogException(ex);
				Debug.LogError("Failed to load UI input modules. File format is invalid.");
			}
			
			Resources.UnloadAsset(textAsset);
		}

		private void OnDestroy()
		{
			Texture2D.DestroyImmediate(_highlightTexture);
			_highlightTexture = null;
		}

		private void OnGUI()
		{
			float importButtonHeight = 24.0f;
			Rect versionListPosition = new Rect(0.0f, 20.0f, this.position.width, this.position.height - (importButtonHeight + 30.0f));
			Rect importButtonPosition = new Rect(this.position.width / 2 - 100.0f, this.position.height - importButtonHeight - 5.0f, 200.0f, importButtonHeight);

			EditorGUILayout.LabelField("Current Version: Unity " + TeamUtility.IO.StandaloneInputModule.VERSION);
			EditorGUILayout.Space();

			DisplayVersionList(versionListPosition);
			
			GUI.enabled = _selection != SELECTION_EMPTY;
			if(GUI.Button(importButtonPosition, "Apply"))
			{
				ApplySelectedVersion();
			}
			GUI.enabled = true;
		}

		private void DisplayVersionList(Rect screenRect)
		{
			GUILayout.BeginArea(screenRect);
			_mappingListScrollPos = EditorGUILayout.BeginScrollView(_mappingListScrollPos);
			GUILayout.Space(5.0f);

			for(int i = 0; i < _moduleNames.Count; i++)
				DispayVersion(screenRect, i);

			GUILayout.Space(5.0f);
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		private void DispayVersion(Rect screenRect, int index)
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
				if(_highlightTexture == null)
					CreateHighlightTexture();

				GUI.DrawTexture(configPos, _highlightTexture, ScaleMode.StretchToFill);
				EditorGUI.LabelField(configPos, _moduleNames[index], _whiteLabel);
			}
			else
			{
				EditorGUI.LabelField(configPos, _moduleNames[index]);
			}
		}

		private void ApplySelectedVersion()
		{
			string scriptPath = GetScriptFilePath();
			if(string.IsNullOrEmpty(scriptPath))
				return;

			TextAsset textAsset = Resources.Load<TextAsset>(_modulePaths[_selection]);
			if(textAsset == null)
			{
				EditorUtility.DisplayDialog("Error", "Unable to load the file for this version. This can happen if you renamed or moved the files.", "OK");
				return;
			}

			using(var writer = System.IO.File.CreateText(scriptPath))
			{
				writer.Write(textAsset.text);
			}

			Resources.UnloadAsset(textAsset);
			AssetDatabase.Refresh();
			EditorUtility.DisplayDialog("Success", "The version of the UI input module has been changed. Wait for the scripts to be recompiled.", "OK");
		}

		private string GetScriptFilePath()
		{
			string path = EditorPrefs.GetString("InputManager.UIInputModules.Path", null);
			if(!string.IsNullOrEmpty(path) && System.IO.File.Exists(path))
				return path;

			path = EditorUtility.SaveFilePanelInProject("Set the path of the StandaloneInputModule script", "StandaloneInputModule", "cs", "");
			if(!string.IsNullOrEmpty(path))
				EditorPrefs.SetString("InputManager.UIInputModules.Path", path);

			return path;
		}

		[MenuItem("Team Utility/Input Manager/Set Input Module Version", false, 200)]
		public static void Open()
		{
			EditorWindow.GetWindow<UIInputModuleVersionManager>("Module Version");
		}
	}
}