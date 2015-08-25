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
using System.Collections.Generic;

namespace TeamUtility.IO
{
	public class InputEventManager : MonoBehaviour
	{
		[SerializeField] private List<InputEvent> _inputEvents;
		private Dictionary<string, InputEvent> _eventLookup;
		
		public int EventCount { get { return _inputEvents.Count; } }
		
		private void Awake()
		{
			_eventLookup = new Dictionary<string, InputEvent>();
			foreach(var evt in _inputEvents)
			{
				if(!_eventLookup.ContainsKey(evt.name))
					_eventLookup.Add(evt.name, evt);
				else
					Debug.LogWarning(string.Format("An input event named \'{0}\' already exists in the lookup table", evt.name), this);
			}
		}
		
		private void Update()
		{
			for(int i = 0; i < _inputEvents.Count; i++)
				_inputEvents[i].Evaluate();
		}
		
		public InputEvent CreateAxisEvent(string name, string axisName)
		{
			if(!_eventLookup.ContainsKey(name))
			{
				InputEvent evt = new InputEvent(name);
				evt.axisName = axisName;
				evt.eventType = InputEventType.Axis;
				
				_inputEvents.Add(evt);
				_eventLookup.Add(name, evt);
				return evt;
			}
			else
			{
				Debug.LogError(string.Format("An input event named {0} already exists", name), this);
				return null;
			}
		}
		
		public InputEvent CreateButtonEvent(string name, string buttonName, InputState inputState)
		{
			if(!_eventLookup.ContainsKey(name))
			{
				InputEvent evt = new InputEvent(name);
				evt.buttonName = buttonName;
				evt.eventType = InputEventType.Button;
				evt.inputState = inputState;
				
				_inputEvents.Add(evt);
				_eventLookup.Add(name, evt);
				return evt;
			}
			else
			{
				Debug.LogError(string.Format("An input event named {0} already exists", name), this);
				return null;
			}
		}
		
		public InputEvent CreateKeyEvent(string name, KeyCode key, InputState inputState)
		{
			if(!_eventLookup.ContainsKey(name))
			{
				InputEvent evt = new InputEvent(name);
				evt.keyCode = key;
				evt.eventType = InputEventType.Key;
				evt.inputState = inputState;
				
				_inputEvents.Add(evt);
				_eventLookup.Add(name, evt);
				return evt;
			}
			else
			{
				Debug.LogError(string.Format("An input event named {0} already exists", name), this);
				return null;
			}
		}
		
		public InputEvent CreateEmptyEvent(string name)
		{
			if(!_eventLookup.ContainsKey(name))
			{
				InputEvent evt = new InputEvent(name);
				_inputEvents.Add(evt);
				_eventLookup.Add(name, evt);
				return evt;
			}
			else
			{
				Debug.LogError(string.Format("An input event named {0} already exists", name), this);
				return null;
			}
		}
		
		public void DeleteEvent(string name)
		{
			InputEvent evt = null;
			if(_eventLookup.TryGetValue(name, out evt))
			{
				_eventLookup.Remove(name);
				_inputEvents.Remove(evt);
			}
		}
		
		/// <summary>
		/// Searches for an event based on the specified name. If an event can't be found the return value will be null.
		/// </summary>
		public InputEvent GetEvent(string name)
		{
			InputEvent evt = null;
			if(_eventLookup.TryGetValue(name, out evt))
				return evt;
			
			return null;
		}
		
		/// <summary>
		/// Gets the event at the specified index. If the index is out of range the return value will be null.
		/// </summary>
		public InputEvent GetEvent(int index)
		{
			if(index >= 0 && index < _inputEvents.Count)
				return _inputEvents[index];
			else
				return null;
		}
	}
}