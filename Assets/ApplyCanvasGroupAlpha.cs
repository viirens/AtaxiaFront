using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ApplyCanvasGroupAlpha : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public SpriteRenderer localRenderer;


    void Update()
    {
        if (localRenderer != null && canvasGroup != null)
        {
            Debug.Log("hit");
            Debug.Log(canvasGroup.alpha);
            localRenderer.material.SetFloat("_CanvasGroupAlpha", canvasGroup.alpha);
        }
    }
}
