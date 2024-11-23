using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using TMPro;
using UnityEngine.UI;
using ExitGames.Client.Photon;

public class Character : MonoBehaviour
{
    public Transform cameraOffset;
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    private PhotonView photonView;

    private Transform xrRig;
    private Transform cameraOffsetRig;
    private Transform headRig;
    private Transform leftHandRig;
    private Transform rightHandRig;

    private bool isSeated;

    private bool hasFinished = false;
    private static int placementCounter = 0;
    private TextMeshProUGUI placementText;

    private Button restartButton;

    // Start is called before the first frame update
    void Start()
    {
        photonView = GetComponent<PhotonView>();
        XROrigin rig = FindObjectOfType<XROrigin>();
        xrRig = rig.transform;
        cameraOffsetRig = rig.transform.Find("Camera Offset");
        headRig = rig.transform.Find("Camera Offset/Main Camera");
        leftHandRig = rig.transform.Find("Camera Offset/Left Controller");
        rightHandRig = rig.transform.Find("Camera Offset/Right Controller");

        return;

        GameObject restartButtonObject = GameObject.Find("RestartButton");
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            placementText = canvas.GetComponentInChildren<TextMeshProUGUI>();
        }
        else
        {
            Debug.LogError("Canvas with TextMeshProUGUI not found!");
        }

        if (restartButtonObject != null)
        {
            restartButton = restartButtonObject.GetComponent<Button>();
            if (restartButton != null)
            {
                restartButton.gameObject.SetActive(false);
                restartButton.onClick.AddListener(OnRestartButtonClicked);
            }
        }
        else
        {
            Debug.LogError("RestartButton not found in the scene!");
        }

    }


    [PunRPC]
    private void SeatedAsChild(int seatNr, int boatViewId, int playerViewId)
    {
        Debug.Log($"Attempting to seat {playerViewId} as child of {boatViewId}");
        {
            GameObject seatedPlayer = PhotonView.Find(playerViewId)?.gameObject;
            GameObject boat = PhotonView.Find(boatViewId)?.gameObject;
            string seat;
            if (seatNr == 0)
            {
                seat = "Seats/Left Seat";
            }
            else
            {
                seat = "Seats/Right Seat";
            }
            seatedPlayer.transform.SetParent(boat.transform.Find(seat).transform);
            seatedPlayer.transform.localPosition = Vector3.zero;
            Debug.Log($"Seated {playerViewId} as child of {boatViewId}");
        }
    }

    // Update is called once per frame
    void Update()
    {   
        // need to hide own network player, as we already have XROrigin (Character.cs) 
        // also disable the gameobjects for ghost photonviews whose id is a ghost
        if (photonView.IsMine)
        {
            // We don't want to disable the root photon player game object.
            //this.gameObject.SetActive(false);
            cameraOffset.gameObject.SetActive(false);
            rightHand.gameObject.SetActive(false);
            leftHand.gameObject.SetActive(false);
            head.gameObject.SetActive(false);

            MapPosition(this.transform, xrRig, true);
            MapPosition(cameraOffset, cameraOffsetRig, true);
            MapPosition(head, headRig, true);
            MapPosition(leftHand, leftHandRig, true);
            MapPosition(rightHand, rightHandRig, true);
        }
    }

    void MapPosition(Transform target, Transform rigTransform, bool seated)
    {

        // why do this ?
        // because non master clients will signifcantly lag if behind synchronization in master clients view, if non master clients are not child of boat as well
        // master client would have to wait for the boat to move, synch movement to other clients, other clients would perceive boat movment and since other clients
        // are children of boat in their view, they move together with the boat and synch their position
        // but master client receives this position change way too late causing rubber banding
        // master client and boat will not lag behind synch in other players view because master client and boat are synched at the same time
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
