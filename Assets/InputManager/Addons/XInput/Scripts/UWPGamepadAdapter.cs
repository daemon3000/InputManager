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
#if !UNITY_EDITOR && UNITY_WSA && ENABLE_X_INPUT
using UnityEngine.Profiling;
using Windows.Gaming.Input;
using WinVibration = Windows.Gaming.Input.GamepadVibration;
using LumiVibration = Luminosity.IO.GamepadVibration;
#endif

namespace Luminosity.IO
{
	public class UWPGamepadAdapter : MonoBehaviour, IGamepadStateAdapter
	{
		[SerializeField]
		private float m_dpadGravity = 3.0f;
		[SerializeField]
		private float m_dpadSensitivity = 3.0f;
		[SerializeField]
		private bool m_dpadSnap = true;
		[SerializeField]
		private bool m_ignoreTimescale = true;

		public float DPadGravity
		{
			get { return m_dpadGravity; }
			set { m_dpadGravity = value; }
		}

		public float DPadSensitivity
		{
			get { return m_dpadSensitivity; }
			set { m_dpadSensitivity = value; }
		}

		public bool DPadSnap
		{
			get { return m_dpadSnap; }
			set { m_dpadSnap = value; }
		}

		public bool IgnoreTimescale
		{
			get { return m_ignoreTimescale; }
			set { m_ignoreTimescale = value; }
		}

#if !UNITY_EDITOR && UNITY_WSA && ENABLE_X_INPUT
		private struct DPADState { public float X; public float Y; }
		private const int MAX_GAMEPADS = 4;

		[System.NonSerialized]
		private Gamepad[] m_gamepads;

		[System.NonSerialized]
		private GamepadReading?[] m_currentState;

		[System.NonSerialized]
		private GamepadReading?[] m_previousState;

		[System.NonSerialized]
		private DPADState[] m_dpadState;

		private void Awake()
		{
			m_gamepads = new Gamepad[MAX_GAMEPADS];
			m_currentState = new GamepadReading?[MAX_GAMEPADS];
			m_previousState = new GamepadReading?[MAX_GAMEPADS];
			m_dpadState = new DPADState[4];

			UpdateGamepadList();
			Gamepad.GamepadAdded += HandleGamepadAdded;
			Gamepad.GamepadRemoved += HandleGamepadRemoved;
			InputManager.BeforeUpdate += OnUpdate;

			if(GamepadState.Adapter == null)
			{
				GamepadState.Adapter = this;
			}
			else
			{
				Debug.LogWarning("You shouldn't have more than one gamepad adapter in the scene");
			}
		}

		private void OnDestroy()
		{
			InputManager.BeforeUpdate -= OnUpdate;
			Gamepad.GamepadAdded -= HandleGamepadAdded;
			Gamepad.GamepadRemoved -= HandleGamepadRemoved;

			if(GamepadState.Adapter == (IGamepadStateAdapter)this)
			{
				GamepadState.Adapter = null;
			}
		}

		private void OnUpdate()
		{
            Profiler.BeginSample("UWPGamepadAdapter.OnUpdate", this);
			for(int i = 0; i < MAX_GAMEPADS; i++)
			{
				m_previousState[i] = m_currentState[i];
				m_currentState[i] = GetGamepadState(i);
			}

			float deltaTime = m_ignoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
			UpdateDPADHorizontal(deltaTime);
			UpdateDPADVertical(deltaTime);
            Profiler.EndSample();
		}

		private void UpdateDPADHorizontal(float deltaTime)
		{
			for(int i = 0; i < m_dpadState.Length; i++)
			{
				bool rightPressed = m_currentState[i].HasValue ? m_currentState[i].Value.Buttons.HasFlag(GamepadButtons.DPadRight) : false;
				bool leftPressed = m_currentState[i].HasValue ? m_currentState[i].Value.Buttons.HasFlag(GamepadButtons.DPadLeft) : false;

				if(rightPressed)
				{
					if(m_dpadState[i].X < InputBinding.AXIS_NEUTRAL && m_dpadSnap)
					{
						m_dpadState[i].X = InputBinding.AXIS_NEUTRAL;
					}

					m_dpadState[i].X += m_dpadSensitivity * deltaTime;
					if(m_dpadState[i].X > InputBinding.AXIS_POSITIVE)
					{
						m_dpadState[i].X = InputBinding.AXIS_POSITIVE;
					}
				}
				else if(leftPressed)
				{
					if(m_dpadState[i].X > InputBinding.AXIS_NEUTRAL && m_dpadSnap)
					{
						m_dpadState[i].X = InputBinding.AXIS_NEUTRAL;
					}

					m_dpadState[i].X -= m_dpadSensitivity * deltaTime;
					if(m_dpadState[i].X < InputBinding.AXIS_NEGATIVE)
					{
						m_dpadState[i].X = InputBinding.AXIS_NEGATIVE;
					}
				}
				else
				{
					if(m_dpadState[i].X < InputBinding.AXIS_NEUTRAL)
					{
						m_dpadState[i].X += m_dpadGravity * deltaTime;
						if(m_dpadState[i].X > InputBinding.AXIS_NEUTRAL)
						{
							m_dpadState[i].X = InputBinding.AXIS_NEUTRAL;
						}
					}
					else if(m_dpadState[i].X > InputBinding.AXIS_NEUTRAL)
					{
						m_dpadState[i].X -= m_dpadGravity * deltaTime;
						if(m_dpadState[i].X < InputBinding.AXIS_NEUTRAL)
						{
							m_dpadState[i].X = InputBinding.AXIS_NEUTRAL;
						}
					}
				}
			}
		}

		private void UpdateDPADVertical(float deltaTime)
		{
			for(int i = 0; i < m_dpadState.Length; i++)
			{
				bool upPressed = m_currentState[i].HasValue ? m_currentState[i].Value.Buttons.HasFlag(GamepadButtons.DPadUp) : false;
				bool downPressed = m_currentState[i].HasValue ? m_currentState[i].Value.Buttons.HasFlag(GamepadButtons.DPadDown) : false;

				if(upPressed)
				{
					if(m_dpadState[i].Y < InputBinding.AXIS_NEUTRAL && m_dpadSnap)
					{
						m_dpadState[i].Y = InputBinding.AXIS_NEUTRAL;
					}

					m_dpadState[i].Y += m_dpadSensitivity * deltaTime;
					if(m_dpadState[i].Y > InputBinding.AXIS_POSITIVE)
					{
						m_dpadState[i].Y = InputBinding.AXIS_POSITIVE;
					}
				}
				else if(downPressed)
				{
					if(m_dpadState[i].Y > InputBinding.AXIS_NEUTRAL && m_dpadSnap)
					{
						m_dpadState[i].Y = InputBinding.AXIS_NEUTRAL;
					}

					m_dpadState[i].Y -= m_dpadSensitivity * deltaTime;
					if(m_dpadState[i].Y < InputBinding.AXIS_NEGATIVE)
					{
						m_dpadState[i].Y = InputBinding.AXIS_NEGATIVE;
					}
				}
				else
				{
					if(m_dpadState[i].Y < InputBinding.AXIS_NEUTRAL)
					{
						m_dpadState[i].Y += m_dpadGravity * deltaTime;
						if(m_dpadState[i].Y > InputBinding.AXIS_NEUTRAL)
						{
							m_dpadState[i].Y = InputBinding.AXIS_NEUTRAL;
						}
					}
					else if(m_dpadState[i].Y > InputBinding.AXIS_NEUTRAL)
					{
						m_dpadState[i].Y -= m_dpadGravity * deltaTime;
						if(m_dpadState[i].Y < InputBinding.AXIS_NEUTRAL)
						{
							m_dpadState[i].Y = InputBinding.AXIS_NEUTRAL;
						}
					}
				}
			}
		}

		private Gamepad GetGamepad(GamepadIndex gamepad)
		{
			return m_gamepads[(int)gamepad];
		}

		private GamepadReading? GetGamepadState(int index)
		{
			if(m_gamepads[index] != null)
				return m_gamepads[index].GetCurrentReading();

			return null;
		}

		private GamepadReading? GetCurrentState(GamepadIndex gamepad)
		{
			return m_currentState[(int)gamepad];
		}

		private GamepadReading? GetPreviousState(GamepadIndex gamepad)
		{
			return m_previousState[(int)gamepad];
		}

		private void UpdateGamepadList()
		{
			for(int i = 0; i < m_gamepads.Length; i++)
			{
				m_gamepads[i] = null;
				m_currentState[i] = null;
				m_previousState[i] = null;
			}

			int index = 0;
			foreach(var gamepad in Gamepad.Gamepads)
			{
				m_gamepads[index++] = gamepad;
				if(index >= MAX_GAMEPADS) break;
			}
		}

		private void HandleGamepadAdded(object sender, Gamepad gamepad)
		{
			UpdateGamepadList();
		}

		private void HandleGamepadRemoved(object sender, Gamepad gamepad)
		{
			UpdateGamepadList();
		}

		public bool IsConnected(GamepadIndex gamepad)
		{
			return m_gamepads[(int)gamepad] != null;
		}

		public float GetAxis(GamepadAxis axis, GamepadIndex gamepad)
		{
			GamepadReading? state = GetCurrentState(gamepad);
			double value = 0.0;

			if(state.HasValue)
			{
				switch(axis)
				{
				case GamepadAxis.LeftThumbstickX:
					value = state.Value.LeftThumbstickX;
					break;
				case GamepadAxis.LeftThumbstickY:
					value = state.Value.LeftThumbstickY;
					break;
				case GamepadAxis.RightThumbstickX:
					value = state.Value.RightThumbstickX;
					break;
				case GamepadAxis.RightThumbstickY:
					value = state.Value.RightThumbstickY;
					break;
				case GamepadAxis.DPadX:
					value = m_dpadState[(int)gamepad].X;
					break;
				case GamepadAxis.DPadY:
					value = m_dpadState[(int)gamepad].Y;
					break;
				case GamepadAxis.LeftTrigger:
					value = state.Value.LeftTrigger;
					break;
				case GamepadAxis.RightTrigger:
					value = state.Value.RightTrigger;
					break;
				}
			}

			return (float)value;
		}

		public float GetAxisRaw(GamepadAxis axis, GamepadIndex gamepad)
		{
			float value = GetAxis(axis, gamepad);
			return Mathf.Approximately(value, 0) ? 0.0f : Mathf.Sign(value);
		}

		public bool GetButton(GamepadButton button, GamepadIndex gamepad)
		{
			GamepadReading? state = GetCurrentState(gamepad);
			return GetButton(button, state);
		}

		public bool GetButtonDown(GamepadButton button, GamepadIndex gamepad)
		{
			GamepadReading? state = GetCurrentState(gamepad);
			GamepadReading? oldState = GetPreviousState(gamepad);

			return GetButton(button, state) && !GetButton(button, oldState);
		}

		public bool GetButtonUp(GamepadButton button, GamepadIndex gamepad)
		{
			GamepadReading? state = GetCurrentState(gamepad);
			GamepadReading? oldState = GetPreviousState(gamepad);

			return !GetButton(button, state) && GetButton(button, oldState);
		}

		public void SetVibration(LumiVibration vibration, GamepadIndex gamepad)
		{
			Gamepad gamepadDevice = GetGamepad(gamepad);
			if(gamepadDevice != null)
			{
				gamepadDevice.Vibration = new WinVibration
				{
					LeftMotor = vibration.LeftMotor,
					RightMotor = vibration.RightMotor,
					LeftTrigger = vibration.LeftTrigger,
					RightTrigger = vibration.RightTrigger
				};
			}
		}

		public LumiVibration GetVibration(GamepadIndex gamepad)
		{
			Gamepad gamepadDevice = GetGamepad(gamepad);
			if(gamepadDevice != null)
			{
				return new LumiVibration
				{
					LeftMotor = (float)gamepadDevice.Vibration.LeftMotor,
					RightMotor = (float)gamepadDevice.Vibration.RightMotor,
					LeftTrigger = (float)gamepadDevice.Vibration.LeftTrigger,
					RightTrigger = (float)gamepadDevice.Vibration.RightTrigger
				};
			}

			return new LumiVibration();
		}

		private bool GetButton(GamepadButton button, GamepadReading? state)
		{
			bool value = false;
			if(state.HasValue)
			{
				switch(button)
				{
				case GamepadButton.ActionBottom:
					value = state.Value.Buttons.HasFlag(GamepadButtons.A);
					break;
				case GamepadButton.ActionRight:
					value = state.Value.Buttons.HasFlag(GamepadButtons.B);
					break;
				case GamepadButton.ActionLeft:
					value = state.Value.Buttons.HasFlag(GamepadButtons.X);
					break;
				case GamepadButton.ActionTop:
					value = state.Value.Buttons.HasFlag(GamepadButtons.Y);
					break;
				case GamepadButton.LeftBumper:
					value = state.Value.Buttons.HasFlag(GamepadButtons.LeftShoulder);
					break;
				case GamepadButton.RightBumper:
					value = state.Value.Buttons.HasFlag(GamepadButtons.RightShoulder);
					break;
				case GamepadButton.LeftStick:
					value = state.Value.Buttons.HasFlag(GamepadButtons.LeftThumbstick);
					break;
				case GamepadButton.RightStick:
					value = state.Value.Buttons.HasFlag(GamepadButtons.RightThumbstick);
					break;
				case GamepadButton.Back:
					value = state.Value.Buttons.HasFlag(GamepadButtons.View);
					break;
				case GamepadButton.Start:
					value = state.Value.Buttons.HasFlag(GamepadButtons.Menu);
					break;
				case GamepadButton.DPadUp:
					value = state.Value.Buttons.HasFlag(GamepadButtons.DPadUp);
					break;
				case GamepadButton.DPadDown:
					value = state.Value.Buttons.HasFlag(GamepadButtons.DPadDown);
					break;
				case GamepadButton.DPadLeft:
					value = state.Value.Buttons.HasFlag(GamepadButtons.DPadLeft);
					break;
				case GamepadButton.DPadRight:
					value = state.Value.Buttons.HasFlag(GamepadButtons.DPadRight);
					break;
				}
			}

			return value;
		}
#else
        private void Awake()
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'ENABLE_X_INPUT' scripting symbol is defined.", gameObject);
		}

		public bool IsConnected(GamepadIndex gamepad)
		{
			return false;
		}

		public float GetAxis(GamepadAxis axis, GamepadIndex gamepad)
		{
			return 0;
		}

		public float GetAxisRaw(GamepadAxis axis, GamepadIndex gamepad)
		{
			return 0;
		}

		public bool GetButton(GamepadButton button, GamepadIndex gamepad)
		{
			return false;
		}

		public bool GetButtonDown(GamepadButton button, GamepadIndex gamepad)
		{
			return false;
		}

		public bool GetButtonUp(GamepadButton button, GamepadIndex gamepad)
		{
			return false;
		}

		public void SetVibration(GamepadVibration vibration, GamepadIndex gamepad)
		{
		}

		public GamepadVibration GetVibration(GamepadIndex gamepad)
		{
			return new GamepadVibration();
		}
#endif
	}
}