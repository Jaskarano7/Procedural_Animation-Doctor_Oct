using UnityEngine;

[System.Serializable]
public class Arm
{
    public string name;
    public Transform rayOrigin;
    public Transform armTarget;
    public Transform armTip;
    public float rayDistance = 10f;
    public LayerMask hitLayers;

    [HideInInspector] public float stepT;
    [HideInInspector] public float idleTimer = 0f;
    [HideInInspector] public bool hasLockedPoint = false;
    [HideInInspector] public bool hasRested = true;
    [HideInInspector] public bool isStepping = false;
    [HideInInspector] public Vector3 lockedPosition;
    [HideInInspector] public Vector3 oldLockedPosition;
    [HideInInspector] public Vector3 currentHitPoint;
}