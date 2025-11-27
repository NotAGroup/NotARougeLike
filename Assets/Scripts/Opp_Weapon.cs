using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Opp_Weapon : MonoBehaviour
{
    // Components
    private Rigidbody rigidBody;

    private float damage;
    private Opponent opponent;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        opponent = GetComponentInParent<Opponent>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        string name = other.gameObject.name;

        if (name == "Opponent")
        {
            // Ignore self triggers
            return;
        }

        Debug.Log("Weapon hit: " + name);

        if (name.Contains("chest"))
        {
            Transform parent = other.gameObject.transform.parent;
            DestroyableObject destroyableObject = parent.GetComponent<DestroyableObject>();

            if (destroyableObject != null)
            {
                Debug.Log("Weapon dealing " + damage + " damage to " + parent.name);
                destroyableObject.TakeDamage(damage);
            }
        }

        if (name.Contains("Player"))
        {
            Player player = other.gameObject.GetComponent<Player>();
            Debug.Log("Weapon dealing " + damage + " damage to " + name);
            if(!opponent.playerGotHit)
            {
                player.TakeDamage(damage);
                opponent.playerGotHit = true;
            }
        }
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
}
