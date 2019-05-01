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
using UnityEngine.Profiling;
using UnityEngine.Events;

namespace Luminosity.IO
{
    public class GenericGamepadStateAdapter : MonoBehaviour, IGamepadStateAdapter
    {
        private struct GamepadStatus
        {
            public string Name;
            public bool IsConnected;

            public GamepadStatus(string name, bool connected)
            {
                Name = name;
                IsConnected = connected;
            }

            public static GamepadStatus NotConnected
            {
                get
                {
                    return new GamepadStatus()
                    {
                        Name = null,
                        IsConnected = false
                    };
                }
            }
        }

        private struct DPadState
        {
            public float X;
            public float Y;
            public ButtonState Up;
            public ButtonState Down;
            public ButtonState Left;
            public ButtonState Right;

            public static DPadState Empty
            {
                get
                {
                    return new DPadState() {
                        Up = ButtonState.Released,
                        Down = ButtonState.Released,
                        Right = ButtonState.Released,
                        Left = ButtonState.Released
                    };
                }
            }
        }

        private struct TriggerState { public float Left; public float Right; }

        [SerializeField]
        private GenericGamepadProfile m_gamepadOne = null;
        [SerializeField]
        private GenericGamepadProfile m_gamepadTwo = null;
        [SerializeField]
        private GenericGamepadProfile m_gamepadThree = null;
        [SerializeField]
        private GenericGamepadProfile m_gamepadFour = null;
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
#pragma warning disable 414
        [SerializeField]
        private bool m_useSharedProfile = true;
#pragma warning restore 414

        [System.NonSerialized]
        private TriggerState[] m_triggerState;
        [System.NonSerialized]
        private DPadState[] m_dpadState;
        [System.NonSerialized]
        private GamepadStatus[] m_gamepadStatus;
        [System.NonSerialized]
        private Dictionary<int, string> m_axisNameLookupTable;

        public event UnityAction<GamepadIndex> GamepadConnected;
        public event UnityAction<GamepadIndex> GamepadDisconnected;

        public GenericGamepadProfile this[GamepadIndex gamepad]
        {
            get { return GetProfile(gamepad); }
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

        private float DeltaTime
        {
            get { return m_ignoreTimescale ? Time.unscaledDeltaTime : Time.deltaTime; }
        }

        private void Awake()
        {
            m_axisNameLookupTable = new Dictionary<int, string>();
            m_triggerState = new TriggerState[4];
            m_gamepadStatus = new GamepadStatus[4] { GamepadStatus.NotConnected, GamepadStatus.NotConnected, GamepadStatus.NotConnected, GamepadStatus.NotConnected };
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
            Profiler.BeginSample("GenericGamepadStateAdapter.OnUpdate", this);
            UpdateTriggers(DeltaTime);
            UpdateDPadButtons();
            UpdateDPadVertical(DeltaTime);
            UpdateDPadHorizontal(DeltaTime);
            Profiler.EndSample();
        }

        private IEnumerator UpdateJoystickState()
        {
            while(true)
            {
                string[] names = InputManager.GetJoystickNames();
                for(int i = 0; i < m_gamepadStatus.Length; i++)
                {
                    GamepadStatus oldStatus = m_gamepadStatus[i];
                    if(names.Length > i && !string.IsNullOrEmpty(names[i]))
                        m_gamepadStatus[i] = new GamepadStatus(names[i], true);
                    else
                        m_gamepadStatus[i] = GamepadStatus.NotConnected;

                    if(oldStatus.IsConnected && !m_gamepadStatus[i].IsConnected)
                    {
                        if(GamepadDisconnected != null)
                            GamepadDisconnected((GamepadIndex)i);

                        Debug.LogFormat("Gamepad Disconnected: {0}", oldStatus.Name);
                    }

                    if(!oldStatus.IsConnected && m_gamepadStatus[i].IsConnected)
                    {
                        if(GamepadConnected != null)
                            GamepadConnected((GamepadIndex)i);

                        Debug.LogFormat("Gamepad Connected: {0}", m_gamepadStatus[i].Name);
                    }
                }

                yield return new WaitForSeconds(m_joystickCheckFrequency);
            }
        }

        private void UpdateTriggers(float deltaTime)
        {
            for(int i = 0; i < m_triggerState.Length; i++)
            {
                GenericGamepadProfile profile = GetProfile(i);
                bool rightPressed = false, leftPressed = false;

                if(profile != null && profile.TriggerType == GamepadTriggerType.Button)
                {
                    rightPressed = GetButton(profile.RightTriggerButton, i);
                    leftPressed = GetButton(profile.LeftTriggerButton, i);
                }

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
                GenericGamepadProfile profile = GetProfile(i);
                bool rightPressed = false, leftPressed = false;

                if(profile != null && profile.DPadType == GamepadDPadType.Button)
                {
                    rightPressed = GetButton(profile.DPadRightButton, i);
                    leftPressed = GetButton(profile.DPadLeftButton, i);
                }

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
                GenericGamepadProfile profile = GetProfile(i);
                bool upPressed = false, downPressed = false;

                if(profile != null && profile.DPadType == GamepadDPadType.Button)
                {
                    upPressed = GetButton(profile.DPadUpButton, i);
                    downPressed = GetButton(profile.DPadDownButton, i);
                }

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

        private void UpdateDPadButtons()
        {
            for(int i = 0; i < m_dpadState.Length; i++)
            {
                GenericGamepadProfile profile = GetProfile(i);
                float x = 0.0f, y = 0.0f;

                if(profile != null && profile.DPadType == GamepadDPadType.Axis)
                {
                    x = Input.GetAxis(m_axisNameLookupTable[i * InputBinding.MAX_JOYSTICK_AXES + profile.DPadXAxis]);
                    y = Input.GetAxis(m_axisNameLookupTable[i * InputBinding.MAX_JOYSTICK_AXES + profile.DPadYAxis]);
                }

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

        public GenericGamepadProfile GetProfile(GamepadIndex gamepad)
        {
            switch(gamepad)
            {
            case GamepadIndex.GamepadOne:
                return m_gamepadOne;
            case GamepadIndex.GamepadTwo:
                return m_gamepadTwo;
            case GamepadIndex.GamepadThree:
                return m_gamepadThree;
            case GamepadIndex.GamepadFour:
                return m_gamepadFour;
            default:
                throw new System.ArgumentException(string.Format("Gamepad '{0}' is not valid.", gamepad), "gamepad");
            }
        }

        public void SetProfile(GamepadIndex gamepad, GenericGamepadProfile profile)
        {
            switch(gamepad)
            {
            case GamepadIndex.GamepadOne:
                m_gamepadOne = profile;
                break;
            case GamepadIndex.GamepadTwo:
                m_gamepadTwo = profile;
                break;
            case GamepadIndex.GamepadThree:
                m_gamepadThree = profile;
                break;
            case GamepadIndex.GamepadFour:
                m_gamepadFour = profile;
                break;
            default:
                throw new System.ArgumentException(string.Format("Gamepad '{0}' is not valid.", gamepad), "gamepad");
            }
        }

        public string GetName(GamepadIndex gamepad)
        {
            return m_gamepadStatus[(int)gamepad].Name;
        }

        public bool IsConnected(GamepadIndex gamepad)
        {
            return m_gamepadStatus[(int)gamepad].IsConnected;
        }

        public float GetAxis(GamepadAxis axis, GamepadIndex gamepad)
        {
            GenericGamepadProfile profile = GetProfile(gamepad);
            if(profile == null)
                return 0.0f;

            int joyID = (int)gamepad, axisID = -1;

            switch(axis)
            {
            case GamepadAxis.LeftThumbstickX:
                axisID = profile.LeftStickXAxis;
                break;
            case GamepadAxis.LeftThumbstickY:
                axisID = profile.LeftStickYAxis;
                break;
            case GamepadAxis.RightThumbstickX:
                axisID = profile.RightStickXAxis;
                break;
            case GamepadAxis.RightThumbstickY:
                axisID = profile.RightStickYAxis;
                break;
            case GamepadAxis.DPadX:
                axisID = profile.DPadXAxis;
                break;
            case GamepadAxis.DPadY:
                axisID = profile.DPadYAxis;
                break;
            case GamepadAxis.LeftTrigger:
                axisID = profile.LeftTriggerAxis;
                break;
            case GamepadAxis.RightTrigger:
                axisID = profile.RightTriggerAxis;
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
            GenericGamepadProfile profile = GetProfile(gamepad);
            if(profile == null)
                return false;

            switch(button)
            {
            case GamepadButton.LeftStick:
                return GetButton(profile.LeftStickButton, (int)gamepad);
            case GamepadButton.RightStick:
                return GetButton(profile.RightStickButton, (int)gamepad);
            case GamepadButton.LeftBumper:
                return GetButton(profile.LeftBumperButton, (int)gamepad);
            case GamepadButton.RightBumper:
                return GetButton(profile.RightBumperButton, (int)gamepad);
            case GamepadButton.DPadUp:
                return profile.DPadType == GamepadDPadType.Button ?
                    GetButton(profile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Up == ButtonState.Pressed || m_dpadState[(int)gamepad].Up == ButtonState.JustPressed;
            case GamepadButton.DPadDown:
                return profile.DPadType == GamepadDPadType.Button ?
                    GetButton(profile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Down == ButtonState.Pressed || m_dpadState[(int)gamepad].Down == ButtonState.JustPressed;
            case GamepadButton.DPadLeft:
                return profile.DPadType == GamepadDPadType.Button ?
                    GetButton(profile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Left == ButtonState.Pressed || m_dpadState[(int)gamepad].Left == ButtonState.JustPressed;
            case GamepadButton.DPadRight:
                return profile.DPadType == GamepadDPadType.Button ?
                    GetButton(profile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Right == ButtonState.Pressed || m_dpadState[(int)gamepad].Right == ButtonState.JustPressed;
            case GamepadButton.Back:
                return GetButton(profile.BackButton, (int)gamepad);
            case GamepadButton.Start:
                return GetButton(profile.StartButton, (int)gamepad);
            case GamepadButton.ActionBottom:
                return GetButton(profile.ActionBottomButton, (int)gamepad);
            case GamepadButton.ActionRight:
                return GetButton(profile.ActionRightButton, (int)gamepad);
            case GamepadButton.ActionLeft:
                return GetButton(profile.ActionLeftButton, (int)gamepad);
            case GamepadButton.ActionTop:
                return GetButton(profile.ActionTopButton, (int)gamepad);
            default:
                return false;
            }
        }

        public bool GetButtonDown(GamepadButton button, GamepadIndex gamepad)
        {
            GenericGamepadProfile profile = GetProfile(gamepad);
            if(profile == null)
                return false;

            switch(button)
            {
            case GamepadButton.LeftStick:
                return GetButtonDown(profile.LeftStickButton, (int)gamepad);
            case GamepadButton.RightStick:
                return GetButtonDown(profile.RightStickButton, (int)gamepad);
            case GamepadButton.LeftBumper:
                return GetButtonDown(profile.LeftBumperButton, (int)gamepad);
            case GamepadButton.RightBumper:
                return GetButtonDown(profile.RightBumperButton, (int)gamepad);
            case GamepadButton.DPadUp:
                return profile.DPadType == GamepadDPadType.Button ?
                    GetButtonDown(profile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Up == ButtonState.JustPressed;
            case GamepadButton.DPadDown:
                return profile.DPadType == GamepadDPadType.Button ?
                    GetButtonDown(profile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Down == ButtonState.JustPressed;
            case GamepadButton.DPadLeft:
                return profile.DPadType == GamepadDPadType.Button ?
                    GetButtonDown(profile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Left == ButtonState.JustPressed;
            case GamepadButton.DPadRight:
                return profile.DPadType == GamepadDPadType.Button ?
                    GetButtonDown(profile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Right == ButtonState.JustPressed;
            case GamepadButton.Back:
                return GetButtonDown(profile.BackButton, (int)gamepad);
            case GamepadButton.Start:
                return GetButtonDown(profile.StartButton, (int)gamepad);
            case GamepadButton.ActionBottom:
                return GetButtonDown(profile.ActionBottomButton, (int)gamepad);
            case GamepadButton.ActionRight:
                return GetButtonDown(profile.ActionRightButton, (int)gamepad);
            case GamepadButton.ActionLeft:
                return GetButtonDown(profile.ActionLeftButton, (int)gamepad);
            case GamepadButton.ActionTop:
                return GetButtonDown(profile.ActionTopButton, (int)gamepad);
            default:
                return false;
            }
        }

        public bool GetButtonUp(GamepadButton button, GamepadIndex gamepad)
        {
            GenericGamepadProfile profile = GetProfile(gamepad);
            if(profile == null)
                return false;

            switch(button)
            {
            case GamepadButton.LeftStick:
                return GetButtonUp(profile.LeftStickButton, (int)gamepad);
            case GamepadButton.RightStick:
                return GetButtonUp(profile.RightStickButton, (int)gamepad);
            case GamepadButton.LeftBumper:
                return GetButtonUp(profile.LeftBumperButton, (int)gamepad);
            case GamepadButton.RightBumper:
                return GetButtonUp(profile.RightBumperButton, (int)gamepad);
            case GamepadButton.DPadUp:
                return profile.DPadType == GamepadDPadType.Button ?
                    GetButtonUp(profile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Up == ButtonState.JustReleased;
            case GamepadButton.DPadDown:
                return profile.DPadType == GamepadDPadType.Button ?
                    GetButtonUp(profile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Down == ButtonState.JustReleased;
            case GamepadButton.DPadLeft:
                return profile.DPadType == GamepadDPadType.Button ?
                    GetButtonUp(profile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Left == ButtonState.JustReleased;
            case GamepadButton.DPadRight:
                return profile.DPadType == GamepadDPadType.Button ?
                    GetButtonUp(profile.DPadUpButton, (int)gamepad) :
                    m_dpadState[(int)gamepad].Right == ButtonState.JustReleased;
            case GamepadButton.Back:
                return GetButtonUp(profile.BackButton, (int)gamepad);
            case GamepadButton.Start:
                return GetButtonUp(profile.StartButton, (int)gamepad);
            case GamepadButton.ActionBottom:
                return GetButtonUp(profile.ActionBottomButton, (int)gamepad);
            case GamepadButton.ActionRight:
                return GetButtonUp(profile.ActionRightButton, (int)gamepad);
            case GamepadButton.ActionLeft:
                return GetButtonUp(profile.ActionLeftButton, (int)gamepad);
            case GamepadButton.ActionTop:
                return GetButtonUp(profile.ActionTopButton, (int)gamepad);
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

        private GenericGamepadProfile GetProfile(int gamepad)
        {
            switch(gamepad)
            {
            case 0:
                return m_gamepadOne;
            case 1:
                return m_gamepadTwo;
            case 2:
                return m_gamepadThree;
            case 3:
                return m_gamepadFour;
            default:
                throw new System.ArgumentException(string.Format("Gamepad '{0}' is not valid.", gamepad), "gamepad");
            }
        }
    }
}