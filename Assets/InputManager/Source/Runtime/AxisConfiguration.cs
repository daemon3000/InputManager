#region [Copyright (c) 2014 Cristian Alexandru Geambasu]
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
		public string name;
		public string description;
		public KeyCode positive;
		public KeyCode negative;
		public KeyCode altPositive;
		public KeyCode altNegative;
		public float deadZone;
		//	The speed(in units/sec) at which a digital axis falls towards neutral.
		public float gravity = 1.0f;
		//	The speed(in units/sec) at which a digital axis moves towards the target value.
		public float sensitivity = 1.0f;
		//	If input switches direction, do we snap to neutral and continue from there?
		//	Only for digital axes.
		public bool snap;
		public bool invert;
		public InputType type = InputType.DigitalAxis;
		public int axis;
		public int joystick;
		
		#endregion
		
		private string _rawAxisName;
		private float _value;
		private int _lastAxis;
		private int _lastJoystick;
		private InputType _lastType;
		private float _lastUpdateTime;
		private float _deltaTime;
		private bool _remoteButtonDown;
		private bool _remoteButtonChangedThisFrame;
		
		public bool AnyInput
		{
			get
			{
				if(type == InputType.Button)
				{
					return (Input.GetKey(positive) || Input.GetKey(altPositive));
				}
				else if(type == InputType.RemoteButton)
				{
					return _remoteButtonDown;
				}
				else if(type == InputType.DigitalAxis || type == InputType.RemoteAxis)
				{
					return Mathf.Abs(_value) >= 1.0f;
				}
				else
				{
					return Mathf.Abs(Input.GetAxisRaw(_rawAxisName)) >= 1.0f;
				}
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
			_remoteButtonDown = false;
			_remoteButtonChangedThisFrame = false;
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
			if(type == InputType.DigitalAxis && !PositiveAndNegativeDown())
			{
				UpdateDigitalAxisValue();
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
		
		private bool PositiveAndNegativeDown()
		{
			return (Input.GetKey(positive) || Input.GetKey(altPositive)) &&
					(Input.GetKey(negative) || Input.GetKey(altNegative));
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
		///	Raw Input - no sensitivity or smoothing applyed.
		///	Button - 0
		///	Mouse Axis - default
		///	Digital Axis - -1, 0, 1
		///	Analog Axis - default
		///	Remote Axis - 0
		/// Remote Button - 0
		/// </summary>
		public float GetAxisRaw()
		{
			float axis = Neutral;
			if(type == InputType.DigitalAxis)
			{
				if(Input.GetKey(positive) || Input.GetKey(altPositive)) {
					axis = Positive;
				}
				else if(Input.GetKey(negative) || Input.GetKey(altNegative)) {
					axis = Negative;
				}
			}
			else if(type == InputType.MouseAxis || type == InputType.AnalogAxis)
			{
				if(_rawAxisName != null)
				{
					axis = Input.GetAxisRaw(_rawAxisName);
				}
			}
			
			return invert ? -axis : axis;
		}
		
		public bool GetButton()
		{
			if(type == InputType.Button)
				return Input.GetKey(positive) || Input.GetKey(altPositive);
			else if(type == InputType.RemoteButton)
				return _remoteButtonDown;
			
			return false;
		}
		
		public bool GetButtonDown()
		{
			if(type == InputType.Button)
				return Input.GetKeyDown(positive) || Input.GetKeyDown(altPositive);
			else if(type == InputType.RemoteButton)
				return _remoteButtonDown && _remoteButtonChangedThisFrame;
			
			return false;
		}
		
		public bool GetButtonUp()
		{
			if(type == InputType.Button)
				return Input.GetKeyUp(positive) || Input.GetKeyUp(altPositive);
			else if(type == InputType.RemoteButton)
				return !_remoteButtonDown && _remoteButtonChangedThisFrame;
			
			return false;
		}
		
		public void Reset()
		{
			_value = Neutral;
			_remoteButtonDown = false;
			_remoteButtonChangedThisFrame = false;
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

		/// <summary>
		/// If the axis' input type is set to "RemoteAxis" the axis value will be changed, else nothing will happen.
		/// </summary>
		public void SetRemoteAxisValue(float value)
		{
			if(type == InputType.RemoteAxis)
				_value = value;
#if UNITY_EDITOR
			else
				Debug.LogWarning(string.Format("You are trying to manually change the value of axis \'{0}\' which is not of type \'RemoteAxis\'", name));
#endif
		}

		/// <summary>
		/// If the axis' input type is set to "RemoteButton" the axis state will be changed, else nothing will happen.
		/// </summary>
		public void SetRemoteButtonValue(bool down, bool changedThisFrame)
		{
			if(type == InputType.RemoteButton)
			{
				_remoteButtonDown = down;
				_remoteButtonChangedThisFrame = changedThisFrame;
			}
#if UNITY_EDITOR
			else
				Debug.LogWarning(string.Format("You are trying to manually change the value of button \'{0}\' which is not of type \'RemoteButton\'", name));
#endif
		}
		
		private void UpdateRawAxisName()
		{
			if(type == InputType.MouseAxis)
			{
#if UNITY_EDITOR
				if(axis >= MaxMouseAxes) 
				{
					string message = string.Format("Desired mouse axis is {0}. Max mouse axis is {1}. Mouse axis has been clamped to {1}.",
													axis, MaxMouseAxes - 1);
					Debug.LogWarning(message);
				}
#endif
				_rawAxisName = string.Concat("mouse_axis_", Mathf.Clamp(axis, 0, MaxMouseAxes));
			}
			else if(type == InputType.AnalogAxis)
			{
#if UNITY_EDITOR
				if(joystick >= MaxJoysticks)
				{
					string message = string.Format("Desired joystick is {0}. Max joystick is {1}. Joystick has been clamped to {1}.",
													joystick, MaxJoysticks - 1);
					Debug.LogWarning(message);
				}
				if(axis >= MaxJoystickAxes)
				{
					string message = string.Format("Desired joystick axis is {0}. Max joystick axis is {1}. Joystick axis has been clamped to {1}.",
													axis, MaxJoystickAxes - 1);
					Debug.LogWarning(message);
				}
#endif
				_rawAxisName = string.Concat("joy_", Mathf.Clamp(joystick, 0, MaxJoysticks), 
											 "_axis_", Mathf.Clamp(axis, 0, MaxJoystickAxes));
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