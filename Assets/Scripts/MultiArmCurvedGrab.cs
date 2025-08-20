using UnityEngine;
using System.Collections.Generic;

public class MultiArmCurvedGrab : MonoBehaviour
{
    [System.Serializable]
    public class Arm
    {
        public string name;
        public Transform armBase;
        public Transform armTip;
        public float moveSpeed = 5f;
        public float curveHeight = 2f;
        public int segments = 20;
        [HideInInspector] public GameObject targetObject;
    }

    public List<Arm> arms;
    public LayerMask grabLayers;

    void Update()
    {
        foreach (Arm arm in arms)
        {
            FindClosestTarget(arm);

            if (arm.targetObject != null)
            {
                // Move the arm tip continuously along the curve
                MoveArmAlongCurve(arm, arm.targetObject.transform.position);

                // Move the player/crane base continuously toward the target
                Vector3 targetPos = arm.targetObject.transform.position;
                transform.position = Vector3.MoveTowards(transform.position, targetPos, arm.moveSpeed * Time.deltaTime);
            }
        }
    }

    void FindClosestTarget(Arm arm)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Grabbable");
        float closestDistance = Mathf.Infinity;
        GameObject closest = null;

        foreach (GameObject obj in targets)
        {
            float distance = Vector3.Distance(arm.armTip.position, obj.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = obj;
            }
        }

        arm.targetObject = closest;
    }

    void MoveArmAlongCurve(Arm arm, Vector3 targetPos)
    {
        Vector3 previousPoint = arm.armTip.position;

        for (int i = 1; i <= arm.segments; i++)
        {
            float t = i / (float)arm.segments;
            Vector3 pointOnCurve = GetParabolaPoint(arm.armTip.position, targetPos, arm.curveHeight, t);

            Debug.DrawLine(previousPoint, pointOnCurve, Color.cyan);

            Vector3 direction = (pointOnCurve - previousPoint).normalized;
            float distance = Vector3.Distance(previousPoint, pointOnCurve);

            if (Physics.Raycast(previousPoint, direction, out RaycastHit hit, distance, grabLayers))
            {
                arm.targetObject = hit.collider.gameObject;
                break;
            }

            previousPoint = pointOnCurve;
        }

        // Continuously move arm tip toward the target
        arm.armTip.position = Vector3.MoveTowards(arm.armTip.position, targetPos, arm.moveSpeed * Time.deltaTime);
    }

    Vector3 GetParabolaPoint(Vector3 start, Vector3 end, float height, float t)
    {
        Vector3 mid = Vector3.Lerp(start, end, t);
        float parabola = 4 * height * t * (1 - t);
        mid.y += parabola;
        return mid;
    }
}
