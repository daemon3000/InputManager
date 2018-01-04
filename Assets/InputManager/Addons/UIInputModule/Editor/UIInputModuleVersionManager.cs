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
using System.Xml;
using System.Collections.Generic;

namespace LuminosityEditor.IO
{
	public class UIInputModuleVersionManager : EditorWindow
	{
		private const int SELECTION_EMPTY = -1;

		[SerializeField]
		private Texture2D m_highlightTexture;
		[SerializeField]
		private Vector2 m_mappingListScrollPos;
		
		private List<string> m_moduleNames;
		private List<string> m_modulePaths;
		private GUIStyle m_whiteLabel;
		private int m_selection = SELECTION_EMPTY;
		
		private void OnEnable()
		{
			m_selection = SELECTION_EMPTY;
			if(m_highlightTexture == null)
			{
				CreateHighlightTexture();
			}
			if(m_whiteLabel == null)
			{
				m_whiteLabel = new GUIStyle(EditorStyles.label);
				m_whiteLabel.normal.textColor = Color.white;
			}
			m_moduleNames = new List<string>();
			m_modulePaths = new List<string>();

			LoadInputModules();
		}

		private void CreateHighlightTexture()
		{
			m_highlightTexture = new Texture2D(1, 1);
			m_highlightTexture.SetPixel(0, 0, new Color32(50, 125, 255, 255));
			m_highlightTexture.Apply();
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
					m_moduleNames.Add(item.Attributes["name"].InnerText);
					m_modulePaths.Add(item.Attributes["path"].InnerText);
				}
			}
			catch(System.Exception ex) 
			{
				m_moduleNames.Clear();
				m_modulePaths.Clear();
				Debug.LogException(ex);
				Debug.LogError("Failed to load UI input modules. File format is invalid.");
			}
			
			Resources.UnloadAsset(textAsset);
		}

		private void OnDestroy()
		{
			Texture2D.DestroyImmediate(m_highlightTexture);
			m_highlightTexture = null;
		}

		private void OnGUI()
		{
			float importButtonHeight = 24.0f;
			Rect versionListPosition = new Rect(0.0f, 20.0f, this.position.width, this.position.height - (importButtonHeight + 30.0f));
			Rect importButtonPosition = new Rect(this.position.width / 2 - 100.0f, this.position.height - importButtonHeight - 5.0f, 200.0f, importButtonHeight);

			EditorGUILayout.LabelField("Current Version: Unity " + Luminosity.IO.StandaloneInputModule.VERSION);
			EditorGUILayout.Space();

			DrawVersionList(versionListPosition);
			
			GUI.enabled = m_selection != SELECTION_EMPTY;
			if(GUI.Button(importButtonPosition, "Apply"))
			{
				ApplySelectedVersion();
			}
			GUI.enabled = true;
		}

		private void DrawVersionList(Rect screenRect)
		{
			GUILayout.BeginArea(screenRect);
			m_mappingListScrollPos = EditorGUILayout.BeginScrollView(m_mappingListScrollPos);
			GUILayout.Space(5.0f);

			for(int i = 0; i < m_moduleNames.Count; i++)
				DrawVersion(screenRect, i);

			GUILayout.Space(5.0f);
			EditorGUILayout.EndScrollView();
			GUILayout.EndArea();
		}

		private void DrawVersion(Rect screenRect, int index)
		{
			Rect configPos = GUILayoutUtility.GetRect(new GUIContent(name), EditorStyles.label, GUILayout.Height(15.0f));
			if(Event.current.type == EventType.MouseDown && Event.current.button == 0)
			{
				if(configPos.Contains(Event.current.mousePosition))
				{
					m_selection = index;
					Repaint();
				}
			}
			
			if(m_selection == index)
			{
				if(m_highlightTexture == null)
					CreateHighlightTexture();

				GUI.DrawTexture(configPos, m_highlightTexture, ScaleMode.StretchToFill);
				EditorGUI.LabelField(configPos, m_moduleNames[index], m_whiteLabel);
			}
			else
			{
				EditorGUI.LabelField(configPos, m_moduleNames[index]);
			}
		}

		private void ApplySelectedVersion()
		{
			string scriptPath = GetScriptFilePath();
			if(string.IsNullOrEmpty(scriptPath))
				return;

			TextAsset textAsset = Resources.Load<TextAsset>(m_modulePaths[m_selection]);
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

		[MenuItem("Luminosity/Input Manager/Set Input Module Version", false, 200)]
		public static void Open()
		{
			EditorWindow.GetWindow<UIInputModuleVersionManager>("Module Version");
		}

		[MenuItem("Luminosity/Input Manager/Use Custom Input Module", false, 201)]
		private static void FixEventSystem()
		{
			UnityEngine.EventSystems.StandaloneInputModule[] im = UnityEngine.Object.FindObjectsOfType<UnityEngine.EventSystems.StandaloneInputModule>();
			if(im.Length > 0)
			{
				for(int i = 0; i < im.Length; i++)
				{
					im[i].gameObject.AddComponent<Luminosity.IO.StandaloneInputModule>();
					UnityEngine.Object.DestroyImmediate(im[i]);
				}
				EditorUtility.DisplayDialog("Success", "All built-in standalone input modules have been replaced!", "OK");
				Debug.LogFormat("{0} built-in standalone input module(s) have been replaced", im.Length);
			}
			else
			{
				EditorUtility.DisplayDialog("Warning", "Unable to find any built-in input modules in the scene!", "OK");
			}
		}
	}
}