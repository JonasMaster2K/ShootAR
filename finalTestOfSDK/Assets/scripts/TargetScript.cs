using UnityEngine;

public class TargetScript : MonoBehaviour
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

        private void OnCollisionEnter(Collision other)
    {

        //TODO change to targets
        if (other.gameObject.tag =="Ball")
        {
            Destroy(gameObject);
            other.gameObject.GetComponent<BoxCollider>().enabled = false;
        }
    }
}
