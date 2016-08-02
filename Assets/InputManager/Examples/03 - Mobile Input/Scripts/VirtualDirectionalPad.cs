using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace TeamUtility.IO.Examples
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
		private DPADStates _states;
		[SerializeField]
		[Tooltip("A percentage of the image's size. An X(Y) value 1.0 would make the padding equal to the width(height) of the image.")]
		private Vector2 _padding;
		[SerializeField]
		private float _gravity;
		[SerializeField]
		private float _sensitivity;
		[SerializeField]
		private Vector2 _deadZone;
		[SerializeField]
		private bool _ignoreTimeScale;
		[SerializeField]
		private string _inputConfiguration;
		[SerializeField]
		private string _horizontalAxis;
		[SerializeField]
		private string _verticalAxis;

		private RectTransform _transform;
		private Image _image;
		private Vector2 _centerPos;
		private Vector2 _pointerPos;
		private float _horizontal;
		private float _vertical;
		private int _dirHorizontal;
		private int _dirVertical;
		private bool _isPointerDown;

		private float DeltaTime
		{
			get
			{
				return _ignoreTimeScale ? Time.unscaledDeltaTime : Time.deltaTime;
			}
		}

		private void Awake()
		{
			_transform = GetComponent<RectTransform>();
			_image = GetComponent<Image>();
			_centerPos = _transform.rect.center;
			_pointerPos = Vector3.zero;
			_horizontal = 0.0f;
			_vertical = 0.0f;
			_dirHorizontal = 0;
			_dirVertical = 0;
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
			if(_dirHorizontal != 0)
				_horizontal = Mathf.MoveTowards(_horizontal, _dirHorizontal, _sensitivity * DeltaTime);
			else
				_horizontal = Mathf.MoveTowards(_horizontal, 0.0f, _gravity * DeltaTime);

			if(_dirVertical != 0)
				_vertical = Mathf.MoveTowards(_vertical, _dirVertical, _sensitivity * DeltaTime);
			else
				_vertical = Mathf.MoveTowards(_vertical, 0.0f, _gravity * DeltaTime);

			SetHorizontalAxis(_horizontal);
			SetVerticalAxis(_vertical);
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

		private void UpdateAxisValues()
		{
			Vector2 delta = _pointerPos - _centerPos;
			float horizontal = Mathf.Clamp(delta.x / (_transform.rect.width / 2), -1.0f, 1.0f);
			float vertical = Mathf.Clamp(delta.y / (_transform.rect.height / 2), -1.0f, 1.0f);

			if(horizontal > -_deadZone.x && horizontal < _deadZone.x)
				horizontal = 0.0f;
			if(vertical > -_deadZone.y && vertical < _deadZone.y)
				vertical = 0.0f;

			if(!Mathf.Approximately(horizontal, 0.0f))
				_dirHorizontal = (int)Mathf.Sign(horizontal);
			else
				_dirHorizontal = 0;

			if(!Mathf.Approximately(vertical, 0.0f))
				_dirVertical = (int)Mathf.Sign(vertical);
			else
				_dirVertical = 0;

			UpdateDPADImage(_dirHorizontal, _dirVertical);
		}

		private void ResetAxisValues()
		{
			_horizontal = 0.0f;
			_vertical = 0.0f;
			_dirHorizontal = 0;
			_dirVertical = 0;
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
						_image.overrideSprite = _states.upRight;
					else
						_image.overrideSprite = _states.upLeft;
				}
				else
				{
					if(dirHorizontal > 0)
						_image.overrideSprite = _states.downRight;
					else
						_image.overrideSprite = _states.downLeft;
				}
			}
			else if(moveHorizontal)
			{
				if(dirHorizontal > 0)
					_image.overrideSprite = _states.right;
				else
					_image.overrideSprite = _states.left;
			}
			else if(moveVertical)
			{
				if(dirVertical > 0)
					_image.overrideSprite = _states.up;
				else
					_image.overrideSprite = _states.down;
			}
			else
			{
				_image.overrideSprite = _states.center;
			}
		}

		private void SetHorizontalAxis(float value)
		{
			if(!string.IsNullOrEmpty(_inputConfiguration))
			{
				if(!string.IsNullOrEmpty(_horizontalAxis))
					InputManager.SetRemoteAxisValue(_inputConfiguration, _horizontalAxis, value);
			}
		}

		private void SetVerticalAxis(float value)
		{
			if(!string.IsNullOrEmpty(_inputConfiguration))
			{
				if(!string.IsNullOrEmpty(_verticalAxis))
					InputManager.SetRemoteAxisValue(_inputConfiguration, _verticalAxis, value);
			}
		}

		private void Reset()
		{
			_padding = Vector2.zero;
			_sensitivity = 3.0f;
			_gravity = 3.0f;
			_deadZone = Vector2.zero;
			_ignoreTimeScale = true;
		}
	}
}