using UnityEngine;
using UnityEngine.UI;

public class FixedAspectRatio : MonoBehaviour
{
    public RawImage leftPanel;  // Assign your left RawImage in the inspector
    public RawImage rightPanel;

    void Start()
    {
        Camera camera = GetComponent<Camera>();
        AdjustCamera(camera);
        AdjustPanels(camera);
        leftPanel.gameObject.SetActive(true);
        rightPanel.gameObject.SetActive(true);
    }

    void AdjustCamera(Camera camera)
    {
        Rect rect = camera.rect;
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float scaleHeight = screenAspect / 1.0f; // 1.0f for square aspect ratio
        Debug.Log($"Camera Rect: {camera.rect}");


        if (scaleHeight < 1.0f)
        {
            rect.height = scaleHeight;
            rect.y = (1.0f - scaleHeight) / 2.0f;
        }
        else
        {
            float scaleWidth = 1.0f / screenAspect;
            rect.width = scaleWidth;
            rect.x = (1.0f - scaleWidth) / 2.0f;
        }

        camera.rect = rect;
    }

    void AdjustPanels(Camera camera)
    {
        float cameraWidth = camera.pixelWidth; // Width of camera in pixels
        float totalScreenWidth = Screen.width; // Total screen width in pixels
        float magicScalingNumber = .805f;


        // Calculate the width for the side panels in pixels
        float panelWidthPixels = (totalScreenWidth - cameraWidth) / 2.0f;
        float moddedPixelWidth = magicScalingNumber * panelWidthPixels;

        // Convert the pixel width to a relative scale (0.0 to 1.0)
        float panelWidthRelative = moddedPixelWidth / totalScreenWidth;
        //Debug.Log($"Screen Width: {Screen.width}, Camera Width: {camera.pixelWidth}");
        //Debug.Log($"Panel Width (Pixels): {panelWidthPixels}, Panel Width (Relative): {panelWidthRelative}");

        // Set the size and position of the left and right panels
        SetPanelSizeAndPosition(leftPanel, 0, panelWidthRelative);
        SetPanelSizeAndPosition(rightPanel, 1 - panelWidthRelative, 1);
    }

    void SetPanelSizeAndPosition(RawImage panel, float anchorMinX, float anchorMaxX)
    {
        panel.rectTransform.anchorMin = new Vector2(anchorMinX, 0);
        panel.rectTransform.anchorMax = new Vector2(anchorMaxX, 1);
        panel.rectTransform.offsetMin = Vector2.zero;
        panel.rectTransform.offsetMax = Vector2.zero;
    }
}

