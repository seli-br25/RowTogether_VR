using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using System;

public class UIManager : MonoBehaviourPunCallbacks
{
    public Button startGameButton;
    public Button exitButton;
    public Button restartButton;
    public TextMeshProUGUI textMesh;
    private bool wasMasterClient;
    //private GameObject boat;
    //private Rigidbody boatRigidbody;
    //private GameplayManager gameplayManager;
    public Button handMenuRestartButton;

    public string masterClientText = "You are the Master-Client! Click on the button to start the game";
    public string nonMasterClientText = "Wait for Master-Client to start the game";

    private void Start()
    {
        showStartScreen();
    }

    private void showStartScreen()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.gameObject.SetActive(true);
            textMesh.text = masterClientText;
            wasMasterClient = true;
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
            textMesh.text = nonMasterClientText;
            wasMasterClient = false;
        }
    }

    public void ResetUI()
    {
        exitButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        showStartScreen();        
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && !wasMasterClient)
        {
            startGameButton.gameObject.SetActive(true);
            textMesh.text = masterClientText;

            startGameButton.onClick.AddListener(StartGame);
            wasMasterClient = true;
        }
    }

    // Only masterclient instance will have these variables set, because initialize ship is only triggered by master client
    public void SetTargetObject(GameplayManager gM)
    {
        //Debug.Log(gM.ToString());
        // best to separate rigidbody management from ui management

        //boat = obj;
        //boatRigidbody = boat.GetComponent<Rigidbody>();

        // todo: how to sync constraints to non master. due to onmaster switch, constraints wont be set for non masters
        //boatRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX;
        //gameplayManager = boat.GetComponent<GameplayManager>();
        //gameplayManager = gM;
        //gameplayManager.UpdateRigidBodyConstraints((int)(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX));
        //restartButton.onClick.AddListener(ResetUI);
        //handMenuRestartButton.onClick.AddListener(ResetUI);
    }

    public void RegisterButtonListener(Action resetGame, Action startGame)
    {
        restartButton.onClick.AddListener(() => resetGame());
        handMenuRestartButton.onClick.AddListener(() => resetGame());
        startGameButton.onClick.AddListener(() => startGame());
    }

    // only triggered by master through gameplaymanager
    public void StartGame()
    {
        startGameButton.gameObject.SetActive(false);
    }

    public void SetGoalUI(float endTime)
    {
        int minutes = Mathf.FloorToInt(endTime / 60F);
        int seconds = Mathf.FloorToInt(endTime % 60F);

        textMesh.text = string.Format("You reached the Goal! CONGRATS!\nYour time: {0:00}:{1:00}", minutes, seconds);
        if (PhotonNetwork.IsMasterClient)
        {
            // only Masterclient can restart
            restartButton.gameObject.SetActive(true);
        }
        exitButton.gameObject.SetActive(true);
    }



    public void UpdateUIText(string t)
    {
        textMesh.text = t;
    }



    public void SetGameOverUI()
    {
        textMesh.text = "Your boat is broken! \nGAME OVER";
        if (PhotonNetwork.IsMasterClient)
        {
            // only Masterclient can restart
            restartButton.gameObject.SetActive(true);
        }
        exitButton.gameObject.SetActive(true);
        
    }
}