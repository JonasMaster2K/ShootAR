using UnityEngine;


public class CreateSphereAtEdges : MonoBehaviour
{
    public GameObject spherePrefab; // Das Prefab für die zu erstellenden Kugeln
    public float sphereRadius = 0.2f; // Radius der Kugeln
    public Material sphereMaterial; // Material der Kugeln
    public Color sphereColor = Color.red; // Farbe der Kugeln
    public GameObject[] sphereReferences = new GameObject[4];
    public bool enableSpheres = true;
    private bool initialized = false;

    void Start()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        
        if (boxCollider == null)
        {
            Debug.LogError("Kein BoxCollider gefunden!");
            return;
        }

        // BoxCollider-Größe und Center erhalten
        Vector3 size = boxCollider.size;
        Vector3 center = boxCollider.center;
        
        // Die halben Ausmaße des BoxColliders berechnen
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;
        float halfDepth = size.z * 0.5f;
        
        // Positionen für die vier vertikalen Kanten (mittig auf der Y-Achse)
        Vector3[] positions = new Vector3[4]
        {
            new Vector3(center.x + halfWidth, center.y, center.z + halfDepth), // Vorne rechts
            new Vector3(center.x - halfWidth, center.y, center.z + halfDepth), // Vorne links
            new Vector3(center.x + halfWidth, center.y, center.z - halfDepth), // Hinten rechts
            new Vector3(center.x - halfWidth, center.y, center.z - halfDepth)  // Hinten links
        };
        
        int sphereCount = 0;
        // Spheres erstellen
        foreach (Vector3 position in positions)
        {
            sphereReferences[sphereCount] = CreateSphere(position, sphereCount);;
            sphereCount++;
        }
        initialized = true;
    }

    private void Update() {
        if(!initialized) return;
        if(GameManager.gameState != GameManager.GameStates.SETUP) enableSpheres = false;
        if (!enableSpheres)
        {
            for (int i = 0; i < sphereReferences.Length; i++)
            {
                sphereReferences[i].SetActive(false);
            }
            return;
        }
        for (int i = 0; i < sphereReferences.Length; i++)
        {
            sphereReferences[i].SetActive(true);
        }
    }
    
    GameObject CreateSphere(Vector3 localPosition, int sphereIndex)
    {
        GameObject sphere = null;

        if (spherePrefab != null)
        {
            sphere = Instantiate(spherePrefab, transform);
        }

        if (sphere != null)
        {
            sphere.transform.localPosition = localPosition;
            sphere.transform.localScale = Vector3.one * (sphereRadius * 2);
            sphere.name = "EdgeSphere: " + sphereIndex + "\t| " + localPosition;
            sphere.transform.parent = transform;
        }
        else
        {
            Debug.LogError("spherePrefab ist nicht zugewiesen! Kugel kann nicht erstellt werden.");
        }

        return sphere;
    }

}