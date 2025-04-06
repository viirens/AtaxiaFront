using System.Collections;
using System.Collections.Generic;
//using System.Numerics;
using UnityEngine;

public class playerController : MonoBehaviour
{
    Animator animator;
    int isWalkingHash;
    int isRunningHash;
    public float rotationSpeed = 100.0f;
    public float movementSpeed = .05f;
    public BaseUnit baseUnit;
    float initialY;
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");

        initialY = transform.position.y;

        // Add a Capsule Collider
        //CapsuleCollider collider = gameObject.AddComponent<CapsuleCollider>();
        //collider.height = 2.0f;  // You may need to adjust this based on your character model
        //collider.center = new Vector3(0, 1, 0);  // You may need to adjust this too

        //// Add a RigidBody
        //Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
        //rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    // Update is called once per frame
    void Update()
    {
        //bool isRunning = animator.GetBool(isRunningHash);
        //bool isWalking = animator.GetBool(isWalkingHash);
        //bool forwardPressed = Input.GetKey("w");
        //bool runPressed = Input.GetKey("left shift");
        //bool turnLeft = Input.GetKey("a");
        //bool turnRight = Input.GetKey("d");


        //if (!isWalking && forwardPressed)
        //{
        //    animator.SetBool(isWalkingHash, true);
        //}
        //if (isWalking && !forwardPressed)
        //{
        //    animator.SetBool(isWalkingHash, false);
        //}

        //if (!isRunning && (forwardPressed && runPressed))
        //{
        //    animator.SetBool(isRunningHash, true);
        //}

        //if (isRunning && (!forwardPressed || !runPressed))
        //{
        //    animator.SetBool(isRunningHash, false);
        //}

        //if (turnLeft)
        //{
        //    transform.RotateAround(transform.position, Vector3.up, -rotationSpeed * Time.deltaTime);
        //}

        //if (turnRight)
        //{
        //    transform.RotateAround(transform.position, Vector3.up, rotationSpeed * Time.deltaTime);
        //}

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))  // 100 is the max distance
            {
                Vector3 targetPosition = hit.point;
                //StartCoroutine(MoveToTarget(targetPosition));
            }
        }
    }

    void OnAnimatorMove()
    {
        // If this is the root of an Animator hierarchy
        if (animator.isHuman && animator.applyRootMotion)
        {
            // Get the root motion translation from the Animator
            Vector3 newPosition = animator.deltaPosition;

            // Zero out the y component of the position to ignore vertical motion
            newPosition.y = 0;

            // Apply the modified root motion to the character
            transform.position += newPosition;
        }

        if (transform.position.y < initialY)
        {
            Vector3 newPosition = transform.position;
            newPosition.y = initialY;
            transform.position = newPosition;
        }
    }

    //IEnumerator MoveToTarget(Vector3 targetPosition)
    //{
    //    animator.SetBool(isWalkingHash, true);

    //    while (Vector3.Distance(transform.position, targetPosition) > 0.1f) // 0.1 is the distance to stop from the target
    //    {
    //        Vector3 direction = (targetPosition - transform.position).normalized;
    //        Quaternion toRotation = Quaternion.LookRotation(new Vector3(direction.x, direction.y, 0));
    //        transform.rotation = Quaternion.Lerp(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
    //        transform.position = Vector3.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

    //        yield return null;  // Wait for one frame
    //    }

    //    animator.SetBool(isWalkingHash, false);
    //}

}
