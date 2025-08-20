using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("References")]
    public Transform playerBody; // The object that rotates left/right (usually your player root)

    [Header("Settings")]
    public float mouseSensitivity = 100f;
    public float clampAngle = 80f; // Limit vertical look

    private float xRotation = 0f;

    private void Start()
    {
        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate body left/right (yaw)
        playerBody.Rotate(Vector3.up * mouseX);

        // Rotate camera up/down (pitch)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -clampAngle, clampAngle);
    }
}
