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
using System.IO;
using Luminosity.IO;
using UnityInputConverter;

namespace LuminosityEditor.IO
{
	public static partial class EditorToolbox
	{
		public const string DEFAULT_INPUT_PROFILE = "input_manager_default_scheme";

		private static string m_snapshotFile;
        private static string[] m_buttonNames;
		private static string[] m_axisNames;
		private static string[] m_joystickNames;

        public static string[] GenerateJoystickButtonNames()
        {
            if(m_buttonNames == null || m_buttonNames.Length != InputBinding.MAX_JOYSTICK_BUTTONS)
            {
                m_buttonNames = new string[InputBinding.MAX_JOYSTICK_BUTTONS];
                for(int i = 0; i < InputBinding.MAX_JOYSTICK_BUTTONS; i++)
                {
                    m_buttonNames[i] = "Joystick Button " + i;
                }
            }

            return m_buttonNames;
        }

		public static string[] GenerateJoystickAxisNames()
		{
			if(m_axisNames == null || m_axisNames.Length != InputBinding.MAX_JOYSTICK_AXES)
			{
				m_axisNames = new string[InputBinding.MAX_JOYSTICK_AXES];
				for(int i = 0; i < InputBinding.MAX_JOYSTICK_AXES; i++)
				{
					if(i == 0)
						m_axisNames[i] = "X";
					else if(i == 1)
						m_axisNames[i] = "Y";
					else if(i == 2)
						m_axisNames[i] = "3rd axis (Joysticks and Scrollwheel)";
					else if(i == 21)
						m_axisNames[i] = "21st axis (Joysticks)";
					else if(i == 22)
						m_axisNames[i] = "22nd axis (Joysticks)";
					else if(i == 23)
						m_axisNames[i] = "23rd axis (Joysticks)";
					else
						m_axisNames[i] = string.Format("{0}th axis (Joysticks)", i + 1);
				}
			}

			return m_axisNames;
		}

		public static string[] GenerateJoystickNames()
		{
			if(m_joystickNames == null || m_joystickNames.Length != InputBinding.MAX_JOYSTICKS)
			{
				m_joystickNames = new string[InputBinding.MAX_JOYSTICKS];
				for(int i = 0; i < InputBinding.MAX_JOYSTICKS; i++)
				{
					m_joystickNames[i] = string.Format("Joystick {0}", i + 1);
				}
			}

			return m_joystickNames;
		}

		public static bool CanLoadSnapshot()
		{
			if(m_snapshotFile == null)
			{
				m_snapshotFile = Path.Combine(Application.temporaryCachePath, "input_config.xml");
			}
			
			return File.Exists(m_snapshotFile);
		}
		
		public static void CreateSnapshot(InputManager inputManager)
		{
			if(m_snapshotFile == null)
			{
				m_snapshotFile = Path.Combine(Application.temporaryCachePath, "input_config.xml");
			}

			InputSaverXML inputSaver = new InputSaverXML(m_snapshotFile);
			inputSaver.Save(inputManager.GetSaveData());
		}
		
		public static void LoadSnapshot(InputManager inputManager)
		{
			if(!CanLoadSnapshot())
				return;

			InputLoaderXML inputLoader = new InputLoaderXML(m_snapshotFile);
			inputManager.SetSaveData(inputLoader.Load());
		}
		
		public static void ShowStartupWarning()
		{
			string key = string.Concat(PlayerSettings.companyName, ".", PlayerSettings.productName, ".InputManager.StartupWarning");
			
			if(!EditorPrefs.GetBool(key, false))
			{
				string message = "In order to use the InputManager plugin you need to overwrite your project's input settings. Your old input axes will be exported to a file which can be imported at a later time from the File menu.\n\nDo you want to overwrite the input settings now?\nYou can always do it later from the File menu.";
				if(EditorUtility.DisplayDialog("Warning", message, "Yes", "No"))
				{
					if(OverwriteProjectSettings())
						EditorPrefs.SetBool(key, true);
				}
			}
		}
		
		public static bool OverwriteProjectSettings()
		{
			int length = Application.dataPath.LastIndexOf('/');
			string projectSettingsFolder = string.Concat(Application.dataPath.Substring(0, length), "/ProjectSettings");
			string inputManagerPath = string.Concat(projectSettingsFolder, "/InputManager.asset");

			if(!Directory.Exists(projectSettingsFolder))
			{
				EditorUtility.DisplayDialog("Error", "Unable to get the correct path to the ProjectSetting folder.", "OK");
				return false;
			}

			if(!File.Exists(inputManagerPath))
			{
				EditorUtility.DisplayDialog("Error", "Unable to get the correct path to the InputManager file from the ProjectSettings folder.", "OK");
				return false;
			}

			int option = EditorUtility.DisplayDialogComplex("Warning", "Do you want to export your old input settings?\n\nYour project needs to have asset serialization mode set to 'Force Text' in the Editor Settings. If text serialization is not enabled press the Abort button.\n\nYou can resume this process after text serialization is enabled from the File menu.", "Yes", "No", "Abort");
			string exportPath = null;

			if(option == 0)
			{
				exportPath = EditorUtility.SaveFilePanel("Export old input axes", "", "unity_input_export", "xml");
				if(string.IsNullOrEmpty(exportPath))
				{
					if(!EditorUtility.DisplayDialog("Warning", "You chose not to export your old input settings. They will be lost forever. Are you sure you want to continue?", "Yes", "No"))
						return false;
				}
			}
			else
			{
				if(option == 1)
				{
					if(!EditorUtility.DisplayDialog("Warning", "You chose not to export your old input settings. They will be lost forever. Are you sure you want to continue?", "Yes", "No"))
						return false;
				}
				else
				{
					return false;
				}
			}

			InputConverter inputConverter = new InputConverter();
			if(!string.IsNullOrEmpty(exportPath))
			{
				try
				{
					inputConverter.ConvertUnityInputManager(inputManagerPath, exportPath);
				}
				catch(System.Exception ex)
				{
					Debug.LogException(ex);

					string message = "Failed to export your old input settings. Please make sure asset serialization mode is set to 'Forced Text' in the Editor Settings.\n\nDo you want to continue? If you continue your old input settings will be lost forever.";
					if(!EditorUtility.DisplayDialog("Error", message, "Yes", "No"))
						return false;
				}
			}

			inputConverter.GenerateDefaultUnityInputManager(inputManagerPath);

			EditorUtility.DisplayDialog("Success", "The input settings have been successfully replaced.\n\nYou might need to minimize and restore Unity to reimport the new settings.", "OK");

			return true;
		}
		
		public static void KeyCodeField(ref string keyString, ref bool isEditing, string label, string controlName, KeyCode currentKey)
		{
			GUI.SetNextControlName(controlName);
			bool hasFocus = (GUI.GetNameOfFocusedControl() == controlName);
			if(!isEditing && hasFocus)
			{
				keyString = currentKey == KeyCode.None ? string.Empty : currentKey.ToString();
			}
			
			isEditing = hasFocus;
			if(isEditing)
			{
				keyString = EditorGUILayout.TextField(label, keyString);
			}
			else
			{
				EditorGUILayout.TextField(label, currentKey == KeyCode.None ? string.Empty : currentKey.ToString());
			}
		}

		public static Texture2D GetUnityIcon(string name)
		{
			return EditorGUIUtility.Load(name + ".png") as Texture2D;
		}

		public static Texture2D GetCustomIcon(string name)
		{
			return Resources.Load<Texture2D>(name) as Texture2D;
		}
	}
}
