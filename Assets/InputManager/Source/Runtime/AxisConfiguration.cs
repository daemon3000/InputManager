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
using System.Collections;

namespace TeamUtility.IO
{
	[Serializable]
	public sealed class AxisConfiguration
	{
		#region [Constants]
		public const float Neutral = 0.0f;
		public const float Positive = 1.0f;
		public const float Negative = -1.0f;
		public const int MaxMouseAxes = 3;
		public const int MaxJoystickAxes = 10;
		public const int MaxJoysticks = 4;
		#endregion
		
		#region [Settings]
		/// <summary>
		/// Do not change the name of an axis at runtime because it will invalidate the lookup tables.
		/// </summary>
		public string name;
		public string description;
		public KeyCode positive;
		public KeyCode negative;
		public KeyCode altPositive;
		public KeyCode altNegative;
		public float deadZone;
		/// <summary
		/// The speed(in units/sec) at which a digital axis falls towards neutral.
		/// </summary>
		public float gravity = 1.0f;
		/// <summary>
		/// The speed(in units/sec) at which a digital axis moves towards the target value.
		/// </summary>
		public float sensitivity = 1.0f;
		/// <summary>
		/// If input switches direction, do we snap to neutral and continue from there?
		///	Only for digital axes.
		/// </summary>
		public bool snap;
		public bool invert;
		public InputType type = InputType.DigitalAxis;
		public int axis;
		public int joystick;
		#endregion

		private enum ButtonState
		{
			Pressed, JustPressed, Released, JustReleased
		}

		private string _rawAxisName;
		private float _value;
		private int _lastAxis;
		private int _lastJoystick;
		private InputType _lastType;
		private float _lastUpdateTime;
		private float _deltaTime;
		private ButtonState _remoteButtonState;
		private ButtonState _analogButtonState;
		
		public bool AnyInput
		{
			get
			{
				if(type == InputType.Button)
					return (Input.GetKey(positive) || Input.GetKey(altPositive));
				else if(type == InputType.RemoteButton)
					return _remoteButtonState == ButtonState.Pressed || _remoteButtonState == ButtonState.JustPressed;
				else if(type == InputType.AnalogButton)
					return _analogButtonState == ButtonState.Pressed || _analogButtonState == ButtonState.JustPressed;
				else if(type == InputType.DigitalAxis || type == InputType.RemoteAxis)
					return Mathf.Abs(_value) >= 1.0f;
				else
					return Mathf.Abs(Input.GetAxisRaw(_rawAxisName)) >= 1.0f;
			}
		}

		public bool AnyKey
		{
			get
			{
				return Input.GetKey(positive) || Input.GetKey(altPositive) || 
						Input.GetKey(negative) || Input.GetKey(altNegative);
			}
		}

		public bool AnyKeyDown
		{
			get
			{
				return Input.GetKeyDown(positive) || Input.GetKeyDown(altPositive) || 
						Input.GetKeyDown(negative) || Input.GetKeyDown(altNegative);
			}
		}

		public bool AnyKeyUp
		{
			get
			{
				return Input.GetKeyUp(positive) || Input.GetKeyUp(altPositive) || 
						Input.GetKeyUp(negative) || Input.GetKeyUp(altNegative);
			}
		}
		
		public AxisConfiguration() :
			this("New Axis") { }
		
		public AxisConfiguration(string name)
		{
			this.name = name;
			description = string.Empty;
			positive = KeyCode.None;
			altPositive = KeyCode.None;
			negative = KeyCode.None;
			altNegative = KeyCode.None;
			type = InputType.Button;
			gravity = 1.0f;
			sensitivity = 1.0f;
		}
		
		public void Initialize()
		{
			UpdateRawAxisName();
			_value = Neutral;
			_lastUpdateTime = Time.realtimeSinceStartup;
			_remoteButtonState = ButtonState.Released;
			_analogButtonState = ButtonState.Released;
		}
		
		public void Update()
		{
			_deltaTime = InputManager.IgnoreTimescale ? (Time.realtimeSinceStartup - _lastUpdateTime) : Time.deltaTime;
			_lastUpdateTime = Time.realtimeSinceStartup;

			if(_lastType != type || _lastAxis != axis || _lastJoystick != joystick)
			{
				if(_lastType != type && (type == InputType.DigitalAxis || type == InputType.RemoteAxis))
					_value = Neutral;
				
				UpdateRawAxisName();
				_lastType = type;
				_lastAxis = axis;
				_lastJoystick = joystick;
			}

			bool positiveAndNegativeDown = (Input.GetKey(positive) || Input.GetKey(altPositive)) &&
											(Input.GetKey(negative) || Input.GetKey(altNegative));
			if(type == InputType.DigitalAxis && !positiveAndNegativeDown)
			{
				UpdateDigitalAxisValue();
			}
			if(type == InputType.AnalogButton)
			{
				UpdateAnalogButtonValue();
			}
		}

		private void UpdateDigitalAxisValue()
		{
			if(Input.GetKey(positive) || Input.GetKey(altPositive))
			{
				if(_value < Neutral && snap) {
					_value = Neutral;
				}

				_value += sensitivity * _deltaTime;
				if(_value > Positive)
				{
					_value = Positive;
				}
			}
			else if(Input.GetKey(negative) || Input.GetKey(altNegative))
			{
				if(_value > Neutral && snap) {
					_value = Neutral;
				}
				
				_value -= sensitivity * _deltaTime;
				if(_value < Negative)
				{
					_value = Negative;
				}
			}
			else
			{
				if(_value < Neutral)
				{
					_value += gravity * _deltaTime;
					if(_value > Neutral)
					{
						_value = Neutral;
					}
				}
				else if( _value > Neutral)
				{
					_value -= gravity * _deltaTime;
					if(_value < Neutral)
					{
						_value = Neutral;
					}
				}
			}
		}
		
		private void UpdateAnalogButtonValue()
		{
			float axis = Input.GetAxisRaw(_rawAxisName) * (invert ? -1 : 1);
			if(axis >= 1.0f)
			{
				if(_analogButtonState == ButtonState.Released || _analogButtonState == ButtonState.JustReleased)
					_analogButtonState = ButtonState.JustPressed;
				else if(_analogButtonState == ButtonState.JustPressed)
					_analogButtonState = ButtonState.Pressed;
			}
			else
			{
				if(_analogButtonState == ButtonState.Pressed || _analogButtonState == ButtonState.JustPressed)
					_analogButtonState = ButtonState.JustReleased;
				else if(_analogButtonState == ButtonState.JustReleased)
					_analogButtonState = ButtonState.Released;
			}
		}
		
		public float GetAxis()
		{
			float axis = Neutral;
			if(type == InputType.DigitalAxis || type == InputType.RemoteAxis)
			{
				axis = _value;
			}
			else if(type == InputType.MouseAxis)
			{
				if(_rawAxisName != null)
				{
					axis = Input.GetAxis(_rawAxisName) * sensitivity;
				}
			}
			else if(type == InputType.AnalogAxis)
			{
				if(_rawAxisName != null)
				{
					axis = Mathf.Clamp(Input.GetAxis(_rawAxisName) * sensitivity, -1, 1);
					if(axis > -deadZone && axis < deadZone)
					{
						axis = Neutral;
					}
				}
			}
			
			return invert ? -axis : axis;
		}

		///<summary>
		///	Returns raw input with no sensitivity or smoothing applyed.
		/// </summary>
		public float GetAxisRaw()
		{
			float axis = Neutral;
			if(type == InputType.DigitalAxis)
			{
				if(Input.GetKey(positive) || Input.GetKey(altPositive))
					axis = Positive;
				else if(Input.GetKey(negative) || Input.GetKey(altNegative))
					axis = Negative;
			}
			else if(type == InputType.MouseAxis || type == InputType.AnalogAxis)
			{
				if(_rawAxisName != null)
					axis = Input.GetAxisRaw(_rawAxisName);
			}
			
			return invert ? -axis : axis;
		}
		
		public bool GetButton()
		{
			if(type == InputType.Button)
				return Input.GetKey(positive) || Input.GetKey(altPositive);
			else if(type == InputType.RemoteButton)
				return _remoteButtonState == ButtonState.Pressed || _remoteButtonState == ButtonState.JustPressed;
			else if(type == InputType.AnalogButton)
				return _analogButtonState == ButtonState.Pressed || _analogButtonState == ButtonState.JustPressed;
			
			return false;
		}
		
		public bool GetButtonDown()
		{
			if(type == InputType.Button)
				return Input.GetKeyDown(positive) || Input.GetKeyDown(altPositive);
			else if(type == InputType.RemoteButton)
				return _remoteButtonState == ButtonState.JustPressed;
			else if(type == InputType.AnalogButton)
				return _analogButtonState == ButtonState.JustPressed;
			
			return false;
		}
		
		public bool GetButtonUp()
		{
			if(type == InputType.Button)
				return Input.GetKeyUp(positive) || Input.GetKeyUp(altPositive);
			else if(type == InputType.RemoteButton)
				return _remoteButtonState == ButtonState.JustReleased;
			else if(type == InputType.AnalogButton)
				return _analogButtonState == ButtonState.JustReleased;
			
			return false;
		}

		public void SetMouseAxis(int axis)
		{
			if(type == InputType.MouseAxis)
			{
				this.axis = axis;
				_lastAxis = axis;
				UpdateRawAxisName();
			}
		}
		
		public void SetAnalogAxis(int joystick, int axis)
		{
			if(type == InputType.AnalogAxis)
			{
				this.joystick = joystick;
				this.axis = axis;
				_lastAxis = axis;
				_lastJoystick = joystick;
				UpdateRawAxisName();
			}
		}

		public void SetAnalogButton(int joystick, int axis)
		{
			if(type == InputType.AnalogButton)
			{
				this.joystick = joystick;
				this.axis = axis;
				_lastAxis = axis;
				_lastJoystick = joystick;
				UpdateRawAxisName();
			}
		}

		/// <summary>
		/// If the axis' input type is set to "RemoteAxis" the axis value will be changed, else nothing will happen.
		/// </summary>
		public void SetRemoteAxisValue(float value)
		{
			if(type == InputType.RemoteAxis)
			{
				_value = value;
			}
			else
			{
				Debug.LogWarning(string.Format("You are trying to manually change the value of axis \'{0}\' which is not of type \'RemoteAxis\'", name));
			}
		}

		/// <summary>
		/// If the axis' input type is set to "RemoteButton" the axis state will be changed, else nothing will happen.
		/// </summary>
		public void SetRemoteButtonValue(bool down, bool justChanged)
		{
			if(type == InputType.RemoteButton)
			{
				if(down)
				{
					if(justChanged)
						_remoteButtonState = ButtonState.JustPressed;
					else
						_remoteButtonState = ButtonState.Pressed;
				}
				else
				{
					if(justChanged)
						_remoteButtonState = ButtonState.JustReleased;
					else
						_remoteButtonState = ButtonState.Released;
				}
			}
			else
			{
				Debug.LogWarning(string.Format("You are trying to manually change the value of button \'{0}\' which is not of type \'RemoteButton\'", name));
			}
		}

		public void Copy(AxisConfiguration source)
		{
			name = source.name;
			description = source.description;
			positive = source.positive;
			altPositive = source.altPositive;
			negative = source.negative;
			altNegative = source.altNegative;
			deadZone = source.deadZone;
			gravity = source.gravity;
			sensitivity = source.sensitivity;
			snap = source.snap;
			invert = source.invert;
			type = source.type;
			axis = source.axis;
			joystick = source.joystick;
		}

		public void Reset()
		{
			_value = Neutral;
			_remoteButtonState = ButtonState.Released;
			_analogButtonState = ButtonState.Released;
		}

		private void UpdateRawAxisName()
		{
			if(type == InputType.MouseAxis)
			{
				if(axis < 0 || axis >= MaxMouseAxes) 
				{
					string message = string.Format("Desired mouse axis is out of range. Mouse axis will be clamped to {0}.", 
					                               Mathf.Clamp(axis, 0, MaxMouseAxes - 1));
					Debug.LogWarning(message);
				}

				_rawAxisName = string.Concat("mouse_axis_", Mathf.Clamp(axis, 0, MaxMouseAxes - 1));
			}
			else if(type == InputType.AnalogAxis || type == InputType.AnalogButton)
			{
				if(joystick < 0 || joystick >= MaxJoysticks)
				{
					string message = string.Format("Desired joystick is out of range. Joystick has been clamped to {0}.",
					                               Mathf.Clamp(joystick, 0, MaxJoysticks - 1));
					Debug.LogWarning(message);
				}
				if(axis >= MaxJoystickAxes)
				{
					string message = string.Format("Desired joystick axis is out of range. Joystick axis will be clamped to {0}.", 
					                               Mathf.Clamp(axis, 0, MaxJoystickAxes - 1));
					Debug.LogWarning(message);
				}

				_rawAxisName = string.Concat("joy_", Mathf.Clamp(joystick, 0, MaxJoysticks - 1), 
				                     "_axis_", Mathf.Clamp(axis, 0, MaxJoystickAxes - 1));
			}
			else
			{
				_rawAxisName = string.Empty;
			}
		}
		
		public static KeyCode StringToKey(string value)
		{
			if(string.IsNullOrEmpty(value)) {
				return KeyCode.None;
			}
			try {
				return (KeyCode)Enum.Parse(typeof(KeyCode), value, true);
			}
			catch {
				return KeyCode.None;
			}
		}
		
		public static InputType StringToInputType(string value)
		{
			if(string.IsNullOrEmpty(value)) {
				return InputType.Button;
			}
			try {
				return (InputType)Enum.Parse(typeof(InputType), value, true);
			}
			catch {
				return InputType.Button;
			}
		}
		
		public static AxisConfiguration Duplicate(AxisConfiguration source)
		{
			AxisConfiguration axisConfig = new AxisConfiguration();
			axisConfig.name = source.name;
			axisConfig.description = source.description;
			axisConfig.positive = source.positive;
			axisConfig.altPositive = source.altPositive;
			axisConfig.negative = source.negative;
			axisConfig.altNegative = source.altNegative;
			axisConfig.deadZone = source.deadZone;
			axisConfig.gravity = source.gravity;
			axisConfig.sensitivity = source.sensitivity;
			axisConfig.snap = source.snap;
			axisConfig.invert = source.invert;
			axisConfig.type = source.type;
			axisConfig.axis = source.axis;
			axisConfig.joystick = source.joystick;
			
			return axisConfig;
		}
	}
}