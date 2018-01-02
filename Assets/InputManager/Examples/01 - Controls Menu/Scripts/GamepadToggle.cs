using UnityEngine;
using UnityEngine.UI;

namespace TeamUtility.IO.Examples
{
	public class GamepadToggle : MonoBehaviour 
	{
		[SerializeField]
		private string m_keyboardScheme;
		[SerializeField]
		private string m_gamepadScheme;
		[SerializeField]
		private Text m_status;

		private bool m_gamepadOn;

		private void Awake()
		{
			if(InputManager.PlayerOneControlScheme.Name == m_keyboardScheme)
			{
				m_gamepadOn = false;
				m_status.text = "Gamepad: Off";
			}
			else
			{
				m_gamepadOn = true;
				m_status.text = "Gamepad: On";
			}

			InputManager.Loaded += HandleInputLoaded;
		}

		private void OnDestroy()
		{
			InputManager.Loaded -= HandleInputLoaded;
		}

		private void HandleInputLoaded()
		{
			if(m_gamepadOn)
			{
				InputManager.SetControlScheme(m_gamepadScheme, PlayerID.One);
				m_status.text = "Gamepad: On";
			}
			else
			{
				InputManager.SetControlScheme(m_keyboardScheme, PlayerID.One);
				m_status.text = "Gamepad: Off";
			}
		}

		public void Toggle()
		{
			if(m_gamepadOn)
			{
				InputManager.SetControlScheme(m_keyboardScheme, PlayerID.One);
				m_status.text = "Gamepad: Off";
				m_gamepadOn = false;
			}
			else
			{
				InputManager.SetControlScheme(m_gamepadScheme, PlayerID.One);
				m_status.text = "Gamepad: On";
				m_gamepadOn = true;
			}
		}
	}
}
