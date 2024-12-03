using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class HandUINetworkManager : MonoBehaviourPunCallbacks
{
    public TextMeshProUGUI nameDisplay;
    public GameObject handUi;

    public void LeaveRoom()
    {
        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.LeaveRoom();
            Debug.Log("Leaving " + PhotonNetwork.CurrentRoom.Name);
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        nameDisplay.text = PhotonNetwork.NickName;
        handUi.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
