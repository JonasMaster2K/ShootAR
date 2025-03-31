using UnityEngine;

public class TargetScript : MonoBehaviour
{
    private bool hasFallen = false;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;  // Physik-Effekte ausschalten
        rb.useGravity = false;  // Schwerkraft aus, da isKinematic aktiv ist
    }

    // Update is called once per frame
    void Update()
    {
        
    }

        private void OnCollisionEnter(Collision other)
    {
        if (!hasFallen && other.gameObject.CompareTag("Ball"))
        {
            hasFallen = true;

            // Sicherstellen, dass ein Rigidbody vorhanden ist
            Rigidbody rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
            rb.isKinematic = false;

            rb.useGravity = true;

            // Kraft nach hinten zum Umfallen
            rb.AddForce(-transform.forward * 15f, ForceMode.Impulse);

            // Ballkollision deaktivieren
            other.gameObject.GetComponent<Collider>().enabled = false;

            // Zerstören nach kurzer Verzögerung
            Destroy(gameObject, 2f);
        }
    }
}
