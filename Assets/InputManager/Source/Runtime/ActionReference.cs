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

namespace Luminosity.IO
{
	[CreateAssetMenu(fileName = "New Input Action Reference", menuName = "Luminosity/Input Manager/Input Action Reference")]
	public class ActionReference : ScriptableObject
	{
		[SerializeField]
		private string m_schemeName = null;
		[SerializeField]
		private string m_actionName = null;

		[System.NonSerialized]
		private InputAction m_cachedInputAction = null;

        private InputAction CachedInputAction
        {
            get
            {
                if(m_cachedInputAction == null && InputManager.Exists)
                {
                    m_cachedInputAction = InputManager.GetAction(m_schemeName, m_actionName);
                }

                return m_cachedInputAction;
            }
        }

		public InputAction Get()
		{
            return CachedInputAction;
		}

        public float GetAxis()
        {
            return CachedInputAction != null ? CachedInputAction.GetAxis() : 0.0f;
        }

        public float GetAxisRaw()
        {
            return CachedInputAction != null ? CachedInputAction.GetAxisRaw() : 0.0f;
        }

        public bool GetButton()
        {
            return CachedInputAction != null ? CachedInputAction.GetButton() : false;
        }

        public bool GetButtonDown()
        {
            return CachedInputAction != null ? CachedInputAction.GetButtonDown() : false;
        }

        public bool GetButtonUp()
        {
            return CachedInputAction != null ? CachedInputAction.GetButtonUp() : false;
        }

		private void OnValidate()
		{
			if(InputManager.Exists)
			{
				m_cachedInputAction = InputManager.GetAction(m_schemeName, m_actionName);
			}
		}
	}
}