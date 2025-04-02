using UnityEngine;

public class SlingShotString : MonoBehaviour
{
    [SerializeField] private Transform LeftPoint;
    [SerializeField] private Transform RightPoint;
    [SerializeField] private Transform PositionAtBall1;
    [SerializeField] private Transform PositionAtBall2;
    [SerializeField] private Transform PositionAtBall3;
    
    public BallController BallController;

    public bool BallWasReleased;
    public bool NewDragStarted;
    
    LineRenderer StringLineRenderer;
    
    void Start()
    {
        BallWasReleased = false;
        StringLineRenderer = GetComponent<LineRenderer>();
        StringLineRenderer.positionCount = 5;
        
        // Sicherstellen, dass der LineRenderer konfiguriert ist
        if (StringLineRenderer.startWidth == 0)
            StringLineRenderer.startWidth = 0.05f;
        if (StringLineRenderer.endWidth == 0)
            StringLineRenderer.endWidth = 0.05f;
    }

    void Update()
    {
        // Überprüfen, ob alle erforderlichen Referenzen vorhanden sind
        if (LeftPoint == null || RightPoint == null || 
            PositionAtBall1 == null || PositionAtBall2 == null || PositionAtBall3 == null)
        {
            Debug.LogError("SlingShotString: Eine oder mehrere Transform-Referenzen fehlen!");
            return;
        }

        if (BallController.hasLaunched) {
            // Wenn der Ball gestartet wurde, zeichne nur eine gerade Linie
            StringLineRenderer.positionCount = 2;
            StringLineRenderer.SetPositions(new Vector3[2] { LeftPoint.position, RightPoint.position });
        } else {
            // Ball nicht gestartet, zeichne die Schleuder mit dem Ball
            StringLineRenderer.positionCount = 5;
            
            // Positionen der Transform-Komponenten verwenden
            Vector3[] positions = new Vector3[5] {
                LeftPoint.position,
                PositionAtBall2.position,
                PositionAtBall1.position,
                PositionAtBall3.position,
                RightPoint.position
            };
            
            StringLineRenderer.SetPositions(positions);
            
            // Debug-Ausgabe
            Debug.DrawLine(LeftPoint.position, PositionAtBall2.position, Color.red);
            Debug.DrawLine(PositionAtBall2.position, PositionAtBall1.position, Color.red);
            Debug.DrawLine(PositionAtBall1.position, PositionAtBall3.position, Color.red);
            Debug.DrawLine(PositionAtBall3.position, RightPoint.position, Color.red);
        }
    }
    
    // Debug-Visualisierung im Editor
    void OnDrawGizmos()
    {
        if (PositionAtBall1 && PositionAtBall2 && PositionAtBall3)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(PositionAtBall1.position, 0.05f);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(PositionAtBall2.position, 0.05f);
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(PositionAtBall3.position, 0.05f);
        }
    }
}