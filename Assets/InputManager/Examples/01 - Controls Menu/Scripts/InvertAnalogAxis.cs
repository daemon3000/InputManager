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
using UnityEngine.UI;

namespace Luminosity.IO.Examples
{
	public class InvertAnalogAxis : MonoBehaviour 
	{
		[SerializeField]
		private string m_controlSchemeName;
		[SerializeField]
		private string m_actionName;
		[SerializeField]
		private int m_bindingIndex;
		[SerializeField]
		private Text m_status;

		private InputAction m_inputAction;

		private void Awake()
		{
			InitializeInputAction();
			InputManager.Loaded += InitializeInputAction;
		}

		private void OnDestroy()
		{
			InputManager.Loaded -= InitializeInputAction;
		}

		private void InitializeInputAction()
		{
			m_inputAction = InputManager.GetAction(m_controlSchemeName, m_actionName);
			if(m_inputAction != null)
			{
				m_status.text = m_inputAction.Bindings[m_bindingIndex].Invert ? "On" : "Off";
			}
			else
			{
				m_status.text = "Off";
				Debug.LogErrorFormat("Input configuration '{0}' does not exist or axis '{1}' does not exist", m_controlSchemeName, m_actionName);
			}
		}

		public void OnClick()
		{
			if(m_inputAction != null)
			{
				m_inputAction.Bindings[m_bindingIndex].Invert = !m_inputAction.Bindings[m_bindingIndex].Invert;
				m_status.text = m_inputAction.Bindings[m_bindingIndex].Invert ? "On" : "Off";
			}
		}
	}
}