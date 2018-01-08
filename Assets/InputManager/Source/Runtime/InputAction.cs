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
	public class InputAction
	{
		public const int MAX_BINDINGS = 16;

		[SerializeField]
		private string m_name;
		[SerializeField]
		private string m_description;
		[SerializeField]
		private List<InputBinding> m_bindings;

		public ReadOnlyCollection<InputBinding> Bindings
		{
			get { return m_bindings.AsReadOnly(); }
		}

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

		public string Description
		{
			get { return m_description; }
			set { m_description = value; }
		}

		public bool AnyInput
		{
			get
			{
				foreach(var binding in m_bindings)
				{
					if(binding.AnyInput)
						return true;
				}

				return false;
			}
		}

		public InputAction() :
			this("New Action") { }
		
		public InputAction(string name)
		{
			m_name = name;
			m_description = string.Empty;
			m_bindings = new List<InputBinding>();
		}
		
		public void Initialize()
		{
			foreach(var binding in m_bindings)
			{
				binding.Initialize();
			}
		}
		
		public void Update(float deltaTime)
		{
			foreach(var binding in m_bindings)
			{
				binding.Update(deltaTime);
			}
		}
		
		public float GetAxis()
		{
			float? value = null;
			foreach(var binding in m_bindings)
			{
				value = binding.GetAxis();
				if(value.HasValue)
					break;
			}

			return value ?? InputBinding.AXIS_NEUTRAL;
		}

		///<summary>
		///	Returns raw input with no sensitivity or smoothing applyed.
		/// </summary>
		public float GetAxisRaw()
		{
			float? value = null;
			foreach(var binding in m_bindings)
			{
				value = binding.GetAxisRaw();
				if(value.HasValue)
					break;
			}

			return value ?? InputBinding.AXIS_NEUTRAL;
		}
		
		public bool GetButton()
		{
			bool? value = null;
			foreach(var binding in m_bindings)
			{
				value = binding.GetButton();
				if(value.HasValue)
					break;
			}

			return value ?? false;
		}
		
		public bool GetButtonDown()
		{
			bool? value = null;
			foreach(var binding in m_bindings)
			{
				value = binding.GetButtonDown();
				if(value.HasValue)
					break;
			}

			return value ?? false;
		}
		
		public bool GetButtonUp()
		{
			bool? value = null;
			foreach(var binding in m_bindings)
			{
				value = binding.GetButtonUp();
				if(value.HasValue)
					break;
			}

			return value ?? false;
		}

		public InputBinding GetBinding(int index)
		{
			if(index >= 0 && index < m_bindings.Count)
				return m_bindings[index];

			return null;
		}

		public InputBinding CreateNewBinding()
		{
			if(m_bindings.Count < MAX_BINDINGS)
			{
				InputBinding binding = new InputBinding();
				m_bindings.Add(binding);

				return binding;
			}

			return null;
		}

		public InputBinding CreateNewBinding(InputBinding source)
		{
			if(m_bindings.Count < MAX_BINDINGS)
			{
				InputBinding binding = InputBinding.Duplicate(source);
				m_bindings.Add(binding);

				return binding;
			}

			return null;
		}

		public InputBinding InsertNewBinding(int index)
		{
			if(m_bindings.Count < MAX_BINDINGS)
			{
				InputBinding binding = new InputBinding();
				m_bindings.Insert(index, binding);

				return binding;
			}

			return null;
		}

		public InputBinding InsertNewBinding(int index, InputBinding source)
		{
			if(m_bindings.Count < MAX_BINDINGS)
			{
				InputBinding binding = InputBinding.Duplicate(source);
				m_bindings.Insert(index, binding);

				return binding;
			}

			return null;
		}

		public void DeleteBinding(int index)
		{
			if(index >= 0 && index < m_bindings.Count)
				m_bindings.RemoveAt(index);
		}

		public void SwapBindings(int fromIndex, int toIndex)
		{
			if(fromIndex >= 0 && fromIndex < m_bindings.Count && toIndex >= 0 && toIndex < m_bindings.Count)
			{
				var temp = m_bindings[toIndex];
				m_bindings[toIndex] = m_bindings[fromIndex];
				m_bindings[fromIndex] = temp;
			}
		}

		public void Copy(InputAction source)
		{
			m_name = source.m_name;
			m_description = source.m_description;

			m_bindings.Clear();
			foreach(var binding in source.m_bindings)
			{
				m_bindings.Add(InputBinding.Duplicate(binding));
			}
		}

		public void Reset()
		{
			foreach(var binding in m_bindings)
			{
				binding.Reset();
			}
		}

		public static InputAction Duplicate(InputAction source)
		{
			return Duplicate(source.m_name, source);
		}

		public static InputAction Duplicate(string name, InputAction source)
		{
			InputAction duplicate = new InputAction();
			duplicate.m_name = name;
			duplicate.m_description = source.m_description;
			duplicate.m_bindings = new List<InputBinding>();
			foreach(var binding in source.m_bindings)
			{
				duplicate.m_bindings.Add(InputBinding.Duplicate(binding));
			}

			return duplicate;
		}
	}
}