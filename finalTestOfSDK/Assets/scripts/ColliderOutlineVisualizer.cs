using UnityEngine;

[ExecuteInEditMode] // Sicherstellen, dass das Skript auch im Editor-Modus funktioniert
public class ColliderOutlineVisualizer : MonoBehaviour
{
    private BoxCollider boxCollider;

    private void OnDrawGizmos()
    {
        // Überprüfen, ob der GameObject einen BoxCollider hat
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.color = Color.red; // Farbe für die Outlines, hier rot

            // Den BoxCollider als Gizmos (3D-Grafiken im Editor) zeichnen
            Gizmos.DrawWireCube(boxCollider.center + gameObject.transform.position, boxCollider.size);
        }
    }
}
