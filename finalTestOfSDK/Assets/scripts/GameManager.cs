using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Interaction;

public class GameManager : MonoBehaviour
{
    public enum GameStates {
        SETUP,
        PLAYING,
        GAME_OVER
    };

    public static GameStates gameState;

    [SerializeField] private static int score;
    [SerializeField] private int MaxScore = 10;
    [SerializeField] private GameStates displayedGameState;

    // References to UI elements or other game components
    [SerializeField] private GameObject setupPanel;
    [SerializeField] private GameObject playingPanel;
    [SerializeField] private GameObject gameOverPanel;

    // Singleton pattern to ensure only one GameManager exists
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Initialize the game to the SETUP state
        SetGameState(GameStates.SETUP);
        score = 0;
    }

    // Update is called once per frame
    void Update()
    {
        displayedGameState = gameState;

        // State-specific update logic
        switch (gameState)
        {
            case GameStates.SETUP:
                HandleSetupState();
                break;
            case GameStates.PLAYING:
                HandlePlayingState();
                break;
            case GameStates.GAME_OVER:
                HandleGameOverState();
                break;
        }
    }

    // Method to change game state with proper setup
    public void SetGameState(GameStates newState)
    {
        gameState = newState;

        // Update UI visibility based on current state
        setupPanel.SetActive(newState == GameStates.SETUP);
        playingPanel.SetActive(newState == GameStates.PLAYING);
        gameOverPanel.SetActive(newState == GameStates.GAME_OVER);

        // Additional state transition logic
        switch (newState)
        {
            case GameStates.SETUP:
                OnSetupEnter();
                break;
            case GameStates.PLAYING:
                OnPlayingEnter();
                break;
            case GameStates.GAME_OVER:
                OnGameOverEnter();
                break;
        }
    }

    // State-specific handling methods
    private void HandleSetupState()
    {
        // Logic for setup phase
        // For example, waiting for player to start the game
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            SetGameState(GameStates.PLAYING);
        }
    }

    private void HandlePlayingState()
    {
        // Logic for playing phase

        // Example game over condition
        if (score >= MaxScore) // Replace with actual game over condition
        {
            SetGameState(GameStates.GAME_OVER);
        }
    }

    private void HandleGameOverState()
    {
        // Logic for game over phase
        // For example, waiting for restart input
        if (OVRInput.GetDown(OVRInput.Button.Two))
        {
            RestartGame();
        }
    }

    // Specific enter methods for each state
    private void OnSetupEnter()
    {
        // Reset game elements
        ResetScore();
        // Additional setup logic
    }

    private void OnPlayingEnter()
    {
        // Start game elements
        // Initialize player, spawn enemies, etc.
    }

    private void OnGameOverEnter()
    {
        // Save high score
        // Show final score
        // Trigger game over animations/effects
    }

    // Restart the game
    public void RestartGame()
    {
        // Reset to setup state
        ResetScore();
        SetGameState(GameStates.SETUP);
        
        // Optionally reload the scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }


    public static void IncreaseScore()
    {
        score++;
    }

    private static void ResetScore()
    {
        score = 0;
    }

    public static void setScore(int newScore)
    {
        score = newScore;
    }

    public static int getScore()
    {
        return score;
    }
}