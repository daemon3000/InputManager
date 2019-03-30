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

        public string Name => m_name;
        public string Comment => m_comment;
        public GamepadDPadType DPadType => m_dpadType;
        public GamepadTriggerType TriggerType => m_triggerType;
        public int LeftStickButton => m_leftStickButton;
        public int RightStickButton => m_rightStickButton;
        public int LeftBumperButton => m_leftBumperButton;
        public int RightBumperButton => m_rightBumperButton;
        public int LeftTriggerButton => m_leftTriggerButton;
        public int RightTriggerButton => m_rightTriggerButton;
        public int DPadUpButton => m_dpadUpButton;
        public int DPadDownButton => m_dpadDownButton;
        public int DPadLeftButton => m_dpadLeftButton;
        public int DPadRightButton => m_dpadRightButton;
        public int BackButton => m_backButton;
        public int StartButton => m_startButton;
        public int ActionTopButton => m_actionTopButton;
        public int ActionBottomButton => m_actionBottomButton;
        public int ActionLeftButton => m_actionLeftButton;
        public int ActionRightButton => m_actionRightButton;
        public int LeftStickXAxis => m_leftStickXAxis;
        public int LeftStickYAxis => m_leftStickYAxis;
        public int RightStickXAxis => m_rightStickXAxis;
        public int RightStickYAxis => m_rightStickYAxis;
        public int DPadXAxis => m_dpadXAxis;
        public int DPadYAxis => m_dpadYAxis;
        public int LeftTriggerAxis => m_leftTriggerAxis;
        public int RightTriggerAxis => m_rightTriggerAxis;
    }
}
