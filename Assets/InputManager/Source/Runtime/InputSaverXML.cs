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
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Globalization;

namespace Luminosity.IO
{
	public class InputSaverXML : IInputSaver 
	{
		private const int VERSION = 2;

		private string m_filename;
		private Stream m_outputStream;
		private StringBuilder m_output;
		
		public InputSaverXML(string filename)
		{
			if(filename == null)
				throw new ArgumentNullException("filename");
			
			m_filename = filename;
			m_outputStream = null;
			m_output = null;
		}

		public InputSaverXML(Stream stream)
		{
			if(stream == null)
				throw new ArgumentNullException("stream");
			
			m_filename = null;
			m_output = null;
			m_outputStream = stream;
		}
		
		public InputSaverXML(StringBuilder output)
		{
			if(output == null)
				throw new ArgumentNullException("output");
			
			m_filename = null;
			m_outputStream = null;
			m_output = output;
		}

		private XmlWriter CreateXmlWriter(XmlWriterSettings settings)
		{
			if(m_filename != null)
			{
#if UNITY_WINRT && !UNITY_EDITOR
				m_outputStream = new MemoryStream();
				return XmlWriter.Create(m_outputStream, settings);
#else
				return XmlWriter.Create(m_filename, settings);
#endif
			}
			else if(m_outputStream != null)
			{
				return XmlWriter.Create(m_outputStream, settings);
			}
			else if(m_output != null)
			{
				return XmlWriter.Create(m_output, settings);
			}

			return null;
		}

		public void Save(SaveData saveData)
		{
			XmlWriterSettings xmlSettings = new XmlWriterSettings();
			xmlSettings.Encoding = Encoding.UTF8;
			xmlSettings.Indent = true;

			using(XmlWriter writer = CreateXmlWriter(xmlSettings))
			{
				writer.WriteStartDocument(true);
				writer.WriteStartElement("Input");
				writer.WriteAttributeString("version", VERSION.ToString());
				writer.WriteElementString("PlayerOneScheme", saveData.PlayerOneScheme);
				writer.WriteElementString("PlayerTwoScheme", saveData.PlayerTwoScheme);
				writer.WriteElementString("PlayerThreeScheme", saveData.PlayerThreeScheme);
				writer.WriteElementString("PlayerFourScheme", saveData.PlayerFourScheme);
				foreach(ControlScheme scheme in saveData.ControlSchemes)
				{
					WriteControlScheme(scheme, writer);
				}

				writer.WriteEndElement();
				writer.WriteEndDocument();
			}

#if UNITY_WINRT && !UNITY_EDITOR
			if(m_filename != null && m_outputStream != null && (m_outputStream is MemoryStream))
			{
				UnityEngine.Windows.File.WriteAllBytes(m_filename, ((MemoryStream)m_outputStream).ToArray());
				m_outputStream.Dispose();
			}
#endif
		}

		private void WriteControlScheme(ControlScheme scheme, XmlWriter writer)
		{
			writer.WriteStartElement("ControlScheme");
			writer.WriteAttributeString("name", scheme.Name);
			writer.WriteAttributeString("id", scheme.UniqueID);
			writer.WriteElementString("Description", scheme.Description);
			foreach(var action in scheme.Actions)
			{
				WriteInputAction(action, writer);
			}

			writer.WriteEndElement();
		}

		private void WriteInputAction(InputAction action, XmlWriter writer)
		{
			writer.WriteStartElement("Action");
			writer.WriteAttributeString("name", action.Name);
			writer.WriteElementString("Description", action.Description);
			foreach(var binding in action.Bindings)
			{
				WriteInputBinding(binding, writer);
			}

			writer.WriteEndElement();
		}

		private void WriteInputBinding(InputBinding binding, XmlWriter writer)
		{
			writer.WriteStartElement("Binding");
			writer.WriteElementString("Positive", binding.Positive.ToString());
			writer.WriteElementString("Negative", binding.Negative.ToString());
			writer.WriteElementString("DeadZone", binding.DeadZone.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("Gravity", binding.Gravity.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("Sensitivity", binding.Sensitivity.ToString(CultureInfo.InvariantCulture));
			writer.WriteElementString("Snap", binding.Snap.ToString().ToLower());
			writer.WriteElementString("Invert", binding.Invert.ToString().ToLower());
			writer.WriteElementString("Type", binding.Type.ToString());
			writer.WriteElementString("Axis", binding.Axis.ToString());
			writer.WriteElementString("Joystick", binding.Joystick.ToString());
			writer.WriteElementString("GamepadButton", binding.GamepadButton.ToString());
			writer.WriteElementString("GamepadAxis", binding.GamepadAxis.ToString());
			writer.WriteElementString("GamepadIndex", binding.GamepadIndex.ToString());

			writer.WriteEndElement();
		}
	}
}
