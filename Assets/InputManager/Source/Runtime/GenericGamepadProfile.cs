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

namespace Luminosity.IO
{
    [CreateAssetMenu(fileName = "New Gamepad Profile", menuName = "Luminosity/Input Manager/Gamepad Profile")]
    public class GenericGamepadProfile : ScriptableObject
    {
        [SerializeField]
        private string m_name = null;

        [SerializeField]
        [Multiline]
        private string m_comment = null;

        [SerializeField]
        private GamepadDPadType m_dpadType = GamepadDPadType.Axis;

        [SerializeField]
        private GamepadTriggerType m_triggerType = GamepadTriggerType.Axis;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_leftStickButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_rightStickButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_leftBumperButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_rightBumperButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_leftTriggerButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_rightTriggerButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_dpadUpButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_dpadDownButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_dpadLeftButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_dpadRightButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_backButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_startButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_actionTopButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_actionBottomButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_actionLeftButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_BUTTONS - 1)]
        private int m_actionRightButton = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)]
        private int m_leftStickXAxis = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)]
        private int m_leftStickYAxis = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)]
        private int m_rightStickXAxis = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)]
        private int m_rightStickYAxis = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)]
        private int m_dpadXAxis = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)]
        private int m_dpadYAxis = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)]
        private int m_leftTriggerAxis = 0;

        [SerializeField]
        [Range(0, InputBinding.MAX_JOYSTICK_AXES - 1)]
        private int m_rightTriggerAxis = 0;

        public string Name { get { return m_name; } }
        public string Comment { get { return m_comment; } }
        public GamepadDPadType DPadType { get { return m_dpadType; } }
        public GamepadTriggerType TriggerType { get { return m_triggerType; } }
        public int LeftStickButton { get { return m_leftStickButton; } }
        public int RightStickButton { get { return m_rightStickButton; } }
        public int LeftBumperButton { get { return m_leftBumperButton; } }
        public int RightBumperButton { get { return m_rightBumperButton; } }
        public int LeftTriggerButton { get { return m_leftTriggerButton; } }
        public int RightTriggerButton { get { return m_rightTriggerButton; } }
        public int DPadUpButton { get { return m_dpadUpButton; } }
        public int DPadDownButton { get { return m_dpadDownButton; } }
        public int DPadLeftButton { get { return m_dpadLeftButton; } }
        public int DPadRightButton { get { return m_dpadRightButton; } }
        public int BackButton { get { return m_backButton; } }
        public int StartButton { get { return m_startButton; } }
        public int ActionTopButton { get { return m_actionTopButton; } }
        public int ActionBottomButton { get { return m_actionBottomButton; } }
        public int ActionLeftButton { get { return m_actionLeftButton; } }
        public int ActionRightButton { get { return m_actionRightButton; } }
        public int LeftStickXAxis { get { return m_leftStickXAxis; } }
        public int LeftStickYAxis { get { return m_leftStickYAxis; } }
        public int RightStickXAxis { get { return m_rightStickXAxis; } }
        public int RightStickYAxis { get { return m_rightStickYAxis; } }
        public int DPadXAxis { get { return m_dpadXAxis; } }
        public int DPadYAxis { get { return m_dpadYAxis; } }
        public int LeftTriggerAxis { get { return m_leftTriggerAxis; } }
        public int RightTriggerAxis { get { return m_rightTriggerAxis; } }
    }
}
