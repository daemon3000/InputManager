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
using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;

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

			using(XmlReader reader = CreateXmlReader())
			{
                parameters.inputConfigurations = new List<InputConfiguration>();
                parameters.playerOneDefault = string.Empty;
                parameters.playerTwoDefault = string.Empty;
                parameters.playerThreeDefault = string.Empty;
                parameters.playerFourDefault = string.Empty;
                while (reader.Read())
				{
					if(reader.IsStartElement("Input"))
					{
                        parameters.playerOneDefault = reader["playerOneDefault"];
                        parameters.playerTwoDefault = reader["playerTwoDefault"];
                        parameters.playerThreeDefault = reader["playerThreeDefault"];
                        parameters.playerFourDefault = reader["playerFourDefault"];
                    }
					else if(reader.IsStartElement("InputConfiguration"))
					{
                        parameters.inputConfigurations.Add(ReadInputConfiguration(reader));
					}
				}
			}

            return parameters;
		}

		public InputConfiguration LoadSelective(string inputConfigName)
		{
			InputConfiguration inputConfig = null;
			using(XmlReader reader = CreateXmlReader())
			{
				while(reader.Read())
				{
					if(reader.IsStartElement("InputConfiguration") && reader["name"] == inputConfigName)
					{
						inputConfig = ReadInputConfiguration(reader);
						break;
					}
				}
			}

			return inputConfig;
		}
		
		private XmlReader CreateXmlReader()
		{
			if(_filename != null)
			{
				return XmlReader.Create(_filename);
			}
			else if(_inputStream != null)
			{
				return XmlReader.Create(_inputStream);
			}
			else if(_textReader != null)
			{
				return XmlReader.Create(_textReader);
			}

			return null;
		}
		
		private InputConfiguration ReadInputConfiguration(XmlReader reader)
		{
			InputConfiguration inputConfig = new InputConfiguration();
			inputConfig.name = reader["name"];
			
			while(reader.Read())
			{
				if(!reader.IsStartElement("AxisConfiguration"))
					break;
				
				inputConfig.axes.Add(ReadAxisConfiguration(reader));
			}
			
			return inputConfig;
		}
		
		private AxisConfiguration ReadAxisConfiguration(XmlReader reader)
		{
			AxisConfiguration axisConfig = new AxisConfiguration();
			axisConfig.name = reader["name"];
			
			bool endOfAxis = false;
			while(reader.Read() && reader.IsStartElement() && !endOfAxis)
			{
				switch(reader.LocalName)
				{
				case "description":
					axisConfig.description = reader.IsEmptyElement ? string.Empty : reader.ReadElementContentAsString();
					break;
				case "positive":
					axisConfig.positive = AxisConfiguration.StringToKey(reader.ReadElementContentAsString());
					break;
				case "altPositive":
					axisConfig.altPositive = AxisConfiguration.StringToKey(reader.ReadElementContentAsString());
					break;
				case "negative":
					axisConfig.negative = AxisConfiguration.StringToKey(reader.ReadElementContentAsString());
					break;
				case "altNegative":
					axisConfig.altNegative = AxisConfiguration.StringToKey(reader.ReadElementContentAsString());
					break;
				case "deadZone":
					axisConfig.deadZone = reader.ReadElementContentAsFloat();
					break;
				case "gravity":
					axisConfig.gravity = reader.ReadElementContentAsFloat();
					break;
				case "sensitivity":
					axisConfig.sensitivity = reader.ReadElementContentAsFloat();
					break;
				case "snap":
					axisConfig.snap = reader.ReadElementContentAsBoolean();
					break;
				case "invert":
					axisConfig.invert = reader.ReadElementContentAsBoolean();
					break;
				case "type":
					axisConfig.type = AxisConfiguration.StringToInputType(reader.ReadElementContentAsString());
					break;
				case "axis":
					axisConfig.axis = reader.ReadElementContentAsInt();
					break;
				case "joystick":
					axisConfig.joystick = reader.ReadElementContentAsInt();
					break;
				default:
					endOfAxis = true;
					break;
				}
			}
			
			return axisConfig;
		}
	}
}
