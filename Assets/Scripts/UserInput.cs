using UnityEngine;
using UnityEngine.InputSystem;

public class UserInput : MonoBehaviour
{
    private Player player;

    private Vector2 direction;

    private bool jump;
    private bool run;
    private bool sneak;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        // Reset variables
        direction = Vector2.zero;

        jump = false;
        run = false;
        sneak = false;

        // Get inputs
        GamepadInput();
        KeyboardInput();
        MouseInput();

        // Apply inputs
        player.Run(run);
        player.Sneak(sneak);

        player.Move(direction);

        if (jump)
        {
            player.Jump();
        }
    }

    void GamepadInput()
    {
        Gamepad gamepad = Gamepad.current;

        if (gamepad == null)
        {
            return;
        }

        bool startButton = gamepad.startButton.isPressed;

        // Movement
        direction = gamepad.leftStick.ReadValue();

        // Camera
        Vector2 rightStick = gamepad.rightStick.ReadValue();

        jump |= gamepad.aButton.isPressed;
        run |= gamepad.leftStickButton.isPressed;
        sneak |= gamepad.leftShoulder.isPressed;

        bool bButton = gamepad.bButton.isPressed;
        bool xButton = gamepad.xButton.isPressed;
        bool yButton = gamepad.yButton.isPressed;
    }

    void KeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return;
        }

        bool escape = keyboard.escapeKey.isPressed;

        // Movement
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

        jump |= keyboard.spaceKey.isPressed;
        run |= keyboard.leftShiftKey.isPressed;
        sneak |= keyboard.leftCtrlKey.isPressed;
    }

    void MouseInput()
    {

    }
}
