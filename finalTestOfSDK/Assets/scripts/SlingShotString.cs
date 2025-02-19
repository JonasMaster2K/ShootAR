using UnityEngine;

public class SlingShotString : MonoBehaviour
{
    [SerializeField] private Transform LeftPoint;
    [SerializeField] private Transform RightPoint;
    [SerializeField] private Transform PositionAtBall1;
    [SerializeField] private Transform PositionAtBall2;

    [SerializeField] private Transform PositionAtBall3;

    
    public BallController BallController;


    public bool BallWasReleased;
    public bool NewDragStarted;
    
    LineRenderer StringLineRenderer;
    void Start()
    {
        BallWasReleased = false;

        StringLineRenderer = GetComponent<LineRenderer>();
        StringLineRenderer.positionCount = 5;
    }

    // Update is called once per frame
    void Update()
    {
        if(BallController.hasLaunched){
            StringLineRenderer.positionCount = 2;
            StringLineRenderer.SetPositions(new Vector3[2] { LeftPoint.position, RightPoint.position });
        }else{
            StringLineRenderer.positionCount = 5;
            StringLineRenderer.SetPositions(new Vector3[5] { LeftPoint.position, PositionAtBall2.position, PositionAtBall1.position,PositionAtBall3.position, RightPoint.position });
        }
    }
}
