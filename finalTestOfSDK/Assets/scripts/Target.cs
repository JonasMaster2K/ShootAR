// TargetScript.cs
using UnityEngine;

public class TargetScript : MonoBehaviour
{
    // Variable, die angibt, ob das Ziel gefallen ist
    private bool hasFallen = false;

    // Referenz auf das Rigidbody des Ziels
    private Rigidbody rb;
    
    // Eigenschaft, die den Status des Ziels zurückgibt (ob es gefallen ist oder nicht)
    public bool IsHit { get { return hasFallen; } }

    void Start()
    {
        // Versucht, das Rigidbody des Ziels zu finden, oder fügt es hinzu, falls es nicht vorhanden ist
        rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        
        rb.isKinematic = true;
        
        rb.useGravity = false;
    }

    // Wird aufgerufen, wenn das Ziel mit einem anderen Objekt kollidiert
    private void OnCollisionEnter(Collision other)
    {
        // Wenn das Ziel noch nicht gefallen ist und das andere Objekt ein Ball ist
        if (!hasFallen && other.gameObject.CompareTag("Ball"))
        {
            hasFallen = true;

            rb.isKinematic = false;
            
            rb.useGravity = true;

            // Fügt dem Ziel eine Impuls-Kraft hinzu, basierend auf der Vorwärtsrichtung des Balls
            rb.AddForce(other.gameObject.transform.forward * 10f, ForceMode.Impulse);

            // Deaktiviert den Collider des Balls, sodass er nicht mehr mit anderen Objekten kollidiert
            other.gameObject.GetComponent<Collider>().enabled = false;

            Destroy(gameObject, 2f);
        }
    }
}
