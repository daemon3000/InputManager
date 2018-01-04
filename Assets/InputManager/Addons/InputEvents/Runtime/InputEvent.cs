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
using System;
using UnityEngine;
using UnityEngine.Events;

namespace Luminosity.IO.Events
{
	[Serializable]
	public class InputEvent
	{
		[Serializable]
		public class AxisEvent : UnityEvent<float> { }

		[Serializable]
		public class ActionEvent : UnityEvent { }

		[SerializeField]
		private string m_name;
		[SerializeField]
		private string m_actionName;
		[SerializeField]
		private KeyCode m_keyCode;
		[SerializeField]
		private InputEventType m_eventType;
		[SerializeField]
		private InputState m_inputState;
		[SerializeField]
		private PlayerID m_playerID;
		[SerializeField]
		private ActionEvent m_onAction;
		[SerializeField]
		private AxisEvent m_onAxis;

		public string Name
		{
			get { return m_name; }
			set
			{
				m_name = value;
				if(Application.isPlaying)
				{
					Debug.LogWarning("You should not change the name of an input action at runtime");
				}
			}
		}

		public string ActionName
		{
			get { return m_actionName; }
			set { m_actionName = value; }
		}

		public KeyCode KeyCode
		{
			get { return m_keyCode; }
			set { m_keyCode = value; }
		}

		public InputEventType EventType
		{
			get { return m_eventType; }
			set { m_eventType = value; }
		}

		public InputState InputState
		{
			get { return m_inputState; }
			set { m_inputState = value; }
		}

		public PlayerID PlayerID
		{
			get { return m_playerID; }
			set { m_playerID = value; }
		}

		public ActionEvent OnAction
		{
			get { return m_onAction; }
			//set { m_onAction = value; }
		}

		public AxisEvent OnAxis
		{
			get { return m_onAxis; }
			//set { m_onAxis = value; }
		}

		public InputEvent() :
			this("New Event") { }

		public InputEvent(string name)
		{
			m_name = name;
			m_actionName = "";
			m_keyCode = KeyCode.None;
			m_eventType = InputEventType.Key;
			m_inputState = InputState.Pressed;
            m_playerID = PlayerID.One;
			m_onAxis = new AxisEvent();
			m_onAction = new ActionEvent();
		}

		public void Update()
		{
			switch(m_eventType)
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
			m_onAxis.Invoke(InputManager.GetAxis(m_actionName, m_playerID));
		}

		private void EvaluateButton()
		{
			switch(m_inputState) 
			{
			case InputState.Pressed:
				if(InputManager.GetButtonDown(m_actionName, m_playerID))
					m_onAction.Invoke();
				break;
			case InputState.Released:
				if(InputManager.GetButtonUp(m_actionName, m_playerID))
					m_onAction.Invoke();
				break;
			case InputState.Held:
				if(InputManager.GetButton(m_actionName, m_playerID))
					m_onAction.Invoke();
				break;
			}
		}

		private void EvaluateKey()
		{
			switch(m_inputState) 
			{
			case InputState.Pressed:
				if(InputManager.GetKeyDown(m_keyCode))
					m_onAction.Invoke();
				break;
			case InputState.Released:
				if(InputManager.GetKeyUp(m_keyCode))
					m_onAction.Invoke();
				break;
			case InputState.Held:
				if(InputManager.GetKey(m_keyCode))
					m_onAction.Invoke();
				break;
			}
		}
	}
}