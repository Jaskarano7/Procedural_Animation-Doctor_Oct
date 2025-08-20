using System.Collections.Generic;
using UnityEngine;

public class TestPlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Leg Settings")]
    public float stepSpeed = 5f;
    public float legDistance = 2f;
    public float stepHeight = 0.5f;
    public List<Arm> arms;

    private bool isStepping = false;
    private int nextLegIndex = 0;

    private DocInputAction action;
    private Quaternion lastRotation;

    private void Awake() => action = new DocInputAction();
    private void OnEnable() => action.Enable();
    private void OnDisable() => action.Disable();

    private void Start()
    {
        lastRotation = transform.rotation;
        InitializeLegs();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        UpdateLegs();
        lastRotation = transform.rotation;
    }

    #region Initialization
    private void InitializeLegs()
    {
        foreach (var arm in arms)
        {
            if (Physics.Raycast(arm.rayOrigin.position, Vector3.down, out RaycastHit hit, Mathf.Infinity, arm.hitLayers))
            {
                arm.lockedPosition = hit.point;
                arm.oldLockedPosition = hit.point;
                arm.hasLockedPoint = true;
                arm.isStepping = false;
                arm.stepT = 1f;
                arm.armTarget.transform.position = hit.point;
                arm.hasRested = true;
            }
        }
    }
    #endregion

    #region Movement
    private void HandleMovement()
    {
        Vector2 input = action.Movement.Move.ReadValue<Vector2>();
        Vector3 move = (transform.right * -input.x + transform.forward * -input.y) * moveSpeed * Time.fixedDeltaTime;
        transform.position += move;

        if (move.sqrMagnitude > 0.001f)
        {
            foreach (var arm in arms)
                arm.hasRested = false;
        }
    }
    #endregion

    #region Leg Logic
    private void UpdateLegs()
    {
        for (int i = 0; i < arms.Count; i++)
        {
            var arm = arms[i];

            if (i == nextLegIndex)
            {
                Vector2 input = action.Movement.Move.ReadValue<Vector2>();
                if (!isStepping) CastParabolaRay(arm, -input);
            }

            if (arm.isStepping) UpdateStep(arm);
            else if (arm.hasLockedPoint) arm.armTarget.transform.position = arm.lockedPosition;
        }
    }

    private void UpdateStep(Arm arm)
    {
        isStepping = true;
        arm.stepT += Time.fixedDeltaTime * stepSpeed;
        float t = Mathf.Clamp01(arm.stepT);

        Vector3 pos = Vector3.Lerp(arm.oldLockedPosition, arm.lockedPosition, t);
        pos.y += Mathf.Sin(t * Mathf.PI) * stepHeight;
        arm.armTarget.transform.position = pos;

        if (t >= 1f)
        {
            arm.armTarget.transform.position = arm.lockedPosition;
            arm.isStepping = false;
            nextLegIndex = (nextLegIndex + 1) % arms.Count;
            isStepping = false;
        }
    }
    #endregion

    #region Raycasting
    private void CastParabolaRay(Arm arm, Vector2 inputDir)
    {
        Vector3 startPos = arm.rayOrigin.position;
        Vector3 moveDir = (transform.right * inputDir.x + transform.forward * inputDir.y).normalized;
        if (moveDir.sqrMagnitude < 0.01f) moveDir = -transform.up;

        float velocity = 2f;
        Vector3 gravity = Vector3.down * 9.8f;

        Vector3 prevPos = startPos;
        for (int i = 1; i <= 25; i++)
        {
            float t = i * 0.1f;
            Vector3 nextPos = startPos + moveDir * velocity * t + 0.5f * gravity * t * t;

            Debug.DrawLine(prevPos, nextPos, Color.cyan);

            if (Physics.Linecast(prevPos, nextPos, out RaycastHit hit, arm.hitLayers))
            {
                HandleHit(arm, hit.point, inputDir);
                return;
            }

            prevPos = nextPos;
        }
    }

    private void HandleHit(Arm arm, Vector3 targetPoint, Vector2 inputDir)
    {
        float rotationDelta = Quaternion.Angle(lastRotation, transform.rotation);
        bool rotated = rotationDelta > 1f;
        bool isMoving = inputDir.magnitude > 0.001f;

        if (!arm.hasLockedPoint)
        {
            SetLockedPosition(arm, targetPoint);
            return;
        }

        float dist = Vector3.Distance(arm.lockedPosition, targetPoint);
        if ((dist > legDistance || rotated || (!isMoving && !arm.hasRested)) && !arm.isStepping)
        {
            arm.oldLockedPosition = arm.armTarget.transform.position;
            SetLockedPosition(arm, targetPoint);
            arm.stepT = 0f;
            arm.isStepping = true;
            arm.hasRested = true;
        }
    }

    private void SetLockedPosition(Arm arm, Vector3 position)
    {
        arm.lockedPosition = position;
        arm.hasLockedPoint = true;
        arm.armTarget.transform.position = position;
    }
    #endregion
}
