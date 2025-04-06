using UnityEngine;

public class CameraControllerTopDown : MonoBehaviour
{
    public float zoomSpeed = 2f;
    public float moveSpeed = 5f; // Movement speed of the camera
    public float minZoom = 2f;
    public float maxZoom = 10f;

    public Vector2 panLimitMin; // Minimum X and Y bounds
    public Vector2 panLimitMax; // Maximum X and Y bounds

    private Camera cameraComponent;
    private Transform cameraTransform;

    private CameraControlActions cameraControls; // Reference to the generated input class

    private void Awake()
    {
        cameraControls = new CameraControlActions();
    }

    private void Start()
    {
        cameraComponent = GetComponentInChildren<Camera>();
        cameraTransform = cameraComponent.transform;
    }

    private void OnEnable()
    {
        cameraControls.Camera.ZoomCamera.performed += ctx => Zoom(ctx.ReadValue<Vector2>().y);
        cameraControls.Camera.Enable();
    }

    private void OnDisable()
    {
        cameraControls.Camera.ZoomCamera.performed -= ctx => Zoom(ctx.ReadValue<Vector2>().y);
        cameraControls.Camera.Disable();
    }

    void Update()
    {
        // Apply the movement
        float xMovement = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float yMovement = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;

        cameraTransform = this.GetComponentInChildren<Camera>().transform;

        // Calculate new position
        Vector3 newPosition = cameraTransform.position + new Vector3(xMovement, yMovement, 0);

        // Clamping the new position within the specified bounds
        newPosition.x = Mathf.Clamp(newPosition.x, panLimitMin.x, panLimitMax.x);
        newPosition.y = Mathf.Clamp(newPosition.y, panLimitMin.y, panLimitMax.y);

        // Update camera position
        cameraTransform.position = newPosition;
    }

    private void Zoom(float scrollValue)
    {
        cameraComponent.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - scrollValue * zoomSpeed * Time.deltaTime, minZoom, maxZoom);
    }
}