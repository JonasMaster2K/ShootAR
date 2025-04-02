using UnityEngine;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using UnityEngine.Events;
using Random = UnityEngine.Random;
using Oculus.Interaction;

public class GameArea : MonoBehaviour
{
    [SerializeField] private GrabInteractable grabInteractable;
    [SerializeField] private HandGrabInteractable leftHandGrabInteractable;
    [SerializeField] private HandGrabInteractable rightHandGrabInteractable;
    [SerializeField] private Transform startPosition;
    private Transform controllerOrHandsAttachPoint;
    private bool isGrabbing;
    private Rigidbody physics;
    private BoxCollider collider;
    private Grabbable grabbable;
    private Vector3 pullInitialPosition;

    void Start()
    {
        physics = GetComponent<Rigidbody>();
        collider = GetComponent<BoxCollider>();

        grabbable = GetComponentInChildren<Grabbable>();
        grabbable.WhenPointerEventRaised += GrabbableOnWhenPointerEventRaised;

        transform.position = startPosition.position;
    }
    
    private void GrabbableOnWhenPointerEventRaised(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type == PointerEventType.Select)
        {
            pullInitialPosition = transform.position;
        }
    }

    private void Update()
    {
        if(GameManager.gameState == GameManager.GameStates.SETUP){
            if (grabbable.SelectingPointsCount > 0)
            {
                isGrabbing = true;
                controllerOrHandsAttachPoint = GetActiveInteractorTransform();
                physics.isKinematic = true;

                float distance = Vector3.Distance(pullInitialPosition, controllerOrHandsAttachPoint.position);

                transform.position = controllerOrHandsAttachPoint.position;
                
            }
        }
    }

    private Transform GetActiveInteractorTransform()
    {
        Transform currentTransform = null;

        if (grabInteractable.State == InteractableState.Select)
        {
            currentTransform = grabInteractable.Interactors.First()
                .transform;
        }
        else if (leftHandGrabInteractable.State == InteractableState.Select)
        {
            currentTransform = leftHandGrabInteractable.Interactors.First()
                .PinchPoint;
        }
        else if (rightHandGrabInteractable.State == InteractableState.Select)
        {
            currentTransform = rightHandGrabInteractable.Interactors.First()
                .PinchPoint;
        }
        return currentTransform;
    }
}