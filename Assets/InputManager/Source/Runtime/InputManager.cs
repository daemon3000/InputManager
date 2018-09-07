#region [Copyright (c) 2018 Cristian Alexandru Geambasu]
//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2018 Cristian Alexandru Geambasu
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
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using System;
using System.Collections.Generic;

namespace Luminosity.IO
{
	/// <summary>
	/// Encapsulates a method that takes one parameter(the scan result) and returns 'true' if
	/// the scan result is accepted or 'false' if it isn't.
	/// </summary>
	public delegate bool ScanHandler(ScanResult result);

	public delegate void RemoteUpdateDelegate(PlayerID playerID);

	public partial class InputManager : MonoBehaviour
	{
		public const string VERSION = "2018.9.7";

		[SerializeField]
		private List<ControlScheme> m_controlSchemes = new List<ControlScheme>();
		[SerializeField]
		private string m_playerOneDefault;
		[SerializeField]
		private string m_playerTwoDefault;
		[SerializeField]
		private string m_playerThreeDefault;
		[SerializeField]
		private string m_playerFourDefault;
		[SerializeField]
		private bool m_ignoreTimescale = true;

		private ControlScheme m_playerOneScheme;
		private ControlScheme m_playerTwoScheme;
		private ControlScheme m_playerThreeScheme;
		private ControlScheme m_playerFourScheme;
		private ScanService m_scanService;
		private Action<PlayerID> m_playerControlsChangedHandler;
		private Action m_controlSchemesChangedHandler;
		private Action m_loadedHandler;
		private Action m_savedHandler;
		private Action m_beforeUpdateHandler;
		private Action m_afterUpdateHandler;
		private RemoteUpdateDelegate m_remoteUpdateHandler;
		private static InputManager m_instance;
		
		private Dictionary<string, ControlScheme> m_schemeLookup;
		private Dictionary<string, ControlScheme> m_schemeLookupByID;
		private Dictionary<string, Dictionary<string, InputAction>> m_actionLookup;

		public List<ControlScheme> ControlSchemes
		{
			get { return m_controlSchemes; }
		}

		public string PlayerOneDefault
		{
			get { return m_playerOneDefault; }
			set { m_playerOneDefault = value; }
		}

		public string PlayerTwoDefault
		{
			get { return m_playerTwoDefault; }
			set { m_playerTwoDefault = value; }
		}

		public string PlayerThreeDefault
		{
			get { return m_playerThreeDefault; }
			set { m_playerThreeDefault = value; }
		}

		public string PlayerFourDefault
		{
			get { return m_playerFourDefault; }
			set { m_playerFourDefault = value; }
		}

		public bool IgnoreTimescale
		{
			get { return m_ignoreTimescale; }
			set { m_ignoreTimescale = value; }
		}

		private void Awake()
		{
			if(m_instance == null)
			{
				m_instance = this;
				m_scanService = new ScanService();
				m_schemeLookup = new Dictionary<string, ControlScheme>();
				m_schemeLookupByID = new Dictionary<string, ControlScheme>();
				m_actionLookup = new Dictionary<string, Dictionary<string, InputAction>>();
				Initialize();
			}
			else
			{
				Debug.LogWarning("You have multiple InputManager instances in the scene!", gameObject);
				Destroy(this);
			}
		}

		private void OnDestroy()
		{
			if(m_instance == this)
			{
				m_instance = null;
			}

			m_playerControlsChangedHandler = null;
			m_controlSchemesChangedHandler = null;
			m_loadedHandler = null;
			m_savedHandler = null;
			m_remoteUpdateHandler = null;
		}

		private void Initialize()
		{
			m_schemeLookup.Clear();
			m_actionLookup.Clear();
			m_playerOneScheme = null;
			m_playerTwoScheme = null;
			m_playerThreeScheme = null;
			m_playerFourScheme = null;

			if(m_controlSchemes.Count == 0)
				return;

			PopulateLookupTables();

			if(!string.IsNullOrEmpty(m_playerOneDefault) && m_schemeLookupByID.ContainsKey(m_playerOneDefault))
			{
				m_playerOneScheme = m_schemeLookupByID[m_playerOneDefault];
			}
			else
			{
				if(m_controlSchemes.Count > 0)
					m_playerOneScheme = m_controlSchemes[0];
			}

			if(!string.IsNullOrEmpty(m_playerTwoDefault) && m_schemeLookupByID.ContainsKey(m_playerTwoDefault))
			{
				m_playerTwoScheme = m_schemeLookupByID[m_playerTwoDefault];
			}

			if(!string.IsNullOrEmpty(m_playerThreeDefault) && m_schemeLookupByID.ContainsKey(m_playerThreeDefault))
			{
				m_playerThreeScheme = m_schemeLookupByID[m_playerThreeDefault];
			}

			if(!string.IsNullOrEmpty(m_playerFourDefault) && m_schemeLookupByID.ContainsKey(m_playerFourDefault))
			{
				m_playerFourScheme = m_schemeLookupByID[m_playerFourDefault];
			}

			foreach(ControlScheme scheme in m_controlSchemes)
			{
				scheme.Initialize();
			}

			Input.ResetInputAxes();
		}

		private void PopulateLookupTables()
		{
			m_schemeLookup.Clear();
			m_schemeLookupByID.Clear();
			foreach(ControlScheme scheme in m_controlSchemes)
			{
				m_schemeLookup[scheme.Name] = scheme;
				m_schemeLookupByID[scheme.UniqueID] = scheme;
			}

			m_actionLookup.Clear();
			foreach(ControlScheme scheme in m_controlSchemes)
			{
				m_actionLookup[scheme.Name] = scheme.GetActionLookupTable();
			}
		}

		private void Update()
		{
			if(m_beforeUpdateHandler != null)
				m_beforeUpdateHandler();

			UpdateControlScheme(m_playerOneScheme, PlayerID.One);
			UpdateControlScheme(m_playerTwoScheme, PlayerID.Two);
			UpdateControlScheme(m_playerThreeScheme, PlayerID.Three);
			UpdateControlScheme(m_playerFourScheme, PlayerID.Four);

			m_scanService.GameTime = m_ignoreTimescale ? Time.unscaledTime : Time.time;
			if(m_scanService.IsScanning)
			{
				m_scanService.Update();
			}

			if(m_afterUpdateHandler != null)
				m_afterUpdateHandler();
		}

		private void UpdateControlScheme(ControlScheme scheme, PlayerID playerID)
		{
			if(scheme != null)
			{
				float deltaTime = m_ignoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
				scheme.Update(deltaTime);

				if(m_remoteUpdateHandler != null)
					m_remoteUpdateHandler(playerID);
			}
		}

		private void SetControlSchemeByPlayerID(PlayerID playerID, ControlScheme scheme)
		{
			if(playerID == PlayerID.One)
				m_playerOneScheme = scheme;
			else if(playerID == PlayerID.Two)
				m_playerTwoScheme = scheme;
			else if(playerID == PlayerID.Three)
				m_playerThreeScheme = scheme;
			else if(playerID == PlayerID.Four)
				m_playerFourScheme = scheme;
		}

		private ControlScheme GetControlSchemeByPlayerID(PlayerID playerID)
		{
			if(playerID == PlayerID.One)
				return m_playerOneScheme;
			else if(playerID == PlayerID.Two)
				return m_playerTwoScheme;
			else if(playerID == PlayerID.Three)
				return m_playerThreeScheme;
			else if(playerID == PlayerID.Four)
				return m_playerFourScheme;
			else
				return null;
		}

		private PlayerID? IsControlSchemeInUse(string name)
		{
			if(m_playerOneScheme != null && m_playerOneScheme.Name == name)
				return PlayerID.One;
			if(m_playerTwoScheme != null && m_playerTwoScheme.Name == name)
				return PlayerID.Two;
			if(m_playerThreeScheme != null && m_playerThreeScheme.Name == name)
				return PlayerID.Three;
			if(m_playerFourScheme != null && m_playerFourScheme.Name == name)
				return PlayerID.Four;

			return null;
		}

		public void SetSaveData(SaveData saveData)
		{
			if(saveData != null)
			{
				m_controlSchemes = saveData.ControlSchemes;
				m_playerOneDefault = saveData.PlayerOneScheme;
				m_playerTwoDefault = saveData.PlayerTwoScheme;
				m_playerThreeDefault = saveData.PlayerThreeScheme;
				m_playerFourDefault = saveData.PlayerFourScheme;
			}
		}

		public SaveData GetSaveData()
		{
			return new SaveData()
			{
				ControlSchemes = m_controlSchemes,
				PlayerOneScheme = m_playerOneDefault,
				PlayerTwoScheme = m_playerTwoDefault,
				PlayerThreeScheme = m_playerThreeDefault,
				PlayerFourScheme = m_playerFourDefault
			};
		}

		private void OnInitializeAfterScriptReload()
		{
			if(m_instance != null && m_instance != this)
			{
				Debug.LogWarning("You have multiple InputManager instances in the scene!", gameObject);
			}
			else if(m_instance == null)
			{
				m_instance = this;
				m_schemeLookup = new Dictionary<string, ControlScheme>();
				m_actionLookup = new Dictionary<string, Dictionary<string, InputAction>>();
				
				Initialize();
			}
		}

		private void RaisePlayerControlsChangedEvent(PlayerID playerID)
		{
			if(m_playerControlsChangedHandler != null)
				m_playerControlsChangedHandler(playerID);
		}

		private void RaiseControlSchemesChangedEvent()
		{
			if(m_controlSchemesChangedHandler != null)
				m_controlSchemesChangedHandler();
		}

		private void RaiseLoadedEvent()
		{
			if(m_loadedHandler != null)
				m_loadedHandler();
		}

		private void RaiseSavedEvent()
		{
			if(m_savedHandler != null)
				m_savedHandler();
		}

		#region [Static Interface]
		public static event Action<PlayerID> PlayerControlsChanged
		{
			add { if(m_instance != null) m_instance.m_playerControlsChangedHandler += value; }
			remove { if(m_instance != null) m_instance.m_playerControlsChangedHandler -= value; }
		}

		public static event Action ControlSchemesChanged
		{
			add { if(m_instance != null) m_instance.m_controlSchemesChangedHandler += value; }
			remove { if(m_instance != null) m_instance.m_controlSchemesChangedHandler -= value; }
		}

		public static event Action Loaded
		{
			add { if(m_instance != null) m_instance.m_loadedHandler += value; }
			remove { if(m_instance != null) m_instance.m_loadedHandler -= value; }
		}

		public static event Action Saved
		{
			add { if(m_instance != null) m_instance.m_savedHandler += value; }
			remove { if(m_instance != null) m_instance.m_savedHandler -= value; }
		}

		public static event Action BeforeUpdate
		{
			add { if(m_instance != null) m_instance.m_beforeUpdateHandler += value; }
			remove { if(m_instance != null) m_instance.m_beforeUpdateHandler -= value; }
		}

		public static event Action AfterUpdate
		{
			add { if(m_instance != null) m_instance.m_afterUpdateHandler += value; }
			remove { if(m_instance != null) m_instance.m_afterUpdateHandler -= value; }
		}

		public static event RemoteUpdateDelegate RemoteUpdate
		{
			add { if(m_instance != null) m_instance.m_remoteUpdateHandler += value; }
			remove { if(m_instance != null) m_instance.m_remoteUpdateHandler -= value; }
		}

		public static bool Exists
		{
			get { return m_instance != null; }
		}

		public static bool IsScanning
		{
			get { return m_instance.m_scanService.IsScanning; }
		}

		public static ControlScheme PlayerOneControlScheme
		{
			get { return m_instance.m_playerOneScheme; }
		}

		public static ControlScheme PlayerTwoControlScheme
		{
			get { return m_instance.m_playerTwoScheme; }
		}

		public static ControlScheme PlayerThreeControlScheme
		{
			get { return m_instance.m_playerThreeScheme; }
		}

		public static ControlScheme PlayerFourControlScheme
		{
			get { return m_instance.m_playerFourScheme; }
		}

		/// <summary>
		/// Returns true if any axis of any active control scheme is receiving input.
		/// </summary>
		public static bool AnyInput()
		{
			return AnyInput(m_instance.m_playerOneScheme) || AnyInput(m_instance.m_playerTwoScheme) ||
					AnyInput(m_instance.m_playerThreeScheme) || AnyInput(m_instance.m_playerFourScheme);
		}

		/// <summary>
		/// Returns true if any axis of the control scheme is receiving input.
		/// </summary>
		public static bool AnyInput(PlayerID playerID)
		{
			return AnyInput(m_instance.GetControlSchemeByPlayerID(playerID));
		}

		/// <summary>
		/// Returns true if any axis of the specified control scheme is receiving input.
		/// </summary>
		public static bool AnyInput(string schemeName)
		{
			ControlScheme scheme;
			if(m_instance.m_schemeLookup.TryGetValue(schemeName, out scheme))
			{
				return scheme.AnyInput;
			}

			return false;
		}

		private static bool AnyInput(ControlScheme scheme)
		{
			if(scheme != null)
				return scheme.AnyInput;

			return false;
		}

		/// <summary>
		/// Resets the internal state of the input manager.
		/// </summary>
		public static void Reinitialize()
		{
			m_instance.RaiseControlSchemesChangedEvent();
			m_instance.Initialize();
		}

		/// <summary>
		/// Changes the active control scheme.
		/// </summary>
		public static void SetControlScheme(string name, PlayerID playerID)
		{
			PlayerID? playerWhoUsesControlScheme = m_instance.IsControlSchemeInUse(name);

			if(playerWhoUsesControlScheme.HasValue && playerWhoUsesControlScheme.Value != playerID)
			{
				Debug.LogErrorFormat("The control scheme named \'{0}\' is already being used by player {1}", name, playerWhoUsesControlScheme.Value.ToString());
				return;
			}

			if(playerWhoUsesControlScheme.HasValue && playerWhoUsesControlScheme.Value == playerID)
				return;

			ControlScheme controlScheme = null;
			if(m_instance.m_schemeLookup.TryGetValue(name, out controlScheme))
			{
				controlScheme.Reset();
				m_instance.SetControlSchemeByPlayerID(playerID, controlScheme);
				m_instance.RaisePlayerControlsChangedEvent(playerID);
			}
			else
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", name));
			}
		}

		public static ControlScheme GetControlScheme(string name)
		{
			ControlScheme scheme = null;
			if(m_instance.m_schemeLookup.TryGetValue(name, out scheme))
				return scheme;

			return null;
		}

		public static ControlScheme GetControlScheme(PlayerID playerID)
		{
			return m_instance.GetControlSchemeByPlayerID(playerID);
		}

		public static InputAction GetAction(string controlSchemeName, string actionName)
		{
			Dictionary<string, InputAction> table;
			if(m_instance.m_actionLookup.TryGetValue(controlSchemeName, out table))
			{
				InputAction action;
				if(table.TryGetValue(actionName, out action))
					return action;
			}

			return null;
		}

		public static InputAction GetAction(PlayerID playerID, string actionName)
		{
			var scheme = m_instance.GetControlSchemeByPlayerID(playerID);
			if(scheme == null)
				return null;

			Dictionary<string, InputAction> table;
			if(m_instance.m_actionLookup.TryGetValue(scheme.Name, out table))
			{
				InputAction action;
				if(table.TryGetValue(actionName, out action))
					return action;
			}

			return null;
		}

		public static ControlScheme CreateControlScheme(string name)
		{
			if(m_instance.m_schemeLookup.ContainsKey(name))
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' already exists", name));
				return null;
			}

			ControlScheme scheme = new ControlScheme(name);
			m_instance.m_controlSchemes.Add(scheme);
			m_instance.m_schemeLookup[name] = scheme;
			m_instance.m_actionLookup[name] = new Dictionary<string, InputAction>();

			return scheme;
		}

		/// <summary>
		/// Deletes the specified control scheme. If the speficied control scheme is
		/// active for any player then the active control scheme for the respective player will be set to null.
		/// </summary>
		public static bool DeleteControlScheme(string name)
		{
			ControlScheme scheme = GetControlScheme(name);
			if(scheme == null)
				return false;

			m_instance.m_actionLookup.Remove(name);
			m_instance.m_schemeLookup.Remove(name);
			m_instance.m_controlSchemes.Remove(scheme);
			if(m_instance.m_playerOneScheme.Name == scheme.Name)
				m_instance.m_playerOneScheme = null;
			if(m_instance.m_playerTwoScheme.Name == scheme.Name)
				m_instance.m_playerTwoScheme = null;
			if(m_instance.m_playerThreeScheme.Name == scheme.Name)
				m_instance.m_playerThreeScheme = null;
			if(m_instance.m_playerFourScheme.Name == scheme.Name)
				m_instance.m_playerFourScheme = null;

			return true;
		}

		public static InputAction CreateButton(string controlSchemeName, string buttonName, KeyCode primaryKey)
		{
			return CreateButton(controlSchemeName, buttonName, primaryKey, KeyCode.None);
		}

		public static InputAction CreateButton(string controlSchemeName, string buttonName, KeyCode primaryKey, KeyCode secondaryKey)
		{
			ControlScheme scheme = GetControlScheme(controlSchemeName);
			if(scheme == null)
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", controlSchemeName));
				return null;
			}
			if(m_instance.m_actionLookup[controlSchemeName].ContainsKey(buttonName))
			{
				Debug.LogError(string.Format("The control scheme named {0} already contains an action named {1}", controlSchemeName, buttonName));
				return null;
			}

			InputAction action = scheme.CreateNewAction(buttonName);
			InputBinding primary = action.CreateNewBinding();
			primary.Type = InputType.Button;
			primary.Positive = primaryKey;

			InputBinding secondary = action.CreateNewBinding(primary);
			secondary.Positive = secondaryKey;

			action.Initialize();
			m_instance.m_actionLookup[controlSchemeName][buttonName] = action;

			return action;
		}

		public static InputAction CreateDigitalAxis(string controlSchemeName, string axisName, KeyCode positive, KeyCode negative, float gravity, float sensitivity)
		{
			return CreateDigitalAxis(controlSchemeName, axisName, positive, negative, KeyCode.None, KeyCode.None, gravity, sensitivity);
		}

		public static InputAction CreateDigitalAxis(string controlSchemeName, string axisName, KeyCode positive, KeyCode negative,
														  KeyCode altPositive, KeyCode altNegative, float gravity, float sensitivity)
		{
			ControlScheme scheme = GetControlScheme(controlSchemeName);
			if(scheme == null)
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", controlSchemeName));
				return null;
			}
			if(m_instance.m_actionLookup[controlSchemeName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The control scheme named {0} already contains an action named {1}", controlSchemeName, axisName));
				return null;
			}

			InputAction action = scheme.CreateNewAction(axisName);
			InputBinding primary = action.CreateNewBinding();
			primary.Type = InputType.DigitalAxis;
			primary.Positive = positive;
			primary.Negative = negative;
			primary.Gravity = gravity;
			primary.Sensitivity = sensitivity;

			InputBinding secondary = action.CreateNewBinding(primary);
			secondary.Positive = altPositive;
			secondary.Negative = altNegative;

			action.Initialize();
			m_instance.m_actionLookup[controlSchemeName][axisName] = action;

			return action;
		}

		public static InputAction CreateMouseAxis(string controlSchemeName, string axisName, int axis, float sensitivity)
		{
			ControlScheme scheme = GetControlScheme(controlSchemeName);
			if(scheme == null)
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", controlSchemeName));
				return null;
			}
			if(m_instance.m_actionLookup[controlSchemeName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The control scheme named {0} already contains an action named {1}", controlSchemeName, axisName));
				return null;
			}
			if(axis < 0 || axis >= InputBinding.MAX_MOUSE_AXES)
			{
				Debug.LogError("Mouse axis is out of range. Cannot create new mouse axis.");
				return null;
			}

			InputAction action = scheme.CreateNewAction(axisName);
			InputBinding binding = action.CreateNewBinding();
			binding.Type = InputType.MouseAxis;
			binding.Axis = axis;
			binding.Sensitivity = sensitivity;

			action.Initialize();
			m_instance.m_actionLookup[controlSchemeName][axisName] = action;

			return action;
		}

		public static InputAction CreateAnalogButton(string controlSchemeName, string buttonName, int joystick, int axis)
		{
			ControlScheme scheme = GetControlScheme(controlSchemeName);
			if(scheme == null)
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", controlSchemeName));
				return null;
			}
			if(m_instance.m_actionLookup[controlSchemeName].ContainsKey(buttonName))
			{
				Debug.LogError(string.Format("The input configuration named {0} already contains an action named {1}", controlSchemeName, buttonName));
				return null;
			}
			if(axis < 0 || axis >= InputBinding.MAX_JOYSTICK_AXES)
			{
				Debug.LogError("Joystick axis is out of range. Cannot create new analog button.");
				return null;
			}
			if(joystick < 0 || joystick >= InputBinding.MAX_JOYSTICKS)
			{
				Debug.LogError("Joystick is out of range. Cannot create new analog button.");
				return null;
			}

			InputAction action = scheme.CreateNewAction(buttonName);
			InputBinding binding = action.CreateNewBinding();
			binding.Type = InputType.AnalogButton;
			binding.Joystick = joystick;
			binding.Axis = axis;

			action.Initialize();
			m_instance.m_actionLookup[controlSchemeName][buttonName] = action;

			return action;
		}

		public static InputAction CreateAnalogAxis(string controlScheme, string axisName, int joystick, int axis, float sensitivity, float deadZone)
		{
			ControlScheme scheme = GetControlScheme(controlScheme);
			if(scheme == null)
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", controlScheme));
				return null;
			}
			if(m_instance.m_actionLookup[controlScheme].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The control scheme named {0} already contains an action named {1}", controlScheme, axisName));
				return null;
			}
			if(axis < 0 || axis >= InputBinding.MAX_JOYSTICK_AXES)
			{
				Debug.LogError("Joystick axis is out of range. Cannot create new analog axis.");
				return null;
			}
			if(joystick < 0 || joystick >= InputBinding.MAX_JOYSTICKS)
			{
				Debug.LogError("Joystick is out of range. Cannot create new analog axis.");
				return null;
			}

			InputAction action = scheme.CreateNewAction(axisName);
			InputBinding binding = action.CreateNewBinding();
			binding.Type = InputType.AnalogAxis;
			binding.Axis = axis;
			binding.Joystick = joystick;
			binding.DeadZone = deadZone;
			binding.Sensitivity = sensitivity;

			action.Initialize();
			m_instance.m_actionLookup[controlScheme][axisName] = action;

			return action;
		}

		public static InputAction CreateRemoteButton(string controlSchemeName, string buttonName)
		{
			ControlScheme scheme = GetControlScheme(controlSchemeName);
			if(scheme == null)
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", controlSchemeName));
				return null;
			}
			if(m_instance.m_actionLookup[controlSchemeName].ContainsKey(buttonName))
			{
				Debug.LogError(string.Format("The control scheme named {0} already contains an action named {1}", controlSchemeName, buttonName));
				return null;
			}

			InputAction action = scheme.CreateNewAction(buttonName);
			InputBinding binding = action.CreateNewBinding();
			binding.Type = InputType.RemoteButton;

			action.Initialize();
			m_instance.m_actionLookup[controlSchemeName][buttonName] = action;

			return action;
		}

		public static InputAction CreateRemoteAxis(string controlSchemeName, string axisName)
		{
			ControlScheme scheme = GetControlScheme(controlSchemeName);
			if(scheme == null)
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", controlSchemeName));
				return null;
			}
			if(m_instance.m_actionLookup[controlSchemeName].ContainsKey(axisName))
			{
				Debug.LogError(string.Format("The control scheme named {0} already contains an action named {1}", controlSchemeName, axisName));
				return null;
			}

			InputAction action = scheme.CreateNewAction(axisName);
			InputBinding binding = action.CreateNewBinding();
			binding.Type = InputType.RemoteAxis;

			action.Initialize();
			m_instance.m_actionLookup[controlSchemeName][axisName] = action;

			return action;
		}
		
		/// <summary>
		/// Creates an uninitialized input action. It's your responsability to configure it properly.
		/// </summary>
		public static InputAction CreateEmptyAction(string controlSchemeName, string actionName)
		{
			ControlScheme scheme = GetControlScheme(controlSchemeName);
			if(scheme == null)
			{
				Debug.LogError(string.Format("A control scheme named \'{0}\' does not exist", controlSchemeName));
				return null;
			}
			if(m_instance.m_actionLookup[controlSchemeName].ContainsKey(actionName))
			{
				Debug.LogError(string.Format("The control scheme named {0} already contains an action named {1}", controlSchemeName, actionName));
				return null;
			}

			InputAction action = scheme.CreateNewAction(actionName);
			m_instance.m_actionLookup[controlSchemeName][actionName] = action;

			return action;
		}

		public static bool DeleteAction(string controlSchemeName, string actionName)
		{
			ControlScheme scheme = GetControlScheme(controlSchemeName);
			InputAction action = GetAction(controlSchemeName, actionName);
			if(scheme != null && action != null)
			{
				m_instance.m_actionLookup[scheme.Name].Remove(action.Name);
				scheme.DeleteAction(action);
				return true;
			}

			return false;
		}

		public static void StartInputScan(ScanSettings settings, ScanHandler scanHandler)
		{
			m_instance.m_scanService.Start(settings, scanHandler);
		}

		public static void StopInputScan()
		{
			m_instance.m_scanService.Stop();
		}

		/// <summary>
		/// Saves the control schemes in an XML file, in Application.persistentDataPath.
		/// </summary>
		public static void Save()
		{
			string filename = Application.persistentDataPath + "/input_config.xml";
			Save(new InputSaverXML(filename));
		}

		/// <summary>
		/// Saves the control schemes in the XML format, at the specified location.
		/// </summary>
		public static void Save(string filename)
		{
			Save(new InputSaverXML(filename));
		}

		public static void Save(IInputSaver inputSaver)
		{
			if(inputSaver != null)
			{
				inputSaver.Save(m_instance.GetSaveData());
				m_instance.RaiseSavedEvent();
			}
			else
			{
				Debug.LogError("InputSaver is null. Cannot save control schemes.");
			}
		}

		/// <summary>
		/// Loads the control schemes from an XML file, from Application.persistentDataPath.
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
			string filename = Application.persistentDataPath + "/input_config.xml";
			if(System.IO.File.Exists(filename))
			{
				Load(new InputLoaderXML(filename));
			}
#endif
		}

		/// <summary>
		/// Loads the control schemes saved in the XML format, from the specified location.
		/// </summary>
		public static void Load(string filename)
		{
			Load(new InputLoaderXML(filename));
		}

		public static void Load(IInputLoader inputLoader)
		{
			if(inputLoader != null)
			{
				m_instance.SetSaveData(inputLoader.Load());
				m_instance.Initialize();
				m_instance.RaiseLoadedEvent();
			}
			else
			{
				Debug.LogError("InputLoader is null. Cannot load control schemes.");
			}
		}

#if UNITY_EDITOR
		[DidReloadScripts(0)]
		private static void OnScriptReload()
		{
			if(EditorApplication.isPlaying)
			{
				InputManager[] inputManagers = FindObjectsOfType<InputManager>();
				for(int i = 0; i < inputManagers.Length; i++)
				{
					inputManagers[i].OnInitializeAfterScriptReload();
				}
			}
		}
#endif
		#endregion
	}
}
