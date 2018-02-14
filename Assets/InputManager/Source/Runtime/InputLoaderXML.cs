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
using System.Globalization;
using System.Collections.Generic;
using System.Linq;

namespace Luminosity.IO
{
	public class InputLoaderXML : IInputLoader
	{
		private string m_filename;
		private Stream m_inputStream;
		private TextReader m_textReader;

		public InputLoaderXML(string filename)
		{
			if(filename == null)
				throw new ArgumentNullException("filename");
			
			m_filename = filename;
			m_inputStream = null;
			m_textReader = null;
		}

		public InputLoaderXML(Stream stream)
		{
			if(stream == null)
				throw new ArgumentNullException("stream");
			
			m_filename = null;
			m_textReader = null;
			m_inputStream = stream;
		}
		
		public InputLoaderXML(TextReader reader)
		{
			if(reader == null)
				throw new ArgumentNullException("reader");
			
			m_filename = null;
			m_inputStream = null;
			m_textReader = reader;
		}

		private XmlDocument CreateXmlDocument()
		{
			if(m_filename != null)
			{
				using(StreamReader reader = File.OpenText(m_filename))
				{
					XmlDocument doc = new XmlDocument();
					doc.Load(reader);

					return doc;
				}
			}
			else if(m_inputStream != null)
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(m_inputStream);

				return doc;
			}
			else if(m_textReader != null)
			{
				XmlDocument doc = new XmlDocument();
				doc.Load(m_textReader);

				return doc;
			}

			return null;
		}

		public SaveData Load()
		{
			XmlDocument doc = CreateXmlDocument();
			if(doc != null)
			{
				int version = ReadAttributeAsInt(doc.DocumentElement, "version", 1);
				if(version == 2) return Load_V2(doc);
				else return Load_V1(doc);
			}

			return new SaveData();
		}

		public ControlScheme Load(string schemeName)
		{
			XmlDocument doc = CreateXmlDocument();
			if(doc != null)
			{
				int version = ReadAttributeAsInt(doc.DocumentElement, "version", 1);
				if(version == 2) return Load_V2(doc, schemeName);
				else return Load_V1(doc, schemeName);
			}

			return null;
		}

		#region [V2]
		private SaveData Load_V2(XmlDocument doc)
		{
			SaveData saveData = new SaveData();
			var root = doc.DocumentElement;

			saveData.PlayerOneScheme = ReadNode(SelectSingleNode(root, "PlayerOneScheme"));
			saveData.PlayerTwoScheme = ReadNode(SelectSingleNode(root, "PlayerTwoScheme"));
			saveData.PlayerThreeScheme = ReadNode(SelectSingleNode(root, "PlayerThreeScheme"));
			saveData.PlayerFourScheme = ReadNode(SelectSingleNode(root, "PlayerFourScheme"));

			var schemeNodes = SelectNodes(root, "ControlScheme");
			foreach(XmlNode node in schemeNodes)
			{
				saveData.ControlSchemes.Add(ReadControlScheme_V2(node));
			}

			return saveData;
		}

		private ControlScheme Load_V2(XmlDocument doc, string schemeName)
		{
			if(string.IsNullOrEmpty(schemeName))
				return null;

			ControlScheme scheme = null;
			var schemeNodes = SelectNodes(doc.DocumentElement, "ControlScheme");
			foreach(XmlNode node in schemeNodes)
			{
				if(ReadAttribute(node, "name") == schemeName)
				{
					scheme = ReadControlScheme_V2(node);
					break;
				}
			}

			return scheme;
		}

		private ControlScheme ReadControlScheme_V2(XmlNode node)
		{
			string name = ReadAttribute(node, "name", "Unnamed Control Scheme");
			string id = ReadAttribute(node, "id", null);
			ControlScheme scheme = new ControlScheme(name);
			scheme.UniqueID = id ?? ControlScheme.GenerateUniqueID();

			var descriptionNode = SelectSingleNode(node, "Description");
			if(descriptionNode != null)
			{
				scheme.Description = descriptionNode.InnerText;
			}

			var actionNodes = SelectNodes(node, "Action");
			foreach(XmlNode child in actionNodes)
			{
				ReadInputAction_V2(scheme, child);
			}

			return scheme;
		}

		private void ReadInputAction_V2(ControlScheme scheme, XmlNode node)
		{
			string name = ReadAttribute(node, "name", "Unnamed Action");
			InputAction action = scheme.CreateNewAction(name);
			action.Description = ReadNode(SelectSingleNode(node, "Description"));

			var bindingNodes = SelectNodes(node, "Binding");
			foreach(XmlNode child in bindingNodes)
			{
				ReadInputBinding_V2(action, child);
			}
		}

		private void ReadInputBinding_V2(InputAction action, XmlNode node)
		{
			InputBinding binding = action.CreateNewBinding();
			foreach(XmlNode child in node.ChildNodes)
			{
				switch(child.LocalName)
				{
				case "Positive":
					binding.Positive = InputBinding.StringToKey(child.InnerText);
					break;
				case "Negative":
					binding.Negative = InputBinding.StringToKey(child.InnerText);
					break;
				case "DeadZone":
					binding.DeadZone = ReadAsFloat(child);
					break;
				case "Gravity":
					binding.Gravity = ReadAsFloat(child, 1.0f);
					break;
				case "Sensitivity":
					binding.Sensitivity = ReadAsFloat(child, 1.0f);
					break;
				case "Snap":
					binding.Snap = ReadAsBool(child);
					break;
				case "Invert":
					binding.Invert = ReadAsBool(child);
					break;
				case "Type":
					binding.Type = InputBinding.StringToInputType(child.InnerText);
					break;
				case "Axis":
					binding.Axis = ReadAsInt(child);
					break;
				case "Joystick":
					binding.Joystick = ReadAsInt(child);
					break;
				case "GamepadButton":
					binding.GamepadButton = InputBinding.StringToGamepadButton(child.InnerText);
					break;
				case "GamepadAxis":
					binding.GamepadAxis = InputBinding.StringToGamepadAxis(child.InnerText);
					break;
				case "GamepadPlayer":
					binding.GamepadIndex = InputBinding.StringToGamepadIndex(child.InnerText);
					break;
				}
			}
		}
		#endregion

		#region [V1]
		private SaveData Load_V1(XmlDocument doc)
		{
			SaveData saveData = new SaveData();
			saveData.PlayerOneScheme = ReadAttribute(doc.DocumentElement, "playerOneDefault");
			saveData.PlayerTwoScheme = ReadAttribute(doc.DocumentElement, "playerTwoDefault");
			saveData.PlayerThreeScheme = ReadAttribute(doc.DocumentElement, "playerThreeDefault");
			saveData.PlayerFourScheme = ReadAttribute(doc.DocumentElement, "playerFourDefault");

			var schemeNodes = SelectNodes(doc.DocumentElement, "InputConfiguration");
			foreach(XmlNode node in schemeNodes)
			{
				saveData.ControlSchemes.Add(ReadControlScheme_V1(node));
			}

			return saveData;
		}

		private ControlScheme Load_V1(XmlDocument doc, string schemeName)
		{
			if(string.IsNullOrEmpty(schemeName))
				return null;

			ControlScheme scheme = null;
			var schemeNodes = SelectNodes(doc.DocumentElement, "InputConfiguration");
			foreach(XmlNode node in schemeNodes)
			{
				if(ReadAttribute(node, "name") == schemeName)
				{
					scheme = ReadControlScheme_V1(node);
					break;
				}
			}

			return scheme;
		}

		private ControlScheme ReadControlScheme_V1(XmlNode node)
		{
			string name = ReadAttribute(node, "name", "Unnamed Configuration");
			ControlScheme scheme = new ControlScheme(name);

			var actionNodes = SelectNodes(node, "AxisConfiguration");
			foreach(XmlNode child in actionNodes)
			{
				ReadInputAction_V1(scheme, child);
			}

			return scheme;
		}

		private void ReadInputAction_V1(ControlScheme scheme, XmlNode node)
		{
			string name = ReadAttribute(node, "name", "Unnamed Axis");
			InputAction action = scheme.CreateNewAction(name);
			InputBinding binding = action.CreateNewBinding();

			foreach(XmlNode child in node.ChildNodes)
			{
				switch(child.LocalName)
				{
				case "description":
					action.Description = child.InnerText;
					break;
				case "positive":
					binding.Positive = InputBinding.StringToKey(child.InnerText);
					break;
				case "negative":
					binding.Negative = InputBinding.StringToKey(child.InnerText);
					break;
				case "deadZone":
					binding.DeadZone = ReadAsFloat(child);
					break;
				case "gravity":
					binding.Gravity = ReadAsFloat(child, 1.0f);
					break;
				case "sensitivity":
					binding.Sensitivity = ReadAsFloat(child, 1.0f);
					break;
				case "snap":
					binding.Snap = ReadAsBool(child);
					break;
				case "invert":
					binding.Invert = ReadAsBool(child);
					break;
				case "type":
					binding.Type = InputBinding.StringToInputType(child.InnerText);
					break;
				case "axis":
					binding.Axis = ReadAsInt(child);
					break;
				case "joystick":
					binding.Joystick = ReadAsInt(child);
					break;
				}
			}

			if(binding.Type == InputType.Button || binding.Type == InputType.DigitalAxis)
			{
				XmlNode altPositiveNode = SelectSingleNode(node, "altPositive");
				XmlNode altNegativeNode = SelectSingleNode(node, "altNegative");
				InputBinding secondary = action.CreateNewBinding(binding);
				secondary.Positive = InputBinding.StringToKey(altPositiveNode.InnerText);
				secondary.Negative = InputBinding.StringToKey(altNegativeNode.InnerText);
			}
		}
		#endregion

		#region [Helper]
		private XmlNode SelectSingleNode(XmlNode parent, string name)
		{
#if UNITY_WSA && !UNITY_EDITOR
			return parent.ChildNodes.Cast<XmlNode>().First(nd => nd.LocalName == name);
#else
			return parent.SelectSingleNode(name);
#endif
		}

		private IEnumerable<XmlNode> SelectNodes(XmlNode parent, string name)
		{
#if UNITY_WSA && !UNITY_EDITOR
			return parent.ChildNodes.Cast<XmlNode>().Where(nd => nd.LocalName == name);
#else
			return parent.SelectNodes(name).Cast<XmlNode>();
#endif
		}

		private string ReadAttribute(XmlNode node, string attribute, string defValue = null)
		{
			if(node.Attributes[attribute] != null)
				return node.Attributes[attribute].InnerText;

			return defValue;
		}

		private int ReadAttributeAsInt(XmlNode node, string attribute, int defValue = 0)
		{
			string attributeValue = ReadAttribute(node, attribute);
			int value = 0;

			if(int.TryParse(attributeValue, out value))
				return value;

			return defValue;
		}

		private float ReadAttributeAsFloat(XmlNode node, string attribute, float defValue = 0.0f)
		{
			string attributeValue = ReadAttribute(node, attribute);
			float value = 0;

			if(float.TryParse(attributeValue, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
				return value;

			return defValue;
		}

		private bool ReadAttributeAsBool(XmlNode node, string attribute, bool defValue = false)
		{
			string attributeValue = ReadAttribute(node, attribute);
			bool value = false;

			if(bool.TryParse(attributeValue, out value))
				return value;

			return defValue;
		}

		private string ReadNode(XmlNode node, string defValue = null)
		{
			return node != null ? node.InnerText : defValue;
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
#endregion
	}
}
