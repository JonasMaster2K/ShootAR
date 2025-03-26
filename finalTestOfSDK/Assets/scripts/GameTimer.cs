using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TextMeshPro timerText; // Optional: TextMeshPro für Zeitanzeige

    void Update()
    {
        // Update timer text from GameManager
        if (GameManager.Instance != null)
        {
            timerText.text = GameManager.Instance.GetFormattedTime();
        }
    }
}