using UnityEngine;

public class SpawnTargetsOverTime : MonoBehaviour
{
 public GameObject prefab;
    public BoxCollider2D spawnArea;
    public float speed = 2f; // Geschwindigkeit der horizontalen Bewegung

    private GameObject currentPrefab;
    public int targetsHit = 0;
    private bool isMoving = false;
    private bool movingLeft = true;

    void Start()
    {
        SpawnPrefab();
    }

    void Update()
    {
        if (currentPrefab == null)
        {
            IncreaseCounter();
            SpawnPrefab();
        }
        else if (isMoving && currentPrefab != null)
        {
            MovePrefab();
        }
    }

    void SpawnPrefab()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
            Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y),
            spawnArea.transform.position.z
        );

        currentPrefab = Instantiate(prefab, spawnPosition, prefab.transform.rotation);

        if (targetsHit >= 5)
        {
            currentPrefab.transform.localScale = prefab.transform.localScale * 0.7f; // Skaliere um 30 % kleiner
        }
        else if (targetsHit >= 10)
        {
            currentPrefab.transform.localScale = prefab.transform.localScale * 0.4f; // Skaliere um 30 % kleiner
        }
    }

    void MovePrefab()
    {
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

        if (targetsHit >= 2)
        {
            isMoving = true;
        }
    }
}
