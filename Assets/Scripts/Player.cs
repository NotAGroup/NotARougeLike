using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    // Components
    private Transform cameraTransform;
    private CharacterController characterController;
    private InteractionArea interactArea;
    private Camera playerCamera;

    private Transform leftShoulderTransform;
    private Transform rightShoulderTransform;

    // Enumerations
    public enum AttackType
    {
        Hit,
        Shoot
    }

    enum HitState
    {
        Idle,
        Return,
        Swing,
        Strike
    }

    private AttackType attackType;
    private HitState hitState;

    private float gravity = -9.81f;

    private bool grounded;
    private bool run;
    private bool slide;
    private bool sneak;

    private Vector3 motion;
    private Vector3 scale;

    private float currentFOV;
    private float currentHealth;
    private float currentMana;
    private float currentStamina;

    private int healthFruits = 0;
    private int manaFruits = 0;
    private int staminaFruits = 0;

    private float defaultYScale;
    private float fireCooldown;

    private float hitCooldown;
    private float hitTime;

    private Vector3 restRotation = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 swingRotation = new Vector3(-130.0f, 0.0f, 0.0f);
    private Vector3 strikeRotation = new Vector3(-30.0f, 0.0f, 0.0f);

    private Quaternion startRotation;
    private Quaternion endRotation;

    private float slideCooldown;
    private float slideTime;

    [Header("Camera")]
    public float normalFOV = 60.0f;
    public float aimFOV = 30.0f;
    public float zoomSpeed = 10.0f;

    [Header("Combat")]
    public Projectile[] projectiles;
    public Weapon weapon;
    public float fireRate = 3.0f;
    public float hitRate = 2.0f;
    public float swingDuration = 0.4f;
    public float strikeDuration = 0.2f;
    public float returnDuration = 0.1f;

    [Header("Mana")]
    public float maxMana = 100.0f;
    public float manaRegRate = 3.0f;

    [Header("Movement")]
    public float runSpeed = 10.0f;
    public float walkSpeed = 5.0f;
    public float sneakSpeed = 2.5f;

    [Header("Sliding")]
    public float maxSlideTime = 3.0f;
    public float slideSpeed = 12.0f;

    [Header("Health")]
    public float maxHealth = 100.0f;
    public float healthRegRate = 2.0f;

    [Header("Jump")]
    public float jumpHeight = 1.5f;

    [Header("Stamina")]
    public float maxStamina = 100.0f;
    public float staminaRegRate = 5.0f;
    public float runConsRate = 10.0f;
    public float slideConsRate = 20.0f;
    public float jumpConsRate = 15.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject mainCamera = GameObject.Find("Main Camera");

        playerCamera = mainCamera.GetComponent<Camera>();
        cameraTransform = mainCamera.GetComponent<Transform>();
        characterController = GetComponent<CharacterController>();
        interactArea = GameObject.Find("Interaction Area").GetComponent<InteractionArea>();

        leftShoulderTransform = GameObject.Find("Left Shoulder").GetComponent<Transform>();
        rightShoulderTransform = GameObject.Find("Right Shoulder").GetComponent<Transform>();

        attackType = AttackType.Shoot;
        hitState = HitState.Idle;

        currentHealth = maxHealth;
        currentMana = maxMana;
        currentStamina = maxStamina;

        fireCooldown = 0.0f;
        hitCooldown = 0.0f;
        hitTime = 0.0f;

        slideCooldown = 0.0f;
        slideTime = 0.0f;

        defaultYScale = transform.localScale.y;

        weapon.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        grounded = characterController.isGrounded;
        scale = transform.localScale;

        scale.y = slide ? defaultYScale * 0.5f : sneak ? defaultYScale * 0.75f : defaultYScale;
        transform.localScale = scale;

        if (!grounded)
        {
            motion.y += gravity * Time.deltaTime;
        }
        else
        {
            motion.y = 0.0f;
        }

        if (fireCooldown > 0.0f)
        {
            fireCooldown -= Time.deltaTime;
            if (fireCooldown < 0.0f)
            {
                fireCooldown = 0.0f;
            }
        }

        if (hitCooldown > 0.0f)
        {
            hitCooldown -= Time.deltaTime;
            if (hitCooldown < 0.0f)
            {
                hitCooldown = 0.0f;
            }
        }

        if (hitState != HitState.Idle)
        {
            hitTime += Time.deltaTime;

            float duration = hitState == HitState.Swing ? swingDuration :
                             hitState == HitState.Strike ? strikeDuration : returnDuration;

            float ratio = Mathf.Clamp(hitTime / duration, 0.0f, 1.0f);

            rightShoulderTransform.localRotation = Quaternion.Slerp(startRotation, endRotation, ratio);

            if (ratio >= 1.0f)
            {
                switch (hitState)
                {
                    case HitState.Swing:
                        hitState = HitState.Strike;
                        hitTime = 0.0f;

                        startRotation = Quaternion.Euler(swingRotation);
                        endRotation = Quaternion.Euler(strikeRotation);
                        break;
                    case HitState.Strike:
                        hitState = HitState.Return;
                        hitTime = 0.0f;

                        startRotation = Quaternion.Euler(strikeRotation);
                        endRotation = Quaternion.Euler(restRotation);
                        break;
                    case HitState.Return:
                        hitState = HitState.Idle;
                        hitCooldown = 1.0f / hitRate;

                        weapon.gameObject.SetActive(false);
                        break;
                    default:
                        Debug.LogError("Unknown Hit State: " + hitState);
                        return;
                }
            }
        }

        if (slide)
        {
            slideTime -= Time.deltaTime;

            if (slideTime <= 0.0f)
            {
                // End slide
                slide = false;
                slideCooldown = 1.0f;
                slideTime = 0.0f;
            }
        }
        else if (slideCooldown > 0.0f)
        {
            slideCooldown -= Time.deltaTime;

            if (slideCooldown < 0.0f)
            {
                slideCooldown = 0.0f;
            }
        }

        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, currentFOV, zoomSpeed * Time.deltaTime);

        characterController.Move(motion * Time.deltaTime);

        RegenerateHealth();
        RegenerateMana();
        RegenrerateStamina();
    }

    public void Aim(bool value)
    {
        if (value)
        {
            currentFOV = aimFOV;
        }
        else
        {
            currentFOV = normalFOV;
        }
    }

    public void Attack(AttackType? type = null)
    {
        // Use current attack type if no type is specified
        attackType = type ?? attackType;

        switch (attackType)
        {
            case AttackType.Hit:
                Hit();
                break;
            case AttackType.Shoot:
                Shoot();
                break;
            default:
                Debug.LogError("Unknown Attack Type: " + attackType);
                break;
        }
    }

    public void ChangeAttack()
    {
        if (attackType == AttackType.Hit)
        {
            attackType = AttackType.Shoot;
        }
        else
        {
            attackType = AttackType.Hit;
        }
    }

    private void Die()
    {
        // TODO
        Debug.Log("Die");
    }

    public float GetHealth()
    {
        return currentHealth;
    }

    public float GetMana()
    {
        return currentMana;
    }

    public float GetStamina()
    {
        return currentStamina;
    }

    void Hit()
    {
        if (hitCooldown > 0.0f || hitState != HitState.Idle)
        {
            return;
        }

        weapon.gameObject.SetActive(true);
        weapon.SetDamage(20.0f);

        hitState = HitState.Swing;
        hitTime = 0.0f;

        startRotation = rightShoulderTransform.localRotation;
        endRotation = Quaternion.Euler(swingRotation);
    }

    public void Interact()
    {
        if (interactArea.triggerObject != null)
        {
            string name = interactArea.triggerObject.name;

            switch (name)
            {
                case "health_fruit":
                    healthFruits += 1;
                    Destroy(interactArea.triggerObject);
                    break;
                    case "mana_fruit":
                    manaFruits += 1;
                    Destroy(interactArea.triggerObject);
                    break;
                    case "stamina_fruit":
                    staminaFruits += 1;
                    Destroy(interactArea.triggerObject);
                    break;
                    default:
                    break;
            }
        }
    }

    public bool isGrounded()
    {
        return grounded;
    }

    public bool isRunning()
    {
        return run;
    }

    public bool isSliding()
    {
        return slide;
    }

    public bool isSneaking()
    {
        return sneak;
    }

    public void Jump()
    {
        if (grounded && currentStamina >= jumpConsRate)
        {
            currentStamina -= jumpConsRate;
            motion.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void Move(Vector2 direction)
    {
        Vector3 movement = transform.right * direction.x + transform.forward * direction.y;
        movement.Normalize();

        float speed = walkSpeed;

        if (run && currentStamina >= runConsRate)
        {
            currentStamina -= runConsRate * Time.deltaTime;
            speed = runSpeed;
        }
        else if (sneak)
        {
            speed = sneakSpeed;
        }

        if (slide)
        {
            speed += slideSpeed * (slideTime / maxSlideTime);
        }

        motion.x = movement.x * speed;
        motion.z = movement.z * speed;

        float ratio = Time.deltaTime * 5.0f;

        if (hitState == HitState.Idle && (direction.x != 0.0f || direction.y != 0.0f) && !slide)
        {
            float amplitude = 2.0f * speed;
            float frequency = 5.0f;

            float wave = Mathf.Sin(Time.time * frequency) * amplitude;

            leftShoulderTransform.localRotation = Quaternion.Slerp(leftShoulderTransform.localRotation,
                Quaternion.Euler(wave, 0.0f, 0.0f), ratio);
            rightShoulderTransform.localRotation = Quaternion.Slerp(rightShoulderTransform.localRotation,
                Quaternion.Euler(-wave, 0.0f, 0.0f), ratio);
        }
        else
        {
            leftShoulderTransform.localRotation = Quaternion.Slerp(leftShoulderTransform.localRotation,
                Quaternion.Euler(0.0f, 0.0f, 0.0f), ratio);
            rightShoulderTransform.localRotation = Quaternion.Slerp(rightShoulderTransform.localRotation,
                Quaternion.Euler(0.0f, 0.0f, 0.0f), ratio);
        }
    }

    void RegenerateHealth()
    {
        if (currentHealth < maxHealth)
        {
            float health = currentHealth + healthRegRate * Time.deltaTime;
            currentHealth = Mathf.Min(health, maxHealth);
        }
    }

    void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            float mana = currentMana + manaRegRate * Time.deltaTime;
            currentMana = Mathf.Min(mana, maxMana);
        }
    }

    void RegenrerateStamina()
    {
        if (currentStamina < maxStamina)
        {
            float rate = staminaRegRate;

            if (motion.x >= -0.1f && motion.x <= 0.1f &&
                motion.y >= -0.1f && motion.y <= 0.1f &&
                motion.z >= -0.1f && motion.z <= 0.1f)
            {
                // Increase regeneration rate when idle
                rate *= 1.5f;
            }

            float stamina = currentStamina + rate * Time.deltaTime;
            currentStamina = Mathf.Min(stamina, maxStamina);
        }
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

    public void Rotate(Vector2 rotation)
    {
        // Player
        transform.localRotation = Quaternion.AngleAxis(rotation.x, Vector3.up);

        // Camera
        float angle = Mathf.Clamp(rotation.y * 2, -90, 90.0f);
        cameraTransform.localRotation = Quaternion.AngleAxis(angle, Vector3.left);
    }

    void Shoot()
    {
        if (fireCooldown > 0.0f || projectiles.Length == 0)
        {
            return;
        }

        Vector3 position = cameraTransform.position + transform.forward * 1.0f;
        Quaternion rotation = cameraTransform.rotation;

        Projectile instance = Instantiate(projectiles[0], position, rotation);
        instance.name = projectiles[0].name;
        instance.SetDamage(10.0f);

        fireCooldown = 1.0f / fireRate;
    }

    public void Slide(bool value)
    {
        if (value && currentStamina >= slideConsRate &&
            grounded && !slide && slideCooldown == 0.0f)
        {
            currentStamina -= slideConsRate;

            slide = true;
            slideTime = maxSlideTime;
        }
        else if (!value && slide)
        {
            slide = false;
        }
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

    public void UseItem(int itemID)
    {
        switch (itemID)
        {
            case 0: // Health Fruit
                if (healthFruits > 0)
                {
                    healthFruits -= 1;
                    currentHealth += 30.0f;
                    currentHealth = Mathf.Clamp(currentHealth, 0.0f, maxHealth);
                }
                break;
            case 1: // Mana Fruit
                if (manaFruits > 0)
                {
                    manaFruits -= 1;
                    currentMana += 10.0f;
                    currentMana = Mathf.Clamp(currentMana, 0.0f, maxMana);
                }
                break;
            case 2: // Stamina Fruit
                if (staminaFruits > 0)
                {
                    staminaFruits -= 1;
                    currentStamina += 20.0f;
                    currentStamina = Mathf.Clamp(currentStamina, 0.0f, maxStamina);
                }
                break;
            default:
                break;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0.0f)
        {
            currentHealth = 0.0f;
            Die();
        }
    }
}
