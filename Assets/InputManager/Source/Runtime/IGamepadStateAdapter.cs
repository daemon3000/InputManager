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

namespace Luminosity.IO
{
	public struct GamepadVibration
	{
		public float LeftMotor;
		public float RightMotor;
		public float LeftTrigger;
		public float RightTrigger;

		public GamepadVibration(float leftMotor, float rightMotor, float leftTrigger, float rightTrigger)
		{
			LeftMotor = leftMotor;
			RightMotor = rightMotor;
			LeftTrigger = leftTrigger;
			RightTrigger = rightTrigger;
		}
	}

	public interface IGamepadStateAdapter
	{
		bool IsConnected(GamepadIndex gamepad);
		float GetAxis(GamepadAxis axis, GamepadIndex gamepad);
		float GetAxisRaw(GamepadAxis axis, GamepadIndex gamepad);
		bool GetButton(GamepadButton button, GamepadIndex gamepad);
		bool GetButtonDown(GamepadButton button, GamepadIndex gamepad);
		bool GetButtonUp(GamepadButton button, GamepadIndex gamepad);
		void SetVibration(GamepadVibration vibration, GamepadIndex gamepad);
		GamepadVibration GetVibration(GamepadIndex gamepad);
	}
}