using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Lexic;


public class RoomManager : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update

    private string roomSuffix = "'s Room";
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        //PhotonNetwork.JoinOrCreateRoom("test", null, null);
        Debug.Log("Joined Lobby!");
    }



    public override void OnConnectedToMaster()
    {
        Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server!");
        base.OnConnectedToMaster();

    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        if (PhotonNetwork.IsMasterClient)
        {

            Hashtable properties = new Hashtable()
            {
                { "LeftFree", 0},
                { "RightFree", 0},
                { "MasterClientActorNumber", PhotonNetwork.MasterClient.ActorNumber }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(properties);

        }
        Debug.Log("Joined a Room");
        PhotonNetwork.LoadLevel(1);
    }


    public void OnClickJoinRoom(string roomName)
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }

        TryJoinRoom(roomName);
    }

    public void OnClickCreateRoom()
    {
        if (!PhotonNetwork.IsConnected)
        {
            return;
        }

        TryCreateRoom();
    }


    private void TryJoinRoom(string roomName)
    {
        Debug.Log($"Trying to join room '{roomName}'.");
        PhotonNetwork.JoinRoom(roomName);
    }

    private void TryCreateRoom()
    {
        if (PhotonNetwork.IsConnected && PhotonNetwork.NickName != "")
        {
            Debug.Log("Trying to create room.");
            RoomOptions roomOps = new RoomOptions();
            roomOps.MaxPlayers = 7;
            roomOps.IsVisible = true;
            roomOps.IsOpen = true;
            roomOps.EmptyRoomTtl = 0;
            roomOps.PlayerTtl = 0;
            PhotonNetwork.CreateRoom(PhotonNetwork.NickName + roomSuffix, roomOps, TypedLobby.Default);
        } else
        {
            Debug.Log("NickName Not Set!");
        }
    }

    public override void OnCreatedRoom()
    {
        Debug.Log($"Room with name '{PhotonNetwork.NickName + roomSuffix}' created, Master Client already joined the room.");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Room Joining failed. Error code: {returnCode}, Message: {message}");
        switch (returnCode)
        {
            case ErrorCode.GameIdAlreadyExists: // 32766
                Debug.LogError("A room with this name already exists. Generating a new name... ?");
                //TryCreateRoom(); // Retry with a new name or notify the user
                break;

            case ErrorCode.InvalidRegion: // 32756
                Debug.LogError("Invalid region. Ensure your Photon configuration is correct.");
                break;

            case ErrorCode.MaxCcuReached: // 32757
                Debug.LogError("Maximum CCU (Concurrent Users) reached. Upgrade your Photon subscription.");
                break;

            default:
                Debug.LogError("An unexpected error occurred. Please try again.");
                break;
        }
    }


    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Room creation failed. Error code: {returnCode}, Message: {message}");
        // Handle specific errors if needed
        switch (returnCode)
        {
            case ErrorCode.GameIdAlreadyExists: // 32766
                Debug.LogError("A room with this name already exists. Generating a new name... ?");
                //TryCreateRoom(); // Retry with a new name or notify the user
                break;

            case ErrorCode.InvalidRegion: // 32756
                Debug.LogError("Invalid region. Ensure your Photon configuration is correct.");
                break;

            case ErrorCode.MaxCcuReached: // 32757
                Debug.LogError("Maximum CCU (Concurrent Users) reached. Upgrade your Photon subscription.");
                break;

            default:
                Debug.LogError("An unexpected error occurred. Please try again.");
                break;
        }

        //TODO display error on UI ?
    
    }

}
