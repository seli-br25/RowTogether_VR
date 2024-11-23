using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkPlayerSpawner : MonoBehaviourPunCallbacks
{
    [Tooltip("Leave blank if prefabs are under Resources folder, otherwise indicate the correct subfolder.")]
    [SerializeField]
    private string pathToPrefabs = "Prefabs/";
    [SerializeField]
    private GameObject spawnedPlayerPrefab;
    private GameObject spawnedPlayer;
    [SerializeField]
    private GameObject spawnedShipPrefab;
    private GameObject spawnedShip;
    [SerializeField]
    private GameObject startingShipLocation;
    [SerializeField]
    private GameObject XROrigin;


    // executed by every client in room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);


        // synchronize the ship gameobject with everyone
        if (PhotonNetwork.IsMasterClient)
        {
        }
    }


    // executed only by local client that just joined
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();


        StartCoroutine(SpawnGameobjects());
    }

    private IEnumerator SpawnGameobjects()
    {

        // Wait until the room properties include "LeftFree" and "RightFree"
        while ((!PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("LeftFree") ||
               !PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("RightFree")))
        {
            yield return null; // Wait for the next frame
        }

        // Proceed with spawning once properties are available
        yield return new WaitForEndOfFrame();


        InitializeShip();

        SpawnPlayer();

    }

    private void SpawnPlayer()
    {

        int leftFree = (int)PhotonNetwork.CurrentRoom.CustomProperties["LeftFree"];
        int rightFree = (int)PhotonNetwork.CurrentRoom.CustomProperties["RightFree"];

        spawnedPlayer = PhotonNetwork.Instantiate(pathToPrefabs + spawnedPlayerPrefab.name, XROrigin.transform.position, XROrigin.transform.rotation);


        // spawn ghost observers
        if (leftFree != 0 && rightFree != 0)
        {
            XROrigin.transform.SetParent(spawnedShip.transform.Find("Ghost Seat").transform, false);
            return;
        }


        if (leftFree == 0)
        {
            XROrigin.transform.SetParent(spawnedShip.transform.Find("Left Seat").transform, false);
            spawnedShip.GetComponent<PhotonView>().RPC("UpdateSeatAvailability", RpcTarget.MasterClient, 0, PhotonNetwork.LocalPlayer.ActorNumber);
            return;
        }

        if (rightFree == 0)
        {
            XROrigin.transform.SetParent(spawnedShip.transform.Find("Right Seat").transform, false);
            spawnedShip.GetComponent<PhotonView>().RPC("UpdateSeatAvailability", RpcTarget.MasterClient, 1, PhotonNetwork.LocalPlayer.ActorNumber);
            return;
        }

        
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Destroy(spawnedPlayer);
        base.OnLeftRoom();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log(otherPlayer.ActorNumber);

            int leftFree = (int)PhotonNetwork.CurrentRoom.CustomProperties["LeftFree"];
            int rightFree = (int)PhotonNetwork.CurrentRoom.CustomProperties["RightFree"];

            Debug.Log(leftFree + " " + rightFree);
            //TODO if player left was master, pass boat ownership to new master

            // check who is seated
            if (leftFree == otherPlayer.ActorNumber)
            {
                spawnedShip.GetComponent<PhotonView>().RPC("UpdateSeatAvailability", RpcTarget.MasterClient, 0, 0);

            }
            else if (rightFree == otherPlayer.ActorNumber)
            {
                spawnedShip.GetComponent<PhotonView>().RPC("UpdateSeatAvailability", RpcTarget.MasterClient, 1, 0);
            }
        }
    }


    private void InitializeShip()
    {

        if (PhotonNetwork.IsMasterClient)
        {
            spawnedShip = PhotonNetwork.Instantiate(pathToPrefabs + spawnedShipPrefab.name, startingShipLocation.transform.position, startingShipLocation.transform.rotation);
            spawnedShip.GetComponent<PhotonView>().RPC("SetShipID", RpcTarget.MasterClient);
        } else
        {
            int id = (int)PhotonNetwork.CurrentRoom.CustomProperties["ShipID"];
            PhotonView shipPhotonView = PhotonView.Find(id);
            spawnedShip = shipPhotonView?.gameObject;
        }

    }

}
