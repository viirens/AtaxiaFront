using UnityEngine;
using UnityEngine.UI;

public class AlignedCam : MonoBehaviour
{
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float zoomStep, minCamSize, maxCamSize;

    [SerializeField]
    private MeshRenderer mapRenderer;

    private float mapMinX, mapMaxX, mapMinY, mapMaxY;

    private Vector2 gridSize;
    private Vector2 gridPosition;

    public Image minimapImage;

    private void Awake()
    {
        mapMinX = mapRenderer.transform.position.x - mapRenderer.bounds.size.x / 2;
        mapMaxX = mapRenderer.transform.position.x + mapRenderer.bounds.size.x / 2;

        mapMinY = mapRenderer.transform.position.y - mapRenderer.bounds.size.y / 2;
        mapMaxY = mapRenderer.transform.position.y + mapRenderer.bounds.size.y / 2;

        // Calculate grid size based on the map renderer bounds
        gridSize = new Vector2(mapRenderer.bounds.size.x / 3, mapRenderer.bounds.size.y / 3);
        // Set the starting grid position (e.g., 0,0 for the first grid)
        // You can change these values to start at a different grid section
        gridPosition = new Vector2(2, 0); // Start at the specified grid
        UpdateCameraPosition(); // Align the camera at start
    }

    public void Zoom(float scrollValue)
    {
        float newSize = cam.orthographicSize + scrollValue;
        cam.orthographicSize = Mathf.Clamp(newSize, minCamSize, maxCamSize);

        cam.transform.position = ClampCamera(cam.transform.position);
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

    // Call this method to move the camera to a specific grid section
    public void MoveToGrid(int x, int y)
    {
        gridPosition = new Vector2(x, y);
        UpdateCameraPosition();

        UpdateMinimap(); // Update the minimap after moving the camera
    }

    // Update the camera's position based on the current grid position
    private void UpdateCameraPosition()
    {
        Vector3 newCameraPosition = new Vector3(
            mapMinX + gridSize.x * (gridPosition.x + 0.5f),
            mapMinY + gridSize.y * (gridPosition.y + 0.5f),
            cam.transform.position.z);

        cam.transform.position = newCameraPosition;
    }

    // Call this method from your UI button script to move the camera
    // direction: "up", "down", "left", "right"
    public void MoveCamera(string direction)
    {
        switch (direction)
        {
            case "up":
                if (gridPosition.y < 2) // Assuming a 3x3 grid
                    gridPosition.y += 1;
                break;
            case "down":
                if (gridPosition.y > 0)
                    gridPosition.y -= 1;
                break;
            case "left":
                if (gridPosition.x > 0)
                    gridPosition.x -= 1;
                break;
            case "right":
                if (gridPosition.x < 2)
                    gridPosition.x += 1;
                break;
        }

        MoveToGrid((int)gridPosition.x, (int)gridPosition.y);
    }

    private void UpdateMinimap()
    {

        string imagePath = "UI/minimap/" + gridPosition.x + "-" + gridPosition.y;
        Sprite newMinimapSprite = Resources.Load<Sprite>(imagePath);

        if (newMinimapSprite != null)
        {
            minimapImage.sprite = newMinimapSprite;
        }
        else
        {
            Debug.LogError("Minimap image not found at path: " + imagePath);
        }
    }
}
