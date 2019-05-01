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
using UnityEngine.Events;
using UnityEngine.Profiling;

namespace Luminosity.IO
{
    public class DeviceChangeService : IInputService
    {
        private UnityAction<InputDevice> m_deviceChangedHandler;

        public event UnityAction<InputDevice> DeviceChanged
        {
            add { m_deviceChangedHandler += value; }
            remove { m_deviceChangedHandler -= value; }
        }

        public InputDevice CurrentDevice { get; private set; }

        public void Startup()
        {
            CurrentDevice = InputDevice.KeyboardAndMouse;
        }

        public void Shutdown()
        {
            m_deviceChangedHandler = null;
        }

        public void OnBeforeUpdate()
        {
        }

        public void OnAfterUpdate()
        {
            Profiler.BeginSample("DeviceChangeService.OnAfterUpdate");
            if(CurrentDevice == InputDevice.KeyboardAndMouse && GamepadState.AnyInput())
            {
                CurrentDevice = InputDevice.Gamepad;
                if(m_deviceChangedHandler != null)
                    m_deviceChangedHandler(CurrentDevice);
            }
            else if(CurrentDevice == InputDevice.Gamepad && KeyboardState.AnyInput())
            {
                CurrentDevice = InputDevice.KeyboardAndMouse;
                if(m_deviceChangedHandler != null)
                    m_deviceChangedHandler(CurrentDevice);
            }
            Profiler.EndSample();
        }
    }
}
