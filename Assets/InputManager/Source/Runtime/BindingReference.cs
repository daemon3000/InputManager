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
	[CreateAssetMenu(fileName = "New Input Binding Reference", menuName = "Luminosity/Input Manager/Input Binding Reference")]
	public class BindingReference : ScriptableObject
	{
		[SerializeField]
		private string m_schemeName;
		[SerializeField]
		private string m_actionName;
		[SerializeField]
		private int m_bindingIndex;

		[System.NonSerialized]
		private InputBinding m_cachedInputBinding = null;

		public InputBinding Get()
		{
			if(m_cachedInputBinding == null && InputManager.Exists)
			{
				var action = InputManager.GetAction(m_schemeName, m_actionName);
				if(action != null)
				{
					m_cachedInputBinding = action.GetBinding(m_bindingIndex);
				}
			}

			return m_cachedInputBinding;
		}

		private void OnValidate()
		{
			if(InputManager.Exists)
			{
				var action = InputManager.GetAction(m_schemeName, m_actionName);
				if(action != null)
				{
					m_cachedInputBinding = action.GetBinding(m_bindingIndex);
				}
				else
				{
					m_cachedInputBinding = null;
				}
			}
		}
	}
}