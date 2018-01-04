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
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Luminosity.IO.Events
{
	public class InputEventManager : MonoBehaviour
	{
		[SerializeField]
        private List<InputEvent> m_inputEvents;

        private Dictionary<string, InputEvent> m_eventLookup;

		public ReadOnlyCollection<InputEvent> Events
		{
			get { return m_inputEvents.AsReadOnly(); }
		}

		public bool ReceiveInput { get; set; }
		
		private void Awake()
		{
			ReceiveInput = true;
			Initialize();
		}

		private void Initialize()
		{
			m_eventLookup = new Dictionary<string, InputEvent>();
			foreach(var evt in m_inputEvents)
			{
				m_eventLookup[evt.Name] = evt;
			}
		}
		
		private void Update()
		{
			if(ReceiveInput)
			{
				for(int i = 0; i < m_inputEvents.Count; i++)
					m_inputEvents[i].Update();
			}
		}
		
		public InputEvent CreateAxisEvent(string name, string axisName, PlayerID playerID = PlayerID.One)
		{
			if(!m_eventLookup.ContainsKey(name))
			{
				InputEvent evt = new InputEvent(name)
				{
					ActionName = axisName,
					EventType = InputEventType.Axis,
					PlayerID = playerID
				};

				m_inputEvents.Add(evt);
				m_eventLookup.Add(name, evt);
				return evt;
			}
			else
			{
				Debug.LogError(string.Format("An input event named {0} already exists", name), this);
				return null;
			}
		}
		
		public InputEvent CreateButtonEvent(string name, string buttonName, InputState inputState, PlayerID playerID = PlayerID.One)
		{
			if(!m_eventLookup.ContainsKey(name))
			{
				InputEvent evt = new InputEvent(name)
				{
					ActionName = buttonName,
					EventType = InputEventType.Button,
					InputState = inputState,
					PlayerID = playerID
				};

				m_inputEvents.Add(evt);
				m_eventLookup.Add(name, evt);
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
			if(!m_eventLookup.ContainsKey(name))
			{
				InputEvent evt = new InputEvent(name)
				{
					KeyCode = key,
					EventType = InputEventType.Key,
					InputState = inputState
				};

				m_inputEvents.Add(evt);
				m_eventLookup.Add(name, evt);
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
			if(!m_eventLookup.ContainsKey(name))
			{
				InputEvent evt = new InputEvent(name);
				m_inputEvents.Add(evt);
				m_eventLookup.Add(name, evt);
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
			if(m_eventLookup.TryGetValue(name, out evt))
			{
				m_eventLookup.Remove(name);
				m_inputEvents.Remove(evt);
			}
		}
		
		/// <summary>
		/// Searches for an event based on the specified name. If an event can't be found the return value will be null.
		/// </summary>
		public InputEvent GetEvent(string name)
		{
			InputEvent evt = null;
			if(m_eventLookup.TryGetValue(name, out evt))
				return evt;
			
			return null;
		}
		
		/// <summary>
		/// Gets the event at the specified index. If the index is out of range the return value will be null.
		/// </summary>
		public InputEvent GetEvent(int index)
		{
			if(index >= 0 && index < m_inputEvents.Count)
				return m_inputEvents[index];
			else
				return null;
		}

		public void OnInitializeAfterScriptReload()
		{
			Initialize();
		}

#if UNITY_EDITOR
		[DidReloadScripts(1)]
		private static void OnScriptReload()
		{
			if(EditorApplication.isPlaying)
			{
				InputEventManager[] inputEventManagers = FindObjectsOfType<InputEventManager>();
				for(int i = 0; i < inputEventManagers.Length; i++)
				{
					inputEventManagers[i].OnInitializeAfterScriptReload();
				}
			}
		}
#endif
	}
}