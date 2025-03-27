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
        currentPrefab.transform.parent = this.transform;

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

        // Berechnen der neuen X-Position
        float newX = currentPrefab.transform.position.x + (movingLeft ? -speed : speed) * Time.deltaTime;

        // Prüfen, ob das Prefab die Grenzen des Colliders erreicht hat
        if (newX <= spawnArea.bounds.min.x)
        {
            newX = spawnArea.bounds.min.x;
            movingLeft = false;
        }
        else if (newX >= spawnArea.bounds.max.x)
        {
            newX = spawnArea.bounds.max.x;
            movingLeft = true;
        }

        // Aktualisieren der Position des Prefabs
        currentPrefab.transform.position = new Vector3(newX, currentPrefab.transform.position.y, currentPrefab.transform.position.z);
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
