using UnityEngine;
using System.Collections;

namespace TeamUtility.IO.Examples
{
	public class PauseMenu : MonoBehaviour 
	{
		[SerializeField] private Canvas m_canvas;
		[SerializeField] private GameObject m_mainPage;
		[SerializeField] private GameObject m_controlsPage;
		[SerializeField] private GameObject m_editKeyboardPage;
		[SerializeField] private GameObject m_editGamepadPage;
		[SerializeField] private bool m_openOnStart;

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