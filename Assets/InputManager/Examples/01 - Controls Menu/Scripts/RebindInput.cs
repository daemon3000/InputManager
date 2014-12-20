using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

namespace TeamUtility.IO.Examples
{
	[RequireComponent(typeof(Image))]
	public class RebindInput : MonoBehaviour, IPointerDownHandler 
	{
		public enum RebindType
		{
			Keyboard, GamepadButton, GamepadAxis
		}

		[SerializeField] private Sprite m_normalState;
		[SerializeField] private Sprite m_scanningState;
		[SerializeField] private Text m_keyDescription;
		[SerializeField] private string m_inputConfigName;
		[SerializeField] private string m_axisConfigName;
		[SerializeField] private string m_cancelButton;
		[SerializeField] private float m_timeout;
		[SerializeField] private bool m_changePositiveKey;
		[SerializeField] private bool m_changeAltKey;
		[SerializeField] private RebindType m_rebindType;
		
		private AxisConfiguration m_axisConfig;
		private Image m_image;
		private string[] m_axisNames = new string[] { "X", "Y", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th" };
		
		private void Awake()
		{
			m_image = GetComponent<Image>();
			m_image.overrideSprite = m_normalState;
			InitializeAxisConfig();
			
			//	The axis config needs to be reinitialized because loading can invalidate
			//	the input configurations
			InputManager.Instance.Loaded += InitializeAxisConfig;
			InputManager.Instance.ConfigurationDirty += HandleConfigurationDirty;
		}
		
		private void OnDestroy()
		{
			if(InputManager.Instance != null)
			{
				InputManager.Instance.Loaded -= InitializeAxisConfig;
				InputManager.Instance.ConfigurationDirty -= HandleConfigurationDirty;
			}
		}
		
		private void InitializeAxisConfig()
		{
			m_axisConfig = InputManager.GetAxisConfiguration(m_inputConfigName, m_axisConfigName);
			if(m_axisConfig != null)
			{
				if(m_rebindType == RebindType.Keyboard || m_rebindType == RebindType.GamepadButton)
				{
					if(m_changePositiveKey)
					{
						if(m_changeAltKey)
							m_keyDescription.text = m_axisConfig.altPositive == KeyCode.None ? "" : m_axisConfig.altPositive.ToString();
						else
							m_keyDescription.text = m_axisConfig.positive == KeyCode.None ? "" : m_axisConfig.positive.ToString();
					}
					else
					{
						if(m_changeAltKey)
							m_keyDescription.text = m_axisConfig.altNegative == KeyCode.None ? "" : m_axisConfig.altNegative.ToString();
						else
							m_keyDescription.text = m_axisConfig.negative == KeyCode.None ? "" : m_axisConfig.negative.ToString();
					}
				}
				else
				{
					m_keyDescription.text = m_axisNames[m_axisConfig.axis];
				}
			}
			else
			{
				m_keyDescription.text = "";
				Debug.LogError(string.Format(@"Input configuration '{0}' does not exist or axis '{1}' does not exist", m_inputConfigName, m_axisConfigName));
			}
		}

		private void HandleConfigurationDirty(InputConfiguration inputConfig)
		{
			if(inputConfig.name == m_inputConfigName)
				InitializeAxisConfig();
		}

		public void OnPointerDown(PointerEventData data)
		{
			if(!InputManager.IsScanning && m_axisConfig != null)
			{
				if(m_rebindType == RebindType.Keyboard || m_rebindType == RebindType.GamepadButton)
					StartKeyScan();
				else
					StartJoystickAxisScan();
			}
		}

		private void StartKeyScan()
		{
			m_image.overrideSprite = m_scanningState;
			m_keyDescription.text = "...";
			InputManager.StartKeyScan(HandleKeyScan, m_timeout, string.IsNullOrEmpty(m_cancelButton) ? null : m_cancelButton);
		}
		
		private bool HandleKeyScan(KeyCode key, object[] arg)
		{
			//	When you return false you tell the InputManager that it should keep scaning for other keys
			if(!IsKeyValid(key))
				return false;
			
			//	The key is KeyCode.None when the timeout has been reached or the scan has been canceled
			//	If the key is KeyCode.Backspace clear the current binding
			if(key != KeyCode.None)
			{
				key = (key == KeyCode.Backspace) ? KeyCode.None : key;
				if(m_changePositiveKey)
				{
					if(m_changeAltKey)
						m_axisConfig.altPositive = key;
					else
						m_axisConfig.positive = key;
				}
				else
				{
					if(m_changeAltKey)
						m_axisConfig.altNegative = key;
					else
						m_axisConfig.negative = key;
				}
			}

			m_image.overrideSprite = m_normalState;
			m_keyDescription.text = (key == KeyCode.None) ? "" : key.ToString();
			return true;
		}
		
		private bool IsKeyValid(KeyCode key)
		{
			if(m_rebindType == RebindType.Keyboard)
			{
				if((int)key >= (int)KeyCode.JoystickButton0)
					return false;
				if(key == KeyCode.LeftApple || key == KeyCode.RightApple)
					return false;
				if(key == KeyCode.LeftWindows || key == KeyCode.RightWindows)
					return false;
			}
			else if(m_rebindType == RebindType.GamepadButton)
			{
				//	Allow KeyCode.None to pass because it means that the scan has been canceled
				//	Allow KeyCode.Backspace to pass so it can clear the current binding
				if((int)key < (int)KeyCode.JoystickButton0 && key != KeyCode.None && key != KeyCode.Backspace)
					return false;
			}
			else
				return false;
			
			return true;
		}

		private void StartJoystickAxisScan()
		{
			m_image.overrideSprite = m_scanningState;
			m_keyDescription.text = "...";
			InputManager.StartJoystickAxisScan(HandleJoystickAxisScan, 0, m_timeout, string.IsNullOrEmpty(m_cancelButton) ? null : m_cancelButton);
		}

		private bool HandleJoystickAxisScan(int axis, object[] arg)
		{
			if(axis >= 0)
			{
				m_axisConfig.SetAnalogAxis(0, axis);
				m_image.overrideSprite = m_normalState;
				m_keyDescription.text = m_axisNames[axis];
				return true;
			}

			return false;
		}
	}
}