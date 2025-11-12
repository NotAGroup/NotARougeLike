using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class InteractionArea : MonoBehaviour
{
    // Components
    private BoxCollider boxCollider;

    public GameObject triggerObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();

        triggerObject = null;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        string name = other.gameObject.name;

        if (name == "Interaction Area" || name == "Player")
        {
            // Ignore self triggers
            return;
        }

        triggerObject = other.gameObject;

        if (triggerObject != null)
        {
            Debug.Log("Enter Trigger: " + name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        triggerObject = null;
    }
}
