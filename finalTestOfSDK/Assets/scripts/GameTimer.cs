using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    // Referenz auf das TextMeshPro-Textobjekt, das die Zeit anzeigt
    [SerializeField] private TextMeshPro timerText;

    void Update()
    {
        if (GameManager.Instance != null)
        {
            timerText.text = GameManager.Instance.GetFormattedTime();
        }
    }
}
