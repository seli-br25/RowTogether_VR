using Lexic;
using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DisplayGeneratedName : MonoBehaviourPun
{
    //public NameGenerator nameGenerator;
    private TextMeshProUGUI display;
    public Button createRoomButton;
    public Button joinRoomButton;
    // Start is called before the first frame update
    public void DisplayName()
    {
        display.text = PhotonNetwork.NickName;
    }

    public void Start()
    {
        string userName;

        if (PhotonNetwork.NickName != "")
        {
            userName = PhotonNetwork.NickName;
            // renable buttons
            createRoomButton.interactable = true;
            joinRoomButton.interactable = true;

        } else {
            userName = "Generate Your Name";
            createRoomButton.interactable = false;
            joinRoomButton.interactable = false;
        }
        //else
        //{
        //    userName = "Name Generator Down";
        //}
        
        display = GetComponent<TextMeshProUGUI>();
        display.text = userName;    
    }
}
