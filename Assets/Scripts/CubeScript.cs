using UnityEngine;

public class CubeScript : MonoBehaviour
{
    public Transform rayOrigin1;
    public Transform rayOrigin2;
    public Transform rayOrigin3;
    public Transform rayOrigin4;

    public GameObject Arm1;
    public GameObject Arm2;
    public GameObject Arm3;
    public GameObject Arm4;

    public float rayDistance = 1000f; // max distance

    private void FixedUpdate()
    {
        CastRay(rayOrigin1, Color.yellow,Arm1);
        CastRay(rayOrigin2, Color.green,Arm2);
        CastRay(rayOrigin3, Color.red,Arm3);
        CastRay(rayOrigin4, Color.blue,Arm4);
    }

    private void CastRay(Transform origin, Color debugColor,GameObject arm)
    {
        if (origin == null) return;

        RaycastHit hit;
        if (Physics.Raycast(origin.position, -origin.up, out hit, rayDistance))
        {
            Debug.DrawRay(origin.position, -origin.up* hit.distance, debugColor);
            arm.transform.position = hit.point;
            Debug.Log($"{origin.name} did hit: {hit.collider.name}");
        }
        else
        {
            //Debug.DrawRay(origin.position, origin.forward * rayDistance, debugColor);
            Debug.Log($"{origin.name} did not hit");
        }
    }
}
