using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

namespace TeamUtility.IO.Examples
{
	public class GamepadToggle : MonoBehaviour 
	{
		[SerializeField]
		[FormerlySerializedAs("m_keyboardInputConfig")]
		private string _keyboardInputConfig;
		[SerializeField]
		[FormerlySerializedAs("m_gamepadInputConfig")]
		private string _gamepadInputConfig;
		[SerializeField]
		[FormerlySerializedAs("m_status")]
		private Text _status;

		private bool _gamepadOn;

		private void Awake()
		{
			if(InputManager.PlayerOneConfiguration.name == _keyboardInputConfig)
			{
				_gamepadOn = false;
				_status.text = "Gamepad: Off";
			}
			else
			{
				_gamepadOn = true;
				_status.text = "Gamepad: On";
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
			if(_gamepadOn)
			{
				InputManager.SetInputConfiguration(_gamepadInputConfig, PlayerID.One);
				_status.text = "Gamepad: On";
			}
			else
			{
				InputManager.SetInputConfiguration(_keyboardInputConfig, PlayerID.One);
				_status.text = "Gamepad: Off";
			}
		}

		public void Toggle()
		{
			if(_gamepadOn)
			{
				InputManager.SetInputConfiguration(_keyboardInputConfig, PlayerID.One);
				_status.text = "Gamepad: Off";
				_gamepadOn = false;
			}
			else
			{
				InputManager.SetInputConfiguration(_gamepadInputConfig, PlayerID.One);
				_status.text = "Gamepad: On";
				_gamepadOn = true;
			}
		}
	}
}
