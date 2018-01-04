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

namespace Luminosity.IO.Examples
{
	public class PauseMenu : MonoBehaviour 
	{
		[SerializeField]
		private Canvas m_canvas;
		[SerializeField]
		private GameObject m_mainPage;
		[SerializeField]
		private GameObject m_controlsPage;
		[SerializeField]
		private GameObject m_editKeyboardPage;
		[SerializeField]
		private GameObject m_editGamepadPage;
		[SerializeField]
		private bool m_openOnStart;

		private bool m_isOpen;

		private void Start()
		{
			m_isOpen = false;
			m_canvas.gameObject.SetActive(false);

			if(m_openOnStart)
				Open();
		}

		private void Update()
		{
			if(InputManager.GetButtonDown("PauseMenu"))
			{
				if(!m_isOpen)
					Open();
			}
		}

		public void Open()
		{
			if(!m_isOpen && !PauseManager.IsPaused)
			{
				m_isOpen = true;
				m_canvas.gameObject.SetActive(true);
				ChangeToMainPage();
				PauseManager.Pause();
			}
		}

		public void Close()
		{
			if(m_isOpen)
			{
				m_isOpen = false;
				m_canvas.gameObject.SetActive(false);
				PauseManager.UnPause();
			}
		}

		public void ChangeToMainPage()
		{
			m_controlsPage.SetActive(false);
			m_editKeyboardPage.SetActive(false);
			m_editGamepadPage.SetActive(false);
			m_mainPage.SetActive(true);
		}

		public void ChangeToControlsPage()
		{
			m_mainPage.SetActive(false);
			m_editKeyboardPage.SetActive(false);
			m_editGamepadPage.SetActive(false);
			m_controlsPage.SetActive(true);
		}

		public void ChangeToEditKeyboardPage()
		{
			m_mainPage.SetActive(false);
			m_controlsPage.SetActive(false);
			m_editGamepadPage.SetActive(false);
			m_editKeyboardPage.SetActive(true);
		}

		public void ChangeToEditGamepadPage()
		{
			m_mainPage.SetActive(false);
			m_controlsPage.SetActive(false);
			m_editKeyboardPage.SetActive(false);
			m_editGamepadPage.SetActive(true);
		}

		public void Quit()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#else
			Application.Quit();
#endif
		}
	}
}