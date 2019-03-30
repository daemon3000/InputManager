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
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Luminosity.IO.Examples
{
	[RequireComponent(typeof(Image))]
	public class TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public enum AxisOption
		{
			Both, OnlyHorizontal, OnlyVertical
		}

		[SerializeField]
		private AxisOption m_axesToUse = AxisOption.Both;
		[SerializeField]
		private Vector2 m_sensitivity = Vector2.one;
		[SerializeField]
		private BindingReference m_horizontalAxisBinding = null;
		[SerializeField]
		private BindingReference m_verticalAxisBinding = null;
		
		private RectTransform m_transform;
		private Vector2 m_pointerPos;
		private float m_horizontal;
		private float m_vertical;
		private bool m_isPointerDown;

		private void Awake()
		{
			m_transform = GetComponent<RectTransform>();
			m_isPointerDown = false;
			m_horizontal = 0.0f;
			m_vertical = 0.0f;
			ResetAxisValues();
			InputManager.RemoteUpdate += OnRemoteInputUpdate;
		}

		private void OnDestroy()
		{
			InputManager.RemoteUpdate -= OnRemoteInputUpdate;
		}

		private void OnRemoteInputUpdate(PlayerID playerID)
		{
			if(playerID == PlayerID.One)
			{
				SetHorizontalAxis(m_horizontal);
				SetVerticalAxis(m_vertical);
			}
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			m_isPointerDown = true;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_transform, eventData.position, eventData.pressEventCamera, out m_pointerPos);
		}
		
		public void OnDrag(PointerEventData eventData)
		{
			if(m_isPointerDown)
			{
				Vector2 lastPointerPos = m_pointerPos;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(m_transform, eventData.position, eventData.pressEventCamera, out m_pointerPos);
				if(m_pointerPos.x >= m_transform.rect.x && m_pointerPos.x <= m_transform.rect.xMax &&
				   m_pointerPos.y >= m_transform.rect.y && m_pointerPos.y <= m_transform.rect.yMax)
				{
					UpdateAxisValues(m_pointerPos - lastPointerPos);
				}
				else
				{
					ResetAxisValues();
					m_isPointerDown = false;
				}
			}
		}

		public void OnPointerUp(PointerEventData eventData) 
		{
			m_isPointerDown = false;
			ResetAxisValues();
		}

		private void UpdateAxisValues(Vector2 delta)
		{
			if(m_axesToUse == AxisOption.Both || m_axesToUse == AxisOption.OnlyHorizontal)
				m_horizontal = delta.x * m_sensitivity.x;
			if(m_axesToUse == AxisOption.Both || m_axesToUse == AxisOption.OnlyVertical)
				m_vertical = delta.y * m_sensitivity.y;
		}

		private void ResetAxisValues()
		{
			m_horizontal = 0.0f;
			m_vertical = 0.0f;
		}

		private void SetHorizontalAxis(float value)
		{
			var binding = m_horizontalAxisBinding.Get();
			binding.SetRemoteAxisValue(value);
		}

		private void SetVerticalAxis(float value)
		{
			var binding = m_verticalAxisBinding.Get();
			binding.SetRemoteAxisValue(value);
		}
	}
}