using UnityEngine;
using UnityEngine.SceneManagement;
using Oculus.Interaction;

public class GameManager : MonoBehaviour
{
    // Definiert verschiedene Zustände des Spiels.
    public enum GameStates 
    {
        SETUP,       // Spielvorbereitung
        PLAYING,     // Spiel läuft
        GAME_WON,    // Spiel gewonnen
        GAME_OVER    // Spiel verloren oder beendet
    }

    // Aktueller Spielzustand und andere statische Variablen, die im Spiel verwendet werden.
    public static GameStates gameState;
    public static bool anchorExists = false; // Zeigt an, ob ein "Anker" existiert (z.B. für Oculus Interaktionen)

    [SerializeField] private float gameDuration = 60.0f; // Dauer des Spiels in Sekunden
    [SerializeField] private float currentTime; // Aktuelle verbleibende Zeit
    [SerializeField] private int maxScore = 10; // Maximale Punktzahl zum Gewinnen

    private static int score; // Aktueller Punktestand
    private static int highScore = 0; // Höchster Punktestand

    // Namen der UI-Panels, die während des Spiels aktiviert/deaktiviert werden
    [SerializeField] private string setupPanelName = "SetupPanel";
    [SerializeField] private string playingPanelName = "PlayingPanel";
    [SerializeField] private string gameOverPanelName = "GameOverPanel";

    // Instanz des GameManagers für den Singleton-Ansatz
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        // Überprüft, ob es bereits eine Instanz des GameManagers gibt. Wenn nicht, wird eine erstellt und die Instanz gespeichert.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
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
        // Führt je nach aktuellem Spielzustand verschiedene Logik aus.
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

    // Ändert den aktuellen Spielzustand und aktiviert/deaktiviert die entsprechenden Panels.
    public void SetGameState(GameStates newState)
    {
        gameState = newState;

        // Setzt Panels basierend auf dem neuen Zustand
        SetPanelActive(setupPanelName, newState == GameStates.SETUP);
        SetPanelActive(playingPanelName, newState == GameStates.PLAYING);
        SetPanelActive(gameOverPanelName, newState == GameStates.GAME_OVER);

        // Führe für den jeweiligen Zustand spezifische Initialisierungen durch
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

    // Setzt den Spielstand und die verbleibende Zeit zurück.
    private void ResetGame()
    {
        score = 0;
        currentTime = gameDuration;
    }

    // Wird aufgerufen, wenn das Spiel gestartet wird. Hier können spezifische Setups durchgeführt werden.
    private void StartGame()
    {
        // Hier könnte man spezielle Logik hinzufügen, die zu Beginn des Spiels ausgeführt wird.
    }

    // Wird aufgerufen, wenn das Spiel beendet wird. Hier können spezifische Aufräumarbeiten durchgeführt werden.
    private void EndGame()
    {
        // Hier könnte man Logik zum Beenden oder Bereinigen des Spiels einfügen.
    }

    // Handhabt die Logik für den Setup-Zustand (Vorbereitung des Spiels).
    private void HandleSetupState() 
    { 
        if (OVRInput.GetDown(OVRInput.Button.One) && anchorExists)
            SetGameState(GameStates.PLAYING); 
    }

    // Handhabt die Logik für den Playing-Zustand (Das Spiel läuft).
    private void HandlePlayingState()
    {
        currentTime = Mathf.Max(0, currentTime - Time.deltaTime);

        // Wenn entweder die maximale Punktzahl erreicht wurde oder die Zeit abgelaufen ist, wird das Spiel beendet.
        if (score >= maxScore || currentTime <= 0)
        {
            SetGameState(GameStates.GAME_OVER);
        }
    }

    // Handhabt die Logik für den GameOver-Zustand (Spiel beendet).
    private void HandleGameOverState() 
    { 
        if (OVRInput.GetDown(OVRInput.Button.One))
            RestartGame();
        else if(OVRInput.GetDown(OVRInput.Button.Two))
        {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
        }
    }

    // Startet das Spiel neu, indem der Zustand auf Setup gesetzt und die Szene neu geladen wird.
    public void RestartGame()
    {
        SetGameState(GameStates.SETUP);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Sucht das Panel im UI anhand des Panelnamens.
    private GameObject FindPanelInUI(string panelName)
    {
        Canvas uiCanvas = GameObject.Find("UI")?.GetComponent<Canvas>();
        if (uiCanvas == null)
        {
            Debug.LogWarning("Canvas mit dem Namen 'UI' nicht gefunden!");
            return null;
        }

        Transform panelTransform = uiCanvas.transform.Find(panelName);
        if (panelTransform != null)
        {
            return panelTransform.gameObject;
        }

        Debug.LogWarning($"Panel '{panelName}' nicht im Canvas 'UI' gefunden!");
        return null;
    }

    // Aktiviert oder deaktiviert das Panel im UI basierend auf dem angegebenen Zustand.
    private void SetPanelActive(string panelName, bool isActive)
    {
        GameObject panel = FindPanelInUI(panelName);
        if (panel != null)
        {
            panel.SetActive(isActive);
        }
    }

    // Gibt die verbleibende Zeit als formatierten String (mm:ss) zurück.
    public string GetFormattedTime()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // Erhöht die Punktzahl um 1.
    public static void IncreaseScore() => score++;

    // Gibt die aktuelle Punktzahl zurück.
    public static int GetScore() => score;

    // Setzt den neuen Höchststand.
    public static void SetHighScore(int newHighScore) => highScore = newHighScore;

    // Gibt den aktuellen Höchststand zurück.
    public static int GetHighScore() => highScore;

    // Wird aufgerufen, wenn eine neue Szene geladen wird und stellt sicher, dass die Panels korrekt angezeigt werden.
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetPanelActive(setupPanelName, gameState == GameStates.SETUP);
        SetPanelActive(playingPanelName, gameState == GameStates.PLAYING);
        SetPanelActive(gameOverPanelName, gameState == GameStates.GAME_OVER);
    }
}
