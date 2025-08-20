using UnityEngine;

[System.Serializable]
public class Arm
{
    public string name;
    public Transform rayOrigin;
    public Transform armTarget;
    public Transform armTip;
    public float detectRadius = 2f;
    public float rayDistance = 10f;
    public float moveSpeed = 5f;
    public LayerMask hitLayers;
    public LayerMask grabLayers;

    public Vector3 lockedPosition;
    public bool hasLockedPoint = false;
    public Vector3 oldLockedPosition;
    public float stepT;
    public bool isStepping = false;

    public bool isOnWall = false;   // when forward ray hits
    public Vector3 targetPosition;

    public bool hasRested = true;

    public float restDelay = 0.3f; // delay before returning to rest
    public float restTimer = 0f;
}