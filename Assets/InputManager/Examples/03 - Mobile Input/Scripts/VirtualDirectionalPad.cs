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
	public class VirtualDirectionalPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		[System.Serializable]
		public class DPADStates
		{
			public Sprite center;
			public Sprite up;
			public Sprite upRight;
			public Sprite upLeft;
			public Sprite down;
			public Sprite downRight;
			public Sprite downLeft;
			public Sprite right;
			public Sprite left;
		}

		[SerializeField]
		private DPADStates m_states;
		[SerializeField]
		[Tooltip("A percentage of the image's size. An X(Y) value 1.0 would make the padding equal to the width(height) of the image.")]
		private Vector2 m_padding;
		[SerializeField]
		private float m_gravity;
		[SerializeField]
		private float m_sensitivity;
		[SerializeField]
		private Vector2 m_deadZone;
		[SerializeField]
		private BindingReference m_horizontalAxisBinding;
		[SerializeField]
		private BindingReference m_verticalAxisBinding;
		
		private RectTransform m_transform;
		private Image m_image;
		private Vector2 m_centerPos;
		private Vector2 m_pointerPos;
		private float m_horizontal;
		private float m_vertical;
		private int m_dirHorizontal;
		private int m_dirVertical;
		private bool m_isPointerDown;

		private float DeltaTime
		{
			get
			{
				return Time.unscaledDeltaTime;
			}
		}

		private void Awake()
		{
			m_transform = GetComponent<RectTransform>();
			m_image = GetComponent<Image>();
			m_centerPos = m_transform.rect.center;
			m_pointerPos = Vector3.zero;
			m_horizontal = 0.0f;
			m_vertical = 0.0f;
			m_dirHorizontal = 0;
			m_dirVertical = 0;
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
			if(m_dirHorizontal != 0)
				m_horizontal = Mathf.MoveTowards(m_horizontal, m_dirHorizontal, m_sensitivity * DeltaTime);
			else
				m_horizontal = Mathf.MoveTowards(m_horizontal, 0.0f, m_gravity * DeltaTime);

			if(m_dirVertical != 0)
				m_vertical = Mathf.MoveTowards(m_vertical, m_dirVertical, m_sensitivity * DeltaTime);
			else
				m_vertical = Mathf.MoveTowards(m_vertical, 0.0f, m_gravity * DeltaTime);
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

		private void UpdateAxisValues()
		{
			Vector2 delta = m_pointerPos - m_centerPos;
			float horizontal = Mathf.Clamp(delta.x / (m_transform.rect.width / 2), -1.0f, 1.0f);
			float vertical = Mathf.Clamp(delta.y / (m_transform.rect.height / 2), -1.0f, 1.0f);

			if(horizontal > -m_deadZone.x && horizontal < m_deadZone.x)
				horizontal = 0.0f;
			if(vertical > -m_deadZone.y && vertical < m_deadZone.y)
				vertical = 0.0f;

			if(!Mathf.Approximately(horizontal, 0.0f))
				m_dirHorizontal = (int)Mathf.Sign(horizontal);
			else
				m_dirHorizontal = 0;

			if(!Mathf.Approximately(vertical, 0.0f))
				m_dirVertical = (int)Mathf.Sign(vertical);
			else
				m_dirVertical = 0;

			UpdateDPADImage(m_dirHorizontal, m_dirVertical);
		}

		private void ResetAxisValues()
		{
			m_horizontal = 0.0f;
			m_vertical = 0.0f;
			m_dirHorizontal = 0;
			m_dirVertical = 0;
			UpdateDPADImage(0, 0);
		}

		private void UpdateDPADImage(int dirHorizontal, int dirVertical)
		{
			bool moveHorizontal = dirHorizontal != 0;
			bool moveVertical = dirVertical != 0;

			if(moveHorizontal && moveVertical)
			{
				if(dirVertical > 0)
				{
					if(dirHorizontal > 0)
						m_image.overrideSprite = m_states.upRight;
					else
						m_image.overrideSprite = m_states.upLeft;
				}
				else
				{
					if(dirHorizontal > 0)
						m_image.overrideSprite = m_states.downRight;
					else
						m_image.overrideSprite = m_states.downLeft;
				}
			}
			else if(moveHorizontal)
			{
				if(dirHorizontal > 0)
					m_image.overrideSprite = m_states.right;
				else
					m_image.overrideSprite = m_states.left;
			}
			else if(moveVertical)
			{
				if(dirVertical > 0)
					m_image.overrideSprite = m_states.up;
				else
					m_image.overrideSprite = m_states.down;
			}
			else
			{
				m_image.overrideSprite = m_states.center;
			}
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

		private void Reset()
		{
			m_padding = Vector2.zero;
			m_sensitivity = 3.0f;
			m_gravity = 3.0f;
			m_deadZone = Vector2.zero;
		}
	}
}