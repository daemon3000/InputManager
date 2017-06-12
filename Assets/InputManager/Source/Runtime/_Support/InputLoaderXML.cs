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
using System;
using System.IO;
using System.Xml;
using System.Globalization;

namespace TeamUtility.IO
{
	public sealed class InputLoaderXML : IInputLoader
	{
		private string _filename;
		private Stream _inputStream;
		private TextReader _textReader;

		public InputLoaderXML(string filename)
		{
			if(filename == null)
				throw new ArgumentNullException("filename");
			
			_filename = filename;
			_inputStream = null;
			_textReader = null;
		}

		public InputLoaderXML(Stream stream)
		{
			if(stream == null)
				throw new ArgumentNullException("stream");
			
			_filename = null;
			_textReader = null;
			_inputStream = stream;
		}
		
		public InputLoaderXML(TextReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException("reader");
			
			_filename = null;
			_inputStream = null;
			_textReader = reader;
		}

		public SaveLoadParameters Load()
		{
            SaveLoadParameters parameters = new SaveLoadParameters();
			XmlDocument doc = CreateXmlDocument();

			if(doc != null)
			{
				parameters.playerOneDefault = ReadAttribute(doc.DocumentElement, "playerOneDefault");
				parameters.playerTwoDefault = ReadAttribute(doc.DocumentElement, "playerTwoDefault");
				parameters.playerThreeDefault = ReadAttribute(doc.DocumentElement, "playerThreeDefault");
				parameters.playerFourDefault = ReadAttribute(doc.DocumentElement, "playerFourDefault");

				var inputConfigNodes = doc.DocumentElement.SelectNodes("InputConfiguration");
				foreach(XmlNode node in inputConfigNodes)
				{
					parameters.inputConfigurations.Add(ReadInputConfiguration(node));
				}
			}

			return parameters;
		}

		public InputConfiguration LoadSelective(string inputConfigName)
		{
			if(string.IsNullOrEmpty(inputConfigName))
				return null;

			XmlDocument doc = CreateXmlDocument();
			InputConfiguration inputConfig = null;

			if(doc != null)
			{
				var inputConfigNodes = doc.SelectNodes("InputConfiguration");
				foreach(XmlNode node in inputConfigNodes)
				{
					if(ReadAttribute(node, "name") == inputConfigName)
					{
						inputConfig = ReadInputConfiguration(node);
						break;
					}
				}
			}

			return inputConfig;
		}

		private XmlDocument CreateXmlDocument()
		{
			if(_filename != null)
			{
				using(StreamReader reader = new StreamReader(_filename, true))
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(reader);

					return doc;
				}
			}
			else if(_inputStream != null)
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(_inputStream);

				return doc;
			}
			else if(_textReader != null)
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(_textReader);

				return doc;
			}

			return null;
		}
		
		private InputConfiguration ReadInputConfiguration(XmlNode node)
		{
			InputConfiguration inputConfig = new InputConfiguration();
			inputConfig.name = ReadAttribute(node, "name", "Unnamed Configuration");

			var axisConfigNodes = node.SelectNodes("AxisConfiguration");
			foreach(XmlNode child in axisConfigNodes)
			{
				inputConfig.axes.Add(ReadAxisConfiguration(child));
			}
			
			return inputConfig;
		}
		
		private AxisConfiguration ReadAxisConfiguration(XmlNode node)
		{
			AxisConfiguration axisConfig = new AxisConfiguration();
			axisConfig.name = ReadAttribute(node, "name", "Unnamed Axis");

			foreach(XmlNode child in node.ChildNodes)
			{
				switch(child.LocalName)
				{
				case "description":
					axisConfig.description = child.InnerText;
					break;
				case "positive":
					axisConfig.positive = AxisConfiguration.StringToKey(child.InnerText);
					break;
				case "altPositive":
					axisConfig.altPositive = AxisConfiguration.StringToKey(child.InnerText);
					break;
				case "negative":
					axisConfig.negative = AxisConfiguration.StringToKey(child.InnerText);
					break;
				case "altNegative":
					axisConfig.altNegative = AxisConfiguration.StringToKey(child.InnerText);
					break;
				case "deadZone":
					axisConfig.deadZone = ReadAsFloat(child);
					break;
				case "gravity":
					axisConfig.gravity = ReadAsFloat(child, 1.0f);
					break;
				case "sensitivity":
					axisConfig.sensitivity = ReadAsFloat(child, 1.0f);
					break;
				case "snap":
					axisConfig.snap = ReadAsBool(child);
					break;
				case "invert":
					axisConfig.invert = ReadAsBool(child);
					break;
				case "type":
					axisConfig.type = AxisConfiguration.StringToInputType(child.InnerText);
					break;
				case "axis":
					axisConfig.axis = ReadAsInt(child);
					break;
				case "joystick":
					axisConfig.joystick = ReadAsInt(child);
					break;
				}
			}
			
			return axisConfig;
		}

		private string ReadAttribute(XmlNode node, string attribute, string defValue = null)
		{
			if(node.Attributes[attribute] != null)
				return node.Attributes[attribute].InnerText;

			return defValue;
		}

		private int ReadAsInt(XmlNode node, int defValue = 0)
		{
			int value = 0;
			if(int.TryParse(node.InnerText, out value))
				return value;

			return defValue;
		}

		private float ReadAsFloat(XmlNode node, float defValue = 0.0f)
		{
			float value = 0;
			if(float.TryParse(node.InnerText, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
				return value;

			return defValue;
		}

		private bool ReadAsBool(XmlNode node, bool defValue = false)
		{
			bool value = false;
			if(bool.TryParse(node.InnerText, out value))
				return value;

			return defValue;
		}
	}
}
