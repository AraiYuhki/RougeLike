using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.Controls;
using System.Linq;

public static class InputUtility
{
    public enum InputAction
    {
        Submit,
        Cancel,
        Up,
        Down,
        Right,
        Left,
        Menu,
        Wait,
        TurnMode,

        // ŽèŽD
        One,
        Two,
        Trhee,
        Four
    }

    public class InputDelegater
    {
        private ButtonControl[] targetButtons;
        public InputDelegater(params ButtonControl[] buttons) => targetButtons = buttons.Where(button => button != null).ToArray();
        public bool IsPressed() => targetButtons.Any(button => button.isPressed);
        public bool IsTriggerd() => targetButtons.Any(button => button.wasPressedThisFrame);
        public bool IsRelease() => targetButtons.Any(button => button.wasReleasedThisFrame);
    }

    public static InputDelegater Submit => new InputDelegater(Keyboard.current?.enterKey, DualShockGamepad.current?.crossButton, XInputController.current?.aButton);
    public static InputDelegater Cancel => new InputDelegater(Keyboard.current?.tabKey, DualShockGamepad.current?.circleButton, XInputController.current?.bButton);
    public static InputDelegater Up => new InputDelegater(Keyboard.current?.wKey, Keyboard.current?.upArrowKey, DualShockGamepad.current?.dpad.up, XInputController.current?.dpad?.up);
    public static InputDelegater Down => new InputDelegater(Keyboard.current?.sKey, Keyboard.current?.downArrowKey, DualShockGamepad.current?.dpad.down, XInputController.current?.dpad?.down);
    public static InputDelegater Right => new InputDelegater(Keyboard.current?.dKey, Keyboard.current?.rightArrowKey, DualShockGamepad.current?.dpad.right, XInputController.current?.dpad?.right);
    public static InputDelegater Left => new InputDelegater(Keyboard.current?.aKey, Keyboard.current?.leftArrowKey, DualShockGamepad.current?.dpad.left, XInputController.current?.dpad.left);
    public static InputDelegater Menu => new InputDelegater(Keyboard.current?.escapeKey, DualShockGamepad.current?.triangleButton, XInputController.current?.yButton);
    public static InputDelegater Wait => new InputDelegater(Keyboard.current?.spaceKey, DualShockGamepad.current?.squareButton, XInputController.current?.xButton);
    public static InputDelegater TurnMode => new InputDelegater(Keyboard.current?.leftShiftKey, Keyboard.current?.rightShiftKey, DualShockGamepad.current?.rightShoulder, XInputController.current?.rightShoulder);

    public static InputDelegater One => new InputDelegater(Keyboard.current?.digit1Key);
    public static InputDelegater Two => new InputDelegater(Keyboard.current?.digit2Key);
    public static InputDelegater Three => new InputDelegater(Keyboard.current?.digit3Key);
    public static InputDelegater Four => new InputDelegater(Keyboard.current?.digit4Key);
}
