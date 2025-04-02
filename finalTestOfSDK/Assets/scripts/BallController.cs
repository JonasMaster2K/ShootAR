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
    // Interaktive Objekte für das Greifen des Balls
    [SerializeField] private GrabInteractable grabInteractable;              // Referenz für das Greifen des Balls durch den Controller
    [SerializeField] private HandGrabInteractable leftHandGrabInteractable;  // Referenz für das Greifen des Balls mit der linken Hand
    [SerializeField] private HandGrabInteractable rightHandGrabInteractable; // Referenz für das Greifen des Balls mit der rechten Hand

    [SerializeField] private Transform startPosition;                       // Startposition des Balls
    
    // Physik und Ballverhalten
    [SerializeField] private float maxPullDistance = 1.0f;                 // Maximale Zugdistanz, in der der Ball bewegt werden kann
    [SerializeField] private ForceMode launchForceMode = ForceMode.Impulse; // Die Art des angewandten Launch-Kraftmodus
    [SerializeField] private float launchForce = 10f;                        // Stärke der geworfenen Ballkraft
    [SerializeField] private float restoreBallDelay = 2.0f;                  // Verzögerung, bevor der Ball nach dem Wurf wiederhergestellt wird
    
    // Öffentliche Ereignisse
    public UnityEvent onBallLaunched = new();  // Ereignis, das ausgelöst wird, wenn der Ball geworfen wurde
    public UnityEvent onBallRestored = new();  // Ereignis, das ausgelöst wird, wenn der Ball wiederhergestellt wurde
    
    // Controller- oder Hand-Befestigungspunkt für die Positionierung des Balls
    private Transform controllerOrHandsAttachPoint;
    
    // Private Variablen
    private TrailRenderer trailRenderer;    // TrailRenderer für die Anzeige des Bewegungsstils des Balls
    private Vector3 pullInitialPosition;    // Anfangsposition des Balls beim Greifen
    private bool isGrabbing;                // Status, ob der Ball gerade gehalten wird
    public bool hasLaunched;                // Status, ob der Ball bereits geworfen wurde
    private Rigidbody physics;              // Referenz auf den Rigidbody des Balls für Physik
    private BoxCollider collider;           // BoxCollider für Kollisionserkennung des Balls
    private Vector3 launchedBallForceDirection;  // Richtung der angewandten Kraft beim Wurf
    private Grabbable grabbable;            // Referenz auf das Grabbable-Objekt des Balls
    
    void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        trailRenderer.enabled = false;                
        physics = GetComponent<Rigidbody>();          
        collider = GetComponent<BoxCollider>();       

        grabbable = GetComponentInChildren<Grabbable>();
        grabbable.WhenPointerEventRaised += GrabbableOnWhenPointerEventRaised;

        transform.position = startPosition.position;
        
        onBallLaunched.AddListener(() =>
        {
            trailRenderer.enabled = true;
        });
        
        onBallRestored.AddListener(() =>
        {
            trailRenderer.enabled = false;
        });
    }
    
    // Wird ausgelöst, wenn der Ball von einem Zeiger (Pointer) gehoben wird
    private void GrabbableOnWhenPointerEventRaised(PointerEvent pointerEvent)
    {
        if (pointerEvent.Type == PointerEventType.Select)
        {
            pullInitialPosition = transform.position;
        }
    }

    private void Update()
    {
        if (GameManager.gameState == GameManager.GameStates.PLAYING)
        {
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
                    // Wenn die maximale Distanz überschritten wird, bleibe an der maximalen Zugdistanz
                    Vector3 direction = (controllerOrHandsAttachPoint.position - pullInitialPosition).normalized;
                    transform.position = pullInitialPosition + direction * maxPullDistance;

                    // Verhindert, dass der Ball ständig bewegt wird, wenn der Controller die maximale Distanz überschreitet
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

    // Wirft den Ball in die Richtung, in der er gezogen wurde
    private void LaunchBall()
    {
        Debug.Log("LAUNCHING BALL");
        Vector3 grabPosition = controllerOrHandsAttachPoint.position;
        Vector3 direction = (pullInitialPosition - grabPosition).normalized;
        physics.isKinematic = false;  
        physics.useGravity = true;    

        float distance = Mathf.Clamp(Vector3.Distance(pullInitialPosition, grabPosition), 0, maxPullDistance);
        
        launchedBallForceDirection = direction * (launchForce * (launchForce * distance));
        
        physics.AddForce(launchedBallForceDirection, launchForceMode);
        
        ResetBall();
        hasLaunched = true;
        Debug.Log("ball has launched" + hasLaunched);
    }

    // Setzt den Ball nach dem Wurf zurück
    private void ResetBall()
    {
        isGrabbing = false;
        StartCoroutine(RestoreBallPositionWithDelay());
    }
    
    // Bestimmt, welcher Controller oder welche Hand gerade den Ball hält
    private Transform GetActiveInteractorTransform()
    {
        Transform currentTransform = null;

        // Wenn der Ball von einem der Controller oder einer Hand gehalten wird, gib die zugehörige Transform-Referenz zurück
        if (grabInteractable.State == InteractableState.Select)
        {
            currentTransform = grabInteractable.Interactors.First().transform;
        }
        else if (leftHandGrabInteractable.State == InteractableState.Select)
        {
            currentTransform = leftHandGrabInteractable.Interactors.First().PinchPoint;
        }
        else if (rightHandGrabInteractable.State == InteractableState.Select)
        {
            currentTransform = rightHandGrabInteractable.Interactors.First().PinchPoint;
        }
        return currentTransform;
    }
    
    // Stellt den Ball an die Startposition zurück, nachdem eine Verzögerung abgelaufen ist
    private IEnumerator RestoreBallPositionWithDelay()
    {
        yield return new WaitForSeconds(restoreBallDelay);
        transform.position = startPosition.position;
        transform.rotation = Quaternion.identity;   
        
        physics.linearVelocity = Vector3.zero;  
        physics.angularVelocity = Vector3.zero; 
        physics.isKinematic = true;             
        launchedBallForceDirection = Vector3.zero;
        
        hasLaunched = false;
        collider.enabled = true;
        
        onBallRestored.Invoke();
    }

    // Wird aufgerufen, wenn das Skript oder das GameObject zerstört wird
    private void OnDestroy()
    {
        onBallLaunched.RemoveAllListeners();
        onBallRestored.RemoveAllListeners();
    }
}
