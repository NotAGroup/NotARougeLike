using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    private Player player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
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

        player.Run(keyboard.leftShiftKey.isPressed);
        player.Sneak(keyboard.leftCtrlKey.isPressed);
        bool escape = keyboard.escapeKey.isPressed;

        // Movement
        Vector2 direction = Vector2.zero;

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            direction.y -= 1;
        }
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            direction.x -= 1;
        }
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            direction.x += 1;
        }
        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            direction.y += 1;
        }

        player.Move(direction);

        // Jump
        if (keyboard.spaceKey.isPressed)
        {
            player.Jump();
        }
    }

    void MouseInput()
    {

    }
}
