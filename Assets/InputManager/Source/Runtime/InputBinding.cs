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
using System;

namespace Luminosity.IO
{
	[Serializable]
	public class InputBinding
	{
		public const float AXIS_NEUTRAL = 0.0f;
		public const float AXIS_POSITIVE = 1.0f;
		public const float AXIS_NEGATIVE = -1.0f;
		public const int MAX_MOUSE_AXES = 3;
		public const int MAX_JOYSTICK_AXES = 28;
		public const int MAX_JOYSTICKS = 11;

		[SerializeField]
		private KeyCode m_positive;
		[SerializeField]
		private KeyCode m_negative;
		[SerializeField]
		private float m_deadZone;
		[SerializeField]
		private float m_gravity;
		[SerializeField]
		private float m_sensitivity;
		[SerializeField]
		private bool m_snap;
		[SerializeField]
		private bool m_invert;
		[SerializeField]
		private InputType m_type;
		[SerializeField]
		private int m_axis;
		[SerializeField]
		private int m_joystick;
		[SerializeField]
		private GamepadButton m_gamepadButton;
		[SerializeField]
		private GamepadAxis m_gamepadAxis;
		[SerializeField]
		private GamepadIndex m_gamepadIndex;

		private string m_rawAxisName;
		private float m_value;
		private bool m_isAxisDirty;
		private bool m_isTypeDirty;
		private ButtonState m_remoteButtonState;
		private ButtonState m_analogButtonState;

		public KeyCode Positive
		{
			get { return m_positive; }
			set { m_positive = value; }
		}

		public KeyCode Negative
		{
			get { return m_negative; }
			set { m_negative = value; }
		}

		public float DeadZone
		{
			get { return m_deadZone; }
			set { m_deadZone = Mathf.Max(value, 0.0f); }
		}

		public float Gravity
		{
			get { return m_gravity; }
			set { m_gravity = Mathf.Max(value, 0.0f); }
		}

		public float Sensitivity
		{
			get { return m_sensitivity; }
			set { m_sensitivity = Math.Max(value, 0.0f); }
		}

		public bool Snap
		{
			get { return m_snap; }
			set { m_snap = value; }
		}

		public bool Invert
		{
			get { return m_invert; }
			set { m_invert = value; }
		}

		public InputType Type
		{
			get { return m_type; }
			set
			{
				m_type = value;
				m_isTypeDirty = true;
			}
		}

		public int Axis
		{
			get { return m_axis; }
			set
			{
				m_axis = Mathf.Clamp(value, 0, MAX_JOYSTICK_AXES - 1);
				m_isAxisDirty = true;
			}
		}

		public int Joystick
		{
			get { return m_joystick; }
			set
			{
				m_joystick = Mathf.Clamp(value, 0, MAX_JOYSTICKS - 1);
				m_isAxisDirty = true;
			}
		}

		public GamepadButton GamepadButton
		{
			get { return m_gamepadButton; }
			set { m_gamepadButton = value; }
		}

		public GamepadAxis GamepadAxis
		{
			get { return m_gamepadAxis; }
			set { m_gamepadAxis = value; }
		}

		public GamepadIndex GamepadIndex
		{
			get { return m_gamepadIndex; }
			set { m_gamepadIndex = value; }
		}

		public bool AnyInput
		{
			get
			{
				switch(m_type)
				{
				case InputType.Button:
					return Input.GetKey(m_positive);
				case InputType.AnalogButton:
				case InputType.GamepadAnalogButton:
					return m_analogButtonState == ButtonState.Pressed || m_analogButtonState == ButtonState.JustPressed;
				case InputType.RemoteButton:
					return m_remoteButtonState == ButtonState.Pressed || m_remoteButtonState == ButtonState.JustPressed;
				case InputType.GamepadButton:
					return GamepadState.GetButton(m_gamepadButton, m_gamepadIndex);
				case InputType.GamepadAxis:
					return Mathf.Abs(GamepadState.GetAxisRaw(m_gamepadAxis, m_gamepadIndex)) >= 1.0f;
				case InputType.DigitalAxis:
				case InputType.RemoteAxis:
					return Mathf.Abs(m_value) >= 1.0f;
				default:
					return Mathf.Abs(Input.GetAxisRaw(m_rawAxisName)) >= 1.0f;
				}
			}
		}

		public bool AnyKey
		{
			get
			{
				return Input.GetKey(m_positive) || Input.GetKey(m_negative);
			}
		}

		public bool AnyKeyDown
		{
			get
			{
				return Input.GetKeyDown(m_positive) || Input.GetKeyDown(m_negative);
			}
		}

		public bool AnyKeyUp
		{
			get
			{
				return Input.GetKeyUp(m_positive) || Input.GetKeyUp(m_negative);
			}
		}

		public InputBinding()
		{
			m_positive = KeyCode.None;
			m_negative = KeyCode.None;
			m_type = InputType.Button;
			m_gravity = 1.0f;
			m_sensitivity = 1.0f;
		}

		public void Initialize()
		{
			UpdateRawAxisName();
			m_value = AXIS_NEUTRAL;
			m_isAxisDirty = false;
			m_isTypeDirty = false;
			m_remoteButtonState = ButtonState.Released;
			m_analogButtonState = ButtonState.Released;
		}

		public void Update(float deltaTime)
		{
			if(m_isTypeDirty)
			{
				Reset();
				m_isTypeDirty = false;
			}

			if(m_isAxisDirty)
			{
				UpdateRawAxisName();
				m_analogButtonState = ButtonState.Released;
				m_isAxisDirty = false;
			}

			bool bothKeysDown = Input.GetKey(m_positive) && Input.GetKey(m_negative);
			if(m_type == InputType.DigitalAxis && !bothKeysDown)
			{
				UpdateDigitalAxisValue(deltaTime);
			}
			if(m_type == InputType.AnalogButton || m_type == InputType.GamepadAnalogButton)
			{
				UpdateAnalogButtonValue();
			}
		}

		private void UpdateDigitalAxisValue(float deltaTime)
		{
			if(Input.GetKey(m_positive))
			{
				if(m_value < AXIS_NEUTRAL && m_snap)
				{
					m_value = AXIS_NEUTRAL;
				}

				m_value += m_sensitivity * deltaTime;
				if(m_value > AXIS_POSITIVE)
				{
					m_value = AXIS_POSITIVE;
				}
			}
			else if(Input.GetKey(m_negative))
			{
				if(m_value > AXIS_NEUTRAL && m_snap)
				{
					m_value = AXIS_NEUTRAL;
				}

				m_value -= m_sensitivity * deltaTime;
				if(m_value < AXIS_NEGATIVE)
				{
					m_value = AXIS_NEGATIVE;
				}
			}
			else
			{
				if(m_value < AXIS_NEUTRAL)
				{
					m_value += m_gravity * deltaTime;
					if(m_value > AXIS_NEUTRAL)
					{
						m_value = AXIS_NEUTRAL;
					}
				}
				else if(m_value > AXIS_NEUTRAL)
				{
					m_value -= m_gravity * deltaTime;
					if(m_value < AXIS_NEUTRAL)
					{
						m_value = AXIS_NEUTRAL;
					}
				}
			}
		}

		private void UpdateAnalogButtonValue()
		{
			float axis = m_type == InputType.AnalogButton ?
									Input.GetAxis(m_rawAxisName) :
									GamepadState.GetAxis(m_gamepadAxis, m_gamepadIndex);

			axis = m_invert ? -axis : axis;

			if(axis > m_deadZone)
			{
				if(m_analogButtonState == ButtonState.Released || m_analogButtonState == ButtonState.JustReleased)
					m_analogButtonState = ButtonState.JustPressed;
				else if(m_analogButtonState == ButtonState.JustPressed)
					m_analogButtonState = ButtonState.Pressed;
			}
			else
			{
				if(m_analogButtonState == ButtonState.Pressed || m_analogButtonState == ButtonState.JustPressed)
					m_analogButtonState = ButtonState.JustReleased;
				else if(m_analogButtonState == ButtonState.JustReleased)
					m_analogButtonState = ButtonState.Released;
			}
		}

		public float? GetAxis()
		{
			float? axis = null;

			if(m_type == InputType.DigitalAxis || m_type == InputType.RemoteAxis)
			{
				axis = m_invert ? -m_value : m_value;
			}
			else if(m_type == InputType.MouseAxis)
			{
				if(m_rawAxisName != null)
				{
					axis = Input.GetAxis(m_rawAxisName) * m_sensitivity;
					axis = m_invert ? -axis : axis;
				}
			}
			else if(m_type == InputType.AnalogAxis)
			{
				if(m_rawAxisName != null)
				{
					axis = Input.GetAxis(m_rawAxisName);
					if(Mathf.Abs(axis.Value) < m_deadZone)
					{
						axis = AXIS_NEUTRAL;
					}

					axis = Mathf.Clamp(axis.Value * m_sensitivity, -1, 1);
					axis = m_invert ? -axis : axis;
				}
			}
			else if(m_type == InputType.GamepadAxis)
			{
				axis = GamepadState.GetAxis(m_gamepadAxis, m_gamepadIndex);
				if(Mathf.Abs(axis.Value) < m_deadZone)
				{
					axis = AXIS_NEUTRAL;
				}

				axis = Mathf.Clamp(axis.Value * m_sensitivity, -1, 1);
				axis = m_invert ? -axis : axis;
			}

			if(axis.HasValue && Mathf.Abs(axis.Value) <= 0.0f)
				axis = null;
			
			return axis;
		}

		///<summary>
		///	Returns raw input with no sensitivity or smoothing applyed.
		/// </summary>
		public float? GetAxisRaw()
		{
			float? axis = null;

			if(m_type == InputType.DigitalAxis)
			{
				if(Input.GetKey(m_positive))
				{
					axis = m_invert ? -AXIS_POSITIVE : AXIS_POSITIVE;
				}
				else if(Input.GetKey(m_negative))
				{
					axis = m_invert ? -AXIS_NEGATIVE : AXIS_NEGATIVE;
				}
			}
			else if(m_type == InputType.MouseAxis || m_type == InputType.AnalogAxis)
			{
				if(m_rawAxisName != null)
				{
					axis = Input.GetAxisRaw(m_rawAxisName);
					axis = m_invert ? -axis : axis;
				}
			}
			else if(m_type == InputType.GamepadAxis)
			{
				axis = GamepadState.GetAxisRaw(m_gamepadAxis, m_gamepadIndex);
				axis = m_invert ? -axis : axis;
			}

			if(axis.HasValue && Mathf.Abs(axis.Value) <= 0.0f)
				axis = null;

			return axis;
		}

		public bool? GetButton()
		{
			bool? value = null;

			if(m_type == InputType.Button)
			{
				value = Input.GetKey(m_positive);
			}
			else if(m_type == InputType.GamepadButton)
			{
				value = GamepadState.GetButton(m_gamepadButton, m_gamepadIndex);
			}
			else if(m_type == InputType.RemoteButton)
			{
				value = m_remoteButtonState == ButtonState.Pressed || m_remoteButtonState == ButtonState.JustPressed;
			}
			else if(m_type == InputType.AnalogButton || m_type == InputType.GamepadAnalogButton)
			{
				value = m_analogButtonState == ButtonState.Pressed || m_analogButtonState == ButtonState.JustPressed;
			}

			if(value.HasValue && !value.Value)
				value = null;

			return value;
		}

		public bool? GetButtonDown()
		{
			bool? value = null;

			if(m_type == InputType.Button)
			{
				value = Input.GetKeyDown(m_positive);
			}
			else if(m_type == InputType.GamepadButton)
			{
				value = GamepadState.GetButtonDown(m_gamepadButton, m_gamepadIndex);
			}
			else if(m_type == InputType.RemoteButton)
			{
				value = m_remoteButtonState == ButtonState.JustPressed;
			}
			else if(m_type == InputType.AnalogButton || m_type == InputType.GamepadAnalogButton)
			{
				value = m_analogButtonState == ButtonState.JustPressed;
			}

			if(value.HasValue && !value.Value)
				value = null;

			return value;
		}

		public bool? GetButtonUp()
		{
			bool? value = null;

			if(m_type == InputType.Button)
			{
				value = Input.GetKeyUp(m_positive);
			}
			else if(m_type == InputType.GamepadButton)
			{
				value = GamepadState.GetButtonUp(m_gamepadButton, m_gamepadIndex);
			}
			else if(m_type == InputType.RemoteButton)
			{
				value = m_remoteButtonState == ButtonState.JustReleased;
			}
			else if(m_type == InputType.AnalogButton || m_type == InputType.GamepadAnalogButton)
			{
				value = m_analogButtonState == ButtonState.JustReleased;
			}

			if(value.HasValue && !value.Value)
				value = null;

			return value;
		}

		/// <summary>
		/// If the input type is set to "RemoteAxis" the axis value will be changed, else nothing will happen.
		/// </summary>
		public void SetRemoteAxisValue(float value)
		{
			if(m_type == InputType.RemoteAxis)
			{
				m_value = value;
			}
		}

		/// <summary>
		/// If the input type is set to "RemoteButton" the axis state will be changed, else nothing will happen.
		/// </summary>
		public void SetRemoteButtonState(ButtonState state)
		{
			if(m_type == InputType.RemoteButton)
			{
				m_remoteButtonState = state;
			}
		}

		public void Copy(InputBinding source)
		{
			m_positive = source.m_positive;
			m_negative = source.m_negative;
			m_deadZone = source.m_deadZone;
			m_gravity = source.m_gravity;
			m_sensitivity = source.m_sensitivity;
			m_snap = source.m_snap;
			m_invert = source.m_invert;
			m_type = source.m_type;
			m_axis = source.m_axis;
			m_joystick = source.m_joystick;
			m_gamepadAxis = source.m_gamepadAxis;
			m_gamepadButton = source.m_gamepadButton;
			m_gamepadIndex = source.m_gamepadIndex;
		}

		public void Reset()
		{
			m_value = AXIS_NEUTRAL;
			m_remoteButtonState = ButtonState.Released;
			m_analogButtonState = ButtonState.Released;
		}

		private void UpdateRawAxisName()
		{
			if(m_type == InputType.MouseAxis)
			{
				if(m_axis < 0 || m_axis >= MAX_MOUSE_AXES)
				{
					string message = string.Format("Desired mouse axis is out of range. Mouse axis will be clamped to {0}.",
												   Mathf.Clamp(m_axis, 0, MAX_MOUSE_AXES - 1));
					Debug.LogWarning(message);
				}

				m_rawAxisName = string.Concat("mouse_axis_", Mathf.Clamp(m_axis, 0, MAX_MOUSE_AXES - 1));
			}
			else if(m_type == InputType.AnalogAxis || m_type == InputType.AnalogButton)
			{
				if(m_joystick < 0 || m_joystick >= MAX_JOYSTICKS)
				{
					string message = string.Format("Desired joystick is out of range. Joystick has been clamped to {0}.",
												   Mathf.Clamp(m_joystick, 0, MAX_JOYSTICKS - 1));
					Debug.LogWarning(message);
				}
				if(m_axis >= MAX_JOYSTICK_AXES)
				{
					string message = string.Format("Desired joystick axis is out of range. Joystick axis will be clamped to {0}.",
												   Mathf.Clamp(m_axis, 0, MAX_JOYSTICK_AXES - 1));
					Debug.LogWarning(message);
				}

				m_rawAxisName = string.Concat("joy_", Mathf.Clamp(m_joystick, 0, MAX_JOYSTICKS - 1),
									 "_axis_", Mathf.Clamp(m_axis, 0, MAX_JOYSTICK_AXES - 1));
			}
			else
			{
				m_rawAxisName = string.Empty;
			}
		}

		public static KeyCode StringToKey(string value)
		{
			return StringToEnum(value, KeyCode.None);
		}

		public static InputType StringToInputType(string value)
		{
			return StringToEnum(value, InputType.Button);
		}

		public static GamepadButton StringToGamepadButton(string value)
		{
			return StringToEnum(value, GamepadButton.ActionBottom);
		}

		public static GamepadAxis StringToGamepadAxis(string value)
		{
			return StringToEnum(value, GamepadAxis.LeftThumbstickX);
		}

		public static GamepadIndex StringToGamepadIndex(string value)
		{
			return StringToEnum(value, GamepadIndex.GamepadOne);
		}

		private static T StringToEnum<T>(string value, T defValue)
		{
			if(string.IsNullOrEmpty(value))
			{
				return defValue;
			}
			try
			{
				return (T)Enum.Parse(typeof(T), value, true);
			}
			catch
			{
				return defValue;
			}
		}

		public static InputBinding Duplicate(InputBinding source)
		{
			InputBinding duplicate = new InputBinding
			{
				m_positive = source.m_positive,
				m_negative = source.m_negative,
				m_deadZone = source.m_deadZone,
				m_gravity = source.m_gravity,
				m_sensitivity = source.m_sensitivity,
				m_snap = source.m_snap,
				m_invert = source.m_invert,
				m_type = source.m_type,
				m_axis = source.m_axis,
				m_joystick = source.m_joystick,
				m_gamepadAxis = source.m_gamepadAxis,
				m_gamepadButton = source.m_gamepadButton,
				m_gamepadIndex = source.m_gamepadIndex,
			};

			return duplicate;
		}
	}
}