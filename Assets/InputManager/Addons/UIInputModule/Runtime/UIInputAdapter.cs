using UnityEngine;
using UnityEngine.EventSystems;

namespace Luminosity.IO
{
	public class UIInputAdapter : BaseInput
	{
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
			return InputManager.GetMouseButtonDown(button);
		}

		public override bool GetMouseButtonUp(int button)
		{
			return InputManager.GetMouseButtonUp(button);
		}

		public override bool GetMouseButton(int button)
		{
			return InputManager.GetMouseButton(button);
		}

		public override Vector2 mousePosition
		{
			get { return InputManager.mousePosition; }
		}

		public override Vector2 mouseScrollDelta
		{
			get { return InputManager.mouseScrollDelta; }
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
