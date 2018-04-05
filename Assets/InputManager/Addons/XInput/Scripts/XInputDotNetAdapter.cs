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
#if(UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && ENABLE_X_INPUT
using XInputDotNetPure;
using XButtonState = XInputDotNetPure.ButtonState;
using XPlayerIndex = XInputDotNetPure.PlayerIndex;
#endif

namespace Luminosity.IO
{
	public class XInputDotNetAdapter : MonoBehaviour, IGamepadStateAdapter
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

#if(UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && ENABLE_X_INPUT
		private struct DPADState { public float X; public float Y; }

		[System.NonSerialized]
		private GamePadState[] m_currentState;

		[System.NonSerialized]
		private GamePadState[] m_previousState;

		[System.NonSerialized]
		private DPADState[] m_dpadState;

		[System.NonSerialized]
		private GamepadVibration[] m_vibration;

		private void Awake()
		{
			m_previousState = new GamePadState[4];
			m_currentState = new GamePadState[4];
			m_dpadState = new DPADState[4];
			m_vibration = new GamepadVibration[4];
			InputManager.BeforeUpdate += OnUpdate;

			if(GamepadState.Adapter == null)
			{
				GamepadState.Adapter = this;
			}
			else
			{
				Debug.LogWarning("You shouldn't have more than one XInputDotNetAdapter in the scene");
			}
		}

		private void OnDestroy()
		{
			InputManager.BeforeUpdate -= OnUpdate;
			if(GamepadState.Adapter == (IGamepadStateAdapter)this)
			{
				GamepadState.Adapter = null;
			}
		}

		private void OnUpdate()
		{
			for(int i = 0; i < m_currentState.Length; i++)
			{
				m_previousState[i] = m_currentState[i];
				m_currentState[i] = GamePad.GetState((XPlayerIndex)i);
			}

			float deltaTime = m_ignoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;
			UpdateDPADHorizontal(deltaTime);
			UpdateDPADVertical(deltaTime);
		}

		private void UpdateDPADHorizontal(float deltaTime)
		{
			for(int i = 0; i < m_dpadState.Length; i++)
			{
				bool rightPressed = m_currentState[i].IsConnected ? m_currentState[i].DPad.Right == XButtonState.Pressed : false;
				bool leftPressed = m_currentState[i].IsConnected ? m_currentState[i].DPad.Left == XButtonState.Pressed : false;

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
				bool upPressed = m_currentState[i].IsConnected ? m_currentState[i].DPad.Up == XButtonState.Pressed : false;
				bool downPressed = m_currentState[i].IsConnected ? m_currentState[i].DPad.Down == XButtonState.Pressed : false;

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

		public bool IsConnected(GamepadIndex gamepad)
		{
			GamePadState state = GetCurrentState(gamepad);
			return state.IsConnected;
		}

		public float GetAxis(GamepadAxis axis, GamepadIndex gamepad)
		{
			GamePadState state = GetCurrentState(gamepad);
			float value = 0.0f;

			if(state.IsConnected)
			{
				switch(axis)
				{
				case GamepadAxis.LeftThumbstickX:
					value = state.ThumbSticks.Left.X;
					break;
				case GamepadAxis.LeftThumbstickY:
					value = state.ThumbSticks.Left.Y;
					break;
				case GamepadAxis.RightThumbstickX:
					value = state.ThumbSticks.Right.X;
					break;
				case GamepadAxis.RightThumbstickY:
					value = state.ThumbSticks.Right.Y;
					break;
				case GamepadAxis.LeftTrigger:
					value = state.Triggers.Left;
					break;
				case GamepadAxis.RightTrigger:
					value = state.Triggers.Right;
					break;
				case GamepadAxis.DPadX:
					value = GetDPADState(gamepad).X;
					break;
				case GamepadAxis.DPadY:
					value = GetDPADState(gamepad).Y;
					break;
				}
			}

			return value;
		}

		public float GetAxisRaw(GamepadAxis axis, GamepadIndex gamepad)
		{
			float value = GetAxis(axis, gamepad);
			return Mathf.Approximately(value, 0) ? 0.0f : Mathf.Sign(value);
		}

		public bool GetButton(GamepadButton button, GamepadIndex gamepad)
		{
			GamePadState state = GetCurrentState(gamepad);
			return GetButton(button, state);
		}

		public bool GetButtonDown(GamepadButton button, GamepadIndex gamepad)
		{
			GamePadState state = GetCurrentState(gamepad);
			GamePadState oldState = GetPreviousState(gamepad);

			return GetButton(button, state) && !GetButton(button, oldState);
		}

		public bool GetButtonUp(GamepadButton button, GamepadIndex gamepad)
		{
			GamePadState state = GetCurrentState(gamepad);
			GamePadState oldState = GetPreviousState(gamepad);

			return !GetButton(button, state) && GetButton(button, oldState);
		}

		public void SetVibration(GamepadVibration vibration, GamepadIndex gamepad)
		{
			m_vibration[(int)gamepad] = vibration;
			GamePad.SetVibration(ToPlayerIndex(gamepad), vibration.LeftMotor, vibration.RightMotor);
		}

		public GamepadVibration GetVibration(GamepadIndex gamepad)
		{
			return m_vibration[(int)gamepad];
		}

		private GamePadState GetCurrentState(GamepadIndex gamepad)
		{
			return m_currentState[(int)gamepad];
		}

		private GamePadState GetPreviousState(GamepadIndex gamepad)
		{
			return m_previousState[(int)gamepad];
		}

		private DPADState GetDPADState(GamepadIndex gamepad)
		{
			return m_dpadState[(int)gamepad];
		}

		private bool GetButton(GamepadButton button, GamePadState state)
		{
			bool value = false;

			if(state.IsConnected)
			{
				switch(button)
				{
				case GamepadButton.ActionBottom:
					value = state.Buttons.A == XButtonState.Pressed;
					break;
				case GamepadButton.ActionRight:
					value = state.Buttons.B == XButtonState.Pressed;
					break;
				case GamepadButton.ActionLeft:
					value = state.Buttons.X == XButtonState.Pressed;
					break;
				case GamepadButton.ActionTop:
					value = state.Buttons.Y == XButtonState.Pressed;
					break;
				case GamepadButton.LeftBumper:
					value = state.Buttons.LeftShoulder == XButtonState.Pressed;
					break;
				case GamepadButton.RightBumper:
					value = state.Buttons.RightShoulder == XButtonState.Pressed;
					break;
				case GamepadButton.LeftStick:
					value = state.Buttons.LeftStick == XButtonState.Pressed;
					break;
				case GamepadButton.RightStick:
					value = state.Buttons.RightStick == XButtonState.Pressed;
					break;
				case GamepadButton.Back:
					value = state.Buttons.Back == XButtonState.Pressed;
					break;
				case GamepadButton.Start:
					value = state.Buttons.Start == XButtonState.Pressed;
					break;
				case GamepadButton.DPadUp:
					value = state.DPad.Up == XButtonState.Pressed;
					break;
				case GamepadButton.DPadDown:
					value = state.DPad.Down == XButtonState.Pressed;
					break;
				case GamepadButton.DPadLeft:
					value = state.DPad.Left == XButtonState.Pressed;
					break;
				case GamepadButton.DPadRight:
					value = state.DPad.Right == XButtonState.Pressed;
					break;
				}
			}

			return value;
		}

		private XPlayerIndex ToPlayerIndex(GamepadIndex gamepad)
		{
			switch(gamepad)
			{
			case GamepadIndex.GamepadOne:
				return XPlayerIndex.One;
			case GamepadIndex.GamepadTwo:
				return XPlayerIndex.Two;
			case GamepadIndex.GamepadThree:
				return XPlayerIndex.Three;
			case GamepadIndex.GamepadFour:
				return XPlayerIndex.Four;
			default:
				return XPlayerIndex.One;
			}
		}
#else
		private void Awake()
		{
			Debug.LogWarning("XInputDotNet works only on Windows Desktop if the 'ENABLE_X_INPUT' scripting symbol is defined.", gameObject);
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
