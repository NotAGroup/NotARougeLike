using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Start user input");
    }

    // Update is called once per frame
    void Update()
    {
        GamepadInput();
        KeyboardInput();
        MouseInput();
    }

    void GamepadInput()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad == null)
        {
            return;
        }

        Vector2 leftStick = gamepad.leftStick.ReadValue();
        Vector2 rightStick = gamepad.rightStick.ReadValue();

        bool aButton = gamepad.aButton.isPressed;
        bool bButton = gamepad.bButton.isPressed;
        bool xButton = gamepad.xButton.isPressed;
        bool yButton = gamepad.yButton.isPressed;

        bool startButton = gamepad.startButton.isPressed;
    }

    void KeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        bool lShift = keyboard.leftShiftKey.isPressed;
        bool lCtrl = keyboard.leftCtrlKey.isPressed;
        bool escape = keyboard.escapeKey.isPressed;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            Debug.Log("S | Downd");
        }
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            Debug.Log("A | Left");
        }
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            Debug.Log("D | Right");
        }
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            Debug.Log("W | Up");
        }
    }

    void MouseInput()
    {

    }
}
