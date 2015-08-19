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

		/// <summary>
		/// Do not change the name of an event at runtime because it will invalidate the lookup table.
		/// </summary>
		public string name;
		public string axisName;
		public string buttonName;
		public KeyCode keyCode;
		public InputEventType eventType;
		public InputState inputState;
		public ActionEvent onAction;
		public AxisEvent onAxis;

		public InputEvent() :
			this("New Event") { }

		public InputEvent(string name)
		{
			this.name = name;
			axisName = "";
			buttonName = "";
			keyCode = KeyCode.None;
			eventType = InputEventType.Key;
			inputState = InputState.Pressed;
			onAxis = new AxisEvent();
			onAction = new ActionEvent();
		}

		public void Evaluate()
		{
			switch(eventType)
			{
			case InputEventType.Axis:
				EvaluateAxis();
				break;
			case InputEventType.Button:
				EvaluateButton();
				break;
			case InputEventType.Key:
				EvaluateKey();
				break;
			}
		}

		private void EvaluateAxis()
		{
			onAxis.Invoke(InputManager.GetAxis(axisName));
		}

		private void EvaluateButton()
		{
			switch(inputState) 
			{
			case InputState.Pressed:
				if(InputManager.GetButtonDown(buttonName))
					onAction.Invoke();
				break;
			case InputState.Released:
				if(InputManager.GetButtonUp(buttonName))
					onAction.Invoke();
				break;
			case InputState.Held:
				if(InputManager.GetButton(buttonName))
					onAction.Invoke();
				break;
			}
		}

		private void EvaluateKey()
		{
			switch(inputState) 
			{
			case InputState.Pressed:
				if(InputManager.GetKeyDown(keyCode))
					onAction.Invoke();
				break;
			case InputState.Released:
				if(InputManager.GetKeyUp(keyCode))
					onAction.Invoke();
				break;
			case InputState.Held:
				if(InputManager.GetKey(keyCode))
					onAction.Invoke();
				break;
			}
		}
	}
}