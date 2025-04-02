using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [SerializeField] private TextMeshPro timerText;

    void Update()
    {
        if (GameManager.Instance != null)
        {
            timerText.text = GameManager.Instance.GetFormattedTime();
        }
    }
}