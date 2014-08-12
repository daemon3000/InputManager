using UnityEngine;
using System;
using System.Text;
using System.Collections;

namespace TeamUtility.IO
{
	#region [Enumerations]
	public enum InputDPADButton
	{
		Left, Right, Up, Down,
		Left_Up, Right_Up,
		Left_Down, Right_Down, Any
	}
	
	public enum InputTriggerButton
	{
		Left, Right, Any
	}
	
	public enum InputTriggerAxis
	{
		Left, Right
	}
	
	public enum InputDPADAxis
	{
		Horizontal, Vertical
	}
	
	public enum InputDevice
	{
		KeyboardAndMouse, 
		Joystick
	}
	#endregion
	
	public sealed class InputAdapter : MonoBehaviour
	{
		public event Action<InputDevice> InputDeviceChanged;

		[SerializeField]
		private bool dontDestroyOnLoad = false;

		[SerializeField]
		private bool allowRealtimeInputDeviceSwitch = false;
		
		[SerializeField]
		[Range(0.1f, 1.0f)]
		private float updateInputDeviceInterval = 1.0f;
		
		[SerializeField]
		[Range(1.0f, 5.0f)]
		private float updateJoystickCountInterval = 1.0f;
		
		[SerializeField]
		private string keyboardConfiguration;
		
		[SerializeField]
		private string windowsJoystickConfiguration;
		
		[SerializeField]
		private string osxJoystickConfiguration;
		
		private Vector2 _lastDpadValues = Vector2.zero;
		private Vector2 _currentDpadValues = Vector2.zero;
		private Vector2 _lastTriggerValues = Vector2.zero;
		private Vector2 _currentTriggerValues = Vector2.zero;
		private InputDevice _inputDevice;
		private int _joystickCount = 0;
		private int _firstJoystickKey = 330;
		private bool _canUpdateInputDevice = false;
		private string _joystickConfiguration;
		private string _keyboardConfiguration;
		private static InputAdapter _instance;
		
		#region [Static Accessors]
		public static InputAdapter Instance
		{
			get
			{
				return _instance;
			}
		}
		
		public static InputDevice inputDevice
		{
			get
			{
				return _instance._inputDevice;
			}
			set
			{
				if(value != _instance._inputDevice)
				{
					if(value == InputDevice.Joystick && _instance._joystickCount > 0)
					{
						_instance.SetInputDevice(InputDevice.Joystick);
					}
					else
					{
						_instance.SetInputDevice(InputDevice.KeyboardAndMouse);
					}
				}
			}
		}
		
		public static string KeyboardConfiguration
		{
			get
			{
				return _instance._keyboardConfiguration;
			}
		}
		
		public static string JoystickConfiguration
		{
			get
			{
				return _instance._joystickConfiguration;
			}
		}
		
		public static Vector3 mousePosition
		{
			get
			{
				return InputManager.mousePosition;
			}
		}
		
		public static float GetAxis(string axisName)
		{
			return InputManager.GetAxis(axisName);
		}
		
		public static float GetTriggerAxis(InputTriggerAxis axis)
		{
			if(_instance._inputDevice == InputDevice.KeyboardAndMouse)
				return 0.0f;
			
			if(axis == InputTriggerAxis.Left)
			{
				return InputManager.GetAxis("LeftTrigger");
			}
			else
			{
				return InputManager.GetAxis("RightTrigger");
			}
		}
		
		public static float GetDPADAxis(InputDPADAxis axis)
		{
			if(_instance._inputDevice == InputDevice.KeyboardAndMouse)
				return 0.0f;
			
			if(axis == InputDPADAxis.Horizontal) 
			{
				return InputManager.GetAxis("DPADHorizontal");
			}
			else
			{
				return InputManager.GetAxis("DPADVertical");
			}
		}
		
		public static bool GetButton(string buttonName)
		{
			return InputManager.GetButton(buttonName);
		}
		
		public static bool GetButtonDown(string buttonName)
		{
			return InputManager.GetButtonDown(buttonName);
		}
		
		public static bool GetButtonUp(string buttonName)
		{
			return InputManager.GetButtonUp(buttonName);
		}
		
		public static bool GetMouseButton(int button)
		{
			return InputManager.GetMouseButton(button);
		}
		
		public static bool GetMouseButtonDown(int button)
		{
			return InputManager.GetMouseButtonDown(button);
		}
		
		public static bool GetMouseButtonUp(int button)
		{
			return InputManager.GetMouseButtonUp(button);
		}
		
		public static bool GetDPADButton(InputDPADButton button)
		{
			if(_instance._inputDevice == InputDevice.KeyboardAndMouse)
				return false;
			if(button == InputDPADButton.Any) {
				return (!Mathf.Approximately(_instance._currentDpadValues.x, 0.0f) || 
						!Mathf.Approximately(_instance._currentDpadValues.y, 0.0f));
			}
			
			bool state = false;
			switch(button)
			{
			case InputDPADButton.Left_Up:
				state = (_instance._currentDpadValues.x <= -1.0f && _instance._currentDpadValues.y >= 1.0f);
				break;
			case InputDPADButton.Right_Up:
				state = (_instance._currentDpadValues.x >= 1.0f && _instance._currentDpadValues.y >= 1.0f);
				break;
			case InputDPADButton.Left_Down:
				state = (_instance._currentDpadValues.x <= -1.0f && _instance._currentDpadValues.y <= -1.0f);
				break;
			case InputDPADButton.Right_Down:
				state = (_instance._currentDpadValues.x >= 1.0f && _instance._currentDpadValues.y <= -1.0f);
				break;
			case InputDPADButton.Left:
				state = (_instance._currentDpadValues.x <= -1.0f);
				break;
			case InputDPADButton.Right:
				state = (_instance._currentDpadValues.x >= 1.0f);
				break;
			case InputDPADButton.Up:
				state = (_instance._currentDpadValues.y >= 1.0f);
				break;
			case InputDPADButton.Down:
				state = (_instance._currentDpadValues.y <= -1.0f);
				break;
			default:
				break;
			}
			
			return state;
		}
		
		public static bool GetDPADButtonDown(InputDPADButton button)
		{
			if(_instance._inputDevice == InputDevice.KeyboardAndMouse)
				return false;
			if(button == InputDPADButton.Any) {
				return ((!Mathf.Approximately(_instance._currentDpadValues.x, 0.0f) && Mathf.Approximately(_instance._lastDpadValues.x, 0.0f)) ||
						(!Mathf.Approximately(_instance._currentDpadValues.y, 0.0f) && Mathf.Approximately(_instance._lastDpadValues.y, 0.0f)));
			}
			
			bool state = false;
			switch(button)
			{
			case InputDPADButton.Left_Up:
				state = (_instance._currentDpadValues.x <= -1.0f && _instance._lastDpadValues.x > -1.0f &&
						 _instance._currentDpadValues.y >= 1.0f && _instance._lastDpadValues.y < 1.0f);
				break;
			case InputDPADButton.Right_Up:
				state = (_instance._currentDpadValues.x >= 1.0f && _instance._lastDpadValues.x < 1.0f &&
						 _instance._currentDpadValues.y >= 1.0f && _instance._lastDpadValues.y < 1.0f);
				break;
			case InputDPADButton.Left_Down:
				state = (_instance._currentDpadValues.x <= -1.0f && _instance._lastDpadValues.x > -1.0f &&
						 _instance._currentDpadValues.y <= -1.0f && _instance._lastDpadValues.y > -1.0f);
				break;
			case InputDPADButton.Right_Down:
				state = (_instance._currentDpadValues.x >= 1.0f && _instance._lastDpadValues.x < 1.0f &&
						 _instance._currentDpadValues.y <= -1.0f && _instance._lastDpadValues.y > -1.0f);
				break;
			case InputDPADButton.Left:
				state = (_instance._currentDpadValues.x <= -1.0f && _instance._lastDpadValues.x > -1.0f);
				break;
			case InputDPADButton.Right:
				state = (_instance._currentDpadValues.x >= 1.0f && _instance._lastDpadValues.x < 1.0f);
				break;
			case InputDPADButton.Up:
				state = (_instance._currentDpadValues.y >= 1.0f && _instance._lastDpadValues.y < 1.0f);
				break;
			case InputDPADButton.Down:
				state = (_instance._currentDpadValues.y <= -1.0f && _instance._lastDpadValues.y > -1.0f);
				break;
			default:
				break;
			}
			
			return state;
		}
		
		public static bool GetDPADButtonUp(InputDPADButton button)
		{
			if(_instance._inputDevice == InputDevice.KeyboardAndMouse)
				return false;
			if(button == InputDPADButton.Any) {
				return ((Mathf.Approximately(_instance._currentDpadValues.x, 0.0f) && !Mathf.Approximately(_instance._lastDpadValues.x, 0.0f)) ||
						(Mathf.Approximately(_instance._currentDpadValues.y, 0.0f) && !Mathf.Approximately(_instance._lastDpadValues.y, 0.0f)));
			}
			
			bool state = false;
			switch(button)
			{
			case InputDPADButton.Left_Up:
				state = (_instance._currentDpadValues.x > -1.0f && _instance._lastDpadValues.x <= -1.0f &&
						 _instance._currentDpadValues.y < 1.0f && _instance._lastDpadValues.y >= 1.0f);
				break;
			case InputDPADButton.Right_Up:
				state = (_instance._currentDpadValues.x < 1.0f && _instance._lastDpadValues.x >= 1.0f &&
						 _instance._currentDpadValues.y < 1.0f && _instance._lastDpadValues.y >= 1.0f);
				break;
			case InputDPADButton.Left_Down:
				state = (_instance._currentDpadValues.x > -1.0f && _instance._lastDpadValues.x <= -1.0f &&
						 _instance._currentDpadValues.y > -1.0f && _instance._lastDpadValues.y <= -1.0f);
				break;
			case InputDPADButton.Right_Down:
				state = (_instance._currentDpadValues.x < 1.0f && _instance._lastDpadValues.x >= 1.0f &&
						 _instance._currentDpadValues.y > -1.0f && _instance._lastDpadValues.y <= -1.0f);
				break;
			case InputDPADButton.Left:
				state = (_instance._currentDpadValues.x > -1.0f && _instance._lastDpadValues.x <= -1.0f);
				break;
			case InputDPADButton.Right:
				state = (_instance._currentDpadValues.x < 1.0f && _instance._lastDpadValues.x >= 1.0f);
				break;
			case InputDPADButton.Up:
				state = (_instance._currentDpadValues.y < 1.0f && _instance._lastDpadValues.y >= 1.0f);
				break;
			case InputDPADButton.Down:
				state = (_instance._currentDpadValues.y > -1.0f && _instance._lastDpadValues.y <= -1.0f);
				break;
			default:
				break;
			}
			
			return state;
		}
		
		public static bool GetTriggerButton(InputTriggerButton button)
		{
			if(_instance._inputDevice == InputDevice.KeyboardAndMouse)
				return false;
			
			if(button == InputTriggerButton.Left)
			{
				return (_instance._currentTriggerValues.x >= 1.0f);
			}
			else if(button == InputTriggerButton.Right)
			{
				return (_instance._currentTriggerValues.y >= 1.0f);
			}
			else
			{
				return (_instance._currentTriggerValues.x >= 1.0f || _instance._currentTriggerValues.y >= 1.0f);
			}
		}
		
		public static bool GetTriggerButtonDown(InputTriggerButton button)
		{
			if(_instance._inputDevice == InputDevice.KeyboardAndMouse)
				return false;
			
			if(button == InputTriggerButton.Left)
			{
				return (_instance._currentTriggerValues.x >= 1.0f && _instance._lastTriggerValues.x < 1.0f);
			}
			else if(button == InputTriggerButton.Right)
			{
				return (_instance._currentTriggerValues.y >= 1.0f && _instance._lastTriggerValues.y < 1.0f);
			}
			else
			{
				return (_instance._currentTriggerValues.x >= 1.0f && _instance._lastTriggerValues.x < 1.0f) ||
						(_instance._currentTriggerValues.y >= 1.0f && _instance._lastTriggerValues.y < 1.0f);
			} 
		}
		
		public static bool GetTriggerButtonUp(InputTriggerButton button)
		{
			if(_instance._inputDevice == InputDevice.KeyboardAndMouse)
				return false;
			
			if(button == InputTriggerButton.Left)
			{
				return (_instance._currentTriggerValues.x < 1.0f && _instance._lastTriggerValues.x >= 1.0f);
			}
			else if(button == InputTriggerButton.Right)
			{
				return (_instance._currentTriggerValues.y < 1.0f && _instance._lastTriggerValues.y >= 1.0f);
			}
			else
			{
				return (_instance._currentTriggerValues.x < 1.0f && _instance._lastTriggerValues.x >= 1.0f) ||
						(_instance._currentTriggerValues.y < 1.0f && _instance._lastTriggerValues.y >= 1.0f);
			}
		}
		
		public static void ResetInputAxes()
		{
			InputManager.ResetInputAxes();
		}
		
		public static string[] GetJoystickNames()
		{
			return InputManager.GetJoystickNames();
		}
		
		public static bool IsUsingJoystick()
		{
			return (inputDevice == InputDevice.Joystick);
		}
		
		public static bool IsUsingKeyboardAndMouse()
		{
			return (inputDevice == InputDevice.KeyboardAndMouse);
		}
		#endregion
		
		private void Awake()
		{
			if(_instance != null)
			{
				Destroy(this);
			}
			else
			{
				SetInputManagerConfigurations();
				SetInputDevice(InputDevice.KeyboardAndMouse);
				
				_instance = this;
				_joystickCount = InputManager.GetJoystickNames().Length;

				if(dontDestroyOnLoad)
				{
					DontDestroyOnLoad(gameObject);
				}
			}
		}
		
		private void Start()
		{
			StartCoroutine(UpdateJoystickCount());
			if(allowRealtimeInputDeviceSwitch) 
			{
				StartCoroutine(SetCanUpdateInputDevice());
			}
		}
		
		private void Update()
		{
			if(_canUpdateInputDevice)
			{
				UpdateInputDevice();
				_canUpdateInputDevice = false;
			}
			
			UpdateTriggerAndDPAD();
		}
		
		private void UpdateTriggerAndDPAD()
		{
			if(_inputDevice == InputDevice.Joystick)
			{
				_lastDpadValues = _currentDpadValues;
				_currentDpadValues.x = InputManager.GetAxis("DPADHorizontal");
				_currentDpadValues.y = InputManager.GetAxis("DPADVertical");
				_lastTriggerValues = _currentTriggerValues;
				_currentTriggerValues.x = InputManager.GetAxis("LeftTrigger");
				_currentTriggerValues.y = InputManager.GetAxis("RightTrigger");
			}
			else
			{
				_lastDpadValues = Vector2.zero;
				_currentDpadValues = Vector2.zero;
				_lastTriggerValues = Vector2.zero;
				_lastTriggerValues = Vector2.zero;
			}
		}
		
		private IEnumerator UpdateJoystickCount()
		{
			while(true)
			{
				_joystickCount = InputManager.GetJoystickNames().Length;
				if(_inputDevice == InputDevice.Joystick && _joystickCount == 0)
				{
#if UNITY_EDITOR
					Debug.LogWarning("Lost connection with joystick. Switching to keyboard and mouse input.");
#endif
					SetInputDevice(InputDevice.KeyboardAndMouse);
				}
				yield return new WaitForSeconds(updateJoystickCountInterval);
			}
		}
		
		private IEnumerator SetCanUpdateInputDevice()
		{
			while(true)
			{
				_canUpdateInputDevice = true;
				yield return new WaitForSeconds(updateInputDeviceInterval);
			}
		}
		
		private void UpdateInputDevice()
		{
			bool anyJoystickKey = false;
			
			if(_inputDevice == InputDevice.Joystick)
			{
				if(InputManager.anyKey)
				{
					for(int i = 0; i <= 19; i++)
					{
						if(InputManager.GetKey((KeyCode)(_firstJoystickKey + i)))
						{
							anyJoystickKey = true;
							break;
						}
					}
					
					if(!anyJoystickKey)
					{
						SetInputDevice(InputDevice.KeyboardAndMouse);
					}
				}
				else
				{
					if(InputManager.AnyInput(_keyboardConfiguration))
					{
						SetInputDevice(InputDevice.KeyboardAndMouse);
					}
				}
			}
			else
			{
				if(InputManager.anyKey)
				{
					for(int i = 0; i <= 19; i++)
					{
						if(InputManager.GetKey((KeyCode)(_firstJoystickKey + i)))
						{
							anyJoystickKey = true;
							break;
						}
					}
					
					if(anyJoystickKey) 
					{
						SetInputDevice(InputDevice.Joystick);
					}
				}
				else
				{
					if(InputManager.AnyInput(_joystickConfiguration))
					{
						SetInputDevice(InputDevice.Joystick);
					}
				}
			}
		}
		
		private void SetInputDevice(InputDevice inpuDevice)
		{
			_inputDevice = inpuDevice;
			if(inpuDevice == InputDevice.Joystick)
			{
				Screen.showCursor = false;
				InputManager.SetConfiguration(_joystickConfiguration);
#if UNITY_EDITOR
				Debug.Log("Current Input Device: Joystick");
#endif
			}
			else
			{
				Screen.showCursor = true;
				InputManager.SetConfiguration(_keyboardConfiguration);
#if UNITY_EDITOR
				Debug.Log("Current Input Device: KeyboardAndMouse");
#endif
			}
			InputManager.ResetInputAxes();
			RaiseInputDeviceChangedEvent();
		}
		
		private void SetInputManagerConfigurations()
		{
			_keyboardConfiguration = keyboardConfiguration;
			switch(Application.platform)
			{
			case RuntimePlatform.WindowsPlayer:
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.WindowsWebPlayer:
				_joystickConfiguration = windowsJoystickConfiguration;
				break;
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.OSXWebPlayer:
				_joystickConfiguration = osxJoystickConfiguration;
				break;
			default:
				_joystickConfiguration = windowsJoystickConfiguration;
#if UNITY_EDITOR
				Debug.LogWarning("Unsupported XBOX 360 Controller driver. Defaulting to Windows driver configuration.");
#endif
				break;
			}
		}
		
		private void RaiseInputDeviceChangedEvent()
		{
			if(InputDeviceChanged != null)
			{
				InputDeviceChanged(_inputDevice);
			}
		}
		
		private void OnDestroy()
		{
			Screen.showCursor = true;
			StopAllCoroutines();
		}

#if UNITY_EDITOR
		[UnityEditor.MenuItem("Team Utility/Input Manager/Create Input Adapter", false, 2)]
		private static void Create()
		{
			GameObject gameObject = new GameObject("Input Adapter");
			gameObject.AddComponent<InputAdapter>();

			UnityEditor.Selection.activeGameObject = gameObject;
		}
#endif
	}
}