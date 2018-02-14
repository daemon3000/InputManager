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
	public static class GamepadState
	{
		private static bool m_hasWarningBeenDisplayed = false;

		public static IGamepadStateAdapter Adapter { get; set; }

		public static bool IsGamepadSupported
		{
			get
			{
#if ENABLE_X_INPUT
				return true;
#else
				return false;
#endif
			}
		}

		public static bool IsConnected(GamepadIndex gamepad)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.IsConnected(gamepad) : false;
		}

		public static float GetAxis(GamepadAxis axis, GamepadIndex gamepad)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.GetAxis(axis, gamepad) : 0;
		}

		public static float GetAxisRaw(GamepadAxis axis, GamepadIndex gamepad)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.GetAxisRaw(axis, gamepad) : 0;
		}

		public static bool GetButton(GamepadButton button, GamepadIndex gamepad)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.GetButton(button, gamepad) : false;
		}

		public static bool GetButtonDown(GamepadButton button, GamepadIndex gamepad)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.GetButtonDown(button, gamepad) : false;
		}

		public static bool GetButtonUp(GamepadButton button, GamepadIndex gamepad)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.GetButtonUp(button, gamepad) : false;
		}

		public static void SetVibration(GamepadVibration vibration, GamepadIndex gamepad)
		{
			PrintMissingAdapterWarningIfNecessary();
			if(Adapter != null) Adapter.SetVibration(vibration, gamepad);
		}

		public static GamepadVibration GetVibration(GamepadIndex gamepad)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.GetVibration(gamepad) : new GamepadVibration();
		}

		private static void PrintMissingAdapterWarningIfNecessary()
		{
			if(Adapter == null && !m_hasWarningBeenDisplayed)
			{
				Debug.LogWarning("No gamepad adapter has been assigned.");
				m_hasWarningBeenDisplayed = true;
			}
		}
	}
}
