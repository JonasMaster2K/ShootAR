using UnityEngine;
using Oculus.Interaction;
using System;
using System.Collections.Generic;  // Füge dies hinzu für die Liste

public class ParabolicRaycast : MonoBehaviour
{
    public GameObject anchorPrefab;  // Das zu spawnende Objekt
    public GameObject targetIndicatorPrefab; // Das Zielkreis-Objekt
    public Vector3 targetScale = Vector3.one;  // Skalierung des Zielobjekts
    public float maxDistance = 10f;  // Maximale Reichweite des Strahls
    public float arcHeight = 2f;  // Höhe der Parabelkurve
    public int curveResolution = 30;  // Anzahl der Punkte der Kurve
    public LayerMask placementLayer;  // Layer für erlaubte Platzierung
    public Color rayColor = Color.green;  // Farbe des Strahls

    private LineRenderer lineRenderer;
    private GameObject targetIndicator;

    private void Start()
    {
        // LineRenderer für die Parabel hinzufügen
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.startWidth = 0.01f;
        lineRenderer.endWidth = 0.002f;
        lineRenderer.material = new Material(Shader.Find("Unlit/Color"));
        lineRenderer.material.color = rayColor;
        lineRenderer.positionCount = curveResolution;
        lineRenderer.loop = false;

        // Zielkreis-Objekt initialisieren
        targetIndicator = Instantiate(targetIndicatorPrefab);
        targetIndicator.transform.localScale = targetScale;
        targetIndicator.SetActive(false); // Unsichtbar zu Beginn
    }

    private void Update()
    {
        if(GameManager.gameState == GameManager.GameStates.SETUP){
            // Position und Richtung des Controllers holen
            Vector3 controllerPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
            Quaternion controllerRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
            Vector3 startDirection = controllerRot * Vector3.forward;  // Richtung des Strahls

            Vector3 hitPoint = Vector3.zero;
            Vector3 hitNormal = Vector3.up;  // Standardwert
            bool hitDetected = CalculateParabolicCurve(controllerPos, startDirection, out hitPoint, out hitNormal);

            // Zeige das Zielobjekt an der Treffpunkt-Position an
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

            // Platzieren des Objekts, wenn der Trigger gedrückt wird
            if (hitDetected && OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            {
                SpawnAnchor(hitPoint, hitNormal);
            }
        } else {
            targetIndicator.SetActive(false);
            lineRenderer.positionCount = 0;
            lineRenderer.SetPositions(new Vector3[0]);
        }
    }

    private bool CalculateParabolicCurve(Vector3 startPos, Vector3 startDir, out Vector3 hitPoint, out Vector3 hitNormal)
    {
        List<Vector3> points = new List<Vector3>();  // Verwende eine Liste statt eines Arrays
        bool hitDetected = false;
        hitPoint = Vector3.zero;
        hitNormal = Vector3.up; // Standardwert

        // Berechne die Punkte der Parabel
        for (int i = 0; i < curveResolution; i++)
        {
            float t = (float)i / (curveResolution - 1);  // Normierte Zeit (0 bis 1)
            Vector3 point = ParabolicPoint(startPos, startDir, t);
            points.Add(point);  // Füge den Punkt zur Liste hinzu

            // Prüfe, ob ein Treffer vorliegt
            if (i > 0 && Physics.Raycast(points[i - 1], points[i] - points[i - 1], out RaycastHit hit, Vector3.Distance(points[i - 1], points[i]), placementLayer))
            {
                hitPoint = hit.point;
                hitNormal = hit.normal; // Speichert die Oberflächenausrichtung
                hitDetected = true;
                break;  // Stoppe, sobald ein Treffer erkannt wurde
            }
        }

        // Wenn ein Treffer erkannt wurde, aktualisiere den LineRenderer
        if (hitDetected)
        {
            // Die Liste in ein Array umwandeln (begrenzt auf die Anzahl der tatsächlichen Punkte)
            lineRenderer.positionCount = Mathf.Min(points.Count, curveResolution);  // Maximal so viele Punkte wie die Auflösung
            lineRenderer.SetPositions(points.ToArray());  // Konvertiere die Liste in ein Array für den LineRenderer
        }
        else
        {
            lineRenderer.positionCount = 0;  // Keine Linie, wenn kein Treffer
        }

        return hitDetected;
    }

    private Vector3 ParabolicPoint(Vector3 start, Vector3 direction, float t)
    {
        // Berechnet die Parabelbahn: Eine Mischung aus der Richtung + Schwerkraft-Effekt
        Vector3 horizontal = direction * maxDistance * t;
        Vector3 vertical = Vector3.down * (arcHeight * (t * t));
        return start + horizontal + vertical;
    }

    private void SpawnAnchor(Vector3 position, Vector3 normal)
    {
        // Erzeuge das Objekt
        GameObject obj = Instantiate(anchorPrefab, position, Quaternion.identity);
        
        // Hole die Controller-Rotation
        Quaternion controllerRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
        
        // Erhalte den Forward-Vektor des Controllers in Weltkoordinaten, aber mit Y=0
        Vector3 controllerForward = controllerRot * Vector3.forward;
        controllerForward.y = 0;
        controllerForward.Normalize();
        
        // Erstelle eine Rotation, die in die Richtung des Controllers zeigt
        // aber flach auf der Oberfläche bleibt (Y-Achse = normale)
        Quaternion targetRotation = Quaternion.LookRotation(controllerForward, normal);
        
        // Setze die Rotation des Objekts
        obj.transform.rotation = targetRotation;
    }
}
