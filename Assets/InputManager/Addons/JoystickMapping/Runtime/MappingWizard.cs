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
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace TeamUtility.IO
{
	public class MappingWizard : MonoBehaviour 
	{
		public enum ScanType
		{
			Button, Axis
		}

		[SerializeField] private TextAsset _testDefinition = null;
		[SerializeField] private GUISkin _guiSkin = null;

		[SerializeField] 
		[Range(0.1f, 1.0f)]
		[Tooltip("Absolute minimum value to register joystick axis movement")]
		private float _axisDetectTreshold = 0.9f;

		private AxisMapping[] _results;
		private MappingWizardItem[] _items;
		private int _currentItem;
		private KeyCode _lastKeyCode;
		private int _lastAxis;
		private string _mappingName;
		private string[] _axisDisplayNames;
		private string[] _rawJoystickAxes;

		private void Awake()
		{
			_currentItem = 0;
			_lastKeyCode = KeyCode.None;
			_lastAxis = 2;
			_mappingName = string.Empty;
			_axisDisplayNames = new string[] { "X", "Y", "3rd(Scrollwheel)", "4th", "5th", "6th", "7th", "8th", "9th", "10th" };
			_rawJoystickAxes = new string[10];
			for(int i = 0; i < 10; i++)
			{
				_rawJoystickAxes[i] = "joy_0_axis_" + i;
			}

			try {
				LoadTestDefinition();
				_results = new AxisMapping[_items.Length];
			}
			catch(System.Exception ex) {
				_items = null;
				Debug.LogException(ex);
			}
		}

		private void LoadTestDefinition()
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(_testDefinition.text);

			_items = new MappingWizardItem[doc.DocumentElement.ChildNodes.Count];
			for(int i = 0; i < _items.Length; i++)
			{
				XmlNode axis = doc.DocumentElement.ChildNodes[i];
				string name = axis.Attributes["name"].InnerText;
				ScanType scanType = (ScanType)System.Enum.Parse(typeof(ScanType), axis.Attributes["scanType"].InnerText, true);

				_items[i] = new MappingWizardItem(name, scanType);
			}
		}

		private void Update()
		{
			if(_items != null && _items.Length > 0 && _currentItem < _items.Length)
			{
				if(_items[_currentItem].ScanType == ScanType.Button)
				{
					for(int i = (int)KeyCode.JoystickButton0; i <= (int)KeyCode.JoystickButton19; i++)
					{
						KeyCode key = (KeyCode)i;
						if(Input.GetKeyDown(key))
						{
							_lastKeyCode = key;
							break;
						}
					}
				}
				else
				{
					for(int i = 0; i < _rawJoystickAxes.Length; i++)
					{
						if(Mathf.Abs(Input.GetAxis(_rawJoystickAxes[i])) > _axisDetectTreshold)
						{
							_lastAxis = i;
							break;
						}
					}
				}
			}
		}

		private void OnGUI()
		{
			GUI.skin = _guiSkin;

			if(_items == null || _items.Length == 0)
			{
				Rect errorPosition = new Rect(0.0f, 0.0f, Screen.width, Screen.height);
				GUI.Label(errorPosition, "<color=red><size=20><b>Test definition is empty or corrupted!</b></size></color>", "centered_label");
			}
			else
			{
				if(_currentItem < _items.Length)
				{
					DisplayCurrentTestItem();
				}
				else
				{
					DisplayTestResults();
				}
			}

			GUI.skin = null;
		}

		private void DisplayCurrentTestItem()
		{
			string description = string.Format("{0} <b>{1}</b>", _items[_currentItem].ScanType == ScanType.Button ? "Press" : "Move",
			                                   _items[_currentItem].AxisName);
			GUI.Label(new Rect(0.0f, 0.0f, Screen.width, 25.0f), description, "centered_label");

			if(_items[_currentItem].ScanType == ScanType.Button && _lastKeyCode != KeyCode.None)
			{
				GUI.Label(new Rect(0.0f, 0.0f, Screen.width, Screen.height), 
				          string.Format("<color=lime><size=30><b>{0}</b></size></color>", _lastKeyCode), 
				          "centered_label");
			}
			else if(_items[_currentItem].ScanType == ScanType.Axis && _lastAxis >= 0)
			{
				GUI.Label(new Rect(0.0f, 0.0f, Screen.width, Screen.height), 
				          string.Format("<color=lime><size=30><b>{0}</b></size></color>", _axisDisplayNames[_lastAxis]), 
				          "centered_label");
			}

			GUI.Label(new Rect(Screen.width - 100.0f, Screen.height - 34.0f, 100.0f, 24.0f), (_currentItem + 1) + "/" + _items.Length, "centered_label");

			GUI.enabled = _items[_currentItem].ScanType == ScanType.Button && _lastKeyCode != KeyCode.None ||
							_items[_currentItem].ScanType == ScanType.Axis && _lastAxis >= 0;
			Rect buttonPosition = new Rect(Screen.width / 2 - 100.0f, Screen.height - 34.0f, 200.0f, 24.0f);
			if(GUI.Button(buttonPosition, "Continue"))
			{
				if(_items[_currentItem].ScanType == ScanType.Button)
				{
					_results[_currentItem] = new AxisMapping(_items[_currentItem].AxisName, _lastKeyCode);
				}
				else
				{
					_results[_currentItem] = new AxisMapping(_items[_currentItem].AxisName, _lastAxis);
				}
				_currentItem++;
				_lastKeyCode = KeyCode.None;
				_lastAxis = -1;
			}

			GUI.enabled = true;
		}

		private void DisplayTestResults()
		{
			GUI.Label(new Rect(0.0f, 0.0f, Screen.width, 25.0f), "Mapping Complete!", "centered_label");

			_mappingName = GUI.TextField(new Rect(Screen.width / 2 - 100.0f, Screen.height / 2 - 12.0f, 200.0f, 24.0f), _mappingName);

			Rect cancelButtonPosition = new Rect(Screen.width / 2 - 205.0f, Screen.height - 34.0f, 200.0f, 24.0f);
			Rect saveButtonPosition = new Rect(Screen.width / 2 + 5.0f, Screen.height - 34.0f, 200.0f, 24.0f);
			if(GUI.Button(cancelButtonPosition, "Cancel"))
			{
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
			}

			GUI.enabled = !string.IsNullOrEmpty(_mappingName);
			if(GUI.Button(saveButtonPosition, "Save and Close"))
			{
				SaveMappingResults();
#if UNITY_EDITOR
				UnityEditor.EditorApplication.isPlaying = false;
#else
				Application.Quit();
#endif
			}
			GUI.enabled = true;
		}

		private void SaveMappingResults()
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = System.Text.Encoding.UTF8;
			settings.Indent = true;

#if UNITY_WINRT && !UNITY_EDITOR
			MemoryStream outputStream;
			string outputFile;
			XmlWriter writer = CreateXmlWriter(settings, out outputStream, out outputFile);
#else
			XmlWriter writer = CreateXmlWriter(settings);
#endif

			writer.WriteStartDocument(true);
			writer.WriteStartElement("Mapping");
			writer.WriteAttributeString("name", _mappingName);
			for(int i = 0; i < _results.Length; i++)
			{
				WriteMappingResult(_results[i], writer);
			}
			
			writer.WriteEndElement();
			writer.WriteEndDocument();

#if UNITY_WINRT && !UNITY_EDITOR
			UnityEngine.Windows.File.WriteAllBytes(outputFile, outputStream.ToArray());
			outputStream.Dispose();
			writer.Dispose();
#else
			writer.Close();
#endif
		}

#if UNITY_WINRT && !UNITY_EDITOR
		private XmlWriter CreateXmlWriter(XmlWriterSettings settings, out MemoryStream stream, out string filename)
		{
			string folder = GetJoystickMappingSaveFolder();
			filename = folder + _mappingName.ToLower().Replace(' ', '_') + ".xml";
			stream = new MemoryStream();

			if(!UnityEngine.Windows.Directory.Exists(folder))
				UnityEngine.Windows.Directory.CreateDirectory(folder);

			return XmlWriter.Create(stream);
		}
#else
		private XmlWriter CreateXmlWriter(XmlWriterSettings settings)
		{
			string folder = GetJoystickMappingSaveFolder();
			string file = folder + _mappingName.ToLower().Replace(' ', '_') + ".xml";

			if(!Directory.Exists(folder))
				Directory.CreateDirectory(folder);

			return XmlWriter.Create(file, settings);
		}
#endif

		private void WriteMappingResult(AxisMapping result, XmlWriter writer)
		{
			writer.WriteStartElement("Axis");
			writer.WriteAttributeString("name", result.Name);
			writer.WriteAttributeString("key", result.Key.ToString());
			writer.WriteAttributeString("joystickAxis", result.JoystickAxis.ToString());
			writer.WriteAttributeString("scanType", result.ScanType.ToString());
			writer.WriteEndElement();
		}

		private string GetJoystickMappingSaveFolder()
		{
			string dataPath = Application.dataPath;
			int slashIndex = dataPath.LastIndexOf('/');

			return dataPath.Substring(0, slashIndex) + "/JoystickMappings/";
		}
	}
}