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
    private Collider boatCollider;

    private void Start()
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
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.gameObject.SetActive(false);
            photonView.RPC("StartCountdown", RpcTarget.All);
        }
    }

    [PunRPC]
    public void setGoalUI(float endTime)
    {
        int minutes = Mathf.FloorToInt(endTime / 60F);
        int seconds = Mathf.FloorToInt(endTime % 60F);

        textMesh.text = string.Format("You reached the Goal! CONGRATS!\nYour time: {0:00}:{1:00}", minutes, seconds);
        restartButton.gameObject.SetActive(true);
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
        boatRigidbody.constraints = RigidbodyConstraints.None;
        yield return new WaitForSeconds(2);

        textMesh.text = "";
        yield return new WaitForSeconds(1);
    }

    public void setGameOverUI()
    {
        textMesh.text = "Your boat is broken! \nGAME OVER";
        exitButton.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
    }
}