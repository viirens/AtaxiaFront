using System.Collections;
using TMPro;
using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public GameObject damagePopupPrefab; // Assign in Inspector
    public Camera mainCamera; // Assign in Inspector
    private Canvas canvas; // Canvas reference

    public static DamagePopupManager Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;

        canvas = FindObjectOfType<Canvas>(); // Find the Canvas in the scene
        if (canvas == null)
        {
            Debug.LogError("No Canvas found in the scene for DamagePopupManager.");
        }
    }

    public void CreateDamagePopup(int damage, Vector3 worldPosition, bool isCriticalHit)
    {
        if (canvas != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, canvas.transform);

            if (popup.TryGetComponent(out TextMeshProUGUI damageText))
            {
                if (damage == 0 && !isCriticalHit) // Indicate a miss
                {
                    damageText.text = "Miss";
                    damageText.color = Color.gray; // Choose a color for 'miss', e.g., gray
                }
                else if (isCriticalHit)
                {
                    damageText.text = "Crit";
                    damageText.color = Color.red;
                }
                else
                {
                    damageText.text = damage.ToString();
                    // Set to default color if needed, e.g., damageText.color = Color.white;
                }
            }

            Vector2 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);

            // Adjust offsets for top-down view
            float verticalOffset = 30f;
            float horizontalOffset = 30f;

            // Apply the offset to the screen position
            screenPosition.y += verticalOffset;
            screenPosition.x += horizontalOffset;

            popup.transform.position = screenPosition;

            StartCoroutine(AnimateDamagePopup(popup.transform));
        }
    }

    private IEnumerator AnimateDamagePopup(Transform popupTransform)
    {
        float duration = 1.0f; // Duration of the animation
        float floatUpSpeed = 50f; // Speed at which the popup will float up
        TextMeshProUGUI textComponent = popupTransform.GetComponent<TextMeshProUGUI>();

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            // Float up
            popupTransform.position += new Vector3(0, floatUpSpeed * Time.deltaTime, 0);

            // Fade out
            if (textComponent != null)
            {
                Color color = textComponent.color;
                color.a = Mathf.Lerp(1, 0, t / duration);
                textComponent.color = color;
            }

            yield return null;
        }

        Destroy(popupTransform.gameObject); // Destroy the popup after the animation
    }
}
