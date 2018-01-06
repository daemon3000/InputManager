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
	public class UWPGamepadAdapter : MonoBehaviour, IGamepadStateAdapter
	{
#if !UNITY_EDITOR && UNITY_WSA && INPUT_MANAGER_X_INPUT
		private void Awake()
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
		}

		public float GetAxis(XInputAxis axis, XInputPlayer player)
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
			return 0;
		}

		public float GetAxisRaw(XInputAxis axis, XInputPlayer player)
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
			return 0;
		}

		public bool GetButton(XInputButton button, XInputPlayer player)
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
			return false;
		}

		public bool GetButtonDown(XInputButton button, XInputPlayer player)
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
			return false;
		}

		public bool GetButtonUp(XInputButton button, XInputPlayer player)
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
			return false;
		}

		public void SetVibration(XInputPlayer player, float leftMotor, float rightMotor)
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
		}
#else
		private void Awake()
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
		}

		public float GetAxis(XInputAxis axis, XInputPlayer player)
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
			return 0;
		}

		public float GetAxisRaw(XInputAxis axis, XInputPlayer player)
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
			return 0;
		}

		public bool GetButton(XInputButton button, XInputPlayer player)
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
			return false;
		}

		public bool GetButtonDown(XInputButton button, XInputPlayer player)
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
			return false;
		}

		public bool GetButtonUp(XInputButton button, XInputPlayer player)
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
			return false;
		}

		public void SetVibration(XInputPlayer player, float leftMotor, float rightMotor)
		{
			Debug.LogWarning("UWPGamepadAdapter works only on Universal Windows Platform if the 'INPUT_MANAGER_X_INPUT' scripting symbol is defined.", gameObject);
		}
#endif
	}
}