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
		[SerializeField] private bool m_allowAnalogButton;

		[SerializeField] 
		[Range(0, AxisConfiguration.MaxJoysticks)]
		private int m_joystick = 0;

		[SerializeField] private RebindType m_rebindType;
		
		private AxisConfiguration m_axisConfig;
		private Image m_image;
		private static string[] m_axisNames = new string[] { "X", "Y", "3rd", "4th", "5th", "6th", "7th", "8th", "9th", "10th" };
		
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

		private void HandleConfigurationDirty(string configName)
		{
			if(configName == m_inputConfigName)
				InitializeAxisConfig();
		}

		public void OnPointerDown(PointerEventData data)
		{
			StartCoroutine(StartInputScanDelayed());
		}

		private IEnumerator StartInputScanDelayed()
		{
			yield return null;

			if(!InputManager.IsScanning && m_axisConfig != null)
			{
				m_image.overrideSprite = m_scanningState;
				m_keyDescription.text = "...";
				
				ScanSettings settings;
				settings.joystick = m_joystick;
				settings.cancelScanButton = m_cancelButton;
				settings.timeout = m_timeout;
				settings.userData = null;
				if(m_rebindType == RebindType.GamepadAxis)
				{
					settings.scanFlags = ScanFlags.JoystickAxis;
					InputManager.StartScan(settings, HandleJoystickAxisScan);
				}
				else if(m_rebindType == RebindType.GamepadButton)
				{
					settings.scanFlags = ScanFlags.Key | ScanFlags.JoystickButton;
					if(m_rebindType == RebindType.GamepadButton && m_allowAnalogButton)
						settings.scanFlags = settings.scanFlags | ScanFlags.JoystickAxis;
					InputManager.StartScan(settings, HandleJoystickButtonScan);
				}
				else
				{
					settings.scanFlags = ScanFlags.Key;
					InputManager.StartScan(settings, HandleKeyScan);
				}
			}
		}
		
		private bool HandleKeyScan(ScanResult result)
		{
			//	When you return false you tell the InputManager that it should keep scaning for other keys
			if(!IsKeyValid(result.key))
				return false;
			
			//	The key is KeyCode.None when the timeout has been reached or the scan has been canceled
			if(result.key != KeyCode.None)
			{
				//	If the key is KeyCode.Backspace clear the current binding
				result.key = (result.key == KeyCode.Backspace) ? KeyCode.None : result.key;
				if(m_changePositiveKey)
				{
					if(m_changeAltKey)
						m_axisConfig.altPositive = result.key;
					else
						m_axisConfig.positive = result.key;
				}
				else
				{
					if(m_changeAltKey)
						m_axisConfig.altNegative = result.key;
					else
						m_axisConfig.negative = result.key;
				}
				m_keyDescription.text = (result.key == KeyCode.None) ? "" : result.key.ToString();
			}
			else
			{
				KeyCode currentKey = GetCurrentKeyCode();
				m_keyDescription.text = (currentKey == KeyCode.None) ? "" : currentKey.ToString();
			}

			m_image.overrideSprite = m_normalState;
			return true;
		}

		private bool IsKeyValid(KeyCode key)
		{
			bool isValid = true;

			if(m_rebindType == RebindType.Keyboard)
			{
				if((int)key >= (int)KeyCode.JoystickButton0)
					isValid = false;
				else if(key == KeyCode.LeftApple || key == KeyCode.RightApple)
					isValid = false;
				else if(key == KeyCode.LeftWindows || key == KeyCode.RightWindows)
					isValid = false;
			}
			else
			{
				isValid = false;
			}

			return isValid;
		}

		private bool HandleJoystickButtonScan(ScanResult result)
		{
			if(result.scanFlags == ScanFlags.Key || result.scanFlags == ScanFlags.JoystickButton)
			{
				//	When you return false you tell the InputManager that it should keep scaning for other keys
				if(!IsJoytickButtonValid(result.key))
					return false;
				
				//	The key is KeyCode.None when the timeout has been reached or the scan has been canceled
				if(result.key != KeyCode.None)
				{
					//	If the key is KeyCode.Backspace clear the current binding
					result.key = (result.key == KeyCode.Backspace) ? KeyCode.None : result.key;
					m_axisConfig.type = InputType.Button;
					if(m_changePositiveKey)
					{
						if(m_changeAltKey)
							m_axisConfig.altPositive = result.key;
						else
							m_axisConfig.positive = result.key;
					}
					else
					{
						if(m_changeAltKey)
							m_axisConfig.altNegative = result.key;
						else
							m_axisConfig.negative = result.key;
					}
					m_keyDescription.text = (result.key == KeyCode.None) ? "" : result.key.ToString();
				}
				else
				{
					if(m_axisConfig.type == InputType.Button)
					{
						KeyCode currentKey = GetCurrentKeyCode();
						m_keyDescription.text = (currentKey == KeyCode.None) ? "" : currentKey.ToString();
					}
					else
					{
						m_keyDescription.text = (m_axisConfig.invert ? "-" : "+") + m_axisNames[m_axisConfig.axis];
					}
				}
				m_image.overrideSprite = m_normalState;
			}
			else
			{
				//	The axis is negative when the timeout has been reached or the scan has been canceled
				if(result.joystickAxis >= 0)
				{
					m_axisConfig.type = InputType.AnalogButton;
					m_axisConfig.invert = result.joystickAxisValue < 0.0f;
					m_axisConfig.SetAnalogButton(m_joystick, result.joystickAxis);
					m_keyDescription.text = (m_axisConfig.invert ? "-" : "+") + m_axisNames[m_axisConfig.axis];
				}
				else
				{
					if(m_axisConfig.type == InputType.AnalogButton)
					{
						m_keyDescription.text = (m_axisConfig.invert ? "-" : "+") + m_axisNames[m_axisConfig.axis];
					}
					else
					{
						KeyCode currentKey = GetCurrentKeyCode();
						m_keyDescription.text = (currentKey == KeyCode.None) ? "" : currentKey.ToString();
					}
				}
				m_image.overrideSprite = m_normalState;
			}
			
			return true;
		}

		private bool IsJoytickButtonValid(KeyCode key)
		{
			bool isValid = true;
			
			if(m_rebindType == RebindType.GamepadButton)
			{
				//	Allow KeyCode.None to pass because it means that the scan has been canceled or the timeout has been reached
				//	Allow KeyCode.Backspace to pass so it can clear the current binding
				if((int)key < (int)KeyCode.JoystickButton0 && key != KeyCode.None && key != KeyCode.Backspace)
					isValid = false;
			}
			else
			{
				isValid = false;
			}
			
			return isValid;
		}

		private bool HandleJoystickAxisScan(ScanResult result)
		{
			//	The axis is negative when the timeout has been reached or the scan has been canceled
			if(result.joystickAxis >= 0)
				m_axisConfig.SetAnalogAxis(m_joystick, result.joystickAxis);

			m_image.overrideSprite = m_normalState;
			m_keyDescription.text = m_axisNames[m_axisConfig.axis];
			return true;
		}

		private KeyCode GetCurrentKeyCode()
		{
			if(m_rebindType == RebindType.GamepadAxis)
				return KeyCode.None;

			if(m_changePositiveKey)
			{
				if(m_changeAltKey)
					return m_axisConfig.altPositive;
				else
					return m_axisConfig.positive;
			}
			else
			{
				if(m_changeAltKey)
					return m_axisConfig.altNegative;
				else
					return m_axisConfig.negative;
			}
		}
	}
}