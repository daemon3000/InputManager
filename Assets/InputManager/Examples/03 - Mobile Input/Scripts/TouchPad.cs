using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TeamUtility.IO.Examples
{
	[RequireComponent(typeof(Image))]
	public class TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public enum AxisOption
		{
			Both, OnlyHorizontal, OnlyVertical
		}
		
		public CrossPlatformInput mobileInputAdapter;
		public AxisOption axesToUse = AxisOption.Both;
		public Vector2 sensitivity = Vector2.one;

		private RectTransform m_transform;
		private Vector2 m_pointerPos;
		private bool m_isPointerDown;

		private void Start()
		{
			m_transform = GetComponent<RectTransform>();
			m_isPointerDown = false;
			ResetAxisValues();
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
			if(axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal)
				mobileInputAdapter.SetMouseX(delta.x * sensitivity.x);
			if(axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical)
				mobileInputAdapter.SetMouseY(delta.y * sensitivity.y);
		}

		private void ResetAxisValues()
		{
			mobileInputAdapter.SetMouseX(0.0f);
			mobileInputAdapter.SetMouseY(0.0f);
		}
	}
}