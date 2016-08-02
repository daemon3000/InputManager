using UnityEngine;
using UnityEngine.Serialization;

namespace TeamUtility.IO.Examples
{
	public class PauseMenu : MonoBehaviour 
	{
		[SerializeField]
		[FormerlySerializedAs("m_canvas")]
		private Canvas _canvas;

		[SerializeField]
		[FormerlySerializedAs("m_mainPage")]
		private GameObject _mainPage;

		[SerializeField]
		[FormerlySerializedAs("m_controlsPage")]
		private GameObject _controlsPage;

		[SerializeField]
		[FormerlySerializedAs("m_editKeyboardPage")]
		private GameObject _editKeyboardPage;

		[SerializeField]
		[FormerlySerializedAs("m_editGamepadPage")]
		private GameObject _editGamepadPage;

		[SerializeField]
		[FormerlySerializedAs("m_openOnStart")]
		private bool _openOnStart;

		private bool _isOpen;

		private void Start()
		{
			_isOpen = false;
			_canvas.gameObject.SetActive(false);

			if(_openOnStart)
				Open();
		}

		private void Update()
		{
			if(InputManager.GetButtonDown("PauseMenu"))
			{
				if(!_isOpen)
					Open();
			}
		}

		public void Open()
		{
			if(!_isOpen && !PauseManager.IsPaused)
			{
				_isOpen = true;
				_canvas.gameObject.SetActive(true);
				ChangeToMainPage();
				PauseManager.Pause();
			}
		}

		public void Close()
		{
			if(_isOpen)
			{
				_isOpen = false;
				_canvas.gameObject.SetActive(false);
				PauseManager.UnPause();
			}
		}

		public void ChangeToMainPage()
		{
			_controlsPage.SetActive(false);
			_editKeyboardPage.SetActive(false);
			_editGamepadPage.SetActive(false);
			_mainPage.SetActive(true);
		}

		public void ChangeToControlsPage()
		{
			_mainPage.SetActive(false);
			_editKeyboardPage.SetActive(false);
			_editGamepadPage.SetActive(false);
			_controlsPage.SetActive(true);
		}

		public void ChangeToEditKeyboardPage()
		{
			_mainPage.SetActive(false);
			_controlsPage.SetActive(false);
			_editGamepadPage.SetActive(false);
			_editKeyboardPage.SetActive(true);
		}

		public void ChangeToEditGamepadPage()
		{
			_mainPage.SetActive(false);
			_controlsPage.SetActive(false);
			_editKeyboardPage.SetActive(false);
			_editGamepadPage.SetActive(true);
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