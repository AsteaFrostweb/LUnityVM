using UnityEngine;

public class SimplePlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform playerHead; // assign your head object here

    [Header("Movement Settings")]
    public float acceleration = 15f;
    public float maxSpeed = 10f;
    public float drag = 5f;

    [Header("Rotation Settings")]
    public float mouseSensitivity = 2f;
    public float pitchClamp = 80f; // stop the head from flipping upside down

    private Vector3 velocity;
    private float pitch = 0f; // current up/down head rotation

    private void Start()
    {
        // Lock cursor to center of screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (GameData.currentFocus.inputFocus != InputFocus.FLIGHT)
            return;

        HandleMovement();
        HandleRotation();
    }

    void HandleMovement()
    {
        // --- Input ---
        float h = Input.GetAxisRaw("Horizontal"); // A/D or Left/Right
        float v = Input.GetAxisRaw("Vertical");   // W/S or Up/Down arrows
        float up = 0f;

        if (Input.GetKey(KeyCode.Space)) up += 1f;
        if (Input.GetKey(KeyCode.LeftControl)) up -= 1f;

        Vector3 inputDir = new Vector3(h, up, v).normalized;

        // --- Acceleration ---
        if (inputDir.sqrMagnitude > 0f)
        {
            Vector3 accel = transform.TransformDirection(inputDir) * acceleration;
            velocity += accel * Time.deltaTime;

            if (velocity.magnitude > maxSpeed)
                velocity = velocity.normalized * maxSpeed;
        }

        // --- Drag ---
        if (velocity.sqrMagnitude > 0.001f)
        {
            velocity = Vector3.Lerp(velocity, Vector3.zero, drag * Time.deltaTime);
        }

        // --- Apply Movement ---
        transform.position += velocity * Time.deltaTime;
    }

    void HandleRotation()
    {
        // --- Body rotation (yaw) ---
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(Vector3.up * mouseX);

        // --- Head rotation (pitch) ---
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        pitch -= mouseY; // invert so mouse up looks up
        pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);

        playerHead.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
}
