using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
public class Projectile : MonoBehaviour
{
    // Components
    private Rigidbody rigidBody;
    private SphereCollider sphereCollider;

    [Header("Properties")]
    public float speed;
    public float lifetime;

    void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
        rigidBody.useGravity = true; // TODO: Make this projectile-specific

        sphereCollider = GetComponent<SphereCollider>();
        sphereCollider.isTrigger = false;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rigidBody.AddForce(transform.forward * speed, ForceMode.Impulse);

        // Destroy the projectile after its lifetime expires
        Destroy(this.gameObject, lifetime);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnBecameInvisible()
    {
        // Destroy the projectile when it goes off-screen
        Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Destroy the projectile on impact
        Debug.Log("Projectile hit: " + collision.gameObject.name);

        Destroy(this.gameObject);
    }
}
