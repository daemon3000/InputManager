using UnityEngine;
using UnityEngine.EventSystems;

namespace Luminosity.IO
{
	public class UIInputAdapter : BaseInput
	{
		public bool IsMouseEnabled { get; set; }

		protected override void Awake()
		{
			base.Awake();
			IsMouseEnabled = true;
		}

		public override string compositionString
		{
			get { return InputManager.compositionString; }
		}

		public override IMECompositionMode imeCompositionMode
		{
			get { return InputManager.imeCompositionMode; }
			set { InputManager.imeCompositionMode = value; }
		}

		public override Vector2 compositionCursorPos
		{
			get { return InputManager.compositionCursorPos; }
			set { InputManager.compositionCursorPos = value; }
		}

		public override bool mousePresent
		{
			get { return InputManager.mousePresent; }
		}

		public override bool GetMouseButtonDown(int button)
		{
			return IsMouseEnabled && InputManager.GetMouseButtonDown(button);
		}

		public override bool GetMouseButtonUp(int button)
		{
			return IsMouseEnabled && InputManager.GetMouseButtonUp(button);
		}

		public override bool GetMouseButton(int button)
		{
			return IsMouseEnabled && InputManager.GetMouseButton(button);
		}

		public override Vector2 mousePosition
		{
			get { return IsMouseEnabled ? InputManager.mousePosition : -Vector2.one; }
		}

		public override Vector2 mouseScrollDelta
		{
			get { return IsMouseEnabled ? InputManager.mouseScrollDelta : Vector2.zero; }
		}

		public override bool touchSupported
		{
			get { return InputManager.touchSupported; }
		}

		public override int touchCount
		{
			get { return InputManager.touchCount; }
		}

		public override Touch GetTouch(int index)
		{
			return InputManager.GetTouch(index);
		}

		public override float GetAxisRaw(string axisName)
		{
			return InputManager.GetAxisRaw(axisName);
		}

		public override bool GetButtonDown(string buttonName)
		{
			return InputManager.GetButtonDown(buttonName);
		}
	}
}
