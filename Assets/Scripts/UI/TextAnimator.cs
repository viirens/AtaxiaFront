using TMPro;
using UnityEngine;

public class TextAnimator : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textElement;
    private float nextTime = 0; // Time for next flicker
    private string stringAddition = "";
    private string stringMessage = "Select board entry";

    // Update is called once per frame
    void Update()
    {
        // Handle flickering effect
        if (Time.time >= nextTime)
        {
            IterateText();
        }
    }

    void IterateText()
    {
        if (stringAddition.Length >= 3)
        {
            stringAddition = "";
            textElement.text = stringMessage;
        }
        else
        {
            stringAddition += ".";
            textElement.text = textElement.text += ".";
        }
        nextTime = Time.time + 1f;
    }
}
