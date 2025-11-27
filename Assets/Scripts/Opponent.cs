using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Opponent : MonoBehaviour
{
    // Components
    private NavMeshAgent navMeshAgent;
    private Transform playerTransform;

    private float currentHealth;
    private float wanderTime;

    private Vector3 spawnPosition;

    [Header("Combat")]
    public Opp_Weapon weapon;
    public float hitRate = 2.0f;
    private float hitTime;
    private float hitCooldown;
    private Transform rightShoulderTransform;
    public float swingDuration = 0.4f;
    public float strikeDuration = 0.2f;
    public float returnDuration = 0.1f;
    public float attackRange = 3.0f;
    public float detectionRange = 15.0f;
    private Vector3 restRotation = new Vector3(0.0f, 0.0f, 0.0f);
    private Vector3 swingRotation = new Vector3(-130.0f, 0.0f, 0.0f);
    private Vector3 strikeRotation = new Vector3(-30.0f, 0.0f, 0.0f);
    enum HitState
    {
        Idle,
        Return,
        Swing,
        Strike
    }

    private HitState hitState;

    private Quaternion startRotation;
    private Quaternion endRotation;

    public bool playerGotHit = false;

    [Header("Health")]
    public float maxHealth = 100.0f;
    public float healthRegRate = 5.0f;

    [Header("Movement")]
    public float wanderInterval = 3.0f;
    public float wanderRadius = 5.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        navMeshAgent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.Find("Player").transform;
        rightShoulderTransform = transform.Find("Opp Right Shoulder");

        currentHealth = maxHealth;
        spawnPosition = transform.position;
        wanderTime = wanderInterval;

        hitState = HitState.Idle;
        hitCooldown = 0.0f;
        hitTime = 0.0f;
        weapon.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        float distance = Vector3.Distance(playerTransform.position, transform.position);

        if (hitCooldown > 0.0f)
        {
            hitCooldown -= Time.deltaTime;
            if (hitCooldown < 0.0f)
            {
                hitCooldown = 0.0f;
            }
        }

        if (distance <= detectionRange)
        {
            if (distance <= attackRange)
            {
                if(hitState == HitState.Idle)
                    Attack();
            }
            else
            {
                navMeshAgent.isStopped = false;
                navMeshAgent.SetDestination(playerTransform.position);
            }
        }
        else
        {
            Wander();
            RegenerateHealth();
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
                        playerGotHit = false;

                        weapon.gameObject.SetActive(false);
                        break;
                    default:
                        Debug.LogError("Unknown Hit State: " + hitState);
                        return;
                }
            }
        }
    }
    void Attack()
    {
        // TODO
        navMeshAgent.isStopped = true;

        //transform.LookAt(playerTransform);

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

    void Die()
    {
        navMeshAgent.isStopped = true;
        Destroy(gameObject, 2.0f);
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

    void RegenerateHealth()
    {
        if (currentHealth < maxHealth)
        {
            float health = currentHealth + healthRegRate * Time.deltaTime;
            currentHealth = Mathf.Min(health, maxHealth);
        }
    }

    void Wander()
    {
        wanderTime += Time.deltaTime;

        if (wanderTime >= wanderInterval)
        {
            Vector3 randomDirection = Random.insideUnitSphere * wanderRadius;
            randomDirection += spawnPosition;

            NavMeshHit navHit;
            NavMesh.SamplePosition(randomDirection, out navHit, wanderRadius, NavMesh.AllAreas);

            navMeshAgent.SetDestination(navHit.position);

            wanderTime = 0.0f;
        }
    }

}
