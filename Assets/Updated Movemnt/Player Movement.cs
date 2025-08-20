using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float stepSpeed = 5f;
    public float legDistance = 2f;
    public float stepHeight = 0.5f; // how high the leg lifts
    public List<Arm> arms;

    public bool LeftArmOnGround;
    public bool RightArmOnGround;

    private DocInputAction action;
    private int nextLegIndex = 0;   // keeps track of which leg steps next
    private bool isStepping = false; // block other legs while one is stepping

    private Rigidbody rb;
    private Quaternion lastRotation; //  to detect rotation

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        action = new DocInputAction();
    }

    private void OnEnable()
    {
        action.Enable();
    }

    private void OnDisable()
    {
        action.Disable();
    }

    private void Start()
    {
        lastRotation = transform.rotation;

        // Place all legs on the ground at the beginning
        foreach (var arm in arms)
        {
            RaycastHit hit;
            if (Physics.Raycast(arm.rayOrigin.position, Vector3.down, out hit, Mathf.Infinity, arm.hitLayers))
            {
                arm.lockedPosition = hit.point;
                arm.oldLockedPosition = hit.point;
                arm.hasLockedPoint = true;
                arm.isStepping = false;
                arm.stepT = 1f; // fully completed step
                arm.armTarget.transform.position = hit.point;
            }
        }
    }

    private void FixedUpdate()
    {
        Move();

        //FindPlayerPos(arms[0], arms[1]);

        for (int i = 0; i < arms.Count; i++)
        {
            var arm = arms[i];

            // Only update the leg that's supposed to step
            if (i == nextLegIndex)
            {
                Vector2 input = action.Movement.Move.ReadValue<Vector2>();
                CastParabolaRay(arm, -input);
            }

            if (arm.isStepping)
            {
                isStepping = true;

                // Progress step
                arm.stepT += Time.fixedDeltaTime * stepSpeed;
                float t = Mathf.Clamp01(arm.stepT);

                Vector3 pos = Vector3.Lerp(arm.oldLockedPosition, arm.lockedPosition, t);

                // Add arc on Y
                float arc = Mathf.Sin(t * Mathf.PI) * stepHeight;
                pos.y += arc;

                arm.armTarget.transform.position = pos;

                if (t >= 1f)
                {
                    // Step finished  snap to ground
                    arm.armTarget.transform.position = arm.lockedPosition;
                    arm.isStepping = false;

                    // Move to the next leg in round-robin order
                    nextLegIndex = (nextLegIndex + 1) % arms.Count;

                    // NEW: choose dynamically
                    //nextLegIndex = GetNextLegIndex();

                    isStepping = false;
                }
            }
            else if (arm.hasLockedPoint)
            {
                // Keep foot pinned when not stepping
                arm.armTarget.transform.position = arm.lockedPosition;
            }
        }

        // update rotation tracker
        lastRotation = transform.rotation;
    }

    private void Move()
    {
        Vector2 input = action.Movement.Move.ReadValue<Vector2>(); // Movement direction relative to player facing
        Vector3 move = (transform.right * -input.x + transform.forward * -input.y) * moveSpeed * Time.fixedDeltaTime; // Apply movement
        transform.position += move;
    }
    private void CastParabolaRay(Arm arm, Vector2 inputDir)
    {
        if (isStepping) return;

        Vector3 startPos = arm.rayOrigin.position;

        // Convert input (x,y) into world direction relative to the player
        Vector3 moveDir = (transform.right * inputDir.x + transform.forward * inputDir.y).normalized;

        // If no input, just use forward
        if (moveDir.sqrMagnitude < 0.01f)
            moveDir = -transform.up;

        float velocity = 2f;               // "throw strength"
        Vector3 gravity = Vector3.down * 9.8f; // arc downward pull

        int steps = 25;        // number of arc samples
        float stepSize = 0.1f; // time step per sample

        Vector3 prevPos = startPos;

        for (int i = 1; i <= steps; i++)
        {
            float t = i * stepSize;

            // parabola in movement direction
            Vector3 nextPos = startPos + moveDir * velocity * t + 0.5f * gravity * (t * t);

            Debug.DrawLine(prevPos, nextPos, Color.cyan); // show arc

            // collision check between points
            if (Physics.Linecast(prevPos, nextPos, out RaycastHit hit, arm.hitLayers))
            {
                HandleHit(arm, hit.point, hit.normal);
                return;
            }

            prevPos = nextPos;
        }
    }
    private void HandleHit(Arm arm, Vector3 targetPoint, Vector3 normal)
    {
        if (!arm.hasLockedPoint)
        {
            arm.lockedPosition = targetPoint;
            arm.oldLockedPosition = targetPoint;
            arm.hasLockedPoint = true;
            arm.armTarget.transform.position = targetPoint;
        }
        else
        {
            float dist = Vector3.Distance(arm.lockedPosition, targetPoint);
            float rotationDelta = Quaternion.Angle(lastRotation, transform.rotation);
            bool rotated = rotationDelta > 1f;

            if ((dist > legDistance || rotated) && !arm.isStepping)
            {
                arm.oldLockedPosition = arm.armTarget.transform.position;
                arm.lockedPosition = targetPoint;
                arm.stepT = 0f;
                arm.isStepping = true;
            }
        }
    }

}