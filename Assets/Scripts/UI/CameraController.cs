using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;

public class CameraController : MonoBehaviour
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
    private float maxSize = 15f;
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

    //pan slide
    private Vector3 velocity = Vector3.zero;
    private const float panDamping = 0.9f;

#if UNITY_IOS || UNITY_ANDROID
    protected Plane Plane;
#endif

    private void Awake()
    {
        cameraActions = new CameraControlActions();
        cameraTransform = this.GetComponentInChildren<Camera>().transform;
#if UNITY_IOS || UNITY_ANDROID
        Plane = new Plane(transform.up, transform.position);
#endif
    }

    private void OnEnable()
    {
        zoomHeight = cameraTransform.localPosition.y;
        cameraTransform.LookAt(this.transform);

        lastPosition = this.transform.position;

        movement = cameraActions.Camera.Movement;
        //cameraActions.Camera.RotateCamera.performed += RotateCamera;
        cameraActions.Camera.ZoomCamera.performed += ZoomCamera;
        cameraActions.Camera.Enable();
    }

    private void OnDisable()
    {
        //cameraActions.Camera.RotateCamera.performed -= RotateCamera;
        cameraActions.Camera.ZoomCamera.performed -= ZoomCamera;
        cameraActions.Camera.Disable();
    }

    private void Update()
    {
        //move base and camera objects
        UpdateVelocity();
        UpdateBasePosition();
        UpdateCameraPosition();
#if UNITY_IOS || UNITY_ANDROID
        HandleTouchInput();
#endif
    }

    private void UpdateVelocity()
    {
        horizontalVelocity = (this.transform.position - lastPosition) / Time.deltaTime;
        horizontalVelocity.y = 0f;
        lastPosition = this.transform.position;
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

#if UNITY_IOS || UNITY_ANDROID
    private void HandleTouchInput()
    {
        // EnhancedTouch Update
        EnhancedTouchSupport.Enable();

        // Update Plane
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count >= 1)
            Plane.SetNormalAndPosition(transform.up, transform.position);

        // Handle pinch to zoom
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count == 2)
        {
            var touch1 = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];
            var touch2 = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[1];

            Vector3 touch1World = PlanePosition(touch1.screenPosition);
            Vector3 touch2World = PlanePosition(touch2.screenPosition);

            Vector3 lastTouch1World = PlanePosition(touch1.screenPosition - touch1.delta);
            Vector3 lastTouch2World = PlanePosition(touch2.screenPosition - touch2.delta);

            float currentDistance = Vector3.Distance(touch1World, touch2World);
            float lastDistance = Vector3.Distance(lastTouch1World, lastTouch2World);
            float distanceDifference = lastDistance - currentDistance;

            Camera mainCamera = Camera.main; // Only get this once

            if (Mathf.Abs(distanceDifference) > 0.1f)
            {
                // This is a zoom
                float newSize = mainCamera.orthographicSize - distanceDifference * stepSize * 0.2f;

                if (newSize < minSize)
                    newSize = minSize;
                else if (newSize > maxSize)
                    newSize = maxSize;

                mainCamera.orthographicSize = newSize;
            }
        }

        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count == 1 && BaseUnitManager.Instance.SelectedPlayer == null)
        {

            // Compute sensitivity based on zoom level
            float baseSensitivity = 0.06f;
            float someReferenceSize = 15;
            float zoomFactor = Camera.main.orthographicSize / someReferenceSize;
            float sensitivity = baseSensitivity * zoomFactor;
            var touch = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[0];

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                Vector2 screenDelta = touch.delta;

                // Convert the screen-space delta to isometric world-space directions
                Vector3 moveDirection = GetIsometricRight() * screenDelta.x + GetIsometricForward() * screenDelta.y;
                moveDirection = moveDirection.normalized * screenDelta.magnitude;

                // Update the velocity
                velocity += moveDirection * sensitivity;
            }
        }

        // Apply the velocity to the target position
        targetPosition += velocity;

        // Dampen the velocity over time
        velocity *= panDamping;

        // If the velocity is very small, reset it to zero to avoid tiny movements
        if (velocity.magnitude < 0.001f)
        {
            velocity = Vector3.zero;
        }

    }

    protected Vector3 PlanePositionDelta(UnityEngine.InputSystem.EnhancedTouch.Touch touch)
    {
        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Moved)
            return Vector3.zero;

        var rayBefore = Camera.main.ScreenPointToRay(touch.screenPosition - touch.delta);
        var rayNow = Camera.main.ScreenPointToRay(touch.screenPosition);

        Debug.DrawRay(rayBefore.origin, rayBefore.direction * 100f, Color.red, 2f);
        Debug.DrawRay(rayNow.origin, rayNow.direction * 100f, Color.blue, 2f);


        if (Plane.Raycast(rayBefore, out float enterBefore) && Plane.Raycast(rayNow, out float enterNow))
            return rayBefore.GetPoint(enterBefore) - rayNow.GetPoint(enterNow);

        return Vector3.zero;
    }

    protected Vector3 PlanePosition(Vector2 screenPos)
    {
        var rayNow = Camera.main.ScreenPointToRay(screenPos);

        if (Plane.Raycast(rayNow, out float enterNow))
            return rayNow.GetPoint(enterNow);

        return Vector3.zero;
    }
#endif
}