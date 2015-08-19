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
using System.Collections;
using System.Collections.Generic;

namespace TeamUtility.IO
{
	[System.Serializable]
	public class JoystickMapping : IEnumerable<AxisMapping>
	{
		private List<AxisMapping> _axes;
		private string _name;

		public string Name { get { return _name; } }
		public int AxisCount { get { return _axes.Count; } }

		public JoystickMapping()
		{
			_name = null;
			_axes = new List<AxisMapping>();
		}

		public void Load(string filename)
		{
#if UNITY_WINRT && !UNITY_EDITOR
			if(UnityEngine.Windows.File.Exists(filename))
			{
				byte[] buffer = UnityEngine.Windows.File.ReadAllBytes(filename);
				string xmlData = System.Text.Encoding.UTF8.GetString(buffer, 0, buffer.Length);
				if(!string.IsNullOrEmpty(xmlData))
				{
                    InternalLoad(xmlData);
                }
            }
#else
            if(File.Exists(filename))
			{
				using(StreamReader reader = File.OpenText(filename))
				{
					InternalLoad(reader.ReadToEnd());
				}
			}
#endif
		}

		public void LoadFromResources(string path)
		{
			TextAsset textAsset = Resources.Load<TextAsset>(path);
			if(textAsset != null)
			{
				InternalLoad(textAsset.text);
				Resources.UnloadAsset(textAsset);
			}
		}

		private void InternalLoad(string xmlData)
		{
			try
			{
				XmlDocument doc = new XmlDocument();
				doc.LoadXml(xmlData);

				_name = doc.DocumentElement.Attributes["name"].InnerText;
				foreach(XmlNode axisNode in doc.DocumentElement)
				{
					string name = axisNode.Attributes["name"].InnerText;
					KeyCode key = (KeyCode)System.Enum.Parse(typeof(KeyCode), axisNode.Attributes["key"].InnerText, true);
					int joystickAxis = int.Parse(axisNode.Attributes["joystickAxis"].InnerText);
					MappingWizard.ScanType scanType = (MappingWizard.ScanType)System.Enum.Parse(typeof(MappingWizard.ScanType), axisNode.Attributes["scanType"].InnerText, true);

					if(scanType == MappingWizard.ScanType.Button)
					{
						_axes.Add(new AxisMapping(name, key));
					}
					else 
					{
						_axes.Add(new AxisMapping(name, joystickAxis));
					}
				}
			}
			catch
			{
				_name = null;
				_axes.Clear();
			}
		}

		public IEnumerator<AxisMapping> GetEnumerator()
		{
			return _axes.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _axes.GetEnumerator();
		}
	}
}