using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private string roomPrefix = "Room ";
    private int currentRoomIndex = 1;

    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("Try connect to server...");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server!");
        base.OnConnectedToMaster();

        TryJoinOrCreateRoom();
    }

    private void TryJoinOrCreateRoom()
    {
        Debug.Log("Trying to join or create room.");
        RoomOptions roomOps = new RoomOptions();
        roomOps.MaxPlayers = 20;
        roomOps.IsVisible = true;
        roomOps.IsOpen = true;
        PhotonNetwork.JoinOrCreateRoom(roomPrefix + currentRoomIndex, roomOps, TypedLobby.Default);
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined a Room");
        base.OnJoinedRoom();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Failed to join room {roomPrefix}{currentRoomIndex}: {message}, trying to join next room...");
        currentRoomIndex++;
        TryJoinOrCreateRoom();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("A new player joined the room");
        base.OnPlayerEnteredRoom(newPlayer);
    }

    public override void OnLeftRoom()
    {
        Debug.Log("Left the room. Restarting room join process...");
        currentRoomIndex = 1;
        TryJoinOrCreateRoom();
    }
}
