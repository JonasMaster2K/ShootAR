using UnityEngine;
using Oculus.Interaction;
using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;

public class CustomSpawnZoneCreator : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    
    private List<MRUKAnchor> anchors = new List<MRUKAnchor>();
    private float maxRayDistance = 10f;

    private void Start()
    {
        // LineRenderer-Konfiguration
        if (lineRenderer != null)
        {
            lineRenderer.positionCount = 2;
        }
        else
        {
            Debug.LogError("LineRenderer wurde nicht zugewiesen!");
        }
    }

    private void Update()
    {
        // Aktualisiere die Ankerliste in jedem Update
        UpdateAnchors();
        
        if (lineRenderer == null)
            return;
            
        // Hole die Position und Rotation des Controllers
        Vector3 controllerPos = OVRInput.GetLocalControllerPosition(OVRInput.Controller.RTouch);
        Quaternion controllerRot = OVRInput.GetLocalControllerRotation(OVRInput.Controller.RTouch);
        Vector3 rayDirection = controllerRot * Vector3.forward;
        
        // Setze den Startpunkt des LineRenderers auf die Controller-Position
        lineRenderer.SetPosition(0, controllerPos);
        
        // Erstelle einen Ray vom Controller aus
        Ray ray = new Ray(controllerPos, rayDirection);
        
        // Standardmäßig geht der Strahl bis zur maximalen Distanz
        float hitDistance = maxRayDistance;
        
        // Überprüfe, ob ein Anker getroffen wurde
        RaycastHit hitInfo;
        bool hitAnyAnchor = false;
        
        foreach (var anchor in anchors)
        {
            if (anchor.Raycast(ray, maxRayDistance, out hitInfo))
            {
                hitAnyAnchor = true;
                
                // Wenn dieser Treffer näher ist als der bisherige, aktualisiere die Distanz
                if (hitInfo.distance < hitDistance)
                {
                    hitDistance = hitInfo.distance;
                    Debug.Log($"Raycast trifft Anker: {anchor.gameObject.name} bei Distanz: {hitDistance}");
                }
            }
        }
        
        // Setze den Endpunkt des LineRenderers basierend auf dem Raycast-Ergebnis
        Vector3 endPoint = controllerPos + rayDirection * hitDistance;
        lineRenderer.SetPosition(1, endPoint);
        
        if (!hitAnyAnchor)
        {
            Debug.Log("Kein Anker getroffen, Strahl geht zur maximalen Distanz: " + maxRayDistance);
        }
    }
    
    // Aktualisiere die Liste aller Anker
    private void UpdateAnchors()
    {
        anchors.Clear();
        MRUKRoom room = FindObjectOfType<MRUKRoom>();
        
        if (room != null && room.Anchors != null)
        {
            anchors = new List<MRUKAnchor>(room.Anchors);
            Debug.Log($"Gefundene Anker: {anchors.Count}");
        }
    }
}