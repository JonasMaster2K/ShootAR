using UnityEngine;

public class AttachToGameAnchor : MonoBehaviour
{    // Update is called once per frame
    void Update()
    {
        GameObject gameAnchor = GameObject.Find("GameAnchor");
        if (gameAnchor != null)
        {
            transform.position = gameAnchor.transform.position;
            transform.rotation = gameAnchor.transform.rotation;
            enabled = false;
        }
    }
}
