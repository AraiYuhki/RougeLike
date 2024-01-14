using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;
using UnityEngine.InputSystem.Controls;
using System.Linq;
using Cysharp.Threading.Tasks;

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
        DiagonalMode,

        // 手札
        One,
        Two,
        Trhee,
        Four
    }

    public class InputDelegater
    {
        private ButtonControl[] targetButtons;
        private bool isPressed;
        private bool isTrigger;
        private bool isRelease;
        private bool isRepeat;
        private float pressedTime = 0f;

        private const float RepeatStartTime = 0.2f;

        public InputDelegater(params ButtonControl[] buttons) => targetButtons = buttons.Where(button => button != null).ToArray();
        public bool IsPressed() => isPressed;
        public bool IsTrigger() => isTrigger;
        public bool IsRelease() => isRelease;
        public bool IsRepeat() => isRepeat;

        public void CheckState()
        {
            isPressed = targetButtons.Any(button => button.isPressed);
            isTrigger = targetButtons.Any(button => button.wasPressedThisFrame);
            isRelease = targetButtons.Any(button => button.wasReleasedThisFrame);
            isRepeat = isTrigger || pressedTime >= RepeatStartTime;
            if (isPressed)
                pressedTime += Time.deltaTime;
            else
                pressedTime = 0f;
        }
    }

    public static InputDelegater Submit { get; private set; } = new InputDelegater(Keyboard.current?.enterKey, DualShockGamepad.current?.crossButton, XInputController.current?.aButton);
    public static InputDelegater Cancel { get; private set; } = new InputDelegater(Keyboard.current?.tabKey, DualShockGamepad.current?.circleButton, XInputController.current?.bButton);
    public static InputDelegater Up { get; private set; } = new InputDelegater(Keyboard.current?.wKey, Keyboard.current?.upArrowKey, DualShockGamepad.current?.dpad.up, XInputController.current?.dpad?.up);
    public static InputDelegater Down { get; private set; } = new InputDelegater(Keyboard.current?.sKey, Keyboard.current?.downArrowKey, DualShockGamepad.current?.dpad.down, XInputController.current?.dpad?.down);
    public static InputDelegater Right { get; private set; } = new InputDelegater(Keyboard.current?.dKey, Keyboard.current?.rightArrowKey, DualShockGamepad.current?.dpad.right, XInputController.current?.dpad?.right);
    public static InputDelegater Left { get; private set; } = new InputDelegater(Keyboard.current?.aKey, Keyboard.current?.leftArrowKey, DualShockGamepad.current?.dpad.left, XInputController.current?.dpad.left);
    public static InputDelegater Menu { get; private set; } = new InputDelegater(Keyboard.current?.escapeKey, DualShockGamepad.current?.triangleButton, XInputController.current?.yButton);
    public static InputDelegater Wait { get; private set; } = new InputDelegater(Keyboard.current?.spaceKey, DualShockGamepad.current?.squareButton, XInputController.current?.xButton);
    public static InputDelegater TurnMode { get; private set; } = new InputDelegater(Keyboard.current?.leftShiftKey, Keyboard.current?.rightShiftKey, DualShockGamepad.current?.rightShoulder, XInputController.current?.rightShoulder);
    public static InputDelegater DiagonalMode { get; private set; } = new InputDelegater(Keyboard.current?.leftCtrlKey, Keyboard.current?.rightCtrlKey, DualShockGamepad.current?.leftShoulder, XInputController.current?.leftShoulder);
    public static InputDelegater RightTrigger { get; private set; } = new InputDelegater(Keyboard.current?.eKey, DualShockGamepad.current?.rightShoulder, XInputController.current?.rightShoulder);
    public static InputDelegater LeftTrigger { get; private set; } = new InputDelegater(Keyboard.current?.qKey, DualShockGamepad.current?.leftShoulder, XInputController.current?.leftShoulder);

    public static InputDelegater One { get; private set; } = new InputDelegater(Keyboard.current?.digit1Key);
    public static InputDelegater Two { get; private set; } = new InputDelegater(Keyboard.current?.digit2Key);
    public static InputDelegater Three { get; private set; } = new InputDelegater(Keyboard.current?.digit3Key);
    public static InputDelegater Four { get; private set; } = new InputDelegater(Keyboard.current?.digit4Key);

    private static readonly InputDelegater[] inputs = new InputDelegater[]
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
        DiagonalMode,
        RightTrigger,
        LeftTrigger,
        One,
        Two,
        Three,
        Four
    };

    static InputUtility()
    {
        UniTask.Void(async token =>
        {
            while (Application.isPlaying)
            {
                foreach (var input in inputs) input.CheckState();
                await UniTask.Yield(cancellationToken: token);
            }
        }, cancellationToken: default);
    }
}
