using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwingArmMotion : MonoBehaviour
{
    // Game Objects
    [SerializeField] private GameObject LeftHand;
    [SerializeField] private GameObject RightHand;
    [SerializeField] private GameObject MainCamera;
    [SerializeField] private GameObject ForwardDirection;

    //Vector3 Positions
    [SerializeField] private Vector3 PositionPreviousFrameLeftHand;
    [SerializeField] private Vector3 PositionPreviousFrameRightHand;
    [SerializeField] private Vector3 PlayerPositionPreviousFrame;
    [SerializeField] private Vector3 PlayerPositionCurrentFrame;
    [SerializeField] private Vector3 PositionCurrentFrameLeftHand;
    [SerializeField] private Vector3 PositionCurrentFrameRightHand;

    //Speed
    [SerializeField] private float Speed = 70;
    [SerializeField] private float HandSpeed;

    void Start()
    {
        PlayerPositionPreviousFrame = transform.position; //set current positions
        PositionPreviousFrameLeftHand = LeftHand.transform.position; //set previous positions
        PositionPreviousFrameRightHand = RightHand.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // Set forward direction based on camera rotation
        float yRotation = MainCamera.transform.eulerAngles.y;
        ForwardDirection.transform.eulerAngles = new Vector3(0, yRotation, 0);

        // Get positions of hands and player
        PositionCurrentFrameLeftHand = LeftHand.transform.position;
        PositionCurrentFrameRightHand = RightHand.transform.position;
        PlayerPositionCurrentFrame = transform.position;

        // Calculate distances moved since last frame
        var playerDistanceMoved = Vector3.Distance(PlayerPositionCurrentFrame, PlayerPositionPreviousFrame);
        var leftHandDistanceMoved = Vector3.Distance(PositionPreviousFrameLeftHand, PositionCurrentFrameLeftHand);
        var rightHandDistanceMoved = Vector3.Distance(PositionPreviousFrameRightHand, PositionCurrentFrameRightHand);

        // Calculate direction of movement for each hand
        Vector3 leftHandDirection = (PositionCurrentFrameLeftHand - PositionPreviousFrameLeftHand).normalized;
        Vector3 rightHandDirection = (PositionCurrentFrameRightHand - PositionPreviousFrameRightHand).normalized;

        // Check if hands are moving in opposite directions using dot product
        float dotProduct = Vector3.Dot(leftHandDirection, rightHandDirection);
        bool areHandsMovingOpposite = dotProduct < -0.5f; // Negative dot product indicates opposite directions
        bool areHandsMovingSameDirection = dotProduct > 0.5f; // Same direction

        // Calculate hand speed based on hand movement relative to body
        HandSpeed = ((leftHandDistanceMoved - playerDistanceMoved) + (rightHandDistanceMoved - playerDistanceMoved));

        // Apply movement only if hands are moving in opposite directions
        if (areHandsMovingOpposite && Time.timeSinceLevelLoad > 1f)
        {
            transform.position += ForwardDirection.transform.forward * HandSpeed * Speed * Time.deltaTime;
        } else if(areHandsMovingSameDirection && Time.timeSinceLevelLoad > 1f){
            transform.position -= ForwardDirection.transform.forward * HandSpeed * Speed * Time.deltaTime;
        }
       
        

        // Update previous positions for the next frame
        PositionPreviousFrameLeftHand = PositionCurrentFrameLeftHand;
        PositionPreviousFrameRightHand = PositionCurrentFrameRightHand;
        PlayerPositionPreviousFrame = PlayerPositionCurrentFrame;
    }

}
