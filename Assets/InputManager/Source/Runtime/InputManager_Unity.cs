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
using System.Collections;

namespace TeamUtility.IO
{
	public partial class InputManager : MonoBehaviour
	{
		public static Vector3 acceleration { get { return Input.acceleration; } }
		public static int accelerationEventCount { get { return Input.accelerationEventCount; } }
		public static AccelerationEvent[] accelerationEvents { get { return Input.accelerationEvents; } }
		public static bool anyKey { get { return Input.anyKey; } }
		public static bool anyKeyDown { get { return Input.anyKeyDown; } }
		public static Compass compass { get { return Input.compass; } }
		public static string compositionString { get { return Input.compositionString; } }
		public static DeviceOrientation deviceOrientation { get { return Input.deviceOrientation; } }
		public static Gyroscope gyro { get { return Input.gyro; } }
		public static bool imeIsSelected { get { return Input.imeIsSelected; } }
		public static string inputString { get { return Input.inputString; } }
		public static LocationService location { get { return Input.location; } }
		public static Vector2 mousePosition { get { return Input.mousePosition; } }
		public static bool mousePresent { get { return Input.mousePresent; } }
		public static bool touchSupported { get { return Input.touchSupported; } }
		public static int touchCount { get { return Input.touchCount; } }
		public static Touch[] touches { get { return Input.touches; } }
		
		public static bool compensateSensors
		{
			get { return Input.compensateSensors; }
			set { Input.compensateSensors = value; }
		}
		
		public static Vector2 compositionCursorPos
		{
			get { return Input.compositionCursorPos; }
			set { Input.compositionCursorPos = value; }
		}
		
		public static IMECompositionMode imeCompositionMode
		{
			get { return Input.imeCompositionMode; }
			set { Input.imeCompositionMode = value; }
		}
		
		public static bool multiTouchEnabled
		{
			get { return Input.multiTouchEnabled; }
			set { Input.multiTouchEnabled = value; }
		}
		
		public static AccelerationEvent GetAccelerationEvent(int index)
		{
			return Input.GetAccelerationEvent(index);
		}
		
		public static float GetAxis(string name, PlayerID playerID = PlayerID.One)
		{
			AxisConfiguration axisConfig = GetAxisConfiguration(playerID, name);
			if(axisConfig != null)
			{
				return axisConfig.GetAxis();
			}
			else
			{
				Debug.LogError(string.Format("An axis named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
				return 0.0f;
			}
		}
		
		public static float GetAxisRaw(string name, PlayerID playerID = PlayerID.One)
		{
			AxisConfiguration axisConfig = GetAxisConfiguration(playerID, name);
			if(axisConfig != null)
			{
				return axisConfig.GetAxisRaw();
			}
			else
			{
                Debug.LogError(string.Format("An axis named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
                return 0.0f;
			}
		}
		
		public static bool GetButton(string name, PlayerID playerID = PlayerID.One)
		{
			AxisConfiguration axisConfig = GetAxisConfiguration(playerID, name);
			if(axisConfig != null)
			{
				return axisConfig.GetButton();
			}
			else
			{
				Debug.LogError(string.Format("An button named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
				return false;
			}
		}
		
		public static bool GetButtonDown(string name, PlayerID playerID = PlayerID.One)
		{
			AxisConfiguration axisConfig = GetAxisConfiguration(playerID, name);
			if(axisConfig != null)
			{
				return axisConfig.GetButtonDown();
			}
			else
			{
                Debug.LogError(string.Format("An button named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
                return false;
			}
		}
		
		public static bool GetButtonUp(string name, PlayerID playerID = PlayerID.One)
		{
			AxisConfiguration axisConfig = GetAxisConfiguration(playerID, name);
			if(axisConfig != null)
			{
				return axisConfig.GetButtonUp();
			}
			else
			{
                Debug.LogError(string.Format("An button named \'{0}\' does not exist in the active input configuration for player {1}", name, playerID));
                return false;
			}
		}
		
		public static bool GetKey(KeyCode key)
		{
			return Input.GetKey(key);
		}
		
		public static bool GetKeyDown(KeyCode key)
		{
			return Input.GetKeyDown(key);
		}
		
		public static bool GetKeyUp(KeyCode key)
		{
			return Input.GetKeyUp(key);
		}
		
		public static bool GetMouseButton(int index)
		{
			return Input.GetMouseButton(index);
		}
		
		public static bool GetMouseButtonDown(int index)
		{
			return Input.GetMouseButtonDown(index);
		}
		
		public static bool GetMouseButtonUp(int index)
		{
			return Input.GetMouseButtonUp(index);
		}
		
		public static Touch GetTouch(int index)
		{
			return Input.GetTouch(index);
		}
		
		public static string[] GetJoystickNames()
		{
			return Input.GetJoystickNames();
		}
        
        public static void ResetInputAxes()
        {
            Input.ResetInputAxes();
        }
	}
}
