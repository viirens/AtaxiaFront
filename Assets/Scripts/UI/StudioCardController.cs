using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudioCardController : MonoBehaviour
{
    public float displayTime = 3.0f; // Duration for which the card is displayed
    public string nextSceneName = "TitleScreen"; // Name of the title screen scene

    void Start()
    {
        StartCoroutine(DisplayStudioCard());
    }

    IEnumerator DisplayStudioCard()
    {
        // Trigger fade-in animation (e.g., set an Animator trigger)

        yield return new WaitForSeconds(displayTime);

        // Trigger fade-out animation (e.g., set an Animator trigger)

        // Wait for the fade-out animation to complete
        yield return new WaitForSeconds(2f);

        // Load the title screen scene
        SceneManager.LoadScene(nextSceneName);
    }
}
