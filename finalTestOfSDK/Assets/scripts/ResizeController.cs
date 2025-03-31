using UnityEngine;
using System.Linq;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine.Events;

public class ResizeController : MonoBehaviour
{
    [SerializeField] private GrabInteractable grabInteractable;
    [SerializeField] private HandGrabInteractable leftHandGrabInteractable;
    [SerializeField] private HandGrabInteractable rightHandGrabInteractable;

    public UnityEvent onBallRestored = new();

    private Transform controllerOrHandsAttachPoint;
    private bool isGrabbing;
    private Rigidbody physics;
    private BoxCollider boxCollider;
    private Transform parentTransform;
    private Grabbable grabbable;

    [SerializeField] private GameObject parent;
    
    private Vector3 initialPosition;
    private CreateSphereAtEdges sphereManager;

    void Start()
    {
        parent = transform.parent.gameObject;
        physics = GetComponent<Rigidbody>();
        boxCollider = parent.GetComponent<BoxCollider>(); // BoxCollider des Elternobjekts holen
        sphereManager = parent.GetComponent<CreateSphereAtEdges>(); // Das Script holen, das die Kugeln verwaltet
        parentTransform = parent.transform;

        grabbable = GetComponentInChildren<Grabbable>();
        grabbable.WhenPointerEventRaised += GrabbableOnWhenPointerEventRaised;
        
        initialPosition = transform.position;
    }

    private void GrabbableOnWhenPointerEventRaised(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type == PointerEventType.Select)
        {
            controllerOrHandsAttachPoint = GetActiveInteractorTransform();
        }
    }

    private void Update()
    {
        initialPosition = transform.position;
        if (GameManager.gameState == GameManager.GameStates.SETUP)
        {
            if (grabbable.SelectingPointsCount > 0)
            {
                isGrabbing = true;
                controllerOrHandsAttachPoint = GetActiveInteractorTransform();
                physics.isKinematic = true;

                if (controllerOrHandsAttachPoint != null)
                {
                    transform.position = controllerOrHandsAttachPoint.position;
                    ResizeCollider();
                }
            }
            else if (isGrabbing)
            {
                physics.isKinematic = true;
                isGrabbing = false;
            }
        }
    }
    
    private Transform GetActiveInteractorTransform()
    {
        if (grabInteractable.State == InteractableState.Select)
        {
            return grabInteractable.Interactors.First().transform;
        }
        if (leftHandGrabInteractable.State == InteractableState.Select)
        {
            return leftHandGrabInteractable.Interactors.First().PinchPoint;
        }
        if (rightHandGrabInteractable.State == InteractableState.Select)
        {
            return rightHandGrabInteractable.Interactors.First().PinchPoint;
        }
        return null;
    }

    private void ResizeCollider()
    {
        Vector3 spherePosition = transform.position;
        Vector3 offset = spherePosition - initialPosition;
        offset.z *= -1;

        Vector3 newSize = boxCollider.size + offset;
        newSize.y = 0;

        boxCollider.size = newSize;
        offset.z *= -1;
        offset.y = 0;
        parentTransform.position += offset * 0.5f;

        initialPosition = spherePosition;

        UpdateSpherePositions();
    }

    private void UpdateSpherePositions()
    {
        if (sphereManager == null || sphereManager.sphereReferences.Length == 0)
            return;

        Vector3 size = boxCollider.size;
        Vector3 center = boxCollider.center;

        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;
        float halfDepth = size.z * 0.5f;

        // Neue Positionen für die vier Kugeln berechnen
        Vector3[] newPositions = new Vector3[4]
        {
            new Vector3(center.x + halfWidth, center.y, center.z + halfDepth), // Vorne rechts
            new Vector3(center.x - halfWidth, center.y, center.z + halfDepth), // Vorne links
            new Vector3(center.x + halfWidth, center.y, center.z - halfDepth), // Hinten rechts
            new Vector3(center.x - halfWidth, center.y, center.z - halfDepth)  // Hinten links
        };

        for (int i = 0; i < sphereManager.sphereReferences.Length; i++)
        {
            if (sphereManager.sphereReferences[i] != null)
            {
                sphereManager.sphereReferences[i].transform.localPosition = newPositions[i];
            }
        }
    }
}
