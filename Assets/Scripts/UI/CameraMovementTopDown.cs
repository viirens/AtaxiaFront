using System.Collections;
using UnityEngine;
using static GameManager;

public class CameraMovementTopDown : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float zoomStep, minCamSize, maxCamSize;

    [SerializeField]
    private MeshRenderer mapRenderer;

    private float mapMinX, mapMaxX, mapMinY, mapMaxY;

    private Vector3 dragOrigin;

    private CameraControlActions cameraControls; // Reference to the generated input class
    private float lastZoomTime = 0f;
    private const float zoomPanTimeout = 1f; // 1 second timeout

    public static CameraMovementTopDown Instance;

    private Coroutine autoPanCoroutine = null; // To keep track of the auto-panning coroutine
    private bool isAutoPanning = false; //
    private Coroutine followCoroutine = null; // To keep track of the follow coroutine


    private void Awake()
    {
        Instance = this;
        mapMinX = mapRenderer.transform.position.x - mapRenderer.bounds.size.x / 2;
        mapMaxX = mapRenderer.transform.position.x + mapRenderer.bounds.size.x / 2;

        mapMinY = mapRenderer.transform.position.y - mapRenderer.bounds.size.y / 2;
        mapMaxY = mapRenderer.transform.position.y + mapRenderer.bounds.size.y / 2;
        cameraControls = new CameraControlActions();
    }

    void Start()
    {
        StartZoomAndCenterOnEntry();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManager.Instance && GameManager.Instance.Mode == GameManager.InputMode.Camera)
        {
            PanCamera();
        }
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

    private void PanCamera()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isAutoPanning)
            {
                StopCoroutine(autoPanCoroutine);
                isAutoPanning = false;
            }
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            cam.transform.position = ClampCamera(cam.transform.position + difference);
        }
    }

    private bool CanPan()
    {
        return Time.time >= lastZoomTime + zoomPanTimeout;
    }

    public void Zoom(float scrollValue)
    {
        float newSize = cam.orthographicSize + scrollValue;
        cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);

        cam.transform.position = ClampCamera(cam.transform.position);
    }

    void ZoomAndCenter(float zoomFactor, Vector3 centerPoint)
    {
        // Adjust the orthographic size for zooming
        float newSize = Camera.main.orthographicSize - zoomFactor;
        //Camera.main.orthographicSize -= zoomFactor;
        Camera.main.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize); ; // Prevents the camera size from going negative

        // Calculate the vector from the camera's position to the center point
        Vector3 directionToCenter = centerPoint - Camera.main.transform.position;

        // Optionally, you can limit the movement speed for a smoother transition
        float moveSpeed = 5.0f; // Adjust this value as needed for smoothness

        // Adjust the camera's position to move towards the center point
        // Time.deltaTime is used to make the movement frame rate independent
        Camera.main.transform.position += directionToCenter * moveSpeed * Time.deltaTime;
    }

    private Vector3 ClampCamera(Vector3 targetPosition)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;

        float minx = mapMinX + camWidth;
        float maxX = mapMaxX - camWidth;
        float minY = mapMinY + camHeight;
        float maxY = mapMaxY - camHeight;

        float newX = Mathf.Clamp(targetPosition.x, minx, maxX);
        float newY = Mathf.Clamp(targetPosition.y, minY, maxY);

        return new Vector3(newX, newY, targetPosition.z);
    }

    private Vector3 ClampCameraToUnit(Vector3 targetPosition, Transform unitTransform = null)
    {
        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;

        float minx = mapMinX + camWidth;
        float maxX = mapMaxX - camWidth;
        float minY = mapMinY + camHeight;
        float maxY = mapMaxY - camHeight;

        // Adjust target position based on unit's viewport position
        if (unitTransform != null)
        {
            Vector3 viewportPoint = cam.WorldToViewportPoint(unitTransform.position);

            // Determine offset based on how close the unit is to the viewport edges
            const float edgeThreshold = 0.1f; // Adjust as necessary
            if (viewportPoint.x > 1 - edgeThreshold) targetPosition.x += camWidth * edgeThreshold;
            if (viewportPoint.x < edgeThreshold) targetPosition.x -= camWidth * edgeThreshold;
            if (viewportPoint.y > 1 - edgeThreshold) targetPosition.y += camHeight * edgeThreshold;
            if (viewportPoint.y < edgeThreshold) targetPosition.y -= camHeight * edgeThreshold;
        }

        float newX = Mathf.Clamp(targetPosition.x, minx, maxX);
        float newY = Mathf.Clamp(targetPosition.y, minY, maxY);

        return new Vector3(newX, newY, targetPosition.z);
    }


    public void MoveToUnit(Vector3 unitPosition)
    {
        if (autoPanCoroutine != null)
        {
            StopCoroutine(autoPanCoroutine);
        }
        autoPanCoroutine = StartCoroutine(SmoothPanToUnit(unitPosition));
    }

    private IEnumerator SmoothPanToUnit(Vector3 targetPosition)
    {
        isAutoPanning = true;
        while (Vector3.Distance(cam.transform.position, new Vector3(targetPosition.x, targetPosition.y, cam.transform.position.z)) > 0.05f)
        {
            // Calculate the next position
            Vector3 newPosition = Vector3.Lerp(cam.transform.position, new Vector3(targetPosition.x, targetPosition.y, cam.transform.position.z), Time.deltaTime * 5);
            cam.transform.position = ClampCamera(newPosition);

            yield return null;
        }
        isAutoPanning = false;
    }

    public void FollowMovingUnit(Transform unitTransform, BaseUnit unit)
    {
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
        }
        followCoroutine = StartCoroutine(SmoothFollow(unitTransform, unit));
    }


    private IEnumerator SmoothFollow(Transform targetTransform, BaseUnit unit)
    {
        while (unit.inMovement)
        {
            Vector3 newPosition = Vector3.Lerp(cam.transform.position, new Vector3(targetTransform.position.x, targetTransform.position.y, cam.transform.position.z), Time.deltaTime * 5);
            cam.transform.position = ClampCameraToUnit(newPosition, targetTransform);

            yield return null;
        }
    }


    public void StopFollowing()
    {
        if (followCoroutine != null)
        {
            StopCoroutine(followCoroutine);
            followCoroutine = null;
        }
    }

    public void StartZoomAndCenterOnEntry()
    {
        string selectedEdge = PlayerPrefs.GetString("SelectedEdge");
        Vector3 edgeCenterPoint = GetCenterPointOfEdge(selectedEdge);
        StartCoroutine(ZoomAndCenterOnEdge(selectedEdge, 5.0f, 20.0f, 3.0f, 9));
    }

    private IEnumerator ZoomAndCenterOnEdge(string edge, float targetSize, float initialSize, float duration, float easeInExponent)
    {
        float currentTime = 0.0f;

        // Set the initial camera size
        cam.orthographicSize = initialSize;

        // Calculate the target position based on the edge
        Vector3 targetPosition = CalculateTargetPositionForEdge(edge, targetSize);
        Vector3 startPosition = cam.transform.position;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float t = currentTime / duration;

            // Ease in with adjustable exponent
            float easedT = Mathf.Pow(t, easeInExponent);

            // Smoothly interpolate the camera size and position
            cam.orthographicSize = Mathf.Lerp(initialSize, targetSize, easedT);
            cam.transform.position = Vector3.Lerp(startPosition, targetPosition, easedT);

            yield return null;
        }

        // Ensure final values are set
        cam.orthographicSize = targetSize;
        cam.transform.position = targetPosition;

        // After finishing the camera animation, initialize the game state
        GameManager.Instance.ChangeState(GameState.InitGame);
        //StartCoroutine(MenuManager.Instance.FadeCanvasIn(2.0f));
    }


    private Vector3 CalculateTargetPositionForEdge(string edge, float targetCamSize)
    {
        float camHeight = targetCamSize;
        float camWidth = targetCamSize * cam.aspect;

        // Calculate target position based on the selected edge
        switch (edge)
        {
            case "Right":
                return new Vector3(mapMaxX - camWidth, (mapMinY + mapMaxY) / 2, cam.transform.position.z);
            case "Top":
                return new Vector3((mapMinX + mapMaxX) / 2, mapMaxY - camHeight, cam.transform.position.z);
            case "Bottom":
                return new Vector3((mapMinX + mapMaxX) / 2, mapMinY + camHeight, cam.transform.position.z);
            default:
                return cam.transform.position; // Default to current position if edge is unknown
        }
    }



    private Vector3 GetCenterPointOfEdge(string edge)
    {
        // Example coordinates for edges - you should adjust these according to your map's setup
        switch (edge)
        {
            case "Right":
                return new Vector3(35, (13 + 22) / 2.0f, 0); // Center of Right edge
            case "Top":
                return new Vector3((13 + 22) / 2.0f, 35, 0); // Center of Top edge
            case "Bottom":
                return new Vector3((13 + 22) / 2.0f, 0, 0); // Center of Bottom edge
            default:
                return Vector3.zero; // Default case
        }
    }

    private Vector3 ClampCameraForZoom(Vector3 targetPosition, float progress)
    {
        // Define how much the camera is allowed to move outside the bounds
        float clampingEase = Mathf.Lerp(1.0f, 0.0f, progress); // Less clamping at start, full clamping at end

        float camHeight = cam.orthographicSize;
        float camWidth = cam.orthographicSize * cam.aspect;

        // Adjusted bounds based on clampingEase
        float minx = mapMinX + camWidth * clampingEase;
        float maxX = mapMaxX - camWidth * clampingEase;
        float minY = mapMinY + camHeight * clampingEase;
        float maxY = mapMaxY - camHeight * clampingEase;

        float newX = Mathf.Clamp(targetPosition.x, minx, maxX);
        float newY = Mathf.Clamp(targetPosition.y, minY, maxY);

        return new Vector3(newX, newY, targetPosition.z);
    }


}
