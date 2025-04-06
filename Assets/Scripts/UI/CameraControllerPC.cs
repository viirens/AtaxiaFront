using UnityEngine;
using UnityEngine.InputSystem;
//using Sirenix.OdinInspector;

public class CameraControllerPC : MonoBehaviour
{
    private CameraControlActions cameraActions;
    private InputAction movement;
    private Transform cameraTransform;

    // horizontal motion
    [SerializeField]
    private float maxSpeed = 5f;
    private float speed;
    [SerializeField]
    private float acceleration = 10f;
    [SerializeField]
    private float damping = 15f;

    // vertical motion
    [SerializeField]
    private float stepSize = 2f;
    [SerializeField]
    private float zoomDampening = 7.5f;
    [SerializeField]
    private float minSize = 3f;
    [SerializeField]
    private float maxSize = 20f;
    [SerializeField]
    private float zoomSpeed = 2f;

    [SerializeField]
    private float minX = -10f;
    [SerializeField]
    private float maxX = 9.30f;
    [SerializeField]
    private float minZ = -3.1f;
    [SerializeField]
    private float maxZ = -3.1f;

    // rotation
    //[SerializeField]
    //private float maxRotationSpeed = 1f;

    // screen edge motion
    [SerializeField]
    [Range(0f, 0.1f)]
    private float edgeTolerance = 0.05f;

    //value set in various functions 
    //used to update the position of the camera base object.
    private Vector3 targetPosition;

    private float zoomHeight;

    //used to track and maintain velocity w/o a rigidbody
    private Vector3 horizontalVelocity;
    private Vector3 lastPosition;

    //tracks where the dragging action started
    Vector3 startDrag;

    private void Awake()
    {
        cameraActions = new CameraControlActions();
        cameraTransform = this.GetComponentInChildren<Camera>().transform;
    }

    private void OnEnable()
    {
        //zoomHeight = cameraTransform.localPosition.y;
        //cameraTransform.LookAt(this.transform);

        //lastPosition = this.transform.position;

        //movement = cameraActions.Camera.Movement;
        ////cameraActions.Camera.RotateCamera.performed += RotateCamera;
        //cameraActions.Camera.ZoomCamera.performed += ZoomCamera;
        //cameraActions.Camera.Enable();
    }

    private void OnDisable()
    {
        //cameraActions.Camera.RotateCamera.performed -= RotateCamera;
        cameraActions.Camera.ZoomCamera.performed -= ZoomCamera;
        cameraActions.Camera.Disable();
    }

    private void Update()
    {
        //inputs
        GetKeyboardMovement();
        //CheckMouseAtScreenEdge();
        //DragCamera();

        //move base and camera objects
        UpdateVelocity();
        UpdateBasePosition();
        UpdateCameraPosition();
    }

    private void UpdateVelocity()
    {
        horizontalVelocity = (this.transform.position - lastPosition) / Time.deltaTime;
        horizontalVelocity.y = 0f;
        lastPosition = this.transform.position;
    }

    private void GetKeyboardMovement()
    {
        Vector2 rawInput = movement.ReadValue<Vector2>();

        // Check if the camera is already at the min/max position along the X axis.
        if ((transform.position.x <= minX && rawInput.x < 0) || (transform.position.x >= maxX && rawInput.x > 0))
        {
            rawInput.x = 0;
        }

        // Check if the camera is already at the min/max position along the Z axis.
        if ((transform.position.z <= minZ && rawInput.y < 0) || (transform.position.z >= maxZ && rawInput.y > 0))
        {
            rawInput.y = 0;
        }

        Vector3 inputValue = rawInput.x * GetIsometricRight() + rawInput.y * GetIsometricForward();
        inputValue = inputValue.normalized;
        if (inputValue.sqrMagnitude > 0.1f && BaseUnitManager.Instance.SelectedPlayer == null)
        {
            targetPosition += inputValue;
        }

    }


    private void DragCamera()
    {
        if (!Mouse.current.rightButton.isPressed)
            return;

        //create plane to raycast to
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (plane.Raycast(ray, out float distance))
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
                startDrag = ray.GetPoint(distance);
            else
                targetPosition += startDrag - ray.GetPoint(distance);
        }
    }

    private void CheckMouseAtScreenEdge()
    {
        //mouse position is in pixels
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 moveDirection = Vector3.zero;

        //horizontal scrolling
        if (mousePosition.x < edgeTolerance * Screen.width)
            moveDirection += -GetCameraRight();
        else if (mousePosition.x > (1f - edgeTolerance) * Screen.width)
            moveDirection += GetCameraRight();

        //vertical scrolling
        if (mousePosition.y < edgeTolerance * Screen.height)
            moveDirection += -GetCameraForward();
        else if (mousePosition.y > (1f - edgeTolerance) * Screen.height)
            moveDirection += GetCameraForward();

        targetPosition += moveDirection;
    }

    private void UpdateBasePosition()
    {
        if (targetPosition.sqrMagnitude > 0.1f)
        {
            // Create a ramp up or acceleration
            speed = Mathf.Lerp(speed, maxSpeed, Time.deltaTime * acceleration);
            transform.position += targetPosition * speed * Time.deltaTime;
        }
        else
        {
            // Create smooth slow down
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, Time.deltaTime * damping);
            transform.position += horizontalVelocity * Time.deltaTime;
        }

        // Clamp the camera position within the boundaries
        float clampedX = Mathf.Clamp(transform.position.x, minX, maxX);
        float clampedZ = Mathf.Clamp(transform.position.z, minZ, maxZ);

        transform.position = new Vector3(clampedX, transform.position.y, clampedZ);


        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z);

        // Reset for next frame
        targetPosition = Vector3.zero;
    }

    private void ZoomCamera(InputAction.CallbackContext obj)
    {
        float inputValue = -obj.ReadValue<Vector2>().y / 100f;
        Camera mainCamera = Camera.main;

        if (Mathf.Abs(inputValue) > 0.1f)
        {
            float newSize = mainCamera.orthographicSize + inputValue * stepSize;

            if (newSize < minSize)
                newSize = minSize;
            else if (newSize > maxSize)
                newSize = maxSize;

            mainCamera.orthographicSize = newSize;
        }
    }

    private void UpdateCameraPosition()
    {
        // Set zoom target
        Vector3 zoomTarget = new Vector3(cameraTransform.localPosition.x, zoomHeight, cameraTransform.localPosition.z);
        // Add vector for forward/backward zoom
        zoomTarget -= zoomSpeed * (zoomHeight - cameraTransform.localPosition.y) * Vector3.forward;

        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, zoomTarget, Time.deltaTime * zoomDampening);
        cameraTransform.LookAt(this.transform);
    }

    //gets the horizontal forward vector of the camera
    private Vector3 GetCameraForward()
    {
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        return forward;
    }

    //gets the horizontal right vector of the camera
    private Vector3 GetCameraRight()
    {
        Vector3 right = cameraTransform.right;
        right.y = 0f;
        return right;
    }

    // Gets the isometric forward vector of the camera
    private Vector3 GetIsometricForward()
    {
        Vector3 forward = cameraTransform.forward - cameraTransform.up * (cameraTransform.forward.y / cameraTransform.up.y);
        forward.y = 0f;
        return forward;
    }

    // Gets the isometric right vector of the camera
    private Vector3 GetIsometricRight()
    {
        Vector3 isometricForward = GetIsometricForward();
        Vector3 right = Vector3.Cross(isometricForward, cameraTransform.up);
        return right;
    }
}