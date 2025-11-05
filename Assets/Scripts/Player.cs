using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private CharacterController characterController;

    private float gravity = -9.81f;

    private bool grounded;
    private bool run;
    private bool sneak;

    private Vector3 motion;

    // Movement
    public float runSpeed = 10.0f;
    public float walkSpeed = 5.0f;
    public float sneakSpeed = 2.5f;

    // Jump
    public float jumpHeight = 1.5f;

    // Stamina
    public float maxStamina = 100.0f;
    public float currentStamina;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        currentStamina = maxStamina;
    }

    // Update is called once per frame
    void Update()
    {
        grounded = characterController.isGrounded;

        if (!grounded)
        {
            motion.y += gravity * Time.deltaTime;
        }
        else
        {
            motion.y = 0.0f;
        }

        characterController.Move(motion * Time.deltaTime);
    }

    public bool isGrounded()
    {
        return grounded;
    }

    public bool isRunning()
    {
        return run;
    }

    public bool isSneaking()
    {
        return sneak;
    }

    public void Jump()
    {
        if (grounded && currentStamina > 0)
        {
            motion.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void Move(Vector2 direction)
    {
        Vector3 movement = transform.right * direction.x + transform.forward * direction.y;
        movement.Normalize();

        float speed = walkSpeed;

        if (run && currentStamina > 0)
        {
            speed = runSpeed;
        }
        else if (sneak)
        {
            speed = sneakSpeed;
        }

        motion.x = movement.x * speed;
        motion.z = movement.z * speed;
    }

    public void Run(bool value)
    {
        if (value)
        {
            // Disable sneak when running
            sneak = false;
        }

        run = value;
    }

    public void Sneak(bool value)
    {
        if (value)
        {
            // Disable run when sneaking
            run = false;
        }

        sneak = value;
    }
}
