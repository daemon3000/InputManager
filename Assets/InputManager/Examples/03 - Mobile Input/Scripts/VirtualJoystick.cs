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
	public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		[SerializeField]
		private RectTransform m_knob;
		[SerializeField]
		[Tooltip("A percentage of the image's size. An X(Y) value 1.0 would make the padding equal to the width(height) of the image.")]
		private Vector2 m_padding;
		[SerializeField]
		private Vector2 m_deadZone;
		[SerializeField]
		[Tooltip("If enabled, the joystick will not reset until you lift your finger of the screen.")]
		private bool m_stickyEdges;
		[SerializeField]
		private BindingReference m_horizontalAxisBinding;
		[SerializeField]
		private BindingReference m_verticalAxisBinding;

		private RectTransform m_transform;
		private Vector2 m_centerPos;
		private Vector2 m_pointerPos;
		private float m_horizontal;
		private float m_vertical;
		private bool m_isPointerDown;

		private void Awake()
		{
			m_transform = GetComponent<RectTransform>();
			m_centerPos = m_transform.rect.center;
			m_pointerPos = Vector3.zero;
			m_horizontal = 0.0f;
			m_vertical = 0.0f;
			m_isPointerDown = false;
			ResetAxisValues();

			InputManager.RemoteUpdate += OnRemoteInputUpdate;
		}

		private void OnDestroy()
		{
			InputManager.RemoteUpdate -= OnRemoteInputUpdate;
		}

		private void OnEnable()
		{
			if(m_isPointerDown)
			{
				ResetAxisValues();
				m_isPointerDown = false;
			}
		}

		private void OnDisable()
		{
			if(m_isPointerDown)
			{
				ResetAxisValues();
				m_isPointerDown = false;
			}
		}

		private void OnRemoteInputUpdate(PlayerID playerID)
		{
			if(playerID == PlayerID.One)
			{
				SetHorizontalAxis(m_horizontal);
				SetVerticalAxis(m_vertical);
			}
		}

		private void LateUpdate()
		{
			UpdateKnobPosition(m_horizontal, m_vertical);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(m_transform, eventData.position,
																	eventData.pressEventCamera, out m_pointerPos);
			UpdateAxisValues();
			m_isPointerDown = true;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			ResetAxisValues();
			m_isPointerDown = false;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if(m_isPointerDown)
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(m_transform, eventData.position, eventData.pressEventCamera, out m_pointerPos);
				if(m_stickyEdges)
				{
					UpdateAxisValues();
				}
				else
				{
					float paddingX = m_padding.x * m_transform.rect.width;
					float paddingY = m_padding.y * m_transform.rect.height;

					if(m_pointerPos.x >= m_transform.rect.x - paddingX && m_pointerPos.x <= m_transform.rect.xMax + paddingX &&
					   m_pointerPos.y >= m_transform.rect.y - paddingY && m_pointerPos.y <= m_transform.rect.yMax + paddingY)
					{
						UpdateAxisValues();
					}
					else
					{
						ResetAxisValues();
						m_isPointerDown = false;
					}
				}
			}
		}

		private void UpdateAxisValues()
		{
			Vector2 delta = m_pointerPos - m_centerPos;
			m_horizontal = Mathf.Clamp(delta.x / (m_transform.rect.width / 2), -1.0f, 1.0f);
			m_vertical = Mathf.Clamp(delta.y / (m_transform.rect.height / 2), -1.0f, 1.0f);
		}

		private void ResetAxisValues()
		{
			m_horizontal = 0.0f;
			m_vertical = 0.0f;
			UpdateKnobPosition(0.0f, 0.0f);
		}

		private void UpdateKnobPosition(float horizontal, float vertical)
		{
			Vector2 direction = new Vector2(horizontal, vertical);
			float radius = ((m_transform.rect.width + m_transform.rect.height) / 2.0f) / 2.0f;
			
			if(direction.magnitude > 1.0f)
				direction.Normalize();
			
			m_knob.anchoredPosition = direction * radius;
		}

		private void SetHorizontalAxis(float value)
		{
			if(value > -m_deadZone.x && value < m_deadZone.x)
				value = 0.0f;

			var binding = m_horizontalAxisBinding.Get();
			binding.SetRemoteAxisValue(value);
		}

		private void SetVerticalAxis(float value)
		{
			if(value > -m_deadZone.y && value < m_deadZone.y)
				value = 0.0f;

			var binding = m_verticalAxisBinding.Get();
			binding.SetRemoteAxisValue(value);
		}

		private void Reset()
		{
			m_padding = Vector2.zero;
			m_deadZone = Vector2.zero;
			m_stickyEdges = true;
		}
	}
}