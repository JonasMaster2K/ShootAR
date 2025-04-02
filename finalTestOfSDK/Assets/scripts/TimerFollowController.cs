using UnityEngine;
using TMPro;

public class TimerFollowController : MonoBehaviour
{
    // Hand- und Controller-Referenzen für das linke und rechte Hand-Tracking bzw. Controller
    [Header("Hand & Controller References")]
    public GameObject handControllerL;  // Linker Hand-Controller
    public GameObject handControllerR;  // Rechter Hand-Controller
    public GameObject controllerL;      // Linker Controller
    public GameObject controllerR;      // Rechter Controller
    
    // UI-Element für die Anzeige des Scores
    [Header("UI Elements")]
    public TextMeshPro scoreText;       // UI Text für die Anzeige des Scores
    
    private GameObject activeController; // Das aktuell aktive Controller-Objekt
    
    void Start()
    {
        activeController = controllerR ?? controllerL;
        StyleScoreText();
    }

    void Update()
    {
        UpdateTracking(); 
        UpdateScoreUI();  
    }

    // Aktualisiert das Tracking der Controller und Hand-Tracking
    private void UpdateTracking()
    {
        bool isSimulator = OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.None;

        bool handTrackingActive = !isSimulator && OVRPlugin.GetHandTrackingEnabled();

        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            SwitchController(handTrackingActive);
        }

        if (activeController != null)
        {
            transform.position = activeController.transform.position + new Vector3(0, 0.05f, 0);

            transform.rotation = activeController.transform.rotation;
        }
    }

    // Wechselt den aktiven Controller je nach Hand-Tracking-Status
    private void SwitchController(bool handTrackingActive)
    {
        // Wenn Hand-Tracking aktiv ist, wechsle zwischen den Hand-Controllern (links und rechts)
        if (handTrackingActive)
        {
            // Wenn der aktuelle aktive Controller der linke Hand-Controller oder der linke Hand-Tracking-Controller ist, wechsle zum rechten Hand-Controller
            activeController = (activeController == controllerL || activeController == handControllerL) ? handControllerR : handControllerL;
        }
        // Wenn Hand-Tracking nicht aktiv ist, wechsle zwischen den normalen Controllern (links und rechts)
        else
        {
            activeController = (activeController == controllerL) ? controllerR : controllerL;
        }
    }

    // Aktualisiert die Score-UI mit dem aktuellen und besten Score
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {GameManager.GetScore()} \n Best: {GameManager.GetHighScore()}";
        }
    }
    
    // Stilisiert den Score-Text (UI), z.B. Schriftgröße, Farbe, Ausrichtung usw.
    private void StyleScoreText()
    {
        if (scoreText != null)
        {
            scoreText.fontSize = 36;                          
            scoreText.color = Color.cyan;                     
            scoreText.alignment = TextAlignmentOptions.Center;
            scoreText.fontStyle = FontStyles.Bold;            
            scoreText.outlineColor = Color.black;             
            scoreText.outlineWidth = 0.2f;                    
        }
    }
}
