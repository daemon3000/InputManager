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
    public class GamepadStateService : IInputService
    {
        private const int NUMBER_OF_GAMEPADS = 4;
        private const int NUMBER_OF_BUTTONS = 14;
        private const int NUMBER_OF_AXES = 8;
        private const float MIN_AXIS_DELTA = 0.1f;

        private bool[,] m_buttonStates;
        private bool[,] m_axisStates;
        private float[,] m_lastAxisValues;
        private IGamepadStateAdapter m_adapter;

        public void SetAdapter(IGamepadStateAdapter adapter)
        {
            if(adapter != m_adapter)
            {
                m_adapter = adapter;
                Reset();
            }
        }

        public void Startup()
        {
            m_buttonStates = new bool[NUMBER_OF_GAMEPADS, NUMBER_OF_BUTTONS];
            m_axisStates = new bool[NUMBER_OF_GAMEPADS, NUMBER_OF_AXES];
            m_lastAxisValues = new float[NUMBER_OF_GAMEPADS, NUMBER_OF_AXES];
            m_adapter = null;
            Reset();
        }

        public void Shutdown()
        {
        }

        public void OnBeforeUpdate()
        {
            Profiler.BeginSample("GamepadStateService.OnBeforeUpdate");

            if(m_adapter != null)
            {
                for(int gi = 0; gi < NUMBER_OF_GAMEPADS; gi++)
                {
                    for(int bi = 0; bi < NUMBER_OF_BUTTONS; bi++)
                    {
                        m_buttonStates[gi, bi] = m_adapter.GetButtonDown((GamepadButton)bi, (GamepadIndex)gi);
                    }

                    for(int ai = 0; ai < NUMBER_OF_AXES; ai++)
                    {
                        float value = m_adapter.GetAxis((GamepadAxis)ai, (GamepadIndex)gi);
                        m_axisStates[gi, ai] = Mathf.Abs(value - m_lastAxisValues[gi, ai]) >= MIN_AXIS_DELTA;
                        m_lastAxisValues[gi, ai] = value;
                    }
                }
            }
            
            Profiler.EndSample();
        }

        public void OnAfterUpdate()
        {
        }

        public bool AnyInput(GamepadIndex gamepad)
        {
            if(m_adapter == null)
                return false;

            for(int bi = 0; bi < NUMBER_OF_BUTTONS; bi++)
            {
                if(m_buttonStates[(int)gamepad, bi])
                    return true;
            }

            for(int ai = 0; ai < NUMBER_OF_AXES; ai++)
            {
                if(m_axisStates[(int)gamepad, ai])
                    return true;
            }

            return false;
        }

        private void Reset()
        {
            for(int gi = 0; gi < NUMBER_OF_GAMEPADS; gi++)
            {
                for(int bi = 0; bi < NUMBER_OF_BUTTONS; bi++)
                {
                    m_buttonStates[gi, bi] = false;
                }

                for(int ai = 0; ai < NUMBER_OF_AXES; ai++)
                {
                    m_axisStates[gi, ai] = false;
                    m_lastAxisValues[gi, ai] = 0.0f;
                }
            }
        }
    }
}
