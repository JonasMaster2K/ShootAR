using UnityEngine;

public class SpawnTargetsOverTime : MonoBehaviour
{
    // Das Prefab des Ziels, das instanziiert werden soll
    public GameObject prefab;

    // Der Bereich, in dem die Ziele erscheinen sollen (BoxCollider)
    public BoxCollider spawnArea;

    // Geschwindigkeit, mit der das Ziel sich bewegen soll
    public float speed = 2f;

    // Aktuelles Ziel, das gerade aktiv ist
    private GameObject currentPrefab;

    // Zähler für die Anzahl der getroffenen Ziele
    public int targetsHit = 0;

    // Boolesche Variablen für den Bewegungsstatus
    private bool isMoving = false;
    private bool movingLeft = true;

    // Gibt an, ob das Ziel-Spawnen bereits gestartet wurde
    private bool hasStarted = false;

    void Start()
    {
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

        else if (GameManager.gameState == GameManager.GameStates.GAME_OVER)
        {
            if(GameManager.GetScore() > GameManager.GetHighScore())
            {
                GameManager.SetHighScore(GameManager.GetScore());
            }
        }
    }

    // Spawnt das Ziel an einer zufälligen Position innerhalb der angegebenen Spawn-Area
    void SpawnPrefab()
    {
        if (spawnArea == null) return;

        // Bestimmt eine zufällige Position innerhalb der Spawn-Area
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
            spawnArea.bounds.min.y,
            spawnArea.bounds.center.z
        );

        // Erstelle eine Instanz des Prefabs an der berechneten Position
        currentPrefab = Instantiate(prefab, spawnPosition, prefab.transform.rotation);

        // Drehe das Ziel basierend auf der Elternrotation
        float parentYRotation = this.transform.parent.eulerAngles.y + 270.0f;
        currentPrefab.transform.rotation = Quaternion.Euler(prefab.transform.rotation.eulerAngles.x, parentYRotation, prefab.transform.rotation.eulerAngles.z);

        // Ändert die Größe des Ziels abhängig von der Anzahl der getroffenen Ziele
        if (targetsHit >= 10)
        {
            currentPrefab.transform.localScale = prefab.transform.localScale * 0.4f;  // Verkleinert das Ziel bei mehr als 10 Treffern
        }
        else if (targetsHit >= 5)
        {
            currentPrefab.transform.localScale = prefab.transform.localScale * 0.7f;  // Verkleinert das Ziel bei mehr als 5 Treffern
        }
    }

    // Bewegt das Ziel innerhalb der Spawn-Area hin und her
    void MovePrefab()
    {
        if (currentPrefab == null) return;

        // Bestimmt die Vorwärts- und Rechtsrichtung der Eltern-Transform
        Vector3 parentForward = transform.parent.forward;
        Vector3 parentRight = transform.parent.right;

        // Bestimmt die Bewegungsrichtung (links oder rechts)
        float moveDirection = movingLeft ? -speed : speed;

        // Berechnet die neue Position des Ziels basierend auf der Bewegungsrichtung
        Vector3 newPosition = currentPrefab.transform.position + parentRight * moveDirection * Time.deltaTime;

        // Konvertiert die Position in lokale Koordinaten der Spawn-Area
        Vector3 localPos = spawnArea.transform.InverseTransformPoint(newPosition);

        // Bestimmt die Grenzen der Spawn-Area in lokalen Koordinaten
        Vector3 localBoundsMin = spawnArea.bounds.min - spawnArea.transform.position;
        Vector3 localBoundsMax = spawnArea.bounds.max - spawnArea.transform.position;

        // Wenn das Ziel die linke Grenze überschreitet, ändere die Richtung nach rechts
        if (localPos.x < localBoundsMin.x)
        {
            localPos.x = localBoundsMin.x;
            movingLeft = false;
        }
        // Wenn das Ziel die rechte Grenze überschreitet, ändere die Richtung nach links
        else if (localPos.x > localBoundsMax.x)
        {
            localPos.x = localBoundsMax.x;
            movingLeft = true;
        }

        // Setzt die neue Position des Ziels zurück in den globalen Raum
        currentPrefab.transform.position = spawnArea.transform.TransformPoint(localPos);
    }

    // Erhöht den Zähler der getroffenen Ziele und überprüft, ob Bewegung des Ziels gestartet werden soll
    public void IncreaseCounter()
    {
        targetsHit++;
        
        if (GameManager.Instance != null)
        {
            GameManager.IncreaseScore();
        }

        if (targetsHit >= 2)
        {
            isMoving = true;
        }
    }
}
