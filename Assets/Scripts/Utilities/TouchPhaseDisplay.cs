using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class NewBehaviourScript : MonoBehaviour
{
    public TextMeshProUGUI phaseDisplayText;
    private Touch theTouch;
    private float timeTouchEnded;
    private string direction;
    private System.Numerics.Vector2 touchStartPosition, touchEndPosition;
    public Text multiTouchInfoDisplay;
    private int maxTapCount = 0;
    private string multiTouchInfo;

    // Update is called once per frame
    void Update()
    {

        // phases
        //if (Input.touchCount > 0)
        //{
        //    theTouch = Input.GetTouch(0);
        //    if (theTouch.phase == TouchPhase.Ended)
        //    {
        //        phaseDisplayText.text = theTouch.phase.ToString();
        //        timeTouchEnded = Time.time;
        //    }
        //    else if (Time.time - timeTouchEnded > displayTime)
        //    {
        //        phaseDisplayText.text = theTouch.phase.ToString();
        //        timeTouchEnded = Time.time;
        //    }
        //}
        //else if (Time.time - timeTouchEnded > displayTime)
        //{
        //    phaseDisplayText.text = "";
        //}

        // directions
        //if (Input.touchCount > 0)
        //{
        //    theTouch = Input.GetTouch(0);
        //    if (theTouch.phase == TouchPhase.Began)
        //    {
        //        touchStartPosition = theTouch.position;
        //    }
        //    else if (theTouch.phase == TouchPhase.Moved || theTouch.phase == TouchPhase.Began)
        //    {
        //        touchEndPosition = theTouch.position;

        //        float x = touchEndPosition.x - touchStartPosition.x;
        //        float y = touchEndPosition.y - touchStartPosition.y;

        //        if (Mathf.Abs(x) == 0 && Mathf.Abs(y) == 0)
        //        {
        //            direction = "Tapped";
        //        }

        //        else if (Mathf.Abs(x) > Mathf.Abs(y))
        //        {
        //            direction = x > 0 ? "Right" : "Left";
        //        }

        //        else
        //        {
        //            direction = y > 0 ? "Up" : "Down";
        //        }
        //    }
        //}
        //phaseDisplayText.text = direction;

        //multi touch
        multiTouchInfo =
                    string.Format("Max tap count: {0}\n", maxTapCount);
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                theTouch = Input.GetTouch(i);

                multiTouchInfo +=
                    string.Format("Touch {0} - Position {1} - Tap Count: {2} - Finger ID: {3}\n", i, theTouch.position, theTouch.tapCount, theTouch.fingerId);

                if (theTouch.tapCount > maxTapCount)
                {
                    maxTapCount = theTouch.tapCount;
                }
            }
        }
        phaseDisplayText.text = multiTouchInfo;

    }
}
