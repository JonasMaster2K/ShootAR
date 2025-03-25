using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static int score;
    [SerializeField] private int displayedScore;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        displayedScore = score;
    }
}
