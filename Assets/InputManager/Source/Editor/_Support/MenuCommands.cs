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
using UnityEditor;
using System.Collections;

namespace TeamUtilityEditor.IO.InputManager
{
	public static class MenuCommands
	{
		[MenuItem("Team Utility/Input Manager/Create Input Manager", false, 2)]
		private static void CreateInputManager()
		{
			GameObject gameObject = new GameObject("Input Manager");
			gameObject.AddComponent<TeamUtility.IO.InputManager>();
			
			Selection.activeGameObject = gameObject;
		}

        [MenuItem("Team Utility/Input Manager/Check For Updates", false, 400)]
        public static void CheckForUpdates()
        {
            Application.OpenURL("https://github.com/daemon3000/InputManager");
        }

        [MenuItem("Team Utility/Input Manager/Documentation", false, 401)]
		public static void OpenDocumentationPage()
		{
			Application.OpenURL("https://github.com/daemon3000/InputManager/wiki");
		}

		[MenuItem("Team Utility/Input Manager/Report Bug", false, 402)]
		public static void OpenReportBugPage()
		{
			Application.OpenURL("https://github.com/daemon3000/InputManager/issues");
		}

        [MenuItem("Team Utility/Input Manager/Contact", false, 403)]
        public static void OpenContactDialog()
        {
            string message = "Email: geambasu.cristi@gmail.com";
            EditorUtility.DisplayDialog("Contact", message, "Close");
        }

        [MenuItem("Team Utility/Input Manager/Forum", false, 404)]
		public static void OpenForumPage()
		{
			Application.OpenURL("http://forum.unity3d.com/threads/223321-Free-Custom-Input-Manager");
		}

		[MenuItem("Team Utility/Input Manager/About", false, 405)]
		public static void OpenAboutDialog()
		{
			string message = "Input Manager, MIT licensed\nCopyright \u00A9 2015 Cristian Alexandru Geambasu\nhttps://github.com/daemon3000/InputManager";
			EditorUtility.DisplayDialog("About", message, "OK");
		}
	}
}
