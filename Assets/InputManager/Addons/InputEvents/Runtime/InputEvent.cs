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
using UnityEngine.Events;
using System;
using System.Collections;

namespace TeamUtility.IO
{
	[Serializable]
	public class InputEvent
	{
		[Serializable]
		public class AxisEvent : UnityEvent<float> { }
		
		[Serializable]
		public class ActionEvent : UnityEvent { }

		[SerializeField] private string _name;
		[SerializeField] private string _axisName;
		[SerializeField] private string _buttonName;
		[SerializeField] private KeyCode _keyCode;
		[SerializeField] private InputEventType _eventType;
		[SerializeField] private InputState _inputState;
		[SerializeField] private ActionEvent _onAction;
		[SerializeField] private AxisEvent _onAxis;

		public void Update()
		{
			switch(_eventType)
			{
			case InputEventType.Axis:
				UpdateAxis();
				break;
			case InputEventType.Button:
				UpdateButton();
				break;
			case InputEventType.Key:
				UpdateKey();
				break;
			}
		}

		private void UpdateAxis()
		{
			if(!string.IsNullOrEmpty(_axisName))
				_onAxis.Invoke(InputManager.GetAxis(_axisName));
		}

		private void UpdateButton()
		{
			if(!string.IsNullOrEmpty(_buttonName))
			{
				switch(_inputState) 
				{
				case InputState.Pressed:
					if(InputManager.GetButtonDown(_buttonName))
						_onAction.Invoke();
					break;
				case InputState.Released:
					if(InputManager.GetButtonUp(_buttonName))
						_onAction.Invoke();
					break;
				case InputState.Held:
					if(InputManager.GetButton(_buttonName))
						_onAction.Invoke();
					break;
				}
			}
		}

		private void UpdateKey()
		{
			switch(_inputState) 
			{
			case InputState.Pressed:
				if(InputManager.GetKeyDown(_keyCode))
					_onAction.Invoke();
				break;
			case InputState.Released:
				if(InputManager.GetKeyUp(_keyCode))
					_onAction.Invoke();
				break;
			case InputState.Held:
				if(InputManager.GetKey(_keyCode))
					_onAction.Invoke();
				break;
			}
		}
	}
}