using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;

public class UIManager : MonoBehaviourPunCallbacks
{
    public Button startGameButton;
    public Button exitButton;
    public Button restartButton;
    public TextMeshProUGUI textMesh;
    private bool wasMasterClient;
    private GameObject boat;
    private Rigidbody boatRigidbody;
    private GameplayManager gameplayManager;
    public Button handMenuRestartButton;

    private void Start()
    {
        showStartScreen();
    }

    private void showStartScreen()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.gameObject.SetActive(true);
            textMesh.text = "You are the Master-Client! Click on the button to start the game";
            startGameButton.onClick.AddListener(StartGame);
            wasMasterClient = true;
        }
        else
        {
            startGameButton.gameObject.SetActive(false);
            textMesh.text = "Wait for Master-Client to start the game";
            wasMasterClient = false;
        }
    }

    [PunRPC]
    public void ResetUI()
    {
        exitButton.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
        showStartScreen();
        boatRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX;
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient && !wasMasterClient)
        {
            startGameButton.gameObject.SetActive(true);
            textMesh.text = "You are the Master-Client! Click on the button to start the game";

            startGameButton.onClick.AddListener(StartGame);
            wasMasterClient = true;
        }
    }

    public void SetTargetObject(GameObject obj)
    {
        boat = obj;
        boatRigidbody = boat.GetComponent<Rigidbody>();
        boatRigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX;
        gameplayManager = boat.GetComponent<GameplayManager>();
        restartButton.onClick.AddListener(gameplayManager.ResetGame);
        handMenuRestartButton.onClick.AddListener(gameplayManager.ResetGame);
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.gameObject.SetActive(false);
            photonView.RPC("StartCountdown", RpcTarget.All);
        }
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

    [PunRPC]
    public void StartCountdown()
    {
        StartCoroutine(CountdownRoutine());
    }

    private IEnumerator CountdownRoutine()
    {
        textMesh.text = "3";
        yield return new WaitForSeconds(1);

        textMesh.text = "2";
        yield return new WaitForSeconds(1);

        textMesh.text = "1";
        yield return new WaitForSeconds(1);

        textMesh.text = "Go!";
        if (PhotonNetwork.IsMasterClient)
        {
            boatRigidbody = transform.parent.parent.parent.gameObject.GetComponent<Rigidbody>();
            boatRigidbody.constraints = RigidbodyConstraints.None;
        }
        yield return new WaitForSeconds(2);

        textMesh.text = "";
        yield return new WaitForSeconds(1);
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