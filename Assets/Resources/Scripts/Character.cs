using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using TMPro;
using UnityEngine.UI;
using ExitGames.Client.Photon;
using Photon.Realtime;
using System.Collections;

public class Character : MonoBehaviourPun
{
    public Transform cameraOffset;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    //private PhotonView photonView;
    private XROrigin rig;

    private Transform xrRig;
    private Transform cameraOffsetRig;
    private Transform headRig;
    private Transform leftHandRig;
    private HandAnimation leftHandAnimation;
    private Transform rightHandRig;
    private HandAnimation rightHandAnimation;

    // TODO maybe isSeated to toggle between world and local position ?
    private bool isSeated = true;

    private bool hasFinished = false;
    private static int placementCounter = 0;
    private TextMeshProUGUI placementText;

    private Button restartButton;

    // Start is called before the first frame update
    void Start()
    {
        //photonView = GetComponent<PhotonView>();
        rig = FindObjectOfType<XROrigin>();
        xrRig = rig.transform;
        cameraOffsetRig = rig.transform.Find("Camera Offset");
        headRig = rig.transform.Find("Camera Offset/Main Camera");
        leftHandRig = rig.transform.Find("Camera Offset/Left Controller");
        leftHandAnimation = leftHand.GetComponentInChildren<HandAnimation>();
        rightHandRig = rig.transform.Find("Camera Offset/Right Controller");
        rightHandAnimation = rightHand.GetComponentInChildren<HandAnimation>();

        if (photonView.IsMine)
        {
            foreach(var item in GetComponentsInChildren<Renderer>())
            {
                item.enabled = false;
            }
        }

    }


    [PunRPC]
    private void SeatedAsChild(int seatNr, int boatViewId, int playerViewId)
    {
        Debug.Log($"Attempting to seat {playerViewId} as child of {boatViewId}");

        GameObject seatedPlayer = PhotonView.Find(playerViewId)?.gameObject;
        GameObject boat = PhotonView.Find(boatViewId)?.gameObject;
        string seat;
        if (seatNr == 0)
        {
            seat = "Seats/Left Seat";
        }
        else if (seatNr == 1)
        {
            seat = "Seats/Right Seat";
        }
        else
        {
            seat = "Seats/Ghost Seat";
        }
        seatedPlayer.transform.SetParent(boat.transform.Find(seat).transform);
        seatedPlayer.transform.localPosition = Vector3.zero;
        Debug.Log($"Seated {playerViewId} as child of {boatViewId}");
    }

    // Update is called once per frame
    void Update()
    {   
        // need to hide own network player, as we already have XROrigin (Character.cs) 
        // also disable the gameobjects for ghost photonviews whose id is a ghost
        if (xrRig == null)
        {
            Debug.Log("XR Rig null");
            return;
        }
        if (cameraOffset == null)
        {
            Debug.Log("offset null");
            return;
        }
        if (photonView.IsMine)
        {
            //Debug.Log($" is my view {photonView.IsMine}, ${GetComponent<PhotonView>().ViewID}");
            // We don't want to disable the root photon player game object.
            //this.gameObject.SetActive(false);

            // these should not be disabled, otherwise hand animation wont synch because
            // if Character is mine, then my photon animation view wont know when I have triggered an animation on the hands

            //cameraOffset.gameObject.SetActive(false);
            //rightHand.gameObject.SetActive(false);
            //leftHand.gameObject.SetActive(false);
            //head.gameObject.SetActive(false);

            MapPosition(this.transform, xrRig, isSeated);
            MapPosition(cameraOffset, cameraOffsetRig, isSeated);
            MapPosition(head, headRig, isSeated);
            MapPosition(leftHand, leftHandRig, isSeated);
            MapPosition(rightHand, rightHandRig, isSeated);


            leftHandAnimation.photonHandAnimation();
            rightHandAnimation.photonHandAnimation();
        }
    }

    void MapPosition(Transform target, Transform rigTransform, bool seated)
    {

        // why do this ?
        // Previous setup:
        // onJoin room, local player (XRrig) get seated on boat (become child of boat seat). Benefit of strong coupled moving along the boat due to parten child relation.
        // all other photon players in the room would not be seated, but transform be dependednt on world transform
        // Since boat and master client are synched by master client, the synch of boat and master are received simulatneously on all other clients, making master and boat movement seem coupled.
        // However, from the master clients perspective, every client is lagging behind the boat movement. 
        // Reason : the movement of other clients are dependent on boat position. Moving the boat moves a clients transform which will then be synched by photon view transform.
            // The master client receives the updated clients position after the fact that the boat has already moved -> rubber banding occurs for master client, but not perceivable for non master clients view.
        // Solution: Keep track of room properties with actors assigned to seats on the boat.
            // OnJoinRoom synchronize all existing player photon views by moving them to the correct seats (child object of a seat) for every newly joined instance
            // With RPC, existing clients will also move newly joined clients to the correct seats. 
            // This way, when seated the only thing needed to be moved is the boat. Because every client in every instance are now seated (become child of the boat),
            // the movement is synched through movement of the boat with 0 rubber banding in every instance.
            // Local movement is therefore important here.

        if (seated)
        {
            target.localPosition = rigTransform.localPosition;
            target.localRotation = rigTransform.localRotation;
        } else
        {
            target.position = rigTransform.position;
            target.rotation = rigTransform.rotation;
        }
    }


    [PunRPC]
    private void HandleFinish(string playerName)
    {
        placementCounter++; 
        Debug.Log(playerName + " has finished in position " + placementCounter);

        if (photonView.IsMine)
        {
            ShowPlacementMessage(placementCounter); 
        }

        if (placementCounter == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            restartButton.gameObject.SetActive(true);
        }
    }

    private void ShowPlacementMessage(int placement)
    {
        if (placementText != null)
        {
            if (placement == 1)
            {
                placementText.text = "Congratulation, you won!";
            } else
            {
                placementText.text = "You are " + placement + ".!";
            }  
        }
        else
        {
            Debug.LogError("PlacementText is not assigned!");
        }
    }

    private void OnRestartButtonClicked()
    {
        PhotonNetwork.LeaveRoom();
    }
}
