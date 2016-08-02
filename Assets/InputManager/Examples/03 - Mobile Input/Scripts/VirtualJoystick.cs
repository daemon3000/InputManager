using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TeamUtility.IO.Examples
{
	[RequireComponent(typeof(Image))]
	public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
	{
		[SerializeField]
		private RectTransform _knob;
		[SerializeField]
		[Tooltip("A percentage of the image's size. An X(Y) value 1.0 would make the padding equal to the width(height) of the image.")]
		private Vector2 _padding;
		[SerializeField]
		private Vector2 _deadZone;
		[SerializeField]
		[Tooltip("If enabled, the joystick will not reset until you lift your finger of the screen.")]
		private bool _stickyEdges;
		[SerializeField]
		private string _inputConfiguration;
		[SerializeField]
		private string _horizontalAxis;
		[SerializeField]
		private string _verticalAxis;

		private RectTransform _transform;
		private Vector2 _centerPos;
		private Vector2 _pointerPos;
		private float _horizontal;
		private float _vertical;
		private bool _isPointerDown;

		private void Awake()
		{
			_transform = GetComponent<RectTransform>();
			_centerPos = _transform.rect.center;
			_pointerPos = Vector3.zero;
			_horizontal = 0.0f;
			_vertical = 0.0f;
			_isPointerDown = false;
			ResetAxisValues();
		}

		private void OnEnable()
		{
			if(_isPointerDown)
			{
				ResetAxisValues();
				_isPointerDown = false;
			}
		}

		private void OnDisable()
		{
			if(_isPointerDown)
			{
				ResetAxisValues();
				_isPointerDown = false;
			}
		}

		private void LateUpdate()
		{
			SetHorizontalAxis(_horizontal);
			SetVerticalAxis(_vertical);
			UpdateKnobPosition(_horizontal, _vertical);
		}

		public void OnPointerDown(PointerEventData eventData)
		{
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_transform, eventData.position,
																	eventData.pressEventCamera, out _pointerPos);
			UpdateAxisValues();
			_isPointerDown = true;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			ResetAxisValues();
			_isPointerDown = false;
		}

		public void OnDrag(PointerEventData eventData)
		{
			if(_isPointerDown)
			{
				RectTransformUtility.ScreenPointToLocalPointInRectangle(_transform, eventData.position, eventData.pressEventCamera, out _pointerPos);
				if(_stickyEdges)
				{
					UpdateAxisValues();
				}
				else
				{
					float paddingX = _padding.x * _transform.rect.width;
					float paddingY = _padding.y * _transform.rect.height;

					if(_pointerPos.x >= _transform.rect.x - paddingX && _pointerPos.x <= _transform.rect.xMax + paddingX &&
					   _pointerPos.y >= _transform.rect.y - paddingY && _pointerPos.y <= _transform.rect.yMax + paddingY)
					{
						UpdateAxisValues();
					}
					else
					{
						ResetAxisValues();
						_isPointerDown = false;
					}
				}
			}
		}

		private void UpdateAxisValues()
		{
			Vector2 delta = _pointerPos - _centerPos;
			_horizontal = Mathf.Clamp(delta.x / (_transform.rect.width / 2), -1.0f, 1.0f);
			_vertical = Mathf.Clamp(delta.y / (_transform.rect.height / 2), -1.0f, 1.0f);
		}

		private void ResetAxisValues()
		{
			_horizontal = 0.0f;
			_vertical = 0.0f;
			UpdateKnobPosition(0.0f, 0.0f);
		}

		private void UpdateKnobPosition(float horizontal, float vertical)
		{
			Vector2 direction = new Vector2(horizontal, vertical);
			float radius = ((_transform.rect.width + _transform.rect.height) / 2.0f) / 2.0f;
			
			if(direction.magnitude > 1.0f)
				direction.Normalize();
			
			_knob.anchoredPosition = direction * radius;
		}

		private void SetHorizontalAxis(float value)
		{
			if(value > -_deadZone.x && value < _deadZone.x)
				value = 0.0f;
			
			if(!string.IsNullOrEmpty(_inputConfiguration))
			{
				if(!string.IsNullOrEmpty(_horizontalAxis))
					InputManager.SetRemoteAxisValue(_inputConfiguration, _horizontalAxis, value);
			}
		}

		private void SetVerticalAxis(float value)
		{
			if(value > -_deadZone.y && value < _deadZone.y)
				value = 0.0f;

			if(!string.IsNullOrEmpty(_inputConfiguration))
			{
				if(!string.IsNullOrEmpty(_verticalAxis))
					InputManager.SetRemoteAxisValue(_inputConfiguration, _verticalAxis, value);
			}
		}

		private void Reset()
		{
			_padding = Vector2.zero;
			_deadZone = Vector2.zero;
			_stickyEdges = true;
		}
	}
}