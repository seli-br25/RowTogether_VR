using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Photon.Pun;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.XR.CoreUtils;
using TMPro;
using UnityEngine.UI;

public class Character : MonoBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;
    private PhotonView photonView;

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
        headRig = rig.transform.Find("Camera Offset/Main Camera");
        leftHandRig = rig.transform.Find("Camera Offset/Left Controller");
        rightHandRig = rig.transform.Find("Camera Offset/Right Controller");

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
        if (photonView.IsMine)
        {
            rightHand.gameObject.SetActive(false);
            leftHand.gameObject.SetActive(false);
            head.gameObject.SetActive(false);

            MapPosition(head, headRig);
            MapPosition(leftHand, leftHandRig);
            MapPosition(rightHand, rightHandRig);
        }

        //GameObject canvas = GameObject.Find("Canvas");
        //if (canvas != null)
        //{
        //    placementText = canvas.GetComponentInChildren<TextMeshProUGUI>();
        //}
        //else
        //{
        //    Debug.LogError("Canvas with TextMeshProUGUI not found!");
        //}

    }

    void MapPosition(Transform target, Transform rigTransform)
    {
        target.position = rigTransform.position;
        target.rotation = rigTransform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal") && !hasFinished)
        {
            hasFinished = true; 
            photonView.RPC("HandleFinish", RpcTarget.AllBuffered, PhotonNetwork.NickName);
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
