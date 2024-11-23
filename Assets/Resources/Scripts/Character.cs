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
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    private PhotonView photonView;

    private Transform xrRig;
    private Transform headRig;
    private Transform leftHandRig;
    private Transform rightHandRig;

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

    // Update is called once per frame
    void Update()
    {   
        // need to hide own network player, as we already have XROrigin (Character.cs) 
        // also disable the gameobjects for ghost photonviews whose id is a ghost
        if (photonView.IsMine)
        {
            // We don't want to disable the root photon player game object.
            //this.gameObject.SetActive(false);
            rightHand.gameObject.SetActive(false);
            leftHand.gameObject.SetActive(false);
            head.gameObject.SetActive(false);

            MapPosition(this.transform, xrRig);
            MapPosition(head, headRig);
            MapPosition(leftHand, leftHandRig);
            MapPosition(rightHand, rightHandRig);
        }
    }

    void MapPosition(Transform target, Transform rigTransform)
    {
        target.position = rigTransform.position;
        target.rotation = rigTransform.rotation;
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
