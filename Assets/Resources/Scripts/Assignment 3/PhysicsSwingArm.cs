using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsSwingArm : MonoBehaviour
{

    [SerializeField] private GameObject leftHand;
    [SerializeField] private GameObject rightHand;
    [SerializeField] private GameObject mainCamera;
    [SerializeField] private GameObject forwardDirection;
    [SerializeField] private float speedFactor = 500f;
    [SerializeField] private float jumpFactor = 20f;
    [SerializeField] private float verticalHandMovementThreshold = 0.01f;
    [SerializeField] private float noHandMovementThreshold = 0.01f;

    [SerializeField] private CheckHandSwing CheckHandSwing;

    private Vector3 prevLeftHandPos;
    private Vector3 prevRightHandPos;
    private Vector3 prevPlayerPos;

    private Vector3 currLeftHandPos;
    private Vector3 currRightHandPos;
    private Vector3 currPlayerPos;


    private Rigidbody playerRigidbody;
    private float handSpeed;
    private bool isGrounded;
    private float swingDirection;

    private bool handsNotSwinging;
    float notSwingingTimer = 0;


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            isGrounded = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Floor")
        {
            isGrounded = false;
        }
    }
    void Start()
    {
        prevPlayerPos = transform.position;
        prevLeftHandPos = leftHand.transform.position;
        prevRightHandPos = rightHand.transform.position;
        playerRigidbody = GetComponent<Rigidbody>();
    }



    void Update()
    {
        // Set forward direction based on camera rotation
        float yRotation = mainCamera.transform.eulerAngles.y;
        forwardDirection.transform.eulerAngles = new Vector3(0, yRotation, 0);

        currLeftHandPos = leftHand.transform.position;
        currRightHandPos = rightHand.transform.position;
        currPlayerPos = transform.position;

        Vector3 leftHandDirection = (currLeftHandPos - prevLeftHandPos).normalized;
        Vector3 rightHandDirection = (currRightHandPos - prevRightHandPos).normalized;

        // Thank you gpt
        swingDirection = Vector3.Dot(leftHandDirection, rightHandDirection);

        bool sameSwingDirection = swingDirection > 0.5f;

        float leftRightPreviousFrameDistance = Vector3.Distance(prevLeftHandPos, prevRightHandPos);
        float leftRightDistance = Vector3.Distance(currLeftHandPos, currRightHandPos);

        // If distance between both hands not changed, then no hand movement at all detected.
        handsNotSwinging = Mathf.Abs(leftRightPreviousFrameDistance - leftRightDistance) < noHandMovementThreshold;

        if (!handsNotSwinging)
        {
            float leftHandYChange = Mathf.Abs(currLeftHandPos.y - prevLeftHandPos.y);
            float rightHandYChange = Mathf.Abs(currRightHandPos.y - prevRightHandPos.y);

            // Disable movement on horizontal hand swinging
            if (leftHandYChange < verticalHandMovementThreshold || rightHandYChange < verticalHandMovementThreshold)
            {
                handSpeed = 0;
            }
            else
            {
                // Calculate distance moved since last frame
                float playerDistanceMoved = Vector3.Distance(currPlayerPos, prevPlayerPos);
                float leftHandDistanceMoved = Vector3.Distance(prevLeftHandPos, currLeftHandPos);
                float rightHandDistanceMoved = Vector3.Distance(prevRightHandPos, currRightHandPos);


                // Calculate hand speed relative to player movement
                handSpeed = ((leftHandDistanceMoved - playerDistanceMoved) + (rightHandDistanceMoved - playerDistanceMoved));
            }
        }

        prevLeftHandPos = currLeftHandPos;
        prevRightHandPos = currRightHandPos;
        prevPlayerPos = currPlayerPos;
    }

    // For Physics!
    void FixedUpdate()
    {
        if (handsNotSwinging)
        {
            notSwingingTimer += Time.deltaTime;

            // Only stop player after 1 second and if not currently airborne
            if (notSwingingTimer >= 1f && isGrounded)
            {
                playerRigidbody.velocity = Vector3.zero;
            }
        }
        else
        {
            notSwingingTimer = 0f;

            if (CheckHandSwing.HandsInFront())
            {
                // Both hands same direction = jump
                if (swingDirection > 0.5f && Time.timeSinceLevelLoad > 1f)
                {
                    // clamp handSpeed as to not jump to infinity due to controller initialization pop in, which accelerates handSpeed to Mars
                    if (isGrounded && handSpeed > 0.1f && handSpeed < 0.7f)
                    {
                        // Just jump little bit forward + up
                        Vector3 jumpDirection = (forwardDirection.transform.forward * 0.2f + Vector3.up).normalized;
                        float dynamicJumpForce = jumpFactor * handSpeed;
                        playerRigidbody.AddForce(jumpDirection * dynamicJumpForce, ForceMode.Impulse);
                    }
                }
                // All other times equals movement.
                // Only works if we are swinging arms infront of us
                else
                {
                    playerRigidbody.AddForce(forwardDirection.transform.forward * handSpeed * speedFactor, ForceMode.Acceleration);
                }
            }
            
        }

        
    }
}