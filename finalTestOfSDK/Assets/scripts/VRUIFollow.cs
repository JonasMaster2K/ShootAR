using UnityEngine;

public class VRUIFollow : MonoBehaviour
{
    public Transform target;  // Die VR-Kamera als Ziel
    public float smoothSpeed = 5f; // Geschwindigkeit der Anpassung

    void Update()
    {
        if (target == null) return;
        
        // Glatte Interpolation zur neuen Position & Rotation
        transform.position = Vector3.Lerp(transform.position, target.position + target.forward * 2f, Time.deltaTime * smoothSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, Time.deltaTime * smoothSpeed);
    }
}
