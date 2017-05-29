using System;
using System.Collections.Generic;
using XInputDotNetPure;
using UnityEngine;

namespace BInput
{
    /// <summary>
    /// New input system
    /// </summary>
    public class BInput
    {
        public static int InputCount = 5; //The amount of input devices supported (kb + 4 controllers)

        public enum InputButtons
        {
            A,
            B,
            Y,
            X,
            RT,
            RB,
            LT,
            LB,
            Start,
            Back,
            LS,
            RS,
        }

        public enum Axis
        {
            LeftStick,
            RightStick,
            Dpad,
        }

        public enum Index { Any, One, Two, Three, Four, Kbm }

        private static readonly Dictionary<InputButtons, InputManager.Button> Buttons = new Dictionary<InputButtons, InputManager.Button>
        {
            {InputButtons.A, InputManager.Button.A},
            {InputButtons.B, InputManager.Button.B},
            {InputButtons.Y, InputManager.Button.Y},
            {InputButtons.X, InputManager.Button.X},
            {InputButtons.RB, InputManager.Button.Rb},
            {InputButtons.LB, InputManager.Button.Lb},
            {InputButtons.Start, InputManager.Button.Start},
            {InputButtons.Back, InputManager.Button.Back},
            {InputButtons.LS, InputManager.Button.Ls},
            {InputButtons.RS, InputManager.Button.Rs},
            {InputButtons.RT, InputManager.Button.Rt},
            {InputButtons.LT, InputManager.Button.Lt},
        };

        /// <summary>
        /// A leftover of my game tribot started out without this input system, to correctly emulate and replace the old way of input the +1 on index was needed
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Index IntToIndex(int index)
        {
            return (Index)index + 1;
        }

        /// <summary>
        /// Returns true if the given button on the given input device is pressed
        /// </summary>
        /// <param name="button"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool GetButton(InputButtons button, int index)
        {
            var gIndex = IntToIndex(index);

            bool result = false;
            if (gIndex == Index.Any)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (XInput.GetButton(Buttons[button], i))
                        result = true;
                }
                if (KbInput.GetButton(Buttons[button]))
                    result = true;
            }
            else if (gIndex == Index.Kbm)
            {
                result = KbInput.GetButton(Buttons[button]);
            }
            else
            {
                result = XInput.GetButton(Buttons[button], index);
            }

            return result;
        }

        /// <summary>
        /// Returns true if the given button on the given input device is pressed this frame
        /// </summary>
        /// <param name="button"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool GetButtonDown(InputButtons button, int index)
        {
            var gIndex = IntToIndex(index);

            bool result = false;
            if (gIndex == Index.Any)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (XInput.GetButtonDown(Buttons[button], i))
                        result = true;
                }
                if (KbInput.GetButtonDown(Buttons[button]))
                    result = true;
            }
            else if (gIndex == Index.Kbm)
            {
                result = KbInput.GetButtonDown(Buttons[button]);
            }
            else
            {
                result = XInput.GetButtonDown(Buttons[button], index);
            }

            return result;
        }

        /// <summary>
        /// Returns true if the given button on the given input device is released this frame
        /// </summary>
        /// <param name="button"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool GetButtonUp(InputButtons button, int index)
        {
            var gIndex = IntToIndex(index);

            bool result = false;
            if (gIndex == Index.Any)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (XInput.GetButtonUp(Buttons[button], i))
                        result = true;
                }
                if (KbInput.GetButtonUp(Buttons[button]))
                    result = true;
            }
            else if (gIndex == Index.Kbm)
            {
                result = KbInput.GetButtonUp(Buttons[button]);
            }
            else
            {
                result = XInput.GetButtonUp(Buttons[button], index);
            }

            return result;
        }

        /// <summary>
        /// Get axis state
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static Vector2 GetAxis(Axis axis, int index)
        {
            var axes = GetAxisFromMap(axis);
            var gIndex = IntToIndex(index);

            Vector2 result = new Vector2(0, 0);

            if (gIndex == Index.Any)
            {
                for (int i = 0; i < 4; i++)
                {
                    result.x += XInput.GetAxis(axes[0], i);
                    result.y += XInput.GetAxis(axes[1], i);
                }

                result.x += KbInput.GetAxis(axes[0]);
                result.y += KbInput.GetAxis(axes[1]);
            }
            else if (gIndex == Index.Kbm)
            {
                result.x = KbInput.GetAxis(axes[0]);
                result.y = KbInput.GetAxis(axes[1]);
            }
            else
            {
                result.x = XInput.GetAxis(axes[0], index);
                result.y = XInput.GetAxis(axes[1], index);
            }

            return result;
        }

        /// <summary>
        /// Get input manager axis from the Axis enum
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        private static InputManager.Axis[] GetAxisFromMap(Axis axis)
        {
            InputManager.Axis[] axes = new InputManager.Axis[2];

            switch (axis)
            {
                case Axis.Dpad:
                    axes[0] = InputManager.Axis.DpadHorizontal;
                    axes[1] = InputManager.Axis.DpadVertical;
                    break;
                case Axis.LeftStick:
                    axes[0] = InputManager.Axis.LsHorizontal;
                    axes[1] = InputManager.Axis.LsVertical;
                    break;
                case Axis.RightStick:
                    axes[0] = InputManager.Axis.RsHorizontal;
                    axes[1] = InputManager.Axis.RsVertical;
                    break;
            }

            return axes;
        }
    }

    /// <summary>
    /// Base class of different input managers to share the enums
    /// </summary>
    public abstract class InputManager
    {
        public enum Button
        {
            A,
            B,
            Y,
            X,
            Rb,
            Lb,
            Rs,
            Ls,
            Back,
            Start,
            Rt,
            Lt
        }

        public enum Axis
        {
            LsHorizontal,
            LsVertical,
            RsHorizontal,
            RsVertical,
            DpadHorizontal,
            DpadVertical,
            Triggers,
        }
    }

    /// <summary>
    /// Handles keyboard input
    /// </summary>
    public class KbInput : InputManager
    {

        /// <summary>
        /// If given button is pressed returns true
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool GetButton(Button button)
        {

            return Input.GetKey(GetButtonCode(button));
        }

        /// <summary>
        /// Returns true if given button is pressed down this frame
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool GetButtonDown(Button button)
        {

            return Input.GetKeyDown(GetButtonCode(button));
        }

        /// <summary>
        /// Returns true if button is released this frame
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool GetButtonUp(Button button)
        {

            return Input.GetKeyUp(GetButtonCode(button));
        }

        /// <summary>
        /// Maps button to keycodes
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        private static KeyCode GetButtonCode(Button button)
        {
            switch (button)
            {
                case Button.A: return KeyCode.Keypad2;
                case Button.B: return KeyCode.Keypad6;
                case Button.X: return KeyCode.Keypad4;
                case Button.Y: return KeyCode.Keypad8;
                case Button.Rb: return KeyCode.Space;
                case Button.Lb: return KeyCode.LeftShift;
                case Button.Back: return KeyCode.Backspace;
                case Button.Start: return KeyCode.Return;
                case Button.Ls: return KeyCode.Q;
                case Button.Rs: return KeyCode.E;
                case Button.Rt: return KeyCode.LeftAlt;
                case Button.Lt: return KeyCode.LeftControl;
            }

            throw new Exception("No valid button state found");
        }

        /// <summary>
        /// Returns a float value between -1 and 1 representing the axis state
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        public static float GetAxis(Axis axis)
        {
            float result = 0;
            var codes = GetAxisCodes(axis);
            if (Input.GetKey(codes[0]))
                result -= 1;
            if (Input.GetKey(codes[1]))
                result += 1;

            return result;
        }

        /// <summary>
        /// Maps Axis to keycodes
        /// </summary>
        /// <param name="axis"></param>
        /// <returns></returns>
        private static KeyCode[] GetAxisCodes(Axis axis)
        {
            KeyCode[] codes = new KeyCode[2];
            switch (axis)
            {
                case Axis.LsHorizontal:
                    codes[0] = KeyCode.A;
                    codes[1] = KeyCode.D;
                    break;
                case Axis.LsVertical:
                    codes[0] = KeyCode.S;
                    codes[1] = KeyCode.W;
                    break;
                case Axis.RsHorizontal:
                    codes[0] = KeyCode.LeftArrow;
                    codes[1] = KeyCode.RightArrow;
                    break;
                case Axis.RsVertical:
                    codes[0] = KeyCode.DownArrow;
                    codes[1] = KeyCode.UpArrow;
                    break;
                case Axis.DpadHorizontal:
                    codes[0] = KeyCode.Keypad4;
                    codes[1] = KeyCode.Keypad6;
                    break;
                case Axis.DpadVertical:
                    codes[0] = KeyCode.Keypad2;
                    codes[1] = KeyCode.Keypad8;
                    break;
                case Axis.Triggers:
                    codes[0] = KeyCode.LeftControl;
                    codes[1] = KeyCode.LeftAlt;
                    break;
            }

            return codes;
        }
    }

    /// <summary>
    /// Handles gamepad using the XInputDotNet Library https://github.com/speps/XInputDotNet
    /// </summary>
    public class XInput : InputManager
    {
        /// <summary>
        /// The instance holder
        /// </summary>
        private static XInput _instance;
        /// <summary>
        /// The 4 gamepad states
        /// </summary>
        private State[] _states;

        public XInput()
        {
            ///Create a Unity game object that updates the input states every frame
            var obj = new GameObject();
            obj.name = "InputUpdater";
            GameObject.DontDestroyOnLoad(obj);
            obj.AddComponent<InputUpdater>().UpdateCallback = Update;

            _states = new State[4];

            for (int i = 0; i < _states.Length; i++)
            {
                var state = new State();
                state.Index = (PlayerIndex)i;
                state.PrevState = state.CurrentState = GamePad.GetState(state.Index);
                _states[i] = state;
            }
        }

        /// <summary>
        /// Returns a float value between -1 and 1 representing the axis state
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="padIndex"></param>
        /// <returns></returns>
        public static float GetAxis(Axis axis, int padIndex)
        {
            var instance = GetInstance();
            var state = instance._states[padIndex];

            if (!state.CurrentState.IsConnected)
                return 0;

            float result = 0;
            switch (axis)
            {
                case Axis.LsHorizontal:
                    result = state.CurrentState.ThumbSticks.Left.X;
                    break;
                case Axis.LsVertical:
                    result = state.CurrentState.ThumbSticks.Left.Y;
                    break;
                case Axis.RsHorizontal:
                    result = state.CurrentState.ThumbSticks.Right.X;
                    break;
                case Axis.RsVertical:
                    result = state.CurrentState.ThumbSticks.Right.Y;
                    break;
                case Axis.DpadHorizontal:
                    if (state.CurrentState.DPad.Left == ButtonState.Pressed)
                        result -= 1;
                    if (state.CurrentState.DPad.Right == ButtonState.Pressed)
                        result += 1;
                    break;
                case Axis.DpadVertical:
                    if (state.CurrentState.DPad.Down == ButtonState.Pressed)
                        result -= 1;
                    if (state.CurrentState.DPad.Up == ButtonState.Pressed)
                        result += 1;
                    break;
                case Axis.Triggers:
                    result += state.CurrentState.Triggers.Right;
                    result -= state.CurrentState.Triggers.Left;
                    break;
            }

            return result;
        }

        /// <summary>
        /// Returns true if given button is released this frame
        /// </summary>
        /// <param name="button"></param>
        /// <param name="padIndex"></param>
        /// <returns></returns>
        public static bool GetButtonUp(Button button, int padIndex)
        {
            var instance = GetInstance();
            var state = instance._states[padIndex];

            if (!state.CurrentState.IsConnected || !state.PrevState.IsConnected)
                return false;

            bool result = false;
            if (button != Button.Lt && button != Button.Rt)
            {
                //Default button behaviour
                ButtonState currentButtonState = GetButtonState(button, state.CurrentState);
                ButtonState previousButtonState = GetButtonState(button, state.PrevState);
                result = previousButtonState == ButtonState.Pressed && currentButtonState == ButtonState.Released;
            }
            else
            {
                //Emulate triggers as button
                float currentState = GetTriggerState(button, state.CurrentState);
                float previousButtonState = GetTriggerState(button, state.PrevState);
                result = previousButtonState > .3f && currentState < .3f;
            }

            return result;
        }

        /// <summary>
        /// Returns true if given button is pressed this frame
        /// </summary>
        /// <param name="button"></param>
        /// <param name="padIndex"></param>
        /// <returns></returns>
        public static bool GetButtonDown(Button button, int padIndex)
        {
            var instance = GetInstance();
            var state = instance._states[padIndex];

            if (!state.CurrentState.IsConnected && !state.PrevState.IsConnected)
                return false;

            bool result = false;
            if (button != Button.Lt && button != Button.Rt)
            {
                ButtonState currentButtonState = GetButtonState(button, state.CurrentState);
                ButtonState previousButtonState = GetButtonState(button, state.PrevState);
                result = previousButtonState == ButtonState.Released && currentButtonState == ButtonState.Pressed;
            }
            else
            {
                //Emulate triggers as button
                float currentState = GetTriggerState(button, state.CurrentState);
                float previousButtonState = GetTriggerState(button, state.PrevState);
                result = previousButtonState < .3f && currentState > .3f;
            }

            return result;
        }

        /// <summary>
        /// Returns true if given button is pressed
        /// </summary>
        /// <param name="button"></param>
        /// <param name="padIndex"></param>
        /// <returns></returns>
        public static bool GetButton(Button button, int padIndex)
        {
            var instance = GetInstance();
            var state = instance._states[padIndex];

            if (!state.CurrentState.IsConnected)
                return false;
            bool result = false;
            if (button != Button.Lt && button != Button.Rt)
            {
                ButtonState currentButtonState = GetButtonState(button, state.CurrentState);
                result = currentButtonState == ButtonState.Pressed;
            }
            else
            {
                float currentState = GetTriggerState(button, state.CurrentState);
                result = currentState > .3f;
            }

            return result;
        }

        /// <summary>
        /// Maps Button to button state
        /// </summary>
        /// <param name="button"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private static ButtonState GetButtonState(Button button, GamePadState state)
        {
            switch (button)
            {
                case Button.A: return state.Buttons.A;
                case Button.B: return state.Buttons.B;
                case Button.X: return state.Buttons.X;
                case Button.Y: return state.Buttons.Y;
                case Button.Rb: return state.Buttons.RightShoulder;
                case Button.Lb: return state.Buttons.LeftShoulder;
                case Button.Back: return state.Buttons.Back;
                case Button.Start: return state.Buttons.Start;
                case Button.Ls: return state.Buttons.LeftStick;
                case Button.Rs: return state.Buttons.RightStick;
            }
            throw new Exception("No valid button state found");
        }

        /// <summary>
        /// Maps Button to trigger state
        /// </summary>
        /// <param name="button"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private static float GetTriggerState(Button button, GamePadState state)
        {
            switch (button)
            {
                case Button.Rt:
                    return state.Triggers.Right;
                case Button.Lt:
                    return state.Triggers.Left;
            }

            return 0;
        }

        /// <summary>
        /// Retrieve instance with only this method
        /// </summary>
        /// <returns></returns>
        private static XInput GetInstance()
        {
            if (_instance == null)
            {
                _instance = new XInput();
            }

            return _instance;
        }

        /// <summary>
        /// Updates previous and current state
        /// </summary>
        void Update()
        {
            foreach (var state in _states)
            {
                state.PrevState = state.CurrentState;
                state.CurrentState = GamePad.GetState(state.Index);
            }
        }

        /// <summary>
        /// Data container for previous current state for each pad
        /// </summary>
        class State
        {
            public PlayerIndex Index;
            public GamePadState PrevState;
            public GamePadState CurrentState;
        }
    }

    /// <summary>
    /// Calls Updatecallback delegate each frame
    /// </summary>
    public class InputUpdater : MonoBehaviour
    {
        public Action UpdateCallback { private get; set; }

        void Update()
        {
            if (UpdateCallback != null)
                UpdateCallback();
        }
    }
}
