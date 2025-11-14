using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Weapon : MonoBehaviour
{
    // Components
    private Rigidbody rigidBody;

    private float damage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        string name = collision.gameObject.name;

        if (name == "Player")
        {
            // Ignore self triggers
            return;
        }

        Debug.Log("Weapon hit: " + name);

        if (name.Contains("chest"))
        {
            Transform parent = collision.gameObject.transform.parent;
            DestroyableObject destroyableObject = parent.GetComponent<DestroyableObject>();

            if (destroyableObject != null)
            {
                Debug.Log("Weapon dealing " + damage + " damage to " + parent.name);
                destroyableObject.TakeDamage(damage);
            }
        }

        if (name.Contains("Opponent"))
        {
            Opponent opponent = collision.gameObject.GetComponent<Opponent>();
            Debug.Log("Weapon dealing " + damage + " damage to " + name);
            opponent.TakeDamage(damage);
        }
    }

    public void SetDamage(float damage)
    {
        this.damage = damage;
    }
}
