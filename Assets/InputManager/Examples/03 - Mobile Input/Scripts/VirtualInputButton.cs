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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Luminosity.IO.Examples
{
	public class VirtualInputButton : Selectable
	{
		[SerializeField]
		private BindingReference m_buttonBinding = null;

		private ButtonState m_buttonState;

		protected override void Awake()
		{
			base.Awake();
			if(Application.isPlaying)
			{
				m_buttonState = ButtonState.Released;
				InputManager.RemoteUpdate += OnRemoteInputUpdate;
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if(Application.isPlaying)
			{
				InputManager.RemoteUpdate -= OnRemoteInputUpdate;
			}
		}

		private void OnRemoteInputUpdate(PlayerID playerID)
		{
			if(playerID == PlayerID.One)
			{
				InputBinding inputBinding = m_buttonBinding.Get();
				inputBinding.SetRemoteButtonState(m_buttonState);
			}

			if(m_buttonState == ButtonState.JustPressed)
				m_buttonState = ButtonState.Pressed;

			if(m_buttonState == ButtonState.JustReleased)
				m_buttonState = ButtonState.Released;
		}

		public override void OnPointerDown(PointerEventData eventData)
		{
			base.OnPointerDown(eventData);
			m_buttonState = ButtonState.JustPressed;
		}

		public override void OnPointerUp(PointerEventData eventData)
		{
			base.OnPointerUp(eventData);
			m_buttonState = ButtonState.JustReleased;
		}
	}
}