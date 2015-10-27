using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace TeamUtility.IO.Examples
{
	public class GamepadToggle : MonoBehaviour 
	{
		[SerializeField] private string m_keyboardInputConfig;
		[SerializeField] private string m_gamepadInputConfig;
		[SerializeField] private Text m_status;

		private bool m_gamepadOn;

		private void Awake()
		{
			if(InputManager.PlayerOneConfiguration.name == m_keyboardInputConfig)
			{
				m_gamepadOn = false;
				m_status.text = "Gamepad: Off";
			}
			else
			{
				m_gamepadOn = true;
				m_status.text = "Gamepad: On";
			}
			InputManager.Instance.Loaded += HandleInputLoaded;
		}

		private void OnDestroy()
		{
			if(InputManager.Instance != null)
				InputManager.Instance.Loaded -= HandleInputLoaded;
		}

		private void HandleInputLoaded()
		{
			if(m_gamepadOn)
			{
				InputManager.SetInputConfiguration(m_gamepadInputConfig, PlayerID.One);
				m_status.text = "Gamepad: On";
			}
			else
			{
				InputManager.SetInputConfiguration(m_keyboardInputConfig, PlayerID.One);
				m_status.text = "Gamepad: Off";
			}
		}

		public void Toggle()
		{
			if(m_gamepadOn)
			{
				InputManager.SetInputConfiguration(m_keyboardInputConfig, PlayerID.One);
				m_status.text = "Gamepad: Off";
				m_gamepadOn = false;
			}
			else
			{
				InputManager.SetInputConfiguration(m_gamepadInputConfig, PlayerID.One);
				m_status.text = "Gamepad: On";
				m_gamepadOn = true;
			}
		}
	}
}
