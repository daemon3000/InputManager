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
using System.Collections.Generic;

namespace Luminosity.IO.Events
{
	public class InputEventGroup : MonoBehaviour
	{
		[SerializeField]
		private bool m_receiveInput = true;
		[SerializeField]
		private List<InputEventGroup> m_inputEventGroups;
		[SerializeField]
		private List<InputEventManager> m_inputEventManagers;

		public bool ReceiveInput
		{
			get { return m_receiveInput; }
			set
			{
				if(value != m_receiveInput)
				{
					m_receiveInput = value;
					UpdateChildren();
				}
			}
		}

		private void Start()
		{
			UpdateChildren();
		}

		private void UpdateChildren()
		{
			if(m_inputEventManagers != null)
			{
				for(int i = 0; i < m_inputEventManagers.Count; i++)
				{
					if(m_inputEventManagers[i] != null)
						m_inputEventManagers[i].ReceiveInput = m_receiveInput;
				}
			}

			if(m_inputEventGroups != null)
			{
				for(int i = 0; i < m_inputEventGroups.Count; i++)
				{
					if(m_inputEventGroups[i] != null)
						m_inputEventGroups[i].ReceiveInput = m_receiveInput;
				}
			}
		}

		private void OnValidate()
		{
			UpdateChildren();
		}

#if UNITY_EDITOR
		public void FindChildren()
		{
			if(!Application.isPlaying)
			{
				if(m_inputEventManagers == null)
				{
					m_inputEventManagers = new List<InputEventManager>();
				}
				if(m_inputEventGroups == null)
				{
					m_inputEventGroups = new List<InputEventGroup>();
				}

				m_inputEventManagers.Clear();
				m_inputEventGroups.Clear();
				FindChildrenRecursive(transform, true);
			}
		}

		private void FindChildrenRecursive(Transform transform, bool isRoot = false)
		{
			if(isRoot)
			{
				var ivm = transform.GetComponent<InputEventManager>();
				if(ivm != null)
				{
					m_inputEventManagers.Add(ivm);
				}

				foreach(Transform child in transform)
				{
					FindChildrenRecursive(child);
				}
			}
			else
			{
				var ivm = transform.GetComponent<InputEventManager>();
				var ivg = transform.GetComponent<InputEventGroup>();

				if(ivm != null)
				{
					m_inputEventManagers.Add(ivm);
				}
				if(ivg != null)
				{
					m_inputEventGroups.Add(ivg);
				}

				if(ivg == null)
				{
					foreach(Transform child in transform)
					{
						FindChildrenRecursive(child);
					}
				}
			}
		}

		[ContextMenu("Find Children")]
		private void FindChildrenContext()
		{
			UnityEditor.Undo.RecordObject(this, "Find InputEventGroup Children");
			FindChildren();
		}
#endif
	}
}
