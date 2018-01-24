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

		public static bool IsConnected(XInputPlayer player)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.IsConnected(player) : false;
		}

		public static float GetAxis(XInputAxis axis, XInputPlayer player)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.GetAxis(axis, player) : 0;
		}

		public static float GetAxisRaw(XInputAxis axis, XInputPlayer player)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.GetAxisRaw(axis, player) : 0;
		}

		public static bool GetButton(XInputButton button, XInputPlayer player)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.GetButton(button, player) : false;
		}

		public static bool GetButtonDown(XInputButton button, XInputPlayer player)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.GetButtonDown(button, player) : false;
		}

		public static bool GetButtonUp(XInputButton button, XInputPlayer player)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.GetButtonUp(button, player) : false;
		}

		public static void SetVibration(GamepadVibration vibration, XInputPlayer player)
		{
			PrintMissingAdapterWarningIfNecessary();
			if(Adapter != null) Adapter.SetVibration(vibration, player);
		}

		public static GamepadVibration GetVibration(XInputPlayer player)
		{
			PrintMissingAdapterWarningIfNecessary();
			return Adapter != null ? Adapter.GetVibration(player) : new GamepadVibration();
		}

		private static void PrintMissingAdapterWarningIfNecessary()
		{
			if(Adapter == null && !m_hasWarningBeenDisplayed)
			{
				Debug.LogWarning("No XInput adapter has been assigned.");
				m_hasWarningBeenDisplayed = true;
			}
		}
	}
}
