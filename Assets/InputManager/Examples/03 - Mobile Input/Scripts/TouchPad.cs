using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TeamUtility.IO.Examples
{
	[RequireComponent(typeof(Image))]
	public class TouchPad : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		public enum AxisOption
		{
			Both, OnlyHorizontal, OnlyVertical
		}
		
		public AxisOption axesToUse = AxisOption.Both;
		public Vector2 sensitivity = Vector2.one;
		public string inputConfiguration;
		public string horizontalAxis;
		public string verticalAxis;

		private RectTransform _transform;
		private Vector2 _pointerPos;
		private bool _isPointerDown;

		private void Start()
		{
			_transform = GetComponent<RectTransform>();
			_isPointerDown = false;
			ResetAxisValues();
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			_isPointerDown = true;
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_transform, eventData.position, eventData.pressEventCamera, out _pointerPos);
		}
		
		public void OnDrag(PointerEventData eventData)
		{
			if(_isPointerDown)
			{
				Vector2 lastPointerPos = _pointerPos;
				RectTransformUtility.ScreenPointToLocalPointInRectangle(_transform, eventData.position, eventData.pressEventCamera, out _pointerPos);
				if(_pointerPos.x >= _transform.rect.x && _pointerPos.x <= _transform.rect.xMax &&
				   _pointerPos.y >= _transform.rect.y && _pointerPos.y <= _transform.rect.yMax)
				{
					UpdateAxisValues(_pointerPos - lastPointerPos);
				}
				else
				{
					ResetAxisValues();
					_isPointerDown = false;
				}
			}
		}

		public void OnPointerUp(PointerEventData eventData) 
		{
			_isPointerDown = false;
			ResetAxisValues();
		}

		private void UpdateAxisValues(Vector2 delta)
		{
			if(axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyHorizontal)
				SetHorizontalAxis(delta.x * sensitivity.x);
			if(axesToUse == AxisOption.Both || axesToUse == AxisOption.OnlyVertical)
				SetVerticalAxis(delta.y * sensitivity.y);
		}

		private void ResetAxisValues()
		{
			SetHorizontalAxis(0.0f);
			SetVerticalAxis(0.0f);
		}

		private void SetHorizontalAxis(float value)
		{
			if(!string.IsNullOrEmpty(inputConfiguration))
			{
				if(!string.IsNullOrEmpty(horizontalAxis))
					InputManager.SetRemoteAxisValue(inputConfiguration, horizontalAxis, value);
			}
		}

		private void SetVerticalAxis(float value)
		{
			if(!string.IsNullOrEmpty(inputConfiguration))
			{
				if(!string.IsNullOrEmpty(verticalAxis))
					InputManager.SetRemoteAxisValue(inputConfiguration, verticalAxis, value);
			}
		}
	}
}