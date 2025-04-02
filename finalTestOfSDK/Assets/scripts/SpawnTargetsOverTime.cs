using UnityEngine;

public class SpawnTargetsOverTime : MonoBehaviour
{
    public GameObject prefab;
    public BoxCollider spawnArea; // Geändert zu BoxCollider (3D)
    public float speed = 2f; // Geschwindigkeit der horizontalen Bewegung
    private GameObject currentPrefab;
    public int targetsHit = 0;
    private bool isMoving = false;
    private bool movingLeft = true;
    private bool hasStarted = false;

    void Start()
    {
        // Sicherstellen, dass alles korrekt initialisiert ist
        if (prefab == null)
        {
            Debug.LogError("Prefab nicht zugewiesen!");
            return;
        }
        if (spawnArea == null)
        {
            Debug.LogError("Spawn Area nicht zugewiesen!");
            return;
        }
    }

    void Update()
    {
        // Überprüfen des Spielstatus
        if (GameManager.gameState == GameManager.GameStates.PLAYING)
        {
            if (!hasStarted)
            {
                hasStarted = true;
                SpawnPrefab();
            }

            if (currentPrefab == null)
            {
                IncreaseCounter();
                SpawnPrefab();
            }
            else if (isMoving)
            {
                MovePrefab();
            }
        }
        else if (GameManager.gameState == GameManager.GameStates.GAME_OVER){
            if(GameManager.GetScore() > GameManager.GetHighScore()){
                GameManager.SetHighScore(GameManager.GetScore());
            }
        }
    }

    void SpawnPrefab()
    {
        if (spawnArea == null) return; // Sicherheit, falls spawnArea null ist

        // Bestimmen einer zufälligen Position innerhalb des Bereichs
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
            spawnArea.bounds.min.y, // Setzt es flach auf den Boden
            spawnArea.bounds.center.z // Position in der Z-Achse beibehalten
        );

        // Prefab erstellen
        currentPrefab = Instantiate(prefab, spawnPosition, prefab.transform.rotation);
        float parentYRotation = this.transform.parent.eulerAngles.y + 270.0f;
        currentPrefab.transform.rotation = Quaternion.Euler(prefab.transform.rotation.eulerAngles.x, parentYRotation, prefab.transform.rotation.eulerAngles.z);

        // Wenn bestimmte Ziele erreicht wurden, skalieren
        if (targetsHit >= 10)
        {
            currentPrefab.transform.localScale = prefab.transform.localScale * 0.4f; // Skaliere um 60 % kleiner
        }
        else if (targetsHit >= 5)
        {
            currentPrefab.transform.localScale = prefab.transform.localScale * 0.7f; // Skaliere um 30 % kleiner
        }
    }

    void MovePrefab()
    {
        if (currentPrefab == null) return; // Sicherheit, falls currentPrefab null ist

        // Berechnen der neuen Position relativ zur Elternachse
        Vector3 parentForward = this.transform.parent.forward;  // Vorwärtsrichtung des Elternobjekts
        Vector3 parentRight = this.transform.parent.right;      // Rechtsrichtung des Elternobjekts

        // Berechnen der neuen X-Position entlang der Eltern-Achse (Right-Richtung)
        float moveDirection = movingLeft ? -speed : speed;
        Vector3 moveVector = parentRight * moveDirection * Time.deltaTime;

        // Berechnen der neuen Position
        currentPrefab.transform.position += moveVector;

        // Überprüfen, ob das Prefab die Grenzen des Colliders erreicht hat
        if (currentPrefab.transform.position.x <= spawnArea.bounds.min.x)
        {
            currentPrefab.transform.position = new Vector3(spawnArea.bounds.min.x, currentPrefab.transform.position.y, currentPrefab.transform.position.z);
            movingLeft = false;
        }
        else if (currentPrefab.transform.position.x >= spawnArea.bounds.max.x)
        {
            currentPrefab.transform.position = new Vector3(spawnArea.bounds.max.x, currentPrefab.transform.position.y, currentPrefab.transform.position.z);
            movingLeft = true;
        }
    }


    public void IncreaseCounter()
    {
        targetsHit++;
        
        if (GameManager.Instance != null)
        {
            GameManager.IncreaseScore();
        }

        // Wenn genügend Ziele getroffen wurden, starte die Bewegung
        if (targetsHit >= 2)
        {
            isMoving = true;
        }
    }
}
