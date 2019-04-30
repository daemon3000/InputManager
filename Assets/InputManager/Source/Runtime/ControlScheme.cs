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
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Luminosity.IO
{
	[Serializable]
	public class ControlScheme
	{
		[SerializeField]
		private string m_name;
		[SerializeField]
		private string m_description;
		[SerializeField]
		private bool m_isExpanded;
		[SerializeField]
		private string m_uniqueID;
		[SerializeField]
		private List<InputAction> m_actions;

		public ReadOnlyCollection<InputAction> Actions
		{
			get { return m_actions.AsReadOnly(); }
		}

		public bool IsExpanded
		{
			get { return m_isExpanded; }
			set { m_isExpanded = value; }
		}

		public string UniqueID
		{
			get { return m_uniqueID; }
			set { m_uniqueID = value; }
		}

		public string Name
		{
			get { return m_name; }
			set
			{
				m_name = value;
				if(Application.isPlaying)
				{
					Debug.LogWarning("You should not change the name of a control scheme at runtime");
				}
			}
		}

		public string Description
		{
			get { return m_description; }
			set { m_description = value; }
		}

		public bool AnyInput
		{
			get
			{
				foreach(var action in m_actions)
				{
					if(action.AnyInput)
						return true;
				}

				return false;
			}
		}
		
		public ControlScheme() :
			this("New Scheme") { }
		
		public ControlScheme(string name)
		{
			m_actions = new List<InputAction>();
			m_name = name;
			m_description = "";
			m_isExpanded = false;
			m_uniqueID = GenerateUniqueID();
		}

		public void Initialize()
		{
			foreach(var action in m_actions)
			{
				action.Initialize();
			}
		}

		public void Update(float deltaTime)
		{
			foreach(var action in m_actions)
			{
				action.Update(deltaTime);
			}
		}

		public void Reset()
		{
			foreach(var action in m_actions)
			{
				action.Reset();
			}
		}

		public InputAction GetAction(int index)
		{
			if(index >= 0 && index < m_actions.Count)
				return m_actions[index];

			return null;
		}

		public InputAction GetAction(string name)
		{
			return m_actions.Find(obj => obj.Name == name);
		}

		public InputAction CreateNewAction(string name)
		{
			InputAction action = new InputAction(name);
			m_actions.Add(action);

			return action;
		}

		public InputAction CreateNewAction(string name, InputAction source)
		{
			InputAction action = InputAction.Duplicate(name, source);
			m_actions.Add(action);

			return action;
		}

		public InputAction InsertNewAction(int index, string name)
		{
			InputAction action = new InputAction(name);
			m_actions.Insert(index, action);

			return action;
		}

		public InputAction InsertNewAction(int index, string name, InputAction source)
		{
			InputAction action = InputAction.Duplicate(name, source);
			m_actions.Insert(index, action);

			return action;
		}

		public void DeleteAction(InputAction action)
		{
			m_actions.Remove(action);
		}

		public void DeleteAction(int index)
		{
			if(index >= 0 && index < m_actions.Count)
				m_actions.RemoveAt(index);
		}

		public void DeleteAction(string name)
		{
			m_actions.RemoveAll(obj => obj.Name == name);
		}

		public void SwapActions(int fromIndex, int toIndex)
		{
			if(fromIndex >= 0 && fromIndex < m_actions.Count && toIndex >= 0 && toIndex < m_actions.Count)
			{
				var temp = m_actions[toIndex];
				m_actions[toIndex] = m_actions[fromIndex];
				m_actions[fromIndex] = temp;
			}
		}

        public void ChangeJoystick(int joystick)
        {
            if(joystick >= 0 && joystick < InputBinding.MAX_JOYSTICKS)
            {
                foreach(var action in m_actions)
                {
                    foreach(var binding in action.Bindings)
                    {
                        binding.Joystick = joystick;
                    }
                }
            }
            else
            {
                Debug.LogFormat("Cannnot replace control scheme joystick. Joystick {0} is out of range.", joystick);
            }
        }

        public void ChangeGamepad(GamepadIndex gamepad)
        {
            foreach(var action in m_actions)
            {
                foreach(var binding in action.Bindings)
                {
                    binding.GamepadIndex = gamepad;
                }
            }
        }

		public Dictionary<string, InputAction> GetActionLookupTable()
		{
			Dictionary<string, InputAction> table = new Dictionary<string, InputAction>();
			foreach(InputAction action in m_actions)
			{
				table[action.Name] = action;
			}

			return table;
		}

		public static ControlScheme Duplicate(ControlScheme source)
		{
			return Duplicate(source.Name, source);
		}

		public static ControlScheme Duplicate(string name, ControlScheme source)
		{
			ControlScheme duplicate = new ControlScheme();
			duplicate.m_name = name;
			duplicate.m_description = source.m_description;
			duplicate.m_uniqueID = GenerateUniqueID(); 
			duplicate.m_actions = new List<InputAction>();
			foreach(var action in source.m_actions)
			{
				duplicate.m_actions.Add(InputAction.Duplicate(action));
			}
			
			return duplicate;
		}

		public static string GenerateUniqueID()
		{
			return Guid.NewGuid().ToString("N");
		}
	}
}