using UnityEngine;

public class SpawnTargetsOverTime : MonoBehaviour
{
    public GameObject prefab;
    public BoxCollider spawnArea;
    public float speed = 2f;
    private GameObject currentPrefab;
    public int targetsHit = 0;
    private bool isMoving = false;
    private bool movingLeft = true;
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
        else if (GameManager.gameState == GameManager.GameStates.GAME_OVER){
            if(GameManager.GetScore() > GameManager.GetHighScore()){
                GameManager.SetHighScore(GameManager.GetScore());
            }
        }
    }

    void SpawnPrefab()
    {
        if (spawnArea == null) return;

        Vector3 spawnPosition = new Vector3(
            Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
            spawnArea.bounds.min.y,
            spawnArea.bounds.center.z
        );

        currentPrefab = Instantiate(prefab, spawnPosition, prefab.transform.rotation);
        float parentYRotation = this.transform.parent.eulerAngles.y + 270.0f;
        currentPrefab.transform.rotation = Quaternion.Euler(prefab.transform.rotation.eulerAngles.x, parentYRotation, prefab.transform.rotation.eulerAngles.z);

        if (targetsHit >= 10)
        {
            currentPrefab.transform.localScale = prefab.transform.localScale * 0.4f;
        }
        else if (targetsHit >= 5)
        {
            currentPrefab.transform.localScale = prefab.transform.localScale * 0.7f;
        }
    }

    void MovePrefab()
    {
        if (currentPrefab == null) return;

        TargetScript targetScript = currentPrefab.GetComponent<TargetScript>();
        
        if (targetScript != null && targetScript.IsHit)
        {
            return;
        }

        Vector3 parentForward = transform.parent.forward;
        Vector3 parentRight = transform.parent.right;

        float moveDirection = movingLeft ? -speed : speed;
        Vector3 newPosition = currentPrefab.transform.position + parentRight * moveDirection * Time.deltaTime;

        Vector3 localPos = spawnArea.transform.InverseTransformPoint(newPosition);
        Vector3 localBoundsMin = spawnArea.bounds.min - spawnArea.transform.position;
        Vector3 localBoundsMax = spawnArea.bounds.max - spawnArea.transform.position;

        if (localPos.x < localBoundsMin.x)
        {
            localPos.x = localBoundsMin.x;
            movingLeft = false;
        }
        else if (localPos.x > localBoundsMax.x)
        {
            localPos.x = localBoundsMax.x;
            movingLeft = true;
        }

        currentPrefab.transform.position = spawnArea.transform.TransformPoint(localPos);
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
