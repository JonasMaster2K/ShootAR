using UnityEngine;
using Oculus.Interaction;
using System.Collections.Generic;

public class GameAreaPlacement : MonoBehaviour
{
    public GameObject anchorPrefab;
    public GameObject targetIndicatorPrefab;
    public Vector3 targetScale = Vector3.one;
    public float maxDistance = 10f;
    public float arcHeight = 2f;
    public int curveResolution = 30;
    public LayerMask placementLayer;
    public Color rayColor = Color.green;

    private LineRenderer lineRenderer;
    private GameObject targetIndicator;
    private GameObject currentAnchor;

    // Initialisiert den LineRenderer und lädt gespeicherte Ankerpositionen
    private void Start()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.002f;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = rayColor;
        lineRenderer.positionCount = curveResolution;

        targetIndicator = Instantiate(targetIndicatorPrefab);
        targetIndicator.transform.localScale = targetScale;
        targetIndicator.SetActive(false);
        
        if (LoadAnchorPosition(out Vector3 anchorPosition, out Quaternion anchorRotation))
        {
            SpawnAnchor(anchorPosition, Vector3.up, false);
            GameManager.anchorExists = true;
        }
        else
        {
            GameManager.anchorExists = false;
        }
    }

    // Überprüft den Spielstatus und aktiviert/deaktiviert den Platzierungsmechanismus
    private void Update()
    {
        if (GameManager.gameState == GameManager.GameStates.SETUP)
        {
            lineRenderer.enabled = true;
            targetIndicator.SetActive(true);
            HandlePlacement();
        } 
        else if(GameManager.gameState != GameManager.GameStates.SETUP) 
        {
            lineRenderer.enabled = false;
            targetIndicator.SetActive(false);
        }
    }

    // Handhabt die Platzierung des Ankers basierend auf Controller-Input
    private void HandlePlacement()
    {
        Vector3 controllerPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Quaternion controllerRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
        Vector3 startDirection = controllerRot * Vector3.forward;

        Vector3 hitPoint = Vector3.zero;
        Vector3 hitNormal = Vector3.up;
        bool hitDetected = CalculateParabolicCurve(controllerPos, startDirection, out hitPoint, out hitNormal);

        if (hitDetected)
        {
            targetIndicator.SetActive(true);
            targetIndicator.transform.position = hitPoint;
            targetIndicator.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hitNormal);
        }
        else
        {
            targetIndicator.SetActive(false);
        }

        if (hitDetected && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
        {
            SpawnAnchor(hitPoint, hitNormal, true);
        }
    }

    // Berechnet eine parabolische Wurfkurve und prüft auf Kollisionen mit der Umgebung
    private bool CalculateParabolicCurve(Vector3 startPos, Vector3 startDir, out Vector3 hitPoint, out Vector3 hitNormal)
    {
        List<Vector3> points = new List<Vector3>();
        bool hitDetected = false;
        hitPoint = Vector3.zero;
        hitNormal = Vector3.up;

        for (int i = 0; i < curveResolution; i++)
        {
            float t = (float)i / (curveResolution - 1);
            Vector3 point = ParabolicPoint(startPos, startDir, t);
            points.Add(point);

            if (i > 0)
            {
                Vector3 segmentStart = points[i - 1];
                Vector3 segmentEnd = points[i];
                Vector3 direction = segmentEnd - segmentStart;
                float distance = Vector3.Distance(segmentStart, segmentEnd);

                if (Physics.Raycast(segmentStart, direction, out RaycastHit hit, distance, placementLayer))
                {
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                    hitDetected = true;
                    break;
                }
            }
        }

        lineRenderer.positionCount = points.Count;
        lineRenderer.SetPositions(points.ToArray());

        return hitDetected;
    }

    // Berechnet einen Punkt auf der Parabel basierend auf einem Parameter t
    private Vector3 ParabolicPoint(Vector3 start, Vector3 direction, float t)
    {
        Vector3 horizontal = direction * maxDistance * t;
        Vector3 vertical = Vector3.down * (arcHeight * (t * t));
        return start + horizontal + vertical;
    }

    // Erstellt oder ersetzt einen Anker an der gegebenen Position
    private void SpawnAnchor(Vector3 position, Vector3 normal, bool isUserPlaced = true)
    {
        if (currentAnchor != null)
        {
            Destroy(currentAnchor);
        }

        currentAnchor = Instantiate(anchorPrefab, position, Quaternion.identity);

        Quaternion controllerRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
        Vector3 controllerForward = controllerRot * Vector3.forward;
        controllerForward.y = 0;
        controllerForward.Normalize();

        currentAnchor.transform.rotation = Quaternion.LookRotation(controllerForward, normal);
        
        if (isUserPlaced)
        {
            GameManager.anchorExists = true;
            SaveAnchorPosition(position, currentAnchor.transform.rotation);
        }
    }

    // Speichert die Position und Rotation des Ankers in den PlayerPrefs
    private void SaveAnchorPosition(Vector3 position, Quaternion rotation)
    {
        PlayerPrefs.SetFloat("AnchorPosX", position.x);
        PlayerPrefs.SetFloat("AnchorPosY", position.y);
        PlayerPrefs.SetFloat("AnchorPosZ", position.z);
        PlayerPrefs.SetFloat("AnchorRotX", rotation.x);
        PlayerPrefs.SetFloat("AnchorRotY", rotation.y);
        PlayerPrefs.SetFloat("AnchorRotZ", rotation.z);
        PlayerPrefs.SetFloat("AnchorRotW", rotation.w);
        PlayerPrefs.Save();
    }

    // Lädt eine gespeicherte Ankerposition aus den PlayerPrefs
    private bool LoadAnchorPosition(out Vector3 position, out Quaternion rotation)
    {
        if (PlayerPrefs.HasKey("AnchorPosX"))
        {
            position = new Vector3(
                PlayerPrefs.GetFloat("AnchorPosX"),
                PlayerPrefs.GetFloat("AnchorPosY"),
                PlayerPrefs.GetFloat("AnchorPosZ")
            );
            rotation = new Quaternion(
                PlayerPrefs.GetFloat("AnchorRotX"),
                PlayerPrefs.GetFloat("AnchorRotY"),
                PlayerPrefs.GetFloat("AnchorRotZ"),
                PlayerPrefs.GetFloat("AnchorRotW")
            );
            return true;
        }
        position = Vector3.zero;
        rotation = Quaternion.identity;
        return false;
    }

    private void DeletePlayerPrefs()
    {
        if (PlayerPrefs.HasKey("AnchorPosX"))
        {
            PlayerPrefs.DeleteKey("AnchorPosX");
            PlayerPrefs.DeleteKey("AnchorPosY");
            PlayerPrefs.DeleteKey("AnchorPosZ");
            PlayerPrefs.DeleteKey("AnchorRotX");
            PlayerPrefs.DeleteKey("AnchorRotY");
            PlayerPrefs.DeleteKey("AnchorRotZ");
            PlayerPrefs.DeleteKey("AnchorRotW");
            PlayerPrefs.Save();
        }
    }

    private void OnApplicationQuit() {
        DeletePlayerPrefs();
    }
}