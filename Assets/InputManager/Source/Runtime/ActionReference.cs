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

namespace Luminosity.IO
{
	[CreateAssetMenu(fileName = "New Input Action Reference", menuName = "Luminosity/Input Manager/Input Action Reference")]
	public class ActionReference : ScriptableObject
	{
		[SerializeField]
		private string m_schemeName;
		[SerializeField]
		private string m_actionName;

		[System.NonSerialized]
		private InputAction m_cachedInputAction = null;

		public InputAction Get()
		{
			if(m_cachedInputAction == null && InputManager.Exists)
			{
				m_cachedInputAction = InputManager.GetAction(m_schemeName, m_actionName);
			}

			return m_cachedInputAction;
		}

		private void OnValidate()
		{
			if(InputManager.Exists)
			{
				m_cachedInputAction = InputManager.GetAction(m_schemeName, m_actionName);
			}
		}
	}
}