using UnityEngine;

public class DestroyableObject : MonoBehaviour
{
    private float currentHealth;

    [Header("Health")]
    public float maxHealth = 50.0f;

    [Header("Items")]
    public GameObject[] items;
    public float[] dropChances;
    public float numberOfItems;

    // Array of pairs

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentHealth = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SelfDestruct()
    {
        Destroy(this.gameObject);

        for (int i = 0; i < numberOfItems; i++)
        {
            float random = Random.Range(0.0f, 1.0f);
            for (int j = 0; j < items.Length; j++)
            {
                if (random <= dropChances[j])
                {
                    Vector3 position = transform.position + new Vector3(Random.Range(-0.5f, 0.5f), 0.0f, Random.Range(-0.5f, 0.5f));
                    Quaternion rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);

                    GameObject instance = Instantiate(items[j], position, rotation);
                    instance.name = items[j].name;
                    break;
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0.0f)
        {
            currentHealth = 0.0f;
            SelfDestruct();
        }
    }
}
