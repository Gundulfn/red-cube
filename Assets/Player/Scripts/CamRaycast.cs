using UnityEngine;

public class CamRaycast : MonoBehaviour
{
    private RaycastHit hit;
    public bool isHit;

    private float rayDistance = 5;
    private int layerMask = 1 << 8;  // for which layers will be detected
    private Vector3 hitFacePos;

    private GameObject currentHit;
    private Color startColor = Color.white;
    
    void FixedUpdate()
    {
        if(Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, rayDistance, layerMask) && hit.collider.CompareTag("Cube"))
        {
            Vector3 distanceVector = transform.position - hit.point;

            if (currentHit != null)
                currentHit.GetComponent<Renderer>().material.color = startColor;

            currentHit = hit.collider.gameObject;

            startColor = currentHit.GetComponent<Renderer>().material.color;
            
            currentHit.GetComponent<Renderer>().material.color = Color.yellow;
            
            isHit = true;
        }
        else
        {
            if (currentHit != null) {
                currentHit.GetComponent<Renderer>().material.color = startColor;
                currentHit = null;
                startColor = Color.white;
            }

            isHit = false;
        }
    }

    public Vector3 GetHitFacePos()
    {
        return hit.collider.transform.localPosition + hit.normal;
    }

    public GameObject GetHitObject()
    {
        return currentHit;
    }
}
