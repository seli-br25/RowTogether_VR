using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Unity.XR.CoreUtils;

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

    private int previousMasterClientActorNr;

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
        // contains
        int leftSeat = (int)PhotonNetwork.CurrentRoom.CustomProperties["LeftFree"];
        int rightSeat = (int)PhotonNetwork.CurrentRoom.CustomProperties["RightFree"];


        // make seating occupied players a child of the seat



        spawnedPlayer = PhotonNetwork.Instantiate(pathToPrefabs + spawnedPlayerPrefab.name, XROrigin.transform.position, XROrigin.transform.rotation);

        // empty 
        // set the xrorigin as child, synchronize this to all connected clients, update the seat availability
        if (leftSeat == 0)
        {
            XROrigin.transform.SetParent(spawnedShip.transform.Find("Seats/Left Seat").transform);
            XROrigin.transform.localPosition = Vector3.zero;
            spawnedPlayer.GetPhotonView().RPC("SeatedAsChild", RpcTarget.All, 0, spawnedShip.GetPhotonView().ViewID, spawnedPlayer.GetPhotonView().ViewID);
            spawnedShip.GetPhotonView().RPC("UpdateSeatAvailability", RpcTarget.MasterClient, 0, spawnedPlayer.GetPhotonView().ViewID, PhotonNetwork.LocalPlayer.ActorNumber);
            return;
        }

        // occupied
        // set the gameobject of already seated players 
        else if (leftSeat > 0)
        {
            GameObject leftSeatedPlayer = PhotonView.Find(leftSeat)?.gameObject;
            leftSeatedPlayer.transform.SetParent(spawnedShip.transform.Find("Seats/Left Seat").transform);
            leftSeatedPlayer.transform.localPosition= Vector3.zero;
        }
        
        if (rightSeat == 0)
        {
            XROrigin.transform.SetParent(spawnedShip.transform.Find("Seats/Right Seat").transform);
            XROrigin.transform.localPosition = Vector3.zero;
            spawnedPlayer.GetPhotonView().RPC("SeatedAsChild", RpcTarget.All, 1, spawnedShip.GetPhotonView().ViewID , spawnedPlayer.GetPhotonView().ViewID);
            spawnedShip.GetPhotonView().RPC("UpdateSeatAvailability", RpcTarget.MasterClient, 1, spawnedPlayer.GetPhotonView().ViewID, PhotonNetwork.LocalPlayer.ActorNumber);
            return;
        }
        else if (rightSeat > 0)
        {
            GameObject rightSeatedPlayer = PhotonView.Find(rightSeat)?.gameObject;
            rightSeatedPlayer.transform.SetParent(spawnedShip.transform.Find("Seats/Right Seat").transform);
            rightSeatedPlayer.transform.localPosition = Vector3.zero;
        }

        // spawn ghost observers
        if (leftSeat != 0 && rightSeat != 0)
        {
            XROrigin.transform.SetParent(spawnedShip.transform.Find("Seats/Ghost Seat").transform);
            XROrigin.transform.localPosition = Vector3.zero;
            spawnedPlayer.GetPhotonView().RPC("SeatedAsChild", RpcTarget.All, 2, spawnedShip.GetPhotonView().ViewID, spawnedPlayer.GetPhotonView().ViewID);
            spawnedShip.GetPhotonView().RPC("UpdateSeatAvailability", RpcTarget.MasterClient, 2, spawnedPlayer.GetPhotonView().ViewID, PhotonNetwork.LocalPlayer.ActorNumber);

            return;
        }
        // TODO move all other ghosts to ghost seat as well
    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Destroy(spawnedPlayer);
        base.OnLeftRoom();
    }

  

    // seems like only master client get notified by this callback
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        if(PhotonNetwork.IsMasterClient)
        {
            spawnedShip.GetComponent<BoatManager>().UpdateSeatStatus(otherPlayer.ActorNumber);
        }
        // if masterclient is the one that left, updateSeatStatus needs to be called on master client takeover
    }

    

    private void InitializeShip()
    {

        if (PhotonNetwork.IsMasterClient)
        {
            spawnedShip = PhotonNetwork.InstantiateRoomObject(pathToPrefabs + spawnedShipPrefab.name, startingShipLocation.transform.position, startingShipLocation.transform.rotation);
            spawnedShip.GetPhotonView().RPC("SetShipID", RpcTarget.MasterClient);
        } else
        {
            int id = (int)PhotonNetwork.CurrentRoom.CustomProperties["ShipID"];
            PhotonView shipPhotonView = PhotonView.Find(id);
            spawnedShip = shipPhotonView?.gameObject;
        }

    }
    public GameObject getSpawnedShip()
    {
        return spawnedShip;
    }

}
