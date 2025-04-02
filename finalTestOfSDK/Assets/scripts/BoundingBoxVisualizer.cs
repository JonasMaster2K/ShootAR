using UnityEngine;

// Setzt das Script so, dass es auch im Editor-Modus ausgeführt wird
[ExecuteInEditMode]
public class BoundingBoxVisualizer : MonoBehaviour
{
    // Das zu visualisierende Zielobjekt (wird im Editor gesetzt)
    [Tooltip("Das zu visualisierende Zielobjekt. Falls leer, wird das eigene GameObject verwendet.")]
    public GameObject targetObject;
    
    // Farbe der Bounding Box
    [Tooltip("Farbe der Bounding Box")]
    public Color boxColor = Color.green;
    
    // Linienbreite der Bounding Box
    [Tooltip("Linienbreite der Bounding Box")]
    [Range(0.001f, 0.1f)]  // Der Bereich der Linienbreite von 0.001 bis 0.1
    public float lineWidth = 0.01f;
    
    // Aktualisierungsrate (0 bedeutet jede Frame)
    [Tooltip("Aktualisierungsrate (0 für jedes Frame)")]
    public float updateInterval = 0;
    
    // Aktiviert oder deaktiviert das Zeichnen der Bounding Box Linien im Spiel
    [Tooltip("Bounding Box Linienrenderer aktivieren (nur In-Game)")]
    public bool enableLineRenderer = true;
    
    // Beschränkung der Y-Höhe der Bounding Box
    [Tooltip("Y-Höhe des BoxColliders begrenzen")]
    public bool limitYHeight = true;
    
    // Maximale Y-Höhe des BoxColliders
    [Tooltip("Maximale Y-Höhe des BoxColliders")]
    public float maxYHeight = 0.01f;
    
    // Array für die Linienobjekte, die die Bounding Box darstellen
    private GameObject[] lineObjects = new GameObject[12];
    
    // Die Bounds der Bounding Box
    private Bounds bounds;
    
    // Zeit seit dem letzten Update
    private float timeSinceLastUpdate = 0;
    
    // Renderer und Collider des Zielobjekts
    private Renderer targetRenderer;
    private Collider targetCollider;

    // Start wird einmal beim Start des Spiels oder beim Initialisieren des Skripts aufgerufen
    void Start()
    {
        if (targetObject == null)
            targetObject = gameObject;
            
        targetRenderer = targetObject.GetComponent<Renderer>();
        targetCollider = targetObject.GetComponent<Collider>();

        if (Application.isPlaying)
        {
            for (int i = 0; i < 12; i++)
            {
                lineObjects[i] = new GameObject("BoundingBoxLine_" + i); 
                lineObjects[i].transform.parent = transform; 
                LineRenderer lr = lineObjects[i].AddComponent<LineRenderer>(); 
                lr.startWidth = lineWidth; 
                lr.endWidth = lineWidth;   
                lr.material = new Material(Shader.Find("Sprites/Default"));
                lr.startColor = boxColor; 
                lr.endColor = boxColor;   
                lr.positionCount = 2;     
                lineObjects[i].SetActive(false);
            }
        }
    }
    
    // Update wird jeden Frame aufgerufen
    void Update()
    {
        if (!Application.isPlaying) return;

        if (GameManager.gameState != GameManager.GameStates.SETUP)
        {
            SetBoundingBoxVisibility(false);
            return;
        }

        timeSinceLastUpdate += Time.deltaTime;

        if (updateInterval == 0 || timeSinceLastUpdate >= updateInterval)
        {
            UpdateBoundingBox();
            timeSinceLastUpdate = 0;
        }
    }

    // Setzt die Sichtbarkeit der Bounding Box Linien
    void SetBoundingBoxVisibility(bool visible)
    {
        foreach (GameObject line in lineObjects)
        {
            if (line != null)
                line.SetActive(visible);
        }
    }
    
    // Aktualisiert die Bounding Box basierend auf den Collider- oder Renderer-Informationen
    void UpdateBoundingBox()
    {
        if (targetObject == null || GameManager.gameState != GameManager.GameStates.SETUP)
        {
            SetBoundingBoxVisibility(false);
            return;
        }
        
        if (targetCollider != null && targetCollider is BoxCollider boxCollider && limitYHeight)
        {
            Vector3 size = boxCollider.size;
            Vector3 center = boxCollider.center;
            
            if (limitYHeight)
            {
                size.y = Mathf.Min(size.y, maxYHeight);
            }
            
            // Berechnen der Ecken des BoxColliders
            Vector3 extents = size * 0.5f;
            Vector3[] corners = new Vector3[8];
            corners[0] = center + new Vector3(-extents.x, -extents.y, -extents.z);
            corners[1] = center + new Vector3(extents.x, -extents.y, -extents.z);
            corners[2] = center + new Vector3(extents.x, -extents.y, extents.z);
            corners[3] = center + new Vector3(-extents.x, -extents.y, extents.z);
            corners[4] = center + new Vector3(-extents.x, extents.y, -extents.z);
            corners[5] = center + new Vector3(extents.x, extents.y, -extents.z);
            corners[6] = center + new Vector3(extents.x, extents.y, extents.z);
            corners[7] = center + new Vector3(-extents.x, extents.y, extents.z);
            
            // Transformiere die lokalen Koordinaten in Weltkoordinaten
            for (int i = 0; i < 8; i++)
            {
                corners[i] = targetObject.transform.TransformPoint(corners[i]);
            }
            
            if (Application.isPlaying && enableLineRenderer)
            {
                DrawLine(0, corners[0], corners[1]);
                DrawLine(1, corners[1], corners[2]);
                DrawLine(2, corners[2], corners[3]);
                DrawLine(3, corners[3], corners[0]);
                
                DrawLine(4, corners[4], corners[5]);
                DrawLine(5, corners[5], corners[6]);
                DrawLine(6, corners[6], corners[7]);
                DrawLine(7, corners[7], corners[4]);
                
                DrawLine(8, corners[0], corners[4]);
                DrawLine(9, corners[1], corners[5]);
                DrawLine(10, corners[2], corners[6]);
                DrawLine(11, corners[3], corners[7]);
            }
        }
        else
        {
            if (targetRenderer != null)
            {
                bounds = targetRenderer.bounds;
            }
            else if (targetCollider != null)
            {
                bounds = targetCollider.bounds;
            }
            else
            {
                Debug.LogWarning("Kein Renderer oder Collider am Zielobjekt gefunden. Bounding Box kann nicht visualisiert werden.");
                return;
            }
            
            // Berechnen der Ecken der Bounding Box basierend auf den Bounds
            Vector3 min = bounds.min;
            Vector3 max = bounds.max;
            
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(min.x, min.y, min.z);
            corners[1] = new Vector3(max.x, min.y, min.z);
            corners[2] = new Vector3(max.x, min.y, max.z);
            corners[3] = new Vector3(min.x, min.y, max.z);
            corners[4] = new Vector3(min.x, max.y, min.z);
            corners[5] = new Vector3(max.x, max.y, min.z);
            corners[6] = new Vector3(max.x, max.y, max.z);
            corners[7] = new Vector3(min.x, max.y, max.z);
            
            // Zeichnen der Bounding Box Linien, wenn der LineRenderer aktiviert ist
            if (Application.isPlaying && enableLineRenderer)
            {
                DrawLine(0, corners[0], corners[1]);
                DrawLine(1, corners[1], corners[2]);
                DrawLine(2, corners[2], corners[3]);
                DrawLine(3, corners[3], corners[0]);
                
                DrawLine(4, corners[4], corners[5]);
                DrawLine(5, corners[5], corners[6]);
                DrawLine(6, corners[6], corners[7]);
                DrawLine(7, corners[7], corners[4]);
                
                DrawLine(8, corners[0], corners[4]);
                DrawLine(9, corners[1], corners[5]);
                DrawLine(10, corners[2], corners[6]);
                DrawLine(11, corners[3], corners[7]);
            }
        }
    }
    
    // Hilfsmethode, um eine Linie zwischen zwei Punkten zu zeichnen
    void DrawLine(int index, Vector3 start, Vector3 end)
    {
        if (!enableLineRenderer || !Application.isPlaying) return;
        
        if (lineObjects[index] != null)
        {
            LineRenderer lr = lineObjects[index].GetComponent<LineRenderer>();
            if (lr != null)
            {
                lr.SetPosition(0, start);
                lr.SetPosition(1, end);
                lr.startColor = boxColor;
                lr.endColor = boxColor;
                lr.startWidth = lineWidth;
                lr.endWidth = lineWidth;
                lineObjects[index].SetActive(true);
            }
        }
    }
    
    // Gizmos werden verwendet, um die Bounding Box im Editor zu zeichnen
    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        
        if (targetCollider != null && targetCollider is BoxCollider box && enableLineRenderer)
        {
            Gizmos.color = boxColor;
            
            if (limitYHeight)
            {
                Vector3 size = box.size;
                size.y = Mathf.Min(size.y, maxYHeight);
                Matrix4x4 oldMatrix = Gizmos.matrix;
                Gizmos.matrix = targetObject.transform.localToWorldMatrix;
                Gizmos.DrawWireCube(box.center, size);
                Gizmos.matrix = oldMatrix;
            }
            else
            {
                Gizmos.DrawWireCube(box.center + targetObject.transform.position, box.size);
            }
        }
    }
    
    // Wird aufgerufen, wenn das Objekt zerstört wird, um Ressourcen zu bereinigen
    void OnDestroy()
    {
        if (!Application.isPlaying) return;
        
        foreach (GameObject line in lineObjects)
        {
            if (line != null)
                Destroy(line);
        }
    }
}
