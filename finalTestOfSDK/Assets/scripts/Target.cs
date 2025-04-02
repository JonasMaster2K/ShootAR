// TargetScript.cs
using UnityEngine;

public class TargetScript : MonoBehaviour
{
    private bool hasFallen = false;
    private Rigidbody rb;
    
    public bool IsHit { get { return hasFallen; } }

    void Start()
    {
        rb = GetComponent<Rigidbody>() ?? gameObject.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (!hasFallen && other.gameObject.CompareTag("Ball"))
        {
            hasFallen = true;

            rb.isKinematic = false;
            rb.useGravity = true;

            rb.AddForce(transform.forward * 15f, ForceMode.Impulse);

            other.gameObject.GetComponent<Collider>().enabled = false;

            Destroy(gameObject, 2f);
        }
    }
}