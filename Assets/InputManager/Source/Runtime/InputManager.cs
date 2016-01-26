#region [Copyright (c) 2015 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2015 Cristian Alexandru Geambasu
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
	/// the key is accepted or 'false' if it isn't.
	/// The 'userData' parameter is used to send additional user data.
	/// </summary>
	public delegate bool KeyScanHandler(KeyCode key, object[] userData);
	
	/// <summary>
	/// Encapsulates a method that takes one parameter(the axis) and returns 'true' if
	/// the axis is accepted or 'false' if it isn't.
	/// The 'userData' parameter is used to send additional user data.
	/// </summary>
	public delegate bool AxisScanHandler(int axis, object[] userData);
	
	/// <summary>
	/// Encapsulates a method that takes one parameter(the scan result) and returns 'true' if
	/// the scan result is accepted or 'false' if it isn't.
	/// </summary>
	public delegate bool ScanHandler(ScanResult result);

    public delegate void RemoteUpdateDelegate(PlayerID playerID);

	public partial class InputManager : MonoBehaviour
	{
		#region [Fields]
		public event Action<PlayerID> ConfigurationChanged;
		public event Action<string> ConfigurationDirty;
		public event Action Loaded;
		public event Action Saved;
		public event RemoteUpdateDelegate RemoteUpdate;
		
		public List<InputConfiguration> inputConfigurations = new List<InputConfiguration>();
		public string playerOneDefault;
        public string playerTwoDefault;
        public string playerThreeDefault;
        public string playerFourDefault;
        public bool dontDestroyOnLoad;
		public bool ignoreTimescale;
		
		private static InputManager _instance;
		private InputConfiguration _playerOneConfig;
        private InputConfiguration _playerTwoConfig;
        private InputConfiguration _playerThreeConfig;
        private InputConfiguration _playerFourConfig;
        private ScanHandler _scanHandler;
		private ScanResult _scanResult;
		private ScanFlags _scanFlags;
		private string _cancelScanButton;
		private float _scanStartTime;
		private float _scanTimeout;
		private int _scanJoystick;
		private object _scanUserData;
		
		private string[] _rawMouseAxes;
		private string[] _rawJoystickAxes;
		private KeyCode[] _keys;
		private Dictionary<string, InputConfiguration> _configurationTable;
		private Dictionary<string, Dictionary<string, AxisConfiguration>> _axesTable;

		#endregion
		
		private void Awake()
		{
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
				_keys = (KeyCode[])Enum.GetValues(typeof(KeyCode));
				_configurationTable = new Dictionary<string, InputConfiguration>();
				_axesTable = new Dictionary<string, Dictionary<string, AxisConfiguration>>();

                SetRawAxisNames();
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
		
		private void Initialize()
		{
            _playerOneConfig = null;
            _playerTwoConfig = null;
            _playerThreeConfig = null;
            _playerFourConfig = null;

            if (inputConfigurations.Count == 0)
				return;
			
			PopulateLookupTables();

            if (!string.IsNullOrEmpty(playerOneDefault) && _configurationTable.ContainsKey(playerOneDefault))
            {
                _playerOneConfig = _configurationTable[playerOneDefault];
            }
            else
            {
                if(inputConfigurations.Count > 0)
                    _playerOneConfig = inputConfigurations[0];
            }

            if (!string.IsNullOrEmpty(playerTwoDefault) && _configurationTable.ContainsKey(playerTwoDefault))
            {
                _playerTwoConfig = _configurationTable[playerTwoDefault];
            }

            if (!string.IsNullOrEmpty(playerThreeDefault) && _configurationTable.ContainsKey(playerThreeDefault))
            {
                _playerThreeConfig = _configurationTable[playerThreeDefault];
            }

            if (!string.IsNullOrEmpty(playerFourDefault) && _configurationTable.ContainsKey(playerFourDefault))
            {
                _playerFourConfig = _configurationTable[playerFourDefault];
            }

            foreach (InputConfiguration inputConfig in inputConfigurations)
			{
				foreach(AxisConfiguration axisConfig in inputConfig.axes)
				{
					axisConfig.Initialize();
				}
			}

            Input.ResetInputAxes();
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
				else
				{
					Debug.LogWarning(string.Format("An input configuration named \'{0}\' already exists in the lookup table", inputConfig.name));
				}
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
					else
					{
						Debug.LogWarning(string.Format("Input configuration \'{0}\' already contains an axis named \'{1}\'", inputConfig.name, axisConfig.name));
					}
				}
				
				_axesTable.Add(inputConfig.name, table);
			}
		}
		
		private void Update()
		{
            UpdateInputConfiguration(_playerOneConfig, PlayerID.One);
            UpdateInputConfiguration(_playerTwoConfig, PlayerID.Two);
            UpdateInputConfiguration(_playerThreeConfig, PlayerID.Three);
            UpdateInputConfiguration(_playerFourConfig, PlayerID.Four);

            if (_playerOneConfig != null)
            {
                if (_scanFlags != ScanFlags.None)
                    ScanInput();
            }
            else
			{
				if(_scanFlags != ScanFlags.None)
					StopInputScan();
			}
		}

        private void UpdateInputConfiguration(InputConfiguration inputConfig, PlayerID playerID)
        {
            if (inputConfig != null)
            {
                for (int i = 0; i < inputConfig.axes.Count; i++)
                    inputConfig.axes[i].Update();

                if (RemoteUpdate != null)
                    RemoteUpdate(playerID);
            }
        }
		
		private void ScanInput()
		{
			float timeout = ignoreTimescale ? (Time.realtimeSinceStartup - _scanStartTime) : (Time.time - _scanStartTime);
			if(!string.IsNullOrEmpty(_cancelScanButton) && GetButtonDown(_cancelScanButton) || timeout >= _scanTimeout)
			{
				StopInputScan();
				return;
			}
			
			bool scanSuccess = false;
			if(((int)_scanFlags & (int)ScanFlags.Key) == (int)ScanFlags.Key)
			{
				scanSuccess = ScanKey();
			}
			if(!scanSuccess && (((int)_scanFlags & (int)ScanFlags.JoystickButton) == (int)ScanFlags.JoystickButton))
			{
				scanSuccess = ScanJoystickButton();
			}
			if(!scanSuccess && (((int)_scanFlags & (int)ScanFlags.JoystickAxis) == (int)ScanFlags.JoystickAxis))
			{
				scanSuccess = ScanJoystickAxis();
			}
			if(!scanSuccess && (((int)_scanFlags & (int)ScanFlags.MouseAxis) == (int)ScanFlags.MouseAxis))
			{
				ScanMouseAxis();
			}
		}
		
		private bool ScanKey()
		{
			int length = _keys.Length;
			for(int i = 0; i < length; i++)
			{
				if((int)_keys[i] >= (int)KeyCode.JoystickButton0)
					break;
				
				if(Input.GetKeyDown(_keys[i]))
				{
					_scanResult.scanFlags = ScanFlags.Key;
					_scanResult.key = _keys[i];
					_scanResult.joystick = -1;
					_scanResult.joystickAxis = -1;
					_scanResult.joystickAxisValue = 0.0f;
					_scanResult.mouseAxis = -1;
					_scanResult.userData = _scanUserData;
					if(_scanHandler(_scanResult))
					{
						_scanHandler = null;
						_scanResult.userData = null;
						_scanFlags = ScanFlags.None;
						return true;
					}
				}
			}
			
			return false;
		}
		
		private bool ScanJoystickButton()
		{
			for(int key = (int)KeyCode.JoystickButton0; key < (int)KeyCode.Joystick4Button19; key++)
			{
				if(Input.GetKeyDown((KeyCode)key))
				{
					_scanResult.scanFlags = ScanFlags.JoystickButton;
					_scanResult.key = (KeyCode)key;
					_scanResult.joystick = -1;
					_scanResult.joystickAxis = -1;
					_scanResult.joystickAxisValue = 0.0f;
					_scanResult.mouseAxis = -1;
					_scanResult.userData = _scanUserData;
					if(_scanHandler(_scanResult))
					{
						_scanHandler = null;
						_scanResult.userData = null;
						_scanFlags = ScanFlags.None;
						return true;
					}
				}
			}
			
			return false;
		}
		
		private bool ScanJoystickAxis()
		{
			int scanStart = _scanJoystick * AxisConfiguration.MaxJoystickAxes;
			float axisRaw = 0.0f;

			for(int i = 0; i < AxisConfiguration.MaxJoystickAxes; i++)
			{
				axisRaw = Input.GetAxisRaw(_rawJoystickAxes[scanStart + i]);
				if(Mathf.Abs(axisRaw) >= 1.0f)
				{
					_scanResult.scanFlags = ScanFlags.JoystickAxis;
					_scanResult.key = KeyCode.None;
					_scanResult.joystick = _scanJoystick;
					_scanResult.joystickAxis = i;
					_scanResult.joystickAxisValue = axisRaw;
					_scanResult.mouseAxis = -1;
					_scanResult.userData = _scanUserData;
					if(_scanHandler(_scanResult))
					{
						_scanHandler = null;
						_scanResult.userData = null;
						_scanFlags = ScanFlags.None;
						return true;
					}
				}
			}
			
			return false;
		}
		
		private bool ScanMouseAxis()
		{
			for(int i = 0; i < _rawMouseAxes.Length; i++)
			{
				if(Mathf.Abs(Input.GetAxis(_rawMouseAxes[i])) > 0.0f)
				{
					_scanResult.scanFlags = ScanFlags.MouseAxis;
					_scanResult.key = KeyCode.None;
					_scanResult.joystick = -1;
					_scanResult.joystickAxis = -1;
					_scanResult.joystickAxisValue = 0.0f;
					_scanResult.mouseAxis = i;
					_scanResult.userData = _scanUserData;
					if(_scanHandler(_scanResult))
					{
						_scanHandler = null;
						_scanResult.userData = null;
						_scanFlags = ScanFlags.None;
						return true;
					}
				}
			}
			
			return false;
		}
		
		private void StopInputScan()
		{
			_scanResult.scanFlags = ScanFlags.None;
			_scanResult.key = KeyCode.None;
			_scanResult.joystick = -1;
			_scanResult.joystickAxis = -1;
			_scanResult.joystickAxisValue = 0.0f;
			_scanResult.mouseAxis = -1;
			_scanResult.userData = _scanUserData;
			
			_scanHandler(_scanResult);
			
			_scanHandler = null;
			_scanResult.userData = null;
			_scanFlags = ScanFlags.None;
		}

        private void SetInputConfigurationByPlayerID(PlayerID playerID, InputConfiguration inputConfig)
        {
            if (playerID == PlayerID.One)
                _playerOneConfig = inputConfig;
            else if (playerID == PlayerID.Two)
                _playerTwoConfig = inputConfig;
            else if (playerID == PlayerID.Three)
                _playerThreeConfig = inputConfig;
            else if (playerID == PlayerID.Four)
                _playerFourConfig = inputConfig;
        }

        private InputConfiguration GetInputConfigurationByPlayerID(PlayerID playerID)
        {
            if (playerID == PlayerID.One)
                return _playerOneConfig;
            else if (playerID == PlayerID.Two)
                return _playerTwoConfig;
            else if (playerID == PlayerID.Three)
                return _playerThreeConfig;
            else if (playerID == PlayerID.Four)
                return _playerFourConfig;
            else
                return null;
        }

		private PlayerID? IsInputConfigurationInUse(string name)
        {
            if(_playerOneConfig != null && _playerOneConfig.name == name)
				return PlayerID.One;
			if(_playerTwoConfig != null && _playerTwoConfig.name == name)
				return PlayerID.Two;
			if(_playerThreeConfig != null && _playerThreeConfig.name == name)
				return PlayerID.Three;
			if(_playerFourConfig != null && _playerFourConfig.name == name)
				return PlayerID.Four;

			return null;
        }

        public void Load(SaveLoadParameters parameters)
        {
            if (parameters != null)
            {
                inputConfigurations = parameters.inputConfigurations;
                playerOneDefault = parameters.playerOneDefault;
                playerTwoDefault = parameters.playerTwoDefault;
                playerThreeDefault = parameters.playerThreeDefault;
                playerFourDefault = parameters.playerFourDefault;
            }
        }

        public SaveLoadParameters GetSaveParameters()
        {
            SaveLoadParameters parameters = new SaveLoadParameters();
            parameters.inputConfigurations = inputConfigurations;
            parameters.playerOneDefault = playerOneDefault;
            parameters.playerTwoDefault = playerTwoDefault;
            parameters.playerThreeDefault = playerThreeDefault;
            parameters.playerFourDefault = playerFourDefault;

            return parameters;
        }

        private void RaiseInputConfigurationChangedEvent(PlayerID playerID)
		{
			if(ConfigurationChanged != null)
				ConfigurationChanged(playerID);
		}
		
		private void RaiseConfigurationDirtyEvent(string configName)
		{
			if(ConfigurationDirty != null)
				ConfigurationDirty(configName);
		}
		
		private void RaiseLoadedEvent()
		{
			if(Loaded != null)
				Loaded();
		}
		
		private void RaiseSavedEvent()
		{
			if(Saved != null)
				Saved();
		}
		
		#region [Static Interface]
		/// <summary>
		/// A reference to the input manager instance. Use it to check if an input manager exists in the scene and
		/// to subscribe to the input manager's events.
		/// </summary>
		public static InputManager Instance { get { return _instance; } }

		[Obsolete("Use InputManager.PlayerOneConfiguration instead", true)]
		public static InputConfiguration CurrentConfiguration { get { return _instance._playerOneConfig; } }

        public static InputConfiguration PlayerOneConfiguration { get { return _instance._playerOneConfig; } }
        public static InputConfiguration PlayerTwoConfiguration { get { return _instance._playerTwoConfig; } }
        public static InputConfiguration PlayerThreeConfiguration { get { return _instance._playerThreeConfig; } }
        public static InputConfiguration PlayerFourConfiguration { get { return _instance._playerFourConfig; } }
        public static bool IsScanning { get { return _instance._scanFlags != ScanFlags.None; } }
		public static bool IgnoreTimescale { get { return _instance.ignoreTimescale; } }
		
		/// <summary>
		/// Returns true if any axis of any active input configuration is receiving input.
		/// </summary>
		public static bool AnyInput()
		{
            return AnyInput(_instance._playerOneConfig) || AnyInput(_instance._playerTwoConfig) ||
                    AnyInput(_instance._playerThreeConfig) || AnyInput(_instance._playerFourConfig);
		}

        /// <summary>
		/// Returns true if any axis of the input configuration is receiving input.
		/// </summary>
        public static bool AnyInput(PlayerID playerID)
        {
            return AnyInput(_instance.GetInputConfigurationByPlayerID(playerID));
        }
		
		/// <summary>
		/// Returns true if any axis of the specified input configuration is receiving input.
		/// If the specified input configuration is not active and the axis is of type
		/// DigialAxis, RemoteAxis, RemoteButton or AnalogButton this method will return false.
		/// </summary>
		public static bool AnyInput(string inputConfigName)
		{
			InputConfiguration inputConfig;
			if(_instance._configurationTable.TryGetValue(inputConfigName, out inputConfig))
			{
				int count = inputConfig.axes.Count;
				for(int i = 0; i < count; i++)
				{
					if(inputConfig.axes[i].AnyInput)
						return true;
				}
			}
			
			return false;
		}

        private static bool AnyInput(InputConfiguration inputConfig)
        {
            if (inputConfig != null)
            {
                int count = inputConfig.axes.Count;
                for (int i = 0; i < count; i++)
                {
                    if (inputConfig.axes[i].AnyInput)
                        return true;
                }
            }

            return false;
        }
		
		/// <summary>
		/// If an axis with the requested name exists, and it is of type 'RemoteAxis', the axis' value will be changed.
		/// </summary>
		[Obsolete("Use the method overload that takes in the input configuration name", true)]
		public static void SetRemoteAxisValue(string axisName, float value)
		{
			SetRemoteAxisValue(_instance._playerOneConfig.name, axisName, value);
		}
		
		/// <summary>
		/// If an axis with the requested name exists, and it is of type 'RemoteAxis', the axis' value will be changed.
		/// </summary>
		public static void SetRemoteAxisValue(string inputConfigName, string axisName, float value)
		{
			AxisConfiguration axisConfig = GetAxisConfiguration(inputConfigName, axisName);
			if(axisConfig != null)
				axisConfig.SetRemoteAxisValue(value);
			else
				Debug.LogError(string.Format("An axis named \'{0}\' does not exist in the input configuration named \'{1}\'", axisName, inputConfigName));
		}

        /// <summary>
        /// If an button with the requested name exists, and it is of type 'RemoteButton', the button's state will be changed.
        /// </summary>
		[Obsolete("Use the method overload that takes in the input configuration name", true)]
        public static void SetRemoteButtonValue(string buttonName, bool down, bool justChanged)
		{
			SetRemoteButtonValue(_instance._playerOneConfig.name, buttonName, down, justChanged);
		}
		
		/// <summary>
		/// If an button with the requested name exists, and it is of type 'RemoteButton', the button's state will be changed.
		/// </summary>
		public static void SetRemoteButtonValue(string inputConfigName, string buttonName, bool down, bool justChanged)
		{
			AxisConfiguration axisConfig = GetAxisConfiguration(inputConfigName, buttonName);
			if(axisConfig != null)
				axisConfig.SetRemoteButtonValue(down, justChanged);
			else
				Debug.LogError(string.Format("A remote button named \'{0}\' does not exist in the input configuration named \'{1}\'", buttonName, inputConfigName));
		}
		
		/// <summary>
		/// Resets the internal state of the input manager.
		/// </summary>
		public static void Reinitialize()
		{
			_instance.Initialize();
		}

        public static void ResetInputConfiguration(PlayerID playerID)
        {
            InputConfiguration inputConfig = _instance.GetInputConfigurationByPlayerID(playerID);
            if (inputConfig != null)
            {
                int count = inputConfig.axes.Count;
                for (int i = 0; i < count; i++)
                {
                    inputConfig.axes[i].Reset();
                }
            }
        }

        /// <summary>
        /// Changes the active input configuration.
        /// </summary>
		[Obsolete("Use the method overload that takes in the player ID", true)]
        public static void SetInputConfiguration(string name)
        {
            SetInputConfiguration(name, PlayerID.One);
        }

        /// <summary>
        /// Changes the active input configuration.
        /// </summary>
        public static void SetInputConfiguration(string name, PlayerID playerID)
		{
			PlayerID? playerWhoUsesInputConfig = _instance.IsInputConfigurationInUse(name);

			if (playerWhoUsesInputConfig.HasValue && playerWhoUsesInputConfig.Value != playerID)
            {
				Debug.LogErrorFormat("The input configuration named \'{0}\' is already being used by player {1}", name, playerWhoUsesInputConfig.Value.ToString());
                return;
            }

			if(playerWhoUsesInputConfig.HasValue && playerWhoUsesInputConfig.Value == playerID)
				return;
            
            InputConfiguration inputConfig = null;
			if(_instance._configurationTable.TryGetValue(name, out inputConfig))
			{
                _instance.SetInputConfigurationByPlayerID(playerID, inputConfig);
                ResetInputConfiguration(playerID);
				_instance.RaiseInputConfigurationChangedEvent(playerID);
			}
			else
			{
				Debug.LogError(string.Format("An input configuration named \'{0}\' does not exist", name));
			}
		}

        public static InputConfiguration GetInputConfiguration(string name)
		{
			InputConfiguration inputConfig = null;
			if(_instance._configurationTable.TryGetValue(name, out inputConfig))
				return inputConfig;
			
			return null;
		}

        public static InputConfiguration GetInputConfiguration(PlayerID playerID)
        {
            return _instance.GetInputConfigurationByPlayerID(playerID);
        }

        public static AxisConfiguration GetAxisConfiguration(string inputConfigName, string axisName)
		{
			Dictionary<string, AxisConfiguration> table;
			if(_instance._axesTable.TryGetValue(inputConfigName, out table))
			{
				AxisConfiguration axisConfig;
				if(table.TryGetValue(axisName, out axisConfig))
					return axisConfig;
			}
			
			return null;
		}

        public static AxisConfiguration GetAxisConfiguration(PlayerID playerID, string axisName)
        {
            var inputConfig = _instance.GetInputConfigurationByPlayerID(playerID);
            if (inputConfig == null)
                return null;

            Dictionary<string, AxisConfiguration> table;
            if (_instance._axesTable.TryGetValue(inputConfig.name, out table))
            {
                AxisConfiguration axisConfig;
                if (table.TryGetValue(axisName, out axisConfig))
                    return axisConfig;
            }

            return null;
        }

        public static InputConfiguration CreateInputConfiguration(string name)
		{
			if(_instance._configurationTable.ContainsKey(name))
			{
				Debug.LogError(string.Format("An input configuration named \'{0}\' already exists", name));
				return null;
			}
			
			InputConfiguration inputConfig = new InputConfiguration(name);
			_instance.inputConfigurations.Add(inputConfig);
			_instance._configurationTable.Add(name, inputConfig);
			_instance._axesTable.Add(name, new Dictionary<string, AxisConfiguration>());
			
			return inputConfig;
		}
		
		/// <summary>
		/// Deletes the specified input configuration. If the speficied input configuration is
		/// active for any player then the active input configuration for the respective player will be set to null.
		/// </summary>
		public static bool DeleteInputConfiguration(string name)
		{
			InputConfiguration inputConfig = GetInputConfiguration(name);
			if(inputConfig == null)
				return false;
			
			_instance._axesTable.Remove(name);
			_instance._configurationTable.Remove(name);
			_instance.inputConfigurations.Remove(inputConfig);
            if (_instance._playerOneConfig.name == inputConfig.name)
                _instance._playerOneConfig = null;
            if (_instance._playerTwoConfig.name == inputConfig.name)
                _instance._playerTwoConfig = null;
            if (_instance._playerThreeConfig.name == inputConfig.name)
                _instance._playerThreeConfig = null;
            if (_instance._playerFourConfig.name == inputConfig.name)
                _instance._playerFourConfig = null;

            return true;
		}
		
		public static AxisConfiguration CreateButton(string inputConfigName, string buttonName, KeyCode primaryKey)
		{
			return CreateButton(inputConfigName, buttonName, primaryKey, KeyCode.None);
		}
		
		public static AxisConfiguration CreateButton(string inputConfigName, string buttonName, KeyCode primaryKey, KeyCode secondaryKey)
		{
			InputConfiguration inputConfig = GetInputConfiguration(inputConfigName);
			if(inputConfig == null)
			{
				Debug.LogError(string.Format("An input configuration named \'{0}\' does not exist", inputConfigName));
				return null;
			}
			if(_instance._axesTable[inputConfigName].ContainsKey(buttonName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, buttonName));
				return null;
			}
			
			AxisConfiguration axisConfig = new AxisConfiguration(buttonName);
			axisConfig.type = InputType.Button;
			axisConfig.positive = primaryKey;
			axisConfig.altPositive = secondaryKey;
			axisConfig.Initialize();
			inputConfig.axes.Add(axisConfig);
			
			var table = _instance._axesTable[inputConfigName];
			table.Add(buttonName, axisConfig);
			
			return axisConfig;
		}
		
		public static AxisConfiguration CreateDigitalAxis(string inputConfigName, string axisName, KeyCode positive, KeyCode negative, float gravity, float sensitivity)
		{
			return CreateDigitalAxis(inputConfigName, axisName, positive, negative, KeyCode.None, KeyCode.None, gravity, sensitivity);
		}
		
		public static AxisConfiguration CreateDigitalAxis(string inputConfigName, string axisName, KeyCode positive, KeyCode negative,
		                                                  KeyCode altPositive, KeyCode altNegative, float gravity, float sensitivity)
		{
			InputConfiguration inputConfig = GetInputConfiguration(inputConfigName);
			if(inputConfig == null)
			{
				Debug.LogError(string.Format("An input configuration named \'{0}\' does not exist", inputConfigName));
				return null;
			}
			if(_instance._axesTable[inputConfigName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, axisName));
				return null;
			}
			
			AxisConfiguration axisConfig = new AxisConfiguration(axisName);
			axisConfig.type = InputType.DigitalAxis;
			axisConfig.positive = positive;
			axisConfig.negative = negative;
			axisConfig.altPositive = altPositive;
			axisConfig.altNegative = altNegative;
			axisConfig.gravity = gravity;
			axisConfig.sensitivity = sensitivity;
			axisConfig.Initialize();
			inputConfig.axes.Add(axisConfig);
			
			var table = _instance._axesTable[inputConfigName];
			table.Add(axisName, axisConfig);
			
			return axisConfig;
		}
		
		public static AxisConfiguration CreateMouseAxis(string inputConfigName, string axisName, int axis, float sensitivity)
		{
			InputConfiguration inputConfig = GetInputConfiguration(inputConfigName);
			if(inputConfig == null)
			{
				Debug.LogError(string.Format("An input configuration named \'{0}\' does not exist", inputConfigName));
				return null;
			}
			if(_instance._axesTable[inputConfigName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, axisName));
				return null;
			}
			if(axis < 0 || axis > 2)
			{
				Debug.LogError("Mouse axis is out of range. Cannot create new mouse axis.");
				return null;
			}
			
			AxisConfiguration axisConfig = new AxisConfiguration(axisName);
			axisConfig.type = InputType.MouseAxis;
			axisConfig.axis = axis;
			axisConfig.sensitivity = sensitivity;
			axisConfig.Initialize();
			inputConfig.axes.Add(axisConfig);
			
			var table = _instance._axesTable[inputConfigName];
			table.Add(axisName, axisConfig);
			
			return axisConfig;
		}
		
		public static AxisConfiguration CreateAnalogAxis(string inputConfigName, string axisName, int joystick, int axis, float sensitivity, float deadZone)
		{
			InputConfiguration inputConfig = GetInputConfiguration(inputConfigName);
			if(inputConfig == null)
			{
				Debug.LogError(string.Format("An input configuration named \'{0}\' does not exist", inputConfigName));
				return null;
			}
			if(_instance._axesTable[inputConfigName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, axisName));
				return null;
			}
			if(axis < 0 || axis >= AxisConfiguration.MaxJoystickAxes)
			{
				Debug.LogError("Joystick axis is out of range. Cannot create new analog axis.");
				return null;
			}
			if(joystick < 0 || joystick >= AxisConfiguration.MaxJoysticks)
			{
				Debug.LogError("Joystick is out of range. Cannot create new analog axis.");
				return null;
			}
			
			AxisConfiguration axisConfig = new AxisConfiguration(axisName);
			axisConfig.type = InputType.AnalogAxis;
			axisConfig.axis = axis;
			axisConfig.joystick = joystick;
			axisConfig.deadZone = deadZone;
			axisConfig.sensitivity = sensitivity;
			axisConfig.Initialize();
			inputConfig.axes.Add(axisConfig);
			
			var table = _instance._axesTable[inputConfigName];
			table.Add(axisName, axisConfig);
			
			return axisConfig;
		}
		
		public static AxisConfiguration CreateRemoteAxis(string inputConfigName, string axisName)
		{
			InputConfiguration inputConfig = GetInputConfiguration(inputConfigName);
			if(inputConfig == null)
			{
				Debug.LogError(string.Format("An input configuration named \'{0}\' does not exist", inputConfigName));
				return null;
			}
			if(_instance._axesTable[inputConfigName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, axisName));
				return null;
			}
			
			AxisConfiguration axisConfig = new AxisConfiguration(axisName);
			axisConfig.type = InputType.RemoteAxis;
			axisConfig.positive = KeyCode.None;
			axisConfig.negative = KeyCode.None;
			axisConfig.altPositive = KeyCode.None;
			axisConfig.altNegative = KeyCode.None;
			axisConfig.Initialize();
			inputConfig.axes.Add(axisConfig);
			
			var table = _instance._axesTable[inputConfigName];
			table.Add(axisName, axisConfig);
			
			return axisConfig;
		}
		
		public static AxisConfiguration CreateRemoteButton(string inputConfigName, string buttonName)
		{
			InputConfiguration inputConfig = GetInputConfiguration(inputConfigName);
			if(inputConfig == null)
			{
				Debug.LogError(string.Format("An input configuration named \'{0}\' does not exist", inputConfigName));
				return null;
			}
			if(_instance._axesTable[inputConfigName].ContainsKey(buttonName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, buttonName));
				return null;
			}
			
			AxisConfiguration axisConfig = new AxisConfiguration(buttonName);
			axisConfig.type = InputType.RemoteButton;
			axisConfig.positive = KeyCode.None;
			axisConfig.negative = KeyCode.None;
			axisConfig.altPositive = KeyCode.None;
			axisConfig.altNegative = KeyCode.None;
			axisConfig.Initialize();
			inputConfig.axes.Add(axisConfig);
			
			var table = _instance._axesTable[inputConfigName];
			table.Add(buttonName, axisConfig);
			
			return axisConfig;
		}
		
		public static AxisConfiguration CreateAnalogButton(string inputConfigName, string buttonName, int joystick, int axis)
		{
			InputConfiguration inputConfig = GetInputConfiguration(inputConfigName);
			if(inputConfig == null)
			{
				Debug.LogError(string.Format("An input configuration named \'{0}\' does not exist", inputConfigName));
				return null;
			}
			if(_instance._axesTable[inputConfigName].ContainsKey(buttonName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, buttonName));
				return null;
			}
			if(axis < 0 || axis >= AxisConfiguration.MaxJoystickAxes)
			{
				Debug.LogError("Joystick axis is out of range. Cannot create new analog button.");
				return null;
			}
			if(joystick < 0 || joystick >= AxisConfiguration.MaxJoysticks)
			{
				Debug.LogError("Joystick is out of range. Cannot create new analog button.");
				return null;
			}
			
			AxisConfiguration axisConfig = new AxisConfiguration(buttonName);
			axisConfig.type = InputType.AnalogButton;
			axisConfig.joystick = joystick;
			axisConfig.axis = axis;
			axisConfig.positive = KeyCode.None;
			axisConfig.negative = KeyCode.None;
			axisConfig.altPositive = KeyCode.None;
			axisConfig.altNegative = KeyCode.None;
			axisConfig.Initialize();
			inputConfig.axes.Add(axisConfig);
			
			var table = _instance._axesTable[inputConfigName];
			table.Add(buttonName, axisConfig);
			
			return axisConfig;
		}
		
		/// <summary>
		/// Creates an uninitialized axis configuration. It's your responsability to configure the axis properly.
		/// </summary>
		public static AxisConfiguration CreateEmptyAxis(string inputConfigName, string axisName)
		{
			InputConfiguration inputConfig = GetInputConfiguration(inputConfigName);
			if(inputConfig == null)
			{
				Debug.LogError(string.Format("An input configuration named \'{0}\' does not exist", inputConfigName));
				return null;
			}
			if(_instance._axesTable[inputConfigName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an axis configuration named {1}", inputConfigName, axisName));
				return null;
			}
			
			AxisConfiguration axisConfig = new AxisConfiguration(axisName);
			axisConfig.Initialize();
			inputConfig.axes.Add(axisConfig);
			
			var table = _instance._axesTable[inputConfigName];
			table.Add(axisName, axisConfig);
			
			return axisConfig;
		}
		
		public static bool DeleteAxisConfiguration(string inputConfigName, string axisName)
		{
			InputConfiguration inputConfig = GetInputConfiguration(inputConfigName);
			AxisConfiguration axisConfig = GetAxisConfiguration(inputConfigName, axisName);
			if(inputConfig != null && axisConfig != null)
			{
				_instance._axesTable[inputConfig.name].Remove(axisConfig.name);
				inputConfig.axes.Remove(axisConfig);
				return true;
			}
			
			return false;
		}
		
		/// <summary>
		/// Scans for keyboard input and calls the handler with the result.
		/// Returns KeyCode.None if timeout is reached or the scan is canceled.
		/// </summary>
		public static void StartKeyScan(KeyScanHandler scanHandler, float timeout, string cancelScanButton, params object[] userData)
		{
			if(_instance._scanFlags != ScanFlags.None)
				_instance.StopInputScan();
			
			_instance._scanTimeout = timeout;
			_instance._scanFlags = ScanFlags.Key | ScanFlags.JoystickButton;
			_instance._scanStartTime = _instance.ignoreTimescale ? Time.realtimeSinceStartup : Time.time;
			_instance._cancelScanButton = cancelScanButton;
			_instance._scanUserData = userData;
			_instance._scanHandler = (result) => {
				return scanHandler(result.key, (object[])result.userData);
			};
		}
		
		/// <summary>
		/// Scans for mouse input and calls the handler with the result.
		/// Returns -1 if timeout is reached or the scan is canceled.
		/// </summary>
		public static void StartMouseAxisScan(AxisScanHandler scanHandler, float timeout, string cancelScanButton, params object[] userData)
		{
			if(_instance._scanFlags != ScanFlags.None)
				_instance.StopInputScan();
			
			_instance._scanTimeout = timeout;
			_instance._scanFlags = ScanFlags.MouseAxis;
			_instance._scanStartTime = _instance.ignoreTimescale ? Time.realtimeSinceStartup : Time.time;
			_instance._cancelScanButton = cancelScanButton;
			_instance._scanUserData = userData;
			_instance._scanHandler = (result) => {
				return scanHandler(result.mouseAxis, (object[])result.userData);
			};
		}
		
		/// <summary>
		/// Scans for joystick input and calls the handler with the result.
		/// Returns -1 if timeout is reached or the scan is canceled.
		/// </summary>
		public static void StartJoystickAxisScan(AxisScanHandler scanHandler, int joystick, float timeout, string cancelScanButton, params object[] userData)
		{
			if(joystick < 0 || joystick >= AxisConfiguration.MaxJoystickAxes)
			{
				Debug.LogError("Joystick is out of range. Cannot start joystick axis scan.");
				return;
			}
			
			if(_instance._scanFlags != ScanFlags.None)
				_instance.StopInputScan();
			
			_instance._scanTimeout = timeout;
			_instance._scanFlags = ScanFlags.JoystickAxis;
			_instance._scanStartTime = _instance.ignoreTimescale ? Time.realtimeSinceStartup : Time.time;
			_instance._cancelScanButton = cancelScanButton;
			_instance._scanJoystick = joystick;
			_instance._scanUserData = userData;
			_instance._scanHandler = (result) => {
				return scanHandler(result.joystickAxis, (object[])result.userData);
			};
		}
		
		public static void StartScan(ScanSettings settings, ScanHandler scanHandler)
		{
			if(settings.joystick < 0 || settings.joystick >= AxisConfiguration.MaxJoystickAxes)
			{
				Debug.LogError("Joystick is out of range. Cannot start scan.");
				return;
			}
			
			if(_instance._scanFlags != ScanFlags.None)
				_instance.StopInputScan();
			
			_instance._scanTimeout = settings.timeout;
			_instance._scanFlags = settings.scanFlags;
			_instance._scanStartTime = _instance.ignoreTimescale ? Time.realtimeSinceStartup : Time.time;
			_instance._cancelScanButton = settings.cancelScanButton;
			_instance._scanJoystick = settings.joystick;
			_instance._scanUserData = settings.userData;
			_instance._scanHandler = scanHandler;
		}
		
		public static void CancelScan()
		{
			if(_instance._scanFlags != ScanFlags.None)
				_instance.StopInputScan();
		}
		
		/// <summary>
		/// Triggers the ConfigurationDirty event.
		/// </summary>
		public static void SetConfigurationDirty(string inputConfigName)
		{
			_instance.RaiseConfigurationDirtyEvent(inputConfigName);
		}
		
		/// <summary>
		/// Saves the input configurations in the XML format, in Application.persistentDataPath.
		/// </summary>
		public static void Save()
		{
#if UNITY_WINRT && !UNITY_EDITOR
			string filename = Application.persistentDataPath + "/input_config.xml";
#else
			string filename = System.IO.Path.Combine(Application.persistentDataPath, "input_config.xml");
#endif
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
			if(inputSaver != null)
			{
                inputSaver.Save(_instance.GetSaveParameters());
				_instance.RaiseSavedEvent();
			}
			else
			{
				Debug.LogError("InputSaver is null. Cannot save input configurations.");
			}
		}

        /// <summary>
		/// Loads the input configurations saved in the XML format, from Application.persistentDataPath.
		/// </summary>
		public static void Load()
		{
#if UNITY_WINRT && !UNITY_EDITOR
			string filename = Application.persistentDataPath + "/input_config.xml";
			if(UnityEngine.Windows.File.Exists(filename))
			{
				Load(new InputLoaderXML(filename));
			}
#else
			string filename = System.IO.Path.Combine(Application.persistentDataPath, "input_config.xml");
			if(System.IO.File.Exists(filename))
			{
				Load(new InputLoaderXML(filename));
			}
#endif
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
			if(inputLoader != null)
			{
				_instance.Load(inputLoader.Load());
                _instance.Initialize();
                _instance.RaiseLoadedEvent();
            }
			else
			{
				Debug.LogError("InputLoader is null. Cannot load input configurations.");
			}
		}
		
		#endregion
	}
}
