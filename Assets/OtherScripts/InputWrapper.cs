using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class InputWrapper
{
    private static Dictionary<InputButtons, string> _buttonMap = new Dictionary<InputButtons, string>
    {
        { InputButtons.Rb, "Rb"},
        { InputButtons.Lb, "Lb"},
        { InputButtons.A, "A"},
        { InputButtons.B, "B"},
        { InputButtons.Y, "Y"},
        { InputButtons.X, "X"},
    };

    private static Dictionary<InputAxis, string> _axisMap = new Dictionary<InputAxis, string>
    {
        { InputAxis.LHorizontal, "Horizontal"},
        { InputAxis.RHorizontal, "RHorizontal"},
        { InputAxis.LVertical, "Vertical"},
        { InputAxis.RVertical, "RVertical"},
        { InputAxis.Rt, "Rt"},
        { InputAxis.Lt, "Lt"},
    };

    public static float GetAxis(InputAxis axis, bool raw = false)
    {
        return raw ? Input.GetAxisRaw(_axisMap[axis]) : Input.GetAxis(_axisMap[axis]);
    }

    public static bool GetButtonUp(InputButtons button)
    {
        return Input.GetButtonUp(_buttonMap[button]);
    }

    public static bool GetButtonDown(InputButtons button)
    {
        return Input.GetButtonDown(_buttonMap[button]);
    }

    public static bool GetButton(InputButtons button)
    {
        return Input.GetButton(_buttonMap[button]);
    }
}

public enum InputButtons
{
    Rb,
    Lb,
    A,
    B,
    Y,
    X,
}

public enum InputAxis
{
    LHorizontal,
    LVertical,
    RHorizontal,
    RVertical,
    Rt,
    Lt,
}
