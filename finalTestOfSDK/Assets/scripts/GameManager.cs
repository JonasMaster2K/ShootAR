using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Interaction;

public class GameManager : MonoBehaviour
{
    public enum GameStates 
    {
        SETUP,
        PLAYING,
        GAME_OVER
    }
    public static GameStates gameState;

    [SerializeField] private float gameDuration = 60.0f;
    [SerializeField] private float currentTime;
    [SerializeField] private int maxScore = 10;

    private static int score;

    // Panel-Namen als Strings
    [SerializeField] private string setupPanelName = "SetupPanel";
    [SerializeField] private string playingPanelName = "PlayingPanel";
    [SerializeField] private string gameOverPanelName = "GameOverPanel";

    #region Singleton Pattern
    public static GameManager Instance { get; private set; }
    #endregion

    #region Initialization Methods
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;  // Hinzufügen des SceneLoaded-Event
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetGameState(GameStates.SETUP);
    }

    void Update()
    {
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
    #endregion

    #region Game State Management
    public void SetGameState(GameStates newState)
    {
        gameState = newState;

        // Panels anhand der Namen suchen und aktivieren/deaktivieren
        SetPanelActive(setupPanelName, newState == GameStates.SETUP);
        SetPanelActive(playingPanelName, newState == GameStates.PLAYING);
        SetPanelActive(gameOverPanelName, newState == GameStates.GAME_OVER);

        switch (newState)
        {
            case GameStates.SETUP:
                ResetGame();
                break;
            case GameStates.PLAYING:
                StartGame();
                break;
            case GameStates.GAME_OVER:
                EndGame();
                break;
        }
    }

    private void ResetGame()
    {
        score = 0;
        currentTime = gameDuration;
    }

    private void StartGame()
    {
        // Any specific setup when starting the game
    }

    private void EndGame()
    {
        // Any specific cleanup when game ends
    }
    #endregion

    #region State Handling Methods
    private void HandleSetupState() 
    { 
        if (OVRInput.GetDown(OVRInput.Button.Two)) 
            SetGameState(GameStates.PLAYING); 
    }

    private void HandlePlayingState()
    {
        currentTime = Mathf.Max(0, currentTime - Time.deltaTime);

        if (score >= maxScore || currentTime <= 0)
        {
            SetGameState(GameStates.GAME_OVER);
        }
    }

    private void HandleGameOverState() 
    { 
        if (OVRInput.GetDown(OVRInput.Button.Two)) 
            RestartGame(); 
    }
    #endregion

    #region Game Management Helpers
    public void RestartGame()
    {
        SetGameState(GameStates.SETUP);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    #endregion

    #region UI Panel Helpers
    private GameObject FindPanelInUI(string panelName)
    {
        // Sucht nach dem Canvas mit dem Namen "UI"
        Canvas uiCanvas = GameObject.Find("UI")?.GetComponent<Canvas>();
        if (uiCanvas == null)
        {
            Debug.LogWarning("Canvas mit dem Namen 'UI' nicht gefunden!");
            return null;
        }

        // Sucht nach dem Panel im "UI"-Canvas
        Transform panelTransform = uiCanvas.transform.Find(panelName);
        if (panelTransform != null)
        {
            return panelTransform.gameObject;
        }

        Debug.LogWarning($"Panel '{panelName}' nicht im Canvas 'UI' gefunden!");
        return null;
    }

    private void SetPanelActive(string panelName, bool isActive)
    {
        GameObject panel = FindPanelInUI(panelName);
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }
    #endregion

    #region Time Management
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    #endregion

    #region Score Management
    public static void IncreaseScore() => score++;
    public static int GetScore() => score;
    #endregion

    #region Scene Management
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetPanelActive(setupPanelName, gameState == GameStates.SETUP);
        SetPanelActive(playingPanelName, gameState == GameStates.PLAYING);
        SetPanelActive(gameOverPanelName, gameState == GameStates.GAME_OVER);
    }
    #endregion
}
