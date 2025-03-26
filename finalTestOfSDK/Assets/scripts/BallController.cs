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

public class BallController : MonoBehaviour
{
    // interactables
    [SerializeField] private GrabInteractable grabInteractable;
    [SerializeField] private HandGrabInteractable leftHandGrabInteractable;
    [SerializeField] private HandGrabInteractable rightHandGrabInteractable;
    

     [SerializeField] private Transform startPosition;
    
    // physics & ball behaviour
    [SerializeField] private float maxPullDistance = 1.0f;
    [SerializeField] private ForceMode launchForceMode = ForceMode.Impulse;
    [SerializeField] private float launchForce = 10f;
    [SerializeField] private float restoreBallDelay = 2.0f;

    // random references
   // [SerializeField] private Transform visuals;
    //[SerializeField] private Transform spawnArea;
   // [SerializeField] private TextMeshPro statsText;
    
    // public events
    public UnityEvent onBallLaunched = new();
    public UnityEvent onBallRestored = new();
    
    // Controller(s) Or Hand(s) attach point for ball positioning
    private Transform controllerOrHandsAttachPoint;
    
    // private variables
    private TrailRenderer trailRenderer;
    private Vector3 pullInitialPosition;
    private bool isGrabbing;
    public bool hasLaunched;
    private Rigidbody physics;
    private Vector3 launchedBallForceDirection;
    private Grabbable grabbable;
    private const string BowlingPinTag = "BowlingPin";
    
    void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.enabled = false;
        physics = GetComponent<Rigidbody>();

        //TODO add this to the instructions, moved grabbable to parent and make it
        //a requireComponent as this will be needed when adding custom hand poses
       grabbable = GetComponentInChildren<Grabbable>();
        grabbable.WhenPointerEventRaised += GrabbableOnWhenPointerEventRaised;

        // let's place the player it at a random position
        transform.position = startPosition.position;
       // initialVisualsRotation = visuals.rotation;
        
        onBallLaunched.AddListener(() =>
        {
            trailRenderer.enabled = true;
        });
        
        onBallRestored.AddListener(() =>
        {
            trailRenderer.enabled = false;
        });
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
        if(GameManager.gameState == GameManager.GameStates.PLAYING){
            if (grabbable.SelectingPointsCount > 0)
            {
                isGrabbing = true;
                controllerOrHandsAttachPoint = GetActiveInteractorTransform();
                physics.isKinematic = true;

                float distance = Vector3.Distance(pullInitialPosition, controllerOrHandsAttachPoint.position);

                if (distance <= maxPullDistance)
                {
                    transform.position = controllerOrHandsAttachPoint.position;
                }
                else
                {
                    Vector3 direction = (controllerOrHandsAttachPoint.position - pullInitialPosition).normalized;
                    transform.position = pullInitialPosition + direction * maxPullDistance;

                    grabInteractable.enabled = false;
                    grabInteractable.enabled = true;

                }
                
            }
            else if (isGrabbing && !hasLaunched)
            {
                onBallLaunched?.Invoke();
                LaunchBall();
            }
        }
    }

   
 

    private void LaunchBall()
    {
        Debug.Log("LAUNCHING BALL");
        Vector3 grabPosition = controllerOrHandsAttachPoint.position;
        Vector3 direction = (pullInitialPosition - grabPosition).normalized;
        physics.isKinematic = false;
        physics.useGravity = true;

        float distance = Mathf.Clamp(Vector3.Distance(pullInitialPosition, grabPosition), 0, maxPullDistance);
        
        // Apply force in the launch direction
        launchedBallForceDirection = direction * (launchForce * (launchForce * distance));
        
        physics.AddForce(launchedBallForceDirection, launchForceMode);
        
      
        ResetBall();
        hasLaunched = true;
        Debug.Log("ball has launched" + hasLaunched);
    }

    private void ResetBall()
    {
        isGrabbing = false;
        StartCoroutine(RestoreBallPositionWithDelay());
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
    
    private IEnumerator RestoreBallPositionWithDelay()
    {
        yield return new WaitForSeconds(restoreBallDelay);
        transform.position = startPosition.position;
        transform.rotation = Quaternion.identity;
        
        // clean up physics
       // visuals.rotation = initialVisualsRotation;
        physics.linearVelocity = Vector3.zero;
        physics.angularVelocity = Vector3.zero;
        physics.isKinematic = true;
        launchedBallForceDirection = Vector3.zero;
        
        hasLaunched = false;
        
        onBallRestored.Invoke();
    }





    private void OnDestroy()
    {
        onBallLaunched.RemoveAllListeners();
        onBallRestored.RemoveAllListeners();
    }
}