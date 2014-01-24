#region [Copyright (c) 2012-2013 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2014 Cristian Alexandru Geambasu
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
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace TeamUtility.IO
{
	/// <summary>
	/// Encapsulates a method that takes one parameter(the key) and returns 'true' if
	/// the key is acce[ted or 'false' if it isn't.
	/// </summary>
	public delegate bool KeyScanHandler(KeyCode key, params object[] args);
	
	/// <summary>
	/// Encapsulates a method that takes one parameter(the axis) and returns 'true' if
	/// the axis is accepted or 'false' if it isn't.
	/// </summary>
	public delegate bool AxisScanHandler(int axis, params object[] args);
	
	[ExecuteInEditMode]
	public sealed class InputManager : MonoBehaviour 
	{
		public const string VERSION = "1.3";
		
		public enum ScanType
		{
			None, Key, MouseAxis, JoystickAxis
		}
		
		public event Action<string> ConfigurationChanged;
		public event Action Loaded;
		public event Action Saved;
		
		public List<InputConfiguration> inputConfigurations;
		public string defaultConfiguration;
		public bool dontDestroyOnLoad;
		
		private static InputManager _instance;
		private float _scanTimeout;
		private int _joystickToScan;
		private string _cancelScanButton;
		private object[] _optionalParameters;
		private ScanType _scanType;
		private KeyScanHandler _keyScanHandler;
		private AxisScanHandler _axisScanHandler;
		private InputConfiguration _currentConfiguration;
		
		//	Cached data for improved performance.
		private string[] _rawMouseAxes;
		private string[] _rawJoystickAxes;
		private Dictionary<string, KeyCode> _stringToKeyTable;
		private Dictionary<string, InputConfiguration> _configurationTable;
		private Dictionary<string, Dictionary<string, AxisConfiguration>> _axesTable;
		
		private void OnEnable()
		{
			if(inputConfigurations == null)
			{
				inputConfigurations = new List<InputConfiguration>();
			}
		}
		
		private void Awake()
		{
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			if(_instance != null) 
			{
				UnityEngine.Object.Destroy(this);
			}
			else
			{
				if(dontDestroyOnLoad) 
				{
					UnityEngine.Object.DontDestroyOnLoad(this);
				}
				
				_instance = this;
				_stringToKeyTable = new Dictionary<string, KeyCode>();
				_configurationTable = new Dictionary<string, InputConfiguration>();
				_axesTable = new Dictionary<string, Dictionary<string, AxisConfiguration>>();
				
				SetRawAxisNames();
				SetKeyNames();
				Initialize();
			}
		}
		
		private void SetRawAxisNames()
		{
			_rawMouseAxes = new string[AxisConfiguration.MaxMouseAxes];
			for(int i = 0; i < _rawMouseAxes.Length; i++)
			{
				_rawMouseAxes[i] = string.Concat("mouse_axis_", i);
			}
			
			_rawJoystickAxes = new string[AxisConfiguration.MaxJoysticks * AxisConfiguration.MaxJoystickAxes];
			for(int i = 0; i < AxisConfiguration.MaxJoysticks; i++)
			{
				for(int j = 0; j < AxisConfiguration.MaxJoystickAxes; j++)
				{
					_rawJoystickAxes[i * AxisConfiguration.MaxJoystickAxes + j] = string.Concat("joy_", i, "_axis_", j);
				}
			}
		}
		
		private void SetKeyNames()
		{
			foreach(KeyCode key in Enum.GetValues(typeof(KeyCode)))
			{
				if((int)key < 330 || (int)key > 340)
				{
					string keyStr = key.ToString().ToLower();
					if(!_stringToKeyTable.ContainsKey(keyStr))
					{
						_stringToKeyTable.Add(keyStr, key);
					}
				}
			}
		}
		
		private void Initialize()
		{
			if(inputConfigurations.Count == 0)
				return;
			
			PopulateLookupTables();
			if(string.IsNullOrEmpty(defaultConfiguration) || !_configurationTable.ContainsKey(defaultConfiguration))
			{
				_currentConfiguration = inputConfigurations[0];
			}
			else
			{
				_currentConfiguration = _configurationTable[defaultConfiguration];
			}
			
			foreach(InputConfiguration inputConfig in inputConfigurations)
			{
				foreach(AxisConfiguration axisConfig in inputConfig.axes)
				{
					axisConfig.Initialize();
				}
			}
			ResetInputAxes();
		}
		
		private void PopulateLookupTables()
		{
			_configurationTable.Clear();
			foreach(InputConfiguration inputConfig in inputConfigurations)
			{
				if(!_configurationTable.ContainsKey(inputConfig.name))
				{
					_configurationTable.Add(inputConfig.name, inputConfig);
				}
#if UNITY_EDITOR
				else
				{
					Debug.LogWarning("InputConfiguration with name: " + inputConfig.name + " already exists");
				}
#endif
			}
			
			_axesTable.Clear();
			foreach(InputConfiguration inputConfig in inputConfigurations)
			{
				Dictionary<string, AxisConfiguration> table = new Dictionary<string, AxisConfiguration>();
				foreach(AxisConfiguration axisConfig in inputConfig.axes)
				{
					if(!table.ContainsKey(axisConfig.name))
					{
						table.Add(axisConfig.name, axisConfig);
					}
#if UNITY_EDITOR
					else
					{
						Debug.LogWarning("InputConfiguration: " + inputConfig.name + "already contains an axis named: " + inputConfig.name);
					}
#endif
				}
				
				_axesTable.Add(inputConfig.name, table);
			}
		}
		
		private void Update()
		{
#if UNITY_EDITOR
			if(!UnityEditor.EditorApplication.isPlaying)
				return;
#endif
			if(_currentConfiguration != null)
			{
				for(int i = 0; i < _currentConfiguration.axes.Count; i++)
				{
					_currentConfiguration.axes[i].Update();
				}
			}
			
			if(_scanType != ScanType.None)
			{
				_scanTimeout -= Time.deltaTime;
				switch(_scanType)
				{
				case ScanType.Key:
					ScanKey();
					break;
				case ScanType.MouseAxis:
					ScanMouseAxis();
					break;
				case ScanType.JoystickAxis:
					ScanJoystickAxis();
					break;
				}
			}
		}
		
		private void ScanKey()
		{
			if(!string.IsNullOrEmpty(_cancelScanButton) && 
				GetButtonDown(_cancelScanButton) || _scanTimeout <= 0.0f)
			{
				StopScan();
			}
			else
			{
				foreach(KeyValuePair<string, KeyCode> entry in _stringToKeyTable)
				{
					if(Input.GetKeyDown(entry.Value))
					{
						if(_keyScanHandler(entry.Value, _optionalParameters))
						{
							_keyScanHandler = null;
							_scanType = ScanType.None;
							break;
						}
					}
				}
			}
		}
		
		private void ScanMouseAxis()
		{
			if(!string.IsNullOrEmpty(_cancelScanButton) && 
				GetButtonDown(_cancelScanButton) || _scanTimeout <= 0.0f)
			{
				StopScan();
			}
			else
			{
				for(int i = 0; i < _rawMouseAxes.Length; i++)
				{
					if(Mathf.Abs(Input.GetAxis(_rawMouseAxes[i])) > 0.0f)
					{
						if(_axisScanHandler(i, _optionalParameters))
						{
							_axisScanHandler = null;
							_scanType = ScanType.None;
							break;
						}
					}
				}
			}
		}
		
		private void ScanJoystickAxis()
		{
			if(!string.IsNullOrEmpty(_cancelScanButton) && 
				GetButtonDown(_cancelScanButton) || _scanTimeout <= 0.0f)
			{
				StopScan();
			}
			else
			{
				int scanStart = _joystickToScan * AxisConfiguration.MaxJoystickAxes;
				for(int i = 0; i < AxisConfiguration.MaxJoystickAxes; i++)
				{
					if(Mathf.Abs(Input.GetAxisRaw(_rawJoystickAxes[scanStart + i])) >=   1.0f)
					{
						if(_axisScanHandler(i, _optionalParameters))
						{
							_axisScanHandler = null;
							_scanTimeout = 0.0f;
							_scanType = ScanType.None;
							break;
						}
					}
				}
			}
		}
		
		private void StopScan()
		{
			if(_scanType == ScanType.Key)
			{
				_keyScanHandler(KeyCode.None, _optionalParameters);
			}
			else if(_scanType == ScanType.MouseAxis || _scanType == ScanType.JoystickAxis)
			{
				_axisScanHandler(-1, _optionalParameters);
			}
			
			_scanType = ScanType.None;
			_axisScanHandler = null;
			_keyScanHandler = null;
		}
		
		private void RaiseConfigurationChangedEvent(string configuration)
		{
			if(ConfigurationChanged != null)
			{
				ConfigurationChanged(configuration);
			}
		}
		
		private void RaiseLoadedEvent()
		{
			if(Loaded != null)
			{
				Loaded();
			}
		}
		
		private void RaiseSavedEvent()
		{
			if(Saved != null)
			{
				Saved();
			}
		}
		
		#region [Static Interface]
		public static InputManager Instance
		{
			get
			{
				return _instance;
			}
		}
		
		public static bool IsScanning
		{
			get
			{
				return (_instance._scanType != ScanType.None);
			}
		}
		
		public static ScanType CurrentScanType
		{
			get
			{
				return _instance._scanType;
			}
		}
		
		public static InputConfiguration CurrentConfiguration
		{
			get
			{
				return _instance._currentConfiguration;
			}
		}
		
		public static List<InputConfiguration> InputConfigurations
		{
			get
			{
				return _instance.inputConfigurations;
			}
		}
		
		public static bool AnyInput()
		{
			return AnyInput(_instance._currentConfiguration);
		}
		
		public static bool AnyInput(string configuration)
		{
			InputConfiguration inputConfig;
			if(_instance._configurationTable.TryGetValue(configuration, out inputConfig))
			{
				return AnyInput(inputConfig);
			}
			
			return false;
		}
		
		private static bool AnyInput(InputConfiguration inputConfig)
		{
			for(int i = 0; i < inputConfig.axes.Count; i++)
			{
				if(inputConfig.axes[i].AnyInput)
					return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Resets the internal state of the InputManager.
		/// </summary>
		public static void Reinitialize()
		{
			_instance.Initialize();
		}
		
		public static void SetConfiguration(string name)
		{
			if(_instance._currentConfiguration == null || name == _instance._currentConfiguration.name)
				return;
			
			if(!_instance._configurationTable.ContainsKey(name)) {
				throw new ArgumentException("Unable to find any InputConfiguration named: " + name);
			}
			
			_instance._currentConfiguration = _instance._configurationTable[name];
			ResetInputAxes();
			_instance.RaiseConfigurationChangedEvent(name);
		}
		
		public static InputConfiguration GetConfiguration(string name)
		{
			InputConfiguration inputConfig = null;
			_instance._configurationTable.TryGetValue(name, out inputConfig);
			
			return inputConfig;
		}
		
		public static string[] GetConfigurationNames()
		{
			string[] names = new string[_instance.inputConfigurations.Count];
			for(int i = 0; i < names.Length; i++)
			{
				names[i] = _instance.inputConfigurations[i].name;
			}
			
			return names;
		}
		
		public static AxisConfiguration GetAxisConfiguration(string axisName)
		{
			return GetAxisConfiguration(_instance._currentConfiguration.name, axisName);
		}
		
		public static AxisConfiguration GetAxisConfiguration(string configuration, string axisName)
		{
			Dictionary<string, AxisConfiguration> table;
			if(_instance._axesTable.TryGetValue(configuration, out table))
			{
				AxisConfiguration axisConfig;
				if(table.TryGetValue(axisName, out axisConfig))
					return axisConfig;
			}
			
			return null;
		}
		
		
		/// <summary>
		/// Scans for keyboard input and calls the handler with the result.
		/// Returns KeyCode.None if timeout is reached.
		/// </summary>
		public static void StartKeyScan(KeyScanHandler scanHandler, float timeout, string cancelScanButton, params object[] optional)
		{
			_instance._scanTimeout = timeout;
			_instance._cancelScanButton = cancelScanButton;
			_instance._optionalParameters = optional;
			_instance._scanType = ScanType.Key;
			_instance._keyScanHandler = scanHandler;
		}
		
		
		/// <summary>
		/// Scans for mouse input and calls the handler with the result.
		/// Returns -1 if timeout is reached.
		/// </summary>
		public static void StartMouseAxisScan(AxisScanHandler scanHandler, float timeout, string cancelScanButton, params object[] optional)
		{
			_instance._scanTimeout = timeout;
			_instance._cancelScanButton = cancelScanButton;
			_instance._optionalParameters = optional;
			_instance._scanType = ScanType.MouseAxis;
			_instance._axisScanHandler = scanHandler;
		}
		
		/// <summary>
		/// Scans for joystick input and calls the handler with the result.
		/// Returns -1 if timeout is reached.
		/// </summary>
		public static void StartJoystickAxisScan(AxisScanHandler scanHandler, int joystick, float timeout, string cancelScanButton, params object[] optional)
		{
			if(joystick < 0 || joystick >= AxisConfiguration.MaxJoystickAxes)
				return;
			
			_instance._scanTimeout = timeout;
			_instance._joystickToScan = joystick;
			_instance._cancelScanButton = cancelScanButton;
			_instance._optionalParameters = optional;
			_instance._scanType = ScanType.JoystickAxis;
			_instance._axisScanHandler = scanHandler;
		}
		
		public static void CancelScan()
		{
			_instance.StopScan();
		}
		
		/// <summary>
		/// Saves the input configurations in the XML format, in Application.persistentDataPath.
		/// </summary>
		public static void Save()
		{
			string filename = System.IO.Path.Combine(Application.persistentDataPath, "input_config.xml");
			Save(new InputSaverXML(filename));
		}
		
		/// <summary>
		/// Saves the input configurations in the XML format, at the specified location.
		/// </summary>
		public static void Save(string filename)
		{
			Save(new InputSaverXML(filename));
		}
		
		public static void Save(IInputSaver inputSaver)
		{
			if(inputSaver == null)
				throw new ArgumentNullException("inputSaver");
			
			inputSaver.Save(_instance.inputConfigurations, _instance.defaultConfiguration);
			_instance.RaiseSavedEvent();
		}
		
		/// <summary>
		/// Loads the input configurations saved in the XML format, from Application.persistentDataPath.
		/// </summary>
		public static void Load()
		{
			string filename = System.IO.Path.Combine(Application.persistentDataPath, "input_config.xml");
			if(System.IO.File.Exists(filename))
			{
				Load(new InputLoaderXML(filename));
			}
		}
		
		/// <summary>
		/// Loads the input configurations saved in the XML format, from the specified location.
		/// </summary>
		public static void Load(string filename)
		{
			Load(new InputLoaderXML(filename));
		}
		
		public static void Load(IInputLoader inputLoader)
		{
			if(inputLoader == null)
				throw new ArgumentNullException("inputLoader");
			
			inputLoader.Load(out _instance.inputConfigurations, out _instance.defaultConfiguration);
			_instance.Initialize();
			_instance.RaiseLoadedEvent();
		}
		
		#region [UNITY Interface]
		public static Vector3 acceleration
		{
			get
			{
				return Input.acceleration;
			}
		}
		
		public static int accelerationEventCount
		{
			get
			{
				return Input.accelerationEventCount;
			}
		}
		
		public static AccelerationEvent[] accelerationEvents
		{
			get
			{
				return Input.accelerationEvents;
			}
		}
		
		public static bool anyKey
		{
			get
			{
				return Input.anyKey;
			}
		}
		
		public static bool anyKeyDown
		{
			get
			{
				return Input.anyKeyDown;
			}
		}
		
		public static Compass compass
		{
			get
			{
				return Input.compass;
			}
		}
		
		public static bool compensateSensors
		{
			get
			{
				return Input.compensateSensors;
			}
			set
			{
				Input.compensateSensors = value;
			}
		}
		
		public static Vector2 compositionCursorPos
		{
			get
			{
				return Input.compositionCursorPos;
			}
			set
			{
				Input.compositionCursorPos = value;
			}
		}
		
		public static string compositionString
		{
			get
			{
				return Input.compositionString;
			}
		}
		
		public static DeviceOrientation deviceOrientation
		{
			get
			{
				return Input.deviceOrientation;
			}
		}
		
		public static Gyroscope gyro
		{
			get
			{
				return Input.gyro;
			}
		}
		
		public static IMECompositionMode imeCompositionMode
		{
			get
			{
				return Input.imeCompositionMode;
			}
			set
			{
				Input.imeCompositionMode = value;
			}
		}
		
		public static bool imeIsSelected
		{
			get
			{
				return Input.imeIsSelected;
			}
		}
		
		public static string inputString
		{
			get
			{
				return Input.inputString;
			}
		}
		
		public static LocationService location
		{
			get
			{
				return Input.location;
			}
		}
		
		public static Vector2 mousePosition
		{
			get
			{
				return Input.mousePosition;
			}
		}
		
		public static bool multiTouchEnabled
		{
			get
			{
				return Input.multiTouchEnabled;
			}
			set
			{
				Input.multiTouchEnabled = value;
			}
		}
		
		public static int touchCount
		{
			get
			{
				return Input.touchCount;
			}
		}
		
		public static Touch[] touches
		{
			get
			{
				return Input.touches;
			}
		}
		
		public static AccelerationEvent GetAccelerationEvent(int index)
		{
			return Input.GetAccelerationEvent(index);
		}
		
		public static float GetAxis(string name)
		{
			AxisConfiguration axisConfig = GetAxisConfiguration(name);
			if(axisConfig == null) 
			{
				string message = string.Format("InputConfiguration: {0} doesn't contain an axis named: {1}",
												_instance._currentConfiguration.name, name);
				throw new ArgumentException(message);
			}
			
			return axisConfig.GetAxis();
		}
		
		public static float GetAxisRaw(string name)
		{
			AxisConfiguration axisConfig = GetAxisConfiguration(name);
			if(axisConfig == null) 
			{
				string message = string.Format("InputConfiguration: {0} doesn't contain an axis named: {1}",
												_instance._currentConfiguration.name, name);
				throw new ArgumentException(message);
			}
			
			return axisConfig.GetAxisRaw();
		}
		
		public static bool GetButton(string name)
		{
			AxisConfiguration axisConfig = GetAxisConfiguration(name);
			if(axisConfig == null) 
			{
				string message = string.Format("InputConfiguration: {0} doesn't contain an axis named: {1}",
												_instance._currentConfiguration.name, name);
				throw new ArgumentException(message);
			}
			
			return axisConfig.GetButton();
		}
		
		public static bool GetButtonDown(string name)
		{
			AxisConfiguration axisConfig = GetAxisConfiguration(name);
			if(axisConfig == null) 
			{
				string message = string.Format("InputConfiguration: {0} doesn't contain an axis named: {1}",
												_instance._currentConfiguration.name, name);
				throw new ArgumentException(message);
			}
			
			return axisConfig.GetButtonDown();
		}
		
		public static bool GetButtonUp(string name)
		{
			AxisConfiguration axisConfig = GetAxisConfiguration(name);
			if(axisConfig == null) 
			{
				string message = string.Format("InputConfiguration: {0} doesn't contain an axis named: {1}",
												_instance._currentConfiguration.name, name);
				throw new ArgumentException(message);
			}
			
			return axisConfig.GetButtonUp();
		}
		
		public static bool GetKey(string name)
		{
			KeyCode key;
			if(_instance._stringToKeyTable.TryGetValue(name, out key))
			{
				return Input.GetKey(key);
			}
			else
			{
				return Input.GetKey(name);
			}
		}
		
		public static bool GetKey(KeyCode key)
		{
			return Input.GetKey(key);
		}
		
		public static bool GetKeyDown(string name)
		{
			KeyCode key;
			if(_instance._stringToKeyTable.TryGetValue(name, out key))
			{
				return Input.GetKeyDown(key);
			}
			else
			{
				return Input.GetKeyDown(name);
			}
		}
		
		public static bool GetKeyDown(KeyCode key)
		{
			return Input.GetKeyDown(key);
		}
		
		public static bool GetKeyUp(string name)
		{
			KeyCode key;
			if(_instance._stringToKeyTable.TryGetValue(name, out key))
			{
				return Input.GetKeyUp(key);
			}
			else
			{
				return Input.GetKeyUp(name);
			}
		}
		
		public static bool GetKeyUp(KeyCode key)
		{
			return Input.GetKeyUp(key);
		}
		
		public static bool GetMouseButton(int index)
		{
			return Input.GetMouseButton(index);
		}
		
		public static bool GetMouseButtonDown(int index)
		{
			return Input.GetMouseButtonDown(index);
		}
		
		public static bool GetMouseButtonUp(int index)
		{
			return Input.GetMouseButtonUp(index);
		}
		
		public static Touch GetTouch(int index)
		{
			return Input.GetTouch(index);
		}
		
		public static string[] GetJoystickNames()
		{
			return Input.GetJoystickNames();
		}
		
		public static void ResetInputAxes()
		{
			InputConfiguration inputConfig = _instance._currentConfiguration;
			for(int i = 0; i < inputConfig.axes.Count; i++)
			{
				inputConfig.axes[i].Reset();
			}
			Input.ResetInputAxes();
		}
		#endregion
		
		#endregion
	}
}