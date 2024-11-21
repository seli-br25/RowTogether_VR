using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Unity.XR.CoreUtils;

public class NetworkPlayerSpawning : MonoBehaviourPunCallbacks
{
    public GameObject playerPrefab;
    public GameObject trackPrefab;
    private GameObject spawnedPlayer;
    private GameObject spawnedTrack;
    public Button startGameButton;
    public TextMeshProUGUI textMesh;
    public GameObject canvas;
    private bool wasMasterClient;
    private Vector3 leftPlayerPos;
    private Vector3 destroyedTrackPos;
    private bool playerLeft;

    public void Update()
    {
        if (PhotonNetwork.IsMasterClient && !wasMasterClient)
        {
            startGameButton.gameObject.SetActive(true);
            textMesh.text = "You are the Master-Client! Click on the button to start the game";

            startGameButton.onClick.AddListener(StartGame);
            wasMasterClient = true;
        }
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if (PhotonNetwork.IsMasterClient)
        {
            startGameButton.gameObject.SetActive(true);
            textMesh.text = "You are the Master-Client! Click on the button to start the game";

            startGameButton.onClick.AddListener(StartGame);
            wasMasterClient = true;
        } else
        {
            startGameButton.gameObject.SetActive(false);
            textMesh.text = "Wait for Master-Client to start the game";
            wasMasterClient = false;
        }

        StartCoroutine(SpawnPlayerAndTrack());
    }

    private IEnumerator SpawnPlayerAndTrack()
    {
        yield return new WaitForEndOfFrame();

        int playerCount = PhotonNetwork.CurrentRoom.PlayerCount - 1;
        Vector3 playerPosition;
        Vector3 trackPosition;
        if (playerLeft)
        {
            trackPosition = destroyedTrackPos;
            spawnedTrack = PhotonNetwork.Instantiate("Prefabs/" + trackPrefab.name, trackPosition, Quaternion.identity);

            playerPosition = spawnedTrack.transform.Find("Spawnpoint").transform.position;
            spawnedPlayer = PhotonNetwork.Instantiate("Prefabs/" + playerPrefab.name, playerPosition, transform.rotation);
            playerLeft = false;
        } else
        {

            trackPosition = new Vector3(playerCount * 30f, 0, 0);
            spawnedTrack = PhotonNetwork.Instantiate("Prefabs/" + trackPrefab.name, trackPosition, Quaternion.identity);

            playerPosition = spawnedTrack.transform.Find("Spawnpoint").transform.position;
            spawnedPlayer = PhotonNetwork.Instantiate("Prefabs/" + playerPrefab.name, playerPosition, transform.rotation);
        }


        XROrigin xrOrigin = FindObjectOfType<XROrigin>();

        if (spawnedPlayer.GetComponent<PhotonView>().IsMine)
        {
            // Align the XR Origin to the spawned player position

            xrOrigin.GetComponent<XROriginPhotonManager>().setPhotonPlayerView(spawnedPlayer.GetPhotonView());

            if (xrOrigin != null)
            {
                xrOrigin.transform.position = playerPosition;
                xrOrigin.transform.rotation = spawnedPlayer.transform.rotation;
            }
        }


        if (spawnedTrack.GetComponent<PhotonView>().IsMine)
        {
            xrOrigin.GetComponent<XROriginPhotonManager>().setClientPlayerTrack(spawnedTrack);
        }
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        if (spawnedPlayer != null)
        {
            leftPlayerPos = spawnedPlayer.transform.position;
            PhotonNetwork.Destroy(spawnedPlayer);
            Debug.Log("Player left the room");
        }

        if (spawnedTrack != null)
        {   
            destroyedTrackPos = spawnedTrack.transform.position;
            PhotonNetwork.Destroy(spawnedTrack);
        }
        playerLeft = true;

    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            PhotonNetwork.CurrentRoom.IsVisible = false;

            startGameButton.gameObject.SetActive(false);
            photonView.RPC("StartCountdown", RpcTarget.All);
        }
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
        yield return new WaitForSeconds(2);

        textMesh.text = "";

        yield return new WaitForSeconds(1);
        canvas.SetActive(false);
    }
}
