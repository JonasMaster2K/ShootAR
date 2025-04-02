using UnityEngine;

public class SlingShotString : MonoBehaviour
{
    // Verweise auf die Transform-Objekte, die die Punkte der Schleuder und des Balls repräsentieren
    [SerializeField] private Transform LeftPoint;    // Linker Punkt der Schleuder
    [SerializeField] private Transform RightPoint;   // Rechter Punkt der Schleuder
    [SerializeField] private Transform PositionAtBall1;  // Position des Balls an der ersten Position (bei maximalem Ziehen)
    [SerializeField] private Transform PositionAtBall2;  // Position des Balls an der zweiten Position
    [SerializeField] private Transform PositionAtBall3;  // Position des Balls an der dritten Position
    
    // Referenz zum BallController, um den Zustand des Balls zu überwachen
    public BallController BallController;

    // Boolean-Werte zur Überwachung des Ballstatus
    public bool BallWasReleased;  // Überprüft, ob der Ball freigegeben wurde
    public bool NewDragStarted;   // Überprüft, ob ein neuer Ziehvorgang gestartet wurde
    
    // Referenz auf den LineRenderer, der für das Zeichnen der Schleuderlinie verantwortlich ist
    LineRenderer StringLineRenderer;

    void Start()
    {
        BallWasReleased = false;
        StringLineRenderer = GetComponent<LineRenderer>();
        StringLineRenderer.positionCount = 5;
        
        // Sicherstellen, dass der LineRenderer die richtige Breite hat, wenn sie nicht gesetzt ist
        if (StringLineRenderer.startWidth == 0)
            StringLineRenderer.startWidth = 0.05f;
        if (StringLineRenderer.endWidth == 0)
            StringLineRenderer.endWidth = 0.05f;
    }

    void Update()
    {
        if (LeftPoint == null || RightPoint == null || 
            PositionAtBall1 == null || PositionAtBall2 == null || PositionAtBall3 == null)
        {
            Debug.LogError("SlingShotString: Eine oder mehrere Transform-Referenzen fehlen!"); 
            return;
        }

        // Wenn der Ball bereits gestartet wurde (wird durch BallController überwacht)
        if (BallController.hasLaunched) {
            // Setzt die Positionen des LineRenderers auf nur zwei Punkte (die Schleuderliniendurch den linken und rechten Punkt)
            StringLineRenderer.positionCount = 2;
            StringLineRenderer.SetPositions(new Vector3[2] { LeftPoint.position, RightPoint.position });
        } else {
            StringLineRenderer.positionCount = 5;
            
            // Setze die Positionen der Schleuderlinie, basierend auf den Transform-Komponenten
            Vector3[] positions = new Vector3[5] {
                LeftPoint.position,
                PositionAtBall2.position,
                PositionAtBall1.position,
                PositionAtBall3.position,
                RightPoint.position
            };
            
            StringLineRenderer.SetPositions(positions);
            
            // Debug-Ausgabe: Zeichnet die Linien im Editor, um den Verlauf der Schleuder zu visualisieren
            Debug.DrawLine(LeftPoint.position, PositionAtBall2.position, Color.red);
            Debug.DrawLine(PositionAtBall2.position, PositionAtBall1.position, Color.red);
            Debug.DrawLine(PositionAtBall1.position, PositionAtBall3.position, Color.red);
            Debug.DrawLine(PositionAtBall3.position, RightPoint.position, Color.red);
        }
    }
    
    // Wird im Editor aufgerufen, um zusätzliche Debug-Visualisierungen zu zeichnen
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
