using UnityEngine;

[ExecuteInEditMode] // Sicherstellen, dass das Skript auch im Editor-Modus funktioniert
public class BoundingBoxColliderVisualizer : MonoBehaviour
{
    [Tooltip("Das zu visualisierende Zielobjekt. Falls leer, wird das eigene GameObject verwendet.")]
    public GameObject targetObject;
    
    [Tooltip("Farbe der Bounding Box")]
    public Color boxColor = Color.green;
    
    [Tooltip("Linienbreite der Bounding Box")]
    [Range(0.001f, 0.1f)]
    public float lineWidth = 0.01f;
    
    [Tooltip("Aktualisierungsrate (0 für jedes Frame)")]
    public float updateInterval = 0;
    
    [Tooltip("Bounding Box Linienrenderer aktivieren (nur In-Game)")]
    public bool enableLineRenderer = true;
    
    private GameObject[] lineObjects = new GameObject[12];
    private Bounds bounds;
    private float timeSinceLastUpdate = 0;
    private Renderer targetRenderer;
    private Collider targetCollider;

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
                lineObjects[i].SetActive(enableLineRenderer);
            }
        }
        
        UpdateBoundingBox();
    }
    
    void Update()
    {
        if (!Application.isPlaying) return;
        
        timeSinceLastUpdate += Time.deltaTime;
        if (updateInterval == 0 || timeSinceLastUpdate >= updateInterval)
        {
            UpdateBoundingBox();
            timeSinceLastUpdate = 0;
        }
    }
    
    void UpdateBoundingBox()
    {
        if (targetObject == null)
            return;
            
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
        
        if (Application.isPlaying)
        {
            for (int i = 0; i < 12; i++)
            {
                lineObjects[i].SetActive(enableLineRenderer);
            }
            
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
    
    void DrawLine(int index, Vector3 start, Vector3 end)
    {
        if (!enableLineRenderer || !Application.isPlaying) return;
        LineRenderer lr = lineObjects[index].GetComponent<LineRenderer>();
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }
    
    void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        
        if (targetCollider != null && targetCollider is BoxCollider box && enableLineRenderer)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(box.center + gameObject.transform.position, box.size);
        }
    }
    
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
