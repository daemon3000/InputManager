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
using UnityEngine.Profiling;

namespace Luminosity.IO
{
    public class KeyboardStateService : IInputService
    {
        private Vector3 m_lastMousePosition;
        private Vector3 m_currentMousePosition;
        int m_minMousePositionDelta;

        /// <summary>
        /// How many pixels(at least 1 pixel) the mouse pointer has to move to register it as input.
        /// </summary>
        public int MinMousePositionDelta
        {
            get { return m_minMousePositionDelta; }
            set { m_minMousePositionDelta = Mathf.Max(value, 1); }
        }

        /// <summary>
        /// Whether to take into account mouse movement. 
        /// In the editor this option is set to false by default to make testing easier.
        /// </summary>
        public bool RegisterMouseMovement { get; set; }

        public bool AnyInput { get; private set; }

        public void Startup()
        {
            m_lastMousePosition = Vector3.zero;
            m_currentMousePosition = Vector3.zero;
            m_minMousePositionDelta = 20;
#if UNITY_EDITOR
            RegisterMouseMovement = false;
#else
            RegisterMouseMovement = true;
#endif
            AnyInput = false;
        }

        public void Shutdown()
        {
        }

        public void OnBeforeUpdate()
        {
            Profiler.BeginSample("KeyboardStateService.OnBeforeUpdate");
            m_lastMousePosition = m_currentMousePosition;
            m_currentMousePosition = Input.mousePosition;
            AnyInput = false;

            if(Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || 
                Input.GetMouseButtonDown(2) || KeyUtils.IsAnyKeyDown())
            {
                AnyInput = true;
            }
            else if(RegisterMouseMovement)
            {
                Vector3 delta = m_currentMousePosition - m_lastMousePosition;
                AnyInput = delta.sqrMagnitude >= m_minMousePositionDelta * m_minMousePositionDelta;
            }
            Profiler.EndSample();
        }

        public void OnAfterUpdate()
        {
        }
    }
}
