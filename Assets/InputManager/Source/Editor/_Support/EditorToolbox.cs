using UnityEngine;
using UnityEditor;
using System;
using System.IO;
using System.Reflection;
using System.Collections;
using TeamUtility.IO;

namespace TeamUtility.Editor.IO.InputManager
{
	public static class EditorToolbox
	{
		private static string _snapshotFile;
		
		public static bool CanLoadSnapshot()
		{
			if(_snapshotFile == null)
			{
				_snapshotFile = Path.Combine(Application.temporaryCachePath, "input_config.xml");
			}
			
			return File.Exists(_snapshotFile);
		}
		
		public static void CreateSnapshot(TeamUtility.IO.InputManager inputManager)
		{
			if(_snapshotFile == null)
			{
				_snapshotFile = Path.Combine(Application.temporaryCachePath, "input_config.xml");
			}
			
			InputSaverXML inputSaver = new InputSaverXML(_snapshotFile);
			inputSaver.Save(inputManager.inputConfigurations, inputManager.defaultConfiguration);
		}
		
		public static void LoadSnapshot(TeamUtility.IO.InputManager inputManager)
		{
			if(!CanLoadSnapshot())
				return;
			
			InputLoaderXML inputLoader = new InputLoaderXML(_snapshotFile);
			inputLoader.Load(out inputManager.inputConfigurations, out inputManager.defaultConfiguration);
		}
		
		public static void ShowStartupWarning()
		{
			string key = string.Concat(PlayerSettings.productName, ".InputManager.StartupWarning");
			
			if(!EditorPrefs.GetBool(key, false))
			{
				string message = "In order to use InputManager you need to overwrite your project's input settings.\n\nDo you want to overwrite the input settings now?\nYou can always do it from the File menu.";
				if(EditorUtility.DisplayDialog("Warning", message, "Yes", "No"))
				{
					OverwriteInputSettings();
				}
				EditorPrefs.SetBool(key, true);
			}
		}
		
		public static void OverwriteInputSettings()
		{
			TextAsset textAsset = Resources.Load("InputManager") as TextAsset;
			if(textAsset == null)
			{
				EditorUtility.DisplayDialog("Error", "Unable to load input settings from the Resources folder.", "OK");
				return;
			}
			
			int length = Application.dataPath.LastIndexOf('/');
			string projectSettingsFolder = string.Concat(Application.dataPath.Substring(0, length), "/ProjectSettings");
			if(!Directory.Exists(projectSettingsFolder))
			{
				Resources.UnloadAsset(textAsset);
				EditorUtility.DisplayDialog("Error", "Unable to get the correct path to the ProjectSetting folder.", "OK");
				return;
			}
			
			string inputManagerPath = string.Concat(projectSettingsFolder, "/InputManager.asset");
			File.Delete(inputManagerPath);
			using(StreamWriter writer = File.CreateText(inputManagerPath))
			{
				writer.Write(textAsset.text);
			}
			EditorUtility.DisplayDialog("Success", "The input settings have been successfully replaced.\nYou might need to minimize and restore Unity to reimport the new settings.", "OK");
			
			Resources.UnloadAsset(textAsset);
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
		
		/// <summary>
		/// Used to get access to the hidden toolbar search field.
		/// Credits go to the user TowerOfBricks for finding the way to do it.
		/// </summary>
		public static string SearchField(string searchString, params GUILayoutOption[] layoutOptions)
		{
			Type type = typeof(EditorGUILayout);
			string methodName = "ToolbarSearchField";
			System.Object[] parameters = new System.Object[] { searchString, layoutOptions };
			string result = null;
			
			Type[] types = new Type[parameters.Length];
			for(int i = 0; i < types.Length; i++)
			{
				types[i] = parameters[i].GetType();
			}
			MethodInfo method = type.GetMethod(methodName, (BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Public),
												null, types, null);
			
			if(method.IsStatic)
			{
				result = (string)method.Invoke(null, parameters);
			}
			else
			{
				var bindingFlags = BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.NonPublic |
									BindingFlags.Instance | BindingFlags.CreateInstance;
				System.Object obj = type.InvokeMember(null, bindingFlags, null, null, new System.Object[0]);
				
				result = (string)method.Invoke(obj, parameters);
			}
			
			return (result != null) ? result : "";
		}
	}
}
