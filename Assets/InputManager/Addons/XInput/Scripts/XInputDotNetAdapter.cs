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
#if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN) && ENABLE_X_INPUT
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
				Debug.LogWarning("You shouldn't have more than one XInput adapters in the scene");
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

		public float GetAxis(XInputAxis axis, XInputPlayer player)
		{
			GamePadState state = GetCurrentState(player);
			float value = 0.0f;

			if(state.IsConnected)
			{
				switch(axis)
				{
				case XInputAxis.LeftThumbstickX:
					value = state.ThumbSticks.Left.X;
					break;
				case XInputAxis.LeftThumbstickY:
					value = state.ThumbSticks.Left.Y;
					break;
				case XInputAxis.RightThumbstickX:
					value = state.ThumbSticks.Right.X;
					break;
				case XInputAxis.RightThumbstickY:
					value = state.ThumbSticks.Right.Y;
					break;
				case XInputAxis.LeftTrigger:
					value = state.Triggers.Left;
					break;
				case XInputAxis.RightTrigger:
					value = state.Triggers.Right;
					break;
				case XInputAxis.DPadX:
					value = GetDPADState(player).X;
					break;
				case XInputAxis.DPadY:
					value = GetDPADState(player).Y;
					break;
				}
			}

			return value;
		}

		public float GetAxisRaw(XInputAxis axis, XInputPlayer player)
		{
			float value = GetAxis(axis, player);
			return Mathf.Approximately(value, 0) ? 0.0f : Mathf.Sign(value);
		}

		public bool GetButton(XInputButton button, XInputPlayer player)
		{
			GamePadState state = GetCurrentState(player);
			return GetButton(button, state);
		}

		public bool GetButtonDown(XInputButton button, XInputPlayer player)
		{
			GamePadState state = GetCurrentState(player);
			GamePadState oldState = GetPreviousState(player);

			return GetButton(button, state) && !GetButton(button, oldState);
		}

		public bool GetButtonUp(XInputButton button, XInputPlayer player)
		{
			GamePadState state = GetCurrentState(player);
			GamePadState oldState = GetPreviousState(player);

			return !GetButton(button, state) && GetButton(button, oldState);
		}

		public void SetVibration(GamepadVibration vibration, XInputPlayer player)
		{
			m_vibration[(int)player] = vibration;
			GamePad.SetVibration(ToPlayerIndex(player), vibration.LeftMotor, vibration.RightMotor);
		}

		public GamepadVibration GetVibration(XInputPlayer player)
		{
			return m_vibration[(int)player];
		}

		private GamePadState GetCurrentState(XInputPlayer player)
		{
			return m_currentState[(int)player];
		}

		private GamePadState GetPreviousState(XInputPlayer player)
		{
			return m_previousState[(int)player];
		}

		private DPADState GetDPADState(XInputPlayer player)
		{
			return m_dpadState[(int)player];
		}

		private bool GetButton(XInputButton button, GamePadState state)
		{
			bool value = false;

			if(state.IsConnected)
			{
				switch(button)
				{
				case XInputButton.A:
					value = state.Buttons.A == XButtonState.Pressed;
					break;
				case XInputButton.B:
					value = state.Buttons.B == XButtonState.Pressed;
					break;
				case XInputButton.X:
					value = state.Buttons.X == XButtonState.Pressed;
					break;
				case XInputButton.Y:
					value = state.Buttons.Y == XButtonState.Pressed;
					break;
				case XInputButton.LeftBumper:
					value = state.Buttons.LeftShoulder == XButtonState.Pressed;
					break;
				case XInputButton.RightBumper:
					value = state.Buttons.RightShoulder == XButtonState.Pressed;
					break;
				case XInputButton.LeftStick:
					value = state.Buttons.LeftStick == XButtonState.Pressed;
					break;
				case XInputButton.RightStick:
					value = state.Buttons.RightStick == XButtonState.Pressed;
					break;
				case XInputButton.Back:
					value = state.Buttons.Back == XButtonState.Pressed;
					break;
				case XInputButton.Start:
					value = state.Buttons.Start == XButtonState.Pressed;
					break;
				case XInputButton.DPadUp:
					value = state.DPad.Up == XButtonState.Pressed;
					break;
				case XInputButton.DPadDown:
					value = state.DPad.Down == XButtonState.Pressed;
					break;
				case XInputButton.DPadLeft:
					value = state.DPad.Left == XButtonState.Pressed;
					break;
				case XInputButton.DPadRight:
					value = state.DPad.Right == XButtonState.Pressed;
					break;
				}
			}

			return value;
		}

		private XPlayerIndex ToPlayerIndex(XInputPlayer player)
		{
			switch(player)
			{
			case XInputPlayer.PlayerOne:
				return XPlayerIndex.One;
			case XInputPlayer.PlayerTwo:
				return XPlayerIndex.Two;
			case XInputPlayer.PlayerThree:
				return XPlayerIndex.Three;
			case XInputPlayer.PlayerFour:
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

		public float GetAxis(XInputAxis axis, XInputPlayer player)
		{
			Debug.LogWarning("XInputDotNet works only on Windows Desktop if the 'ENABLE_X_INPUT' scripting symbol is defined.", gameObject);
			return 0;
		}

		public float GetAxisRaw(XInputAxis axis, XInputPlayer player)
		{
			Debug.LogWarning("XInputDotNet works only on Windows Desktop if the 'ENABLE_X_INPUT' scripting symbol is defined.", gameObject);
			return 0;
		}

		public bool GetButton(XInputButton button, XInputPlayer player)
		{
			Debug.LogWarning("XInputDotNet works only on Windows Desktop if the 'ENABLE_X_INPUT' scripting symbol is defined.", gameObject);
			return false;
		}

		public bool GetButtonDown(XInputButton button, XInputPlayer player)
		{
			Debug.LogWarning("XInputDotNet works only on Windows Desktop if the 'ENABLE_X_INPUT' scripting symbol is defined.", gameObject);
			return false;
		}

		public bool GetButtonUp(XInputButton button, XInputPlayer player)
		{
			Debug.LogWarning("XInputDotNet works only on Windows Desktop if the 'ENABLE_X_INPUT' scripting symbol is defined.", gameObject);
			return false;
		}

		public void SetVibration(GamepadVibration vibration, XInputPlayer player)
		{
			Debug.LogWarning("XInputDotNet works only on Windows Desktop if the 'ENABLE_X_INPUT' scripting symbol is defined.", gameObject);
		}

		public GamepadVibration GetVibration(XInputPlayer player)
		{
			Debug.LogWarning("XInputDotNet works only on Windows Desktop if the 'ENABLE_X_INPUT' scripting symbol is defined.", gameObject);
			return new GamepadVibration();
		}
#endif
	}
}
