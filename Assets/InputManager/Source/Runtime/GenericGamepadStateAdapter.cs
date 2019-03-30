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
using System.Collections;
using System.Collections.Generic;

namespace Luminosity.IO
{
    public class GenericGamepadStateAdapter : MonoBehaviour, IGamepadStateAdapter
    {
        private struct DPadState
        {
            public float X;
            public float Y;
            public ButtonState Up;
            public ButtonState Down;
            public ButtonState Left;
            public ButtonState Right;

            public static DPadState Empty => new DPadState() {
                Up = ButtonState.Released,
                Down = ButtonState.Released,
                Right = ButtonState.Released,
                Left = ButtonState.Released
            };
        }

        private struct TriggerState { public float Left; public float Right; }

        [SerializeField]
        private GenericGamepadProfile m_gamepadProfile;
        [SerializeField]
        [Tooltip("At what interval(in sec) to check how many joysticks are connected.")]
        private float m_joystickCheckFrequency = 1.0f;
        [SerializeField]
        private float m_triggerGravity = 3.0f;
        [SerializeField]
        private float m_triggerSensitivity = 3.0f;
        [SerializeField]
        private float m_dpadGravity = 3.0f;
        [SerializeField]
        private float m_dpadSensitivity = 3.0f;
        [SerializeField]
        private bool m_dpadSnap = true;
        [SerializeField]
        private bool m_ignoreTimescale = true;

        [System.NonSerialized]
        private TriggerState[] m_triggerState;
        [System.NonSerialized]
        private DPadState[] m_dpadState;
        [System.NonSerialized]
        private bool[] m_joystickState;
        [System.NonSerialized]
        private Dictionary<int, string> m_axisNameLookupTable;

        public GenericGamepadProfile GamepadProfile
        {
            get { return m_gamepadProfile; }
            set { m_gamepadProfile = value; }
        }

        public float TriggerGravity
        {
            get { return m_triggerGravity; }
            set { m_triggerGravity = value; }
        }

        public float TriggerSensitivity
        {
            get { return m_triggerSensitivity; }
            set { m_triggerSensitivity = value; }
        }

        public float DPadGravity
        {
            get { return m_dpadGravity; }
            set { m_dpadGravity = value; }
        }

        public float DPadSensitivity
        {
            get { return m_dpadSensitivity; }
            set { m_dpadSensitivity = value; }
        }

        public bool DPadSnap
        {
            get { return m_dpadSnap; }
            set { m_dpadSnap = value; }
        }

        public bool IgnoreTimescale
        {
            get { return m_ignoreTimescale; }
            set { m_ignoreTimescale = value; }
        }

        private float DeltaTime => m_ignoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime;

        private void Awake()
        {
            m_axisNameLookupTable = new Dictionary<int, string>();
            m_triggerState = new TriggerState[4];
            m_joystickState = new bool[4];
            m_dpadState = new DPadState[4] { DPadState.Empty, DPadState.Empty, DPadState.Empty, DPadState.Empty };

            GenerateAxisNameLookupTable();
            StartCoroutine(UpdateJoystickState());

            InputManager.BeforeUpdate += OnUpdate;

            if(GamepadState.Adapter == null)
            {
                GamepadState.Adapter = this;
            }
            else
            {
                Debug.LogWarning("You shouldn't have more than one gamepad adapters in the scene");
            }
        }

        private void GenerateAxisNameLookupTable()
        {
            for(int joy = 0; joy < InputBinding.MAX_JOYSTICKS; joy++)
            {
                for(int axis = 0; axis < InputBinding.MAX_JOYSTICK_AXES; axis++)
                {
                    int key = joy * InputBinding.MAX_JOYSTICK_AXES + axis;
                    m_axisNameLookupTable[key] = string.Format("joy_{0}_axis_{1}", joy, axis);
                }
            }
        }

        private void OnDestroy()
        {
            InputManager.BeforeUpdate -= OnUpdate;
            if(GamepadState.Adapter == (IGamepadStateAdapter)this)
            {
                GamepadState.Adapter = null;
            }
        }

        private void OnUpdate()
        {
            if(m_gamepadProfile != null)
            {
                if(m_gamepadProfile.TriggerType == GamepadTriggerType.Button)
                {
                    UpdateTriggers(DeltaTime);
                }
                if(m_gamepadProfile.DPadType == GamepadDPadType.Button)
                {
                    UpdateDPadHorizontal(DeltaTime);
                    UpdateDPadVertical(DeltaTime);
                }
                else
                {
                    UpdateDPadButton();
                }
            }
        }

        private IEnumerator UpdateJoystickState()
        {
            while(true)
            {
                string[] names = InputManager.GetJoystickNames();
                for(int i = 0; i < m_joystickState.Length; i++)
                {
                    m_joystickState[i] = names.Length > i && !string.IsNullOrEmpty(names[i]);
                }

                yield return new WaitForSeconds(m_joystickCheckFrequency);
            }
        }

        private void UpdateTriggers(float deltaTime)
        {
            for(int i = 0; i < m_triggerState.Length; i++)
            {
                bool rightPressed = m_joystickState[i] ? GetButton(m_gamepadProfile.RightTriggerButton, i) : false;
                bool leftPressed = m_joystickState[i] ? GetButton(m_gamepadProfile.LeftTriggerButton, i) : false;

                if(rightPressed)
                {
                    m_triggerState[i].Right += m_triggerSensitivity * deltaTime;
                    if(m_triggerState[i].Right > InputBinding.AXIS_POSITIVE)
                    {
                        m_triggerState[i].Right = InputBinding.AXIS_POSITIVE;
                    }
                }
                else
                {
                    m_triggerState[i].Right -= m_triggerGravity * deltaTime;
                    if(m_triggerState[i].Right < InputBinding.AXIS_NEUTRAL)
                    {
                        m_triggerState[i].Right = InputBinding.AXIS_NEUTRAL;
                    }
                }

                if(leftPressed)
                {
                    m_triggerState[i].Left += m_triggerSensitivity * deltaTime;
                    if(m_triggerState[i].Left > InputBinding.AXIS_POSITIVE)
                    {
                        m_triggerState[i].Left = InputBinding.AXIS_POSITIVE;
                    }
                }
                else
                {
                    m_triggerState[i].Left -= m_triggerGravity * deltaTime;
                    if(m_triggerState[i].Left < InputBinding.AXIS_NEUTRAL)
                    {
                        m_triggerState[i].Left = InputBinding.AXIS_NEUTRAL;
                    }
                }
            }
        }

        private void UpdateDPadHorizontal(float deltaTime)
        {
            for(int i = 0; i < m_dpadState.Length; i++)
            {
                bool rightPressed = m_joystickState[i] ? GetButton(m_gamepadProfile.DPadRightButton, i) : false;
                bool leftPressed = m_joystickState[i] ? GetButton(m_gamepadProfile.DPadLeftButton, i) : false;

                if(rightPressed)
                {
                    if(m_dpadState[i].X < InputBinding.AXIS_NEUTRAL && m_dpadSnap)
                    {
                        m_dpadState[i].X = InputBinding.AXIS_NEUTRAL;
                    }

                    m_dpadState[i].X += m_dpadSensitivity * deltaTime;
                    if(m_dpadState[i].X > InputBinding.AXIS_POSITIVE)
                    {
                        m_dpadState[i].X = InputBinding.AXIS_POSITIVE;
                    }
                }
                else if(leftPressed)
                {
                    if(m_dpadState[i].X > InputBinding.AXIS_NEUTRAL && m_dpadSnap)
                    {
                        m_dpadState[i].X = InputBinding.AXIS_NEUTRAL;
                    }

                    m_dpadState[i].X -= m_dpadSensitivity * deltaTime;
                    if(m_dpadState[i].X < InputBinding.AXIS_NEGATIVE)
                    {
                        m_dpadState[i].X = InputBinding.AXIS_NEGATIVE;
                    }
                }
                else
                {
                    if(m_dpadState[i].X < InputBinding.AXIS_NEUTRAL)
                    {
                        m_dpadState[i].X += m_dpadGravity * deltaTime;
                        if(m_dpadState[i].X > InputBinding.AXIS_NEUTRAL)
                        {
                            m_dpadState[i].X = InputBinding.AXIS_NEUTRAL;
                        }
                    }
                    else if(m_dpadState[i].X > InputBinding.AXIS_NEUTRAL)
                    {
                        m_dpadState[i].X -= m_dpadGravity * deltaTime;
                        if(m_dpadState[i].X < InputBinding.AXIS_NEUTRAL)
                        {
                            m_dpadState[i].X = InputBinding.AXIS_NEUTRAL;
                        }
                    }
                }
            }
        }

        private void UpdateDPadVertical(float deltaTime)
        {
            for(int i = 0; i < m_dpadState.Length; i++)
            {
                bool upPressed = m_joystickState[i] ? GetButton(m_gamepadProfile.DPadUpButton, i) : false;
                bool downPressed = m_joystickState[i] ? GetButton(m_gamepadProfile.DPadDownButton, i) : false;

                if(upPressed)
                {
                    if(m_dpadState[i].Y < InputBinding.AXIS_NEUTRAL && m_dpadSnap)
                    {
                        m_dpadState[i].Y = InputBinding.AXIS_NEUTRAL;
                    }

                    m_dpadState[i].Y += m_dpadSensitivity * deltaTime;
                    if(m_dpadState[i].Y > InputBinding.AXIS_POSITIVE)
                    {
                        m_dpadState[i].Y = InputBinding.AXIS_POSITIVE;
                    }
                }
                else if(downPressed)
                {
                    if(m_dpadState[i].Y > InputBinding.AXIS_NEUTRAL && m_dpadSnap)
                    {
                        m_dpadState[i].Y = InputBinding.AXIS_NEUTRAL;
                    }

                    m_dpadState[i].Y -= m_dpadSensitivity * deltaTime;
                    if(m_dpadState[i].Y < InputBinding.AXIS_NEGATIVE)
                    {
                        m_dpadState[i].Y = InputBinding.AXIS_NEGATIVE;
                    }
                }
                else
                {
                    if(m_dpadState[i].Y < InputBinding.AXIS_NEUTRAL)
                    {
                        m_dpadState[i].Y += m_dpadGravity * deltaTime;
                        if(m_dpadState[i].Y > InputBinding.AXIS_NEUTRAL)
                        {
                            m_dpadState[i].Y = InputBinding.AXIS_NEUTRAL;
                        }
                    }
                    else if(m_dpadState[i].Y > InputBinding.AXIS_NEUTRAL)
                    {
                        m_dpadState[i].Y -= m_dpadGravity * deltaTime;
                        if(m_dpadState[i].Y < InputBinding.AXIS_NEUTRAL)
                        {
                            m_dpadState[i].Y = InputBinding.AXIS_NEUTRAL;
                        }
                    }
                }
            }
        }

        private void UpdateDPadButton()
        {
            for(int i = 0; i < m_dpadState.Length; i++)
            {
                float x = Input.GetAxis(m_axisNameLookupTable[i * InputBinding.MAX_JOYSTICK_AXES + m_gamepadProfile.DPadXAxis]);
                float y = Input.GetAxis(m_axisNameLookupTable[i * InputBinding.MAX_JOYSTICK_AXES + m_gamepadProfile.DPadYAxis]);

                m_dpadState[i].Up = GetNewDPadButtonState(y >= 0.9f, m_dpadState[i].Up);
                m_dpadState[i].Down = GetNewDPadButtonState(y <= -0.9f, m_dpadState[i].Down);
                m_dpadState[i].Left = GetNewDPadButtonState(x <= -0.9f, m_dpadState[i].Left);
                m_dpadState[i].Right = GetNewDPadButtonState(x >= 0.9f, m_dpadState[i].Right);
            }
        }

        private ButtonState GetNewDPadButtonState(bool isPressed, ButtonState oldState)
        {
            ButtonState newState = isPressed ? ButtonState.Pressed : ButtonState.Released;
            if(oldState == ButtonState.Pressed || oldState == ButtonState.JustPressed)
                newState = isPressed ? ButtonState.Pressed : ButtonState.JustReleased;
            else if(oldState == ButtonState.Released || oldState == ButtonState.JustReleased)
                newState = isPressed ? ButtonState.JustPressed : ButtonState.Released;

            return newState;
        }

        public bool IsConnected(GamepadIndex gamepad)
        {
            return m_joystickState[(int)gamepad];
        }

        public float GetAxis(GamepadAxis axis, GamepadIndex gamepad)
        {
            if(m_gamepadProfile == null)
                return 0.0f;

            int joyID = (int)gamepad, axisID = -1;

            switch(axis)
            {
            case GamepadAxis.LeftThumbstickX:
                axisID = m_gamepadProfile.LeftStickXAxis;
                break;
            case GamepadAxis.LeftThumbstickY:
                axisID = m_gamepadProfile.LeftStickYAxis;
                break;
            case GamepadAxis.RightThumbstickX:
                axisID = m_gamepadProfile.RightStickXAxis;
                break;
            case GamepadAxis.RightThumbstickY:
                axisID = m_gamepadProfile.RightStickYAxis;
                break;
            case GamepadAxis.DPadX:
                axisID = m_gamepadProfile.DPadXAxis;
                break;
            case GamepadAxis.DPadY:
                axisID = m_gamepadProfile.DPadYAxis;
                break;
            case GamepadAxis.LeftTrigger:
                axisID = m_gamepadProfile.LeftTriggerAxis;
                break;
            case GamepadAxis.RightTrigger:
                axisID = m_gamepadProfile.RightTriggerAxis;
                break;
            }

            return axisID >= 0 ? Input.GetAxis(m_axisNameLookupTable[joyID * InputBinding.MAX_JOYSTICK_AXES + axisID]) : 0.0f;
        }

        public float GetAxisRaw(GamepadAxis axis, GamepadIndex gamepad)
        {
            float value = GetAxis(axis, gamepad);
            return Mathf.Approximately(value, 0) ? 0.0f : Mathf.Sign(value);
        }

        public bool GetButton(GamepadButton button, GamepadIndex gamepad)
        {
            if(m_gamepadProfile == null)
                return false;

            switch(button)
            {
            case GamepadButton.LeftStick:
                return GetButton(m_gamepadProfile.LeftStickButton, (int)gamepad);
            case GamepadButton.RightStick:
                return GetButton(m_gamepadProfile.RightStickButton, (int)gamepad);
            case GamepadButton.LeftBumper:
                return GetButton(m_gamepadProfile.LeftBumperButton, (int)gamepad);
            case GamepadButton.RightBumper:
                return GetButton(m_gamepadProfile.RightBumperButton, (int)gamepad);
            case GamepadButton.DPadUp:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ?
                    GetButton(m_gamepadProfile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Up == ButtonState.Pressed || m_dpadState[(int)gamepad].Up == ButtonState.JustPressed;
            case GamepadButton.DPadDown:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ?
                    GetButton(m_gamepadProfile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Down == ButtonState.Pressed || m_dpadState[(int)gamepad].Down == ButtonState.JustPressed;
            case GamepadButton.DPadLeft:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ?
                    GetButton(m_gamepadProfile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Left == ButtonState.Pressed || m_dpadState[(int)gamepad].Left == ButtonState.JustPressed;
            case GamepadButton.DPadRight:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ?
                    GetButton(m_gamepadProfile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Right == ButtonState.Pressed || m_dpadState[(int)gamepad].Right == ButtonState.JustPressed;
            case GamepadButton.Back:
                return GetButton(m_gamepadProfile.BackButton, (int)gamepad);
            case GamepadButton.Start:
                return GetButton(m_gamepadProfile.StartButton, (int)gamepad);
            case GamepadButton.ActionBottom:
                return GetButton(m_gamepadProfile.ActionBottomButton, (int)gamepad);
            case GamepadButton.ActionRight:
                return GetButton(m_gamepadProfile.ActionRightButton, (int)gamepad);
            case GamepadButton.ActionLeft:
                return GetButton(m_gamepadProfile.ActionLeftButton, (int)gamepad);
            case GamepadButton.ActionTop:
                return GetButton(m_gamepadProfile.ActionTopButton, (int)gamepad);
            default:
                return false;
            }
        }

        public bool GetButtonDown(GamepadButton button, GamepadIndex gamepad)
        {
            if(m_gamepadProfile == null)
                return false;

            switch(button)
            {
            case GamepadButton.LeftStick:
                return GetButtonDown(m_gamepadProfile.LeftStickButton, (int)gamepad);
            case GamepadButton.RightStick:
                return GetButtonDown(m_gamepadProfile.RightStickButton, (int)gamepad);
            case GamepadButton.LeftBumper:
                return GetButtonDown(m_gamepadProfile.LeftBumperButton, (int)gamepad);
            case GamepadButton.RightBumper:
                return GetButtonDown(m_gamepadProfile.RightBumperButton, (int)gamepad);
            case GamepadButton.DPadUp:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ?
                    GetButtonDown(m_gamepadProfile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Up == ButtonState.JustPressed;
            case GamepadButton.DPadDown:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ?
                    GetButtonDown(m_gamepadProfile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Down == ButtonState.JustPressed;
            case GamepadButton.DPadLeft:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ?
                    GetButtonDown(m_gamepadProfile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Left == ButtonState.JustPressed;
            case GamepadButton.DPadRight:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ?
                    GetButtonDown(m_gamepadProfile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Right == ButtonState.JustPressed;
            case GamepadButton.Back:
                return GetButtonDown(m_gamepadProfile.BackButton, (int)gamepad);
            case GamepadButton.Start:
                return GetButtonDown(m_gamepadProfile.StartButton, (int)gamepad);
            case GamepadButton.ActionBottom:
                return GetButtonDown(m_gamepadProfile.ActionBottomButton, (int)gamepad);
            case GamepadButton.ActionRight:
                return GetButtonDown(m_gamepadProfile.ActionRightButton, (int)gamepad);
            case GamepadButton.ActionLeft:
                return GetButtonDown(m_gamepadProfile.ActionLeftButton, (int)gamepad);
            case GamepadButton.ActionTop:
                return GetButtonDown(m_gamepadProfile.ActionTopButton, (int)gamepad);
            default:
                return false;
            }
        }

        public bool GetButtonUp(GamepadButton button, GamepadIndex gamepad)
        {
            if(m_gamepadProfile == null)
                return false;

            switch(button)
            {
            case GamepadButton.LeftStick:
                return GetButtonUp(m_gamepadProfile.LeftStickButton, (int)gamepad);
            case GamepadButton.RightStick:
                return GetButtonUp(m_gamepadProfile.RightStickButton, (int)gamepad);
            case GamepadButton.LeftBumper:
                return GetButtonUp(m_gamepadProfile.LeftBumperButton, (int)gamepad);
            case GamepadButton.RightBumper:
                return GetButtonUp(m_gamepadProfile.RightBumperButton, (int)gamepad);
            case GamepadButton.DPadUp:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ?
                    GetButtonUp(m_gamepadProfile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Up == ButtonState.JustReleased;
            case GamepadButton.DPadDown:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ?
                    GetButtonUp(m_gamepadProfile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Down == ButtonState.JustReleased;
            case GamepadButton.DPadLeft:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ?
                    GetButtonUp(m_gamepadProfile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Left == ButtonState.JustReleased;
            case GamepadButton.DPadRight:
                return m_gamepadProfile.DPadType == GamepadDPadType.Button ?
                    GetButtonUp(m_gamepadProfile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Right == ButtonState.JustReleased;
            case GamepadButton.Back:
                return GetButtonUp(m_gamepadProfile.BackButton, (int)gamepad);
            case GamepadButton.Start:
                return GetButtonUp(m_gamepadProfile.StartButton, (int)gamepad);
            case GamepadButton.ActionBottom:
                return GetButtonUp(m_gamepadProfile.ActionBottomButton, (int)gamepad);
            case GamepadButton.ActionRight:
                return GetButtonUp(m_gamepadProfile.ActionRightButton, (int)gamepad);
            case GamepadButton.ActionLeft:
                return GetButtonUp(m_gamepadProfile.ActionLeftButton, (int)gamepad);
            case GamepadButton.ActionTop:
                return GetButtonUp(m_gamepadProfile.ActionTopButton, (int)gamepad);
            default:
                return false;
            }
        }

        private bool GetButton(int button, int gamepad)
        {
            KeyCode keyCode = (KeyCode)((int)KeyCode.Joystick1Button0 + (gamepad * InputBinding.MAX_JOYSTICK_BUTTONS) + button);
            return InputManager.GetKey(keyCode);
        }

        private bool GetButtonDown(int button, int gamepad)
        {
            KeyCode keyCode = (KeyCode)((int)KeyCode.Joystick1Button0 + (gamepad * InputBinding.MAX_JOYSTICK_BUTTONS) + button);
            return InputManager.GetKeyDown(keyCode);
        }

        private bool GetButtonUp(int button, int gamepad)
        {
            KeyCode keyCode = (KeyCode)((int)KeyCode.Joystick1Button0 + (gamepad * InputBinding.MAX_JOYSTICK_BUTTONS) + button);
            return InputManager.GetKeyUp(keyCode);
        }

        public void SetVibration(GamepadVibration vibration, GamepadIndex gamepad)
        {
        }

        public GamepadVibration GetVibration(GamepadIndex gamepad)
        {
            return GamepadVibration.None;
        }
    }
}