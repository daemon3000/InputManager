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
using UnityEditor;
using System.Collections;

namespace TeamUtility.Editor.IO.InputManager
{
	public static class MenuCommands
	{
		[MenuItem("Team Utility/Input Manager/Create Input Manager", false, 1)]
		private static void CreateInputManager()
		{
			GameObject gameObject = new GameObject("Input Manager");
			gameObject.AddComponent<TeamUtility.IO.InputManager>();
			
			Selection.activeGameObject = gameObject;
		}

		[MenuItem("Team Utility/Input Manager/Forum", false, 200)]
		public static void OpenForumPage()
		{
			Application.OpenURL("http://forum.unity3d.com/threads/223321-Free-Custom-Input-Manager");
		}

		[MenuItem("Team Utility/Input Manager/About", false, 201)]
		public static void OpenAboutDialog()
		{
			string message = string.Format("Input Manager v{0}, MIT licensed\nCopyright \u00A9 2014 Cristian Alexandru Geambasu\nhttps://github.com/daemon3000/InputManager", TeamUtility.IO.InputManager.VERSION);
			EditorUtility.DisplayDialog("About", message, "OK");
		}
	}
}