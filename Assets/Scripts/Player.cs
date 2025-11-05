using UnityEngine;

public class Player : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private bool grounded;
    private bool run;
    private bool sneak;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    bool isGrounded()
    {
        return grounded;
    }

    bool isRunning()
    {
        return run;
    }

    bool isSneaking()
    {
        return sneak;
    }

    void Jump()
    {

    }

    void Run(bool value)
    {
        run = value;
    }

    void Sneak(bool value)
    {
        sneak = value;
    }
}
