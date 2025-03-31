using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SocketManager : MonoBehaviour
{
    [System.Serializable]
    public class Socket
    {
        public Transform socketTransform;
        public GameObject attachedObject;
        [Range(0f, 1f)]
        public float relativePositionX = 0.5f;
        [Range(0f, 1f)]
        public float relativePositionY = 0.5f;
        [Range(0f, 1f)]
        public float relativePositionZ = 0.5f;
        public Vector3 localOffset = Vector3.zero;
        [HideInInspector] public float attachedObjectOriginalY;
    }

    [Header("Socket Configuration")]
    [SerializeField] private Socket[] sockets = new Socket[3];
    [SerializeField] private Transform socketsParent;
    [SerializeField] private GameObject socketVisualPrefab;
    
    [Header("References")]
    [SerializeField] private BoxCollider boxCollider;
    
    [Header("Visibility")]
    [SerializeField] private bool showSockets = true;
    
    [Header("Position Options")]
    [SerializeField] private bool preserveYPosition = true;

    private Vector3 previousSize;
    private Vector3 previousCenter;
    private bool initialized = false;
    private GameObject[] socketVisuals;

    private void OnValidate()
    {
        // Update socket visibility in editor
        if (initialized && Application.isEditor)
        {
            UpdateSocketVisibility();
        }
    }

    private void Awake()
    {
        if (socketsParent == null)
        {
            socketsParent = new GameObject("Sockets").transform;
            socketsParent.SetParent(transform);
            socketsParent.localPosition = Vector3.zero;
        }

        if (boxCollider == null)
        {
            boxCollider = GetComponent<BoxCollider>();
            if (boxCollider == null)
            {
                Debug.LogError("No BoxCollider found on " + gameObject.name);
                return;
            }
        }

        socketVisuals = new GameObject[sockets.Length];
        InitializeSockets();
    }

    private void Start()
    {
        previousSize = boxCollider.size;
        previousCenter = boxCollider.center;
        initialized = true;
        
        // Set initial visibility
        UpdateSocketVisibility();
    }

    private void InitializeSockets()
    {
        for (int i = 0; i < sockets.Length; i++)
        {
            if (sockets[i].socketTransform == null)
            {
                GameObject socketObj = new GameObject("Socket_" + (i + 1));
                socketObj.transform.SetParent(socketsParent);
                sockets[i].socketTransform = socketObj.transform;

                // Create visual indicator for the socket
                if (socketVisualPrefab != null)
                {
                    GameObject visual = Instantiate(socketVisualPrefab, socketObj.transform);
                    visual.transform.localPosition = Vector3.zero;
                    visual.transform.localRotation = Quaternion.identity;
                    socketVisuals[i] = visual;
                }
            }
            else if (socketVisuals[i] == null && socketVisualPrefab != null)
            {
                // If socket exists but visual doesn't
                GameObject visual = Instantiate(socketVisualPrefab, sockets[i].socketTransform);
                visual.transform.localPosition = Vector3.zero;
                visual.transform.localRotation = Quaternion.identity;
                socketVisuals[i] = visual;
            }

            // Set initial position for socket
            UpdateSocketPosition(i);
        }
    }

    private void Update()
    {
        if (!initialized) return;

        // Check if box collider size or center changed
        if (previousSize != boxCollider.size || previousCenter != boxCollider.center)
        {
            UpdateAllSocketPositions();
            previousSize = boxCollider.size;
            previousCenter = boxCollider.center;
        }
    }

    private void LateUpdate()
    {
        // Double-check in LateUpdate to ensure we catch any resizing that happens during the frame
        if (initialized && (previousSize != boxCollider.size || previousCenter != boxCollider.center))
        {
            UpdateAllSocketPositions();
            previousSize = boxCollider.size;
            previousCenter = boxCollider.center;
        }

        // Update attached objects to make sure they follow their sockets
        UpdateAttachedObjects();
    }

    private void UpdateAllSocketPositions()
    {
        for (int i = 0; i < sockets.Length; i++)
        {
            UpdateSocketPosition(i);
        }
    }

    private void UpdateSocketPosition(int socketIndex)
    {
        if (socketIndex >= sockets.Length || sockets[socketIndex].socketTransform == null) return;

        Socket socket = sockets[socketIndex];
        
        // Calculate socket position based on relative positioning within box collider bounds
        Vector3 size = boxCollider.size;
        Vector3 center = boxCollider.center;
        
        // Map from 0-1 range to actual position within bounds
        Vector3 relativePos = new Vector3(
            Mathf.Lerp(-size.x * 0.5f, size.x * 0.5f, socket.relativePositionX),
            Mathf.Lerp(-size.y * 0.5f, size.y * 0.5f, socket.relativePositionY),
            Mathf.Lerp(-size.z * 0.5f, size.z * 0.5f, socket.relativePositionZ)
        );
        
        // Set socket position in local space
        socket.socketTransform.localPosition = center + relativePos + socket.localOffset;
    }

    private void UpdateAttachedObjects()
    {
        for (int i = 0; i < sockets.Length; i++)
        {
            if (sockets[i].attachedObject != null && sockets[i].socketTransform != null)
            {
                Vector3 newPosition = sockets[i].socketTransform.position;
                
                // Preserve Y position if enabled
                if (preserveYPosition)
                {
                    newPosition.y = sockets[i].attachedObjectOriginalY;
                }
                
                // Update object position with preserved Y if needed
                sockets[i].attachedObject.transform.position = newPosition;
            }
        }
    }

    /// <summary>
    /// Updates the visibility of all socket visuals based on showSockets flag
    /// </summary>
    private void UpdateSocketVisibility()
    {
        if (socketVisuals == null) return;
        
        for (int i = 0; i < socketVisuals.Length; i++)
        {
            if (socketVisuals[i] != null)
            {
                socketVisuals[i].SetActive(showSockets);
            }
        }
    }

    /// <summary>
    /// Sets whether socket visuals should be shown or hidden
    /// </summary>
    /// <param name="show">True to show sockets, false to hide</param>
    public void SetSocketVisibility(bool show)
    {
        showSockets = show;
        UpdateSocketVisibility();
    }

    /// <summary>
    /// Attaches a GameObject to the specified socket
    /// </summary>
    /// <param name="socketIndex">Index of the socket (0-2)</param>
    /// <param name="obj">GameObject to attach</param>
    /// <returns>True if attachment was successful</returns>
    public bool AttachToSocket(int socketIndex, GameObject obj)
    {
        if (socketIndex < 0 || socketIndex >= sockets.Length || obj == null)
            return false;

        sockets[socketIndex].attachedObject = obj;
        
        // Store the original Y position of the object
        sockets[socketIndex].attachedObjectOriginalY = obj.transform.position.y;
        
        // Set initial position (X and Z only if Y preservation is enabled)
        if (preserveYPosition)
        {
            Vector3 socketPos = sockets[socketIndex].socketTransform.position;
            obj.transform.position = new Vector3(socketPos.x, obj.transform.position.y, socketPos.z);
        }
        else
        {
            obj.transform.position = sockets[socketIndex].socketTransform.position;
        }
        
        return true;
    }

    /// <summary>
    /// Detaches any GameObject from the specified socket
    /// </summary>
    /// <param name="socketIndex">Index of the socket (0-2)</param>
    /// <returns>The detached GameObject or null if socket was empty</returns>
    public GameObject DetachFromSocket(int socketIndex)
    {
        if (socketIndex < 0 || socketIndex >= sockets.Length)
            return null;

        GameObject detachedObject = sockets[socketIndex].attachedObject;
        sockets[socketIndex].attachedObject = null;
        return detachedObject;
    }

    /// <summary>
    /// Gets the position of a socket
    /// </summary>
    /// <param name="socketIndex">Index of the socket (0-2)</param>
    /// <returns>World position of the socket</returns>
    public Vector3 GetSocketPosition(int socketIndex)
    {
        if (socketIndex < 0 || socketIndex >= sockets.Length)
            return Vector3.zero;

        return sockets[socketIndex].socketTransform.position;
    }

    /// <summary>
    /// Sets the relative position of a socket within the box collider (0-1 range for each axis)
    /// </summary>
    /// <param name="socketIndex">Index of the socket (0-2)</param>
    /// <param name="relativePos">Relative position (x,y,z values between 0-1)</param>
    public void SetSocketRelativePosition(int socketIndex, Vector3 relativePos)
    {
        if (socketIndex < 0 || socketIndex >= sockets.Length)
            return;

        sockets[socketIndex].relativePositionX = Mathf.Clamp01(relativePos.x);
        sockets[socketIndex].relativePositionY = Mathf.Clamp01(relativePos.y);
        sockets[socketIndex].relativePositionZ = Mathf.Clamp01(relativePos.z);
        
        UpdateSocketPosition(socketIndex);
    }

    /// <summary>
    /// Checks if a socket has an object attached
    /// </summary>
    /// <param name="socketIndex">Index of the socket (0-2)</param>
    /// <returns>True if there's an object attached to the socket</returns>
    public bool IsSocketOccupied(int socketIndex)
    {
        if (socketIndex < 0 || socketIndex >= sockets.Length)
            return false;

        return sockets[socketIndex].attachedObject != null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SocketManager))]
public class SocketManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        SocketManager manager = (SocketManager)target;
        
        // Draw the default inspector
        DrawDefaultInspector();
        
        // Add button to toggle visibility
        EditorGUILayout.Space();
        if (GUILayout.Button("Toggle Socket Visibility"))
        {
            // Use reflection to access the private field and toggle it
            var showSocketsProperty = serializedObject.FindProperty("showSockets");
            showSocketsProperty.boolValue = !showSocketsProperty.boolValue;
            serializedObject.ApplyModifiedProperties();
            
            // Call the public method to update visibility
            manager.SetSocketVisibility(showSocketsProperty.boolValue);
        }
    }
}
#endif