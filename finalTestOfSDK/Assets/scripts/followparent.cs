using UnityEngine;
using TMPro;

public class FollowParent : MonoBehaviour
{
    [Header("Hand & Controller References")]
    public GameObject handControllerL;
    public GameObject handControllerR;
    public GameObject controllerL;
    public GameObject controllerR;
    
    [Header("UI Elements")]
    public TextMeshPro scoreText;
    
    private GameObject activeController;
    
    void Start()
    {
        // Standardmäßig linken Controller setzen
        activeController = controllerL ?? controllerR;
        StyleScoreText();
    }

    void Update()
    {
        UpdateTracking();
        UpdateScoreUI();
    }

    private void UpdateTracking()
    {
        bool isSimulator = OVRPlugin.GetSystemHeadsetType() == OVRPlugin.SystemHeadset.None;
        bool handTrackingActive = !isSimulator && OVRPlugin.GetHandTrackingEnabled();
        
        // Wechsel zwischen Controllern bei Tastendruck
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            SwitchController(handTrackingActive);
        }

        // Position & Rotation setzen
        if (activeController != null)
        {
            transform.position = activeController.transform.position + new Vector3(0, 0.05f, 0);
            //transform.rotation = activeController.transform.rotation;
            transform.rotation = activeController.transform.rotation;
        }
    }

    private void SwitchController(bool handTrackingActive)
    {
        if (handTrackingActive)
        {
            activeController = (activeController == controllerL || activeController == handControllerL) ? handControllerR : handControllerL;
        }
        else
        {
            activeController = (activeController == controllerL) ? controllerR : controllerL;
        }
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {GameManager.score}";
        }
    }
    
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