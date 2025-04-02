using UnityEngine;
using TMPro;

public class TimerFollowController : MonoBehaviour
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
        activeController = controllerR ?? controllerL;
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
        
        if (OVRInput.GetDown(OVRInput.Button.One))
        {
            SwitchController(handTrackingActive);
        }

        if (activeController != null)
        {
            transform.position = activeController.transform.position + new Vector3(0, 0.05f, 0);
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
            scoreText.text = $"Score: {GameManager.GetScore()} \n Best: {GameManager.GetHighScore()}";
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