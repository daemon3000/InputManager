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
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.Generic;

namespace TeamUtility.IO
{
	public sealed class InputSaverXML : IInputSaver 
	{
		private string _filename;
		private Stream _outputStream;
		private StringBuilder _output;
		
		public InputSaverXML(string filename)
		{
			if(filename == null)
				throw new ArgumentNullException("filename");
			
			_filename = filename;
			_outputStream = null;
			_output = null;
		}
		
		public InputSaverXML(Stream stream)
		{
			if(stream == null)
				throw new ArgumentNullException("stream");
			
			_filename = null;
			_output = null;
			_outputStream = stream;
		}
		
		public InputSaverXML(StringBuilder output)
		{
			if(output == null)
				throw new ArgumentNullException("output");
			
			_filename = null;
			_outputStream = null;
			_output = output;
		}
		
		public void Save(List<InputConfiguration> inputConfigurations, string defaultConfiguration)
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Encoding = System.Text.Encoding.UTF8;
			settings.Indent = true;
			
			using(XmlWriter writer = CreateXmlWriter(settings))
			{
				writer.WriteStartDocument(true);
				writer.WriteStartElement("Input");
				writer.WriteAttributeString("defaultConfiguration", defaultConfiguration);
				foreach(InputConfiguration inputConfig in inputConfigurations)
				{
					WriteInputConfiguration(inputConfig, writer);
				}
				
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}
		}
		
		private XmlWriter CreateXmlWriter(XmlWriterSettings settings)
		{
			if(_filename != null)
			{
				return XmlWriter.Create(_filename, settings);
			}
			else if(_outputStream != null)
			{
				return XmlWriter.Create(_outputStream, settings);
			}
			else if(_output != null)
			{
				return XmlWriter.Create(_output, settings);
			}
			
			return null;
		}
		
		private void WriteInputConfiguration(InputConfiguration inputConfig, XmlWriter writer)
		{
			writer.WriteStartElement("InputConfiguration");
			writer.WriteAttributeString("name", inputConfig.name);
			foreach(AxisConfiguration axisConfig in inputConfig.axes)
			{
				WriteAxisConfiguration(axisConfig, writer);
			}
			
			writer.WriteEndElement();
		}
		
		private void WriteAxisConfiguration(AxisConfiguration axisConfig, XmlWriter writer)
		{
			writer.WriteStartElement("AxisConfiguration");
			writer.WriteAttributeString("name", axisConfig.name);
			writer.WriteElementString("description", axisConfig.description);
			writer.WriteElementString("positive", axisConfig.positive.ToString());
			writer.WriteElementString("altPositive", axisConfig.altPositive.ToString());
			writer.WriteElementString("negative", axisConfig.negative.ToString());
			writer.WriteElementString("altNegative", axisConfig.altNegative.ToString());
			writer.WriteElementString("deadZone", axisConfig.deadZone.ToString());
			writer.WriteElementString("gravity", axisConfig.gravity.ToString());
			writer.WriteElementString("sensitivity", axisConfig.sensitivity.ToString());
			writer.WriteElementString("snap", axisConfig.snap.ToString().ToLower());
			writer.WriteElementString("invert", axisConfig.invert.ToString().ToLower());
			writer.WriteElementString("type", axisConfig.type.ToString());
			writer.WriteElementString("axis", axisConfig.axis.ToString());
			writer.WriteElementString("joystick", axisConfig.joystick.ToString());
			
			writer.WriteEndElement();
		}
	}
}
