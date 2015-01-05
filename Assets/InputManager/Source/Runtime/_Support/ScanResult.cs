using UnityEngine;
using System.Collections;

namespace TeamUtility.IO
{
	public struct ScanResult
	{
		public ScanFlags scanFlags;
		public KeyCode key;
		public int joystick;
		public int joystickAxis;
		public int mouseAxis;
		public object userData;
	}
}