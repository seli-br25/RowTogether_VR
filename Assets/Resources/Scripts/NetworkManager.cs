using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using Photon.Voice.PUN;


public class NetworkManager : MonoBehaviourPunCallbacks
{
    private string roomSuffix = "'s Room";

    public bool DEBUG = false;

    // Start is called before the first frame update
    private void Awake()
    {
        if (DEBUG)
        {
            PhotonNetwork.ConnectUsingSettings();
            PhotonNetwork.NickName = "Player " + Random.Range(0, 100);
        }
    }
    void Start()
    {
        //Debug.Log("Try connect to server...");
        Debug.Log($"Scene Loaded, Photon network connection status: '{PhotonNetwork.IsConnected}'");

        PhotonNetwork.SendRate = 40;
        PhotonNetwork.SerializationRate = 10;
    }

    // Deprecated, will only be triggered in scene 0 when connected to lobby
    public override void OnConnectedToMaster()
    {
        if (DEBUG)
        {
            Debug.Log("Recon due to left room...");
            Debug.Log("We are now connected to the " + PhotonNetwork.CloudRegion + " server!");
            base.OnConnectedToMaster();

            TryJoinOrCreateRoom();
        }
    }

    // Deprecated, Room creation or joining managed by scene 0 in RoomManager
    private void TryJoinOrCreateRoom()
    {
        Debug.Log("Trying to join or create room.");
        RoomOptions roomOps = new RoomOptions();
        roomOps.MaxPlayers = 7;
        roomOps.IsVisible = true;
        roomOps.IsOpen = true;
        roomOps.EmptyRoomTtl = 0;
        PhotonNetwork.JoinOrCreateRoom("Test " + roomSuffix, roomOps, TypedLobby.Default);
    }

    // Deprecated, Room initialization will be managed by scene 0 in RoomManager. will only be triggered by scene 0
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
    }


    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Debug.Log("MasterClient switched!");
        base.OnMasterClientSwitched(newMasterClient);
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    int prevMasterClientActorNr = (int)PhotonNetwork.CurrentRoom.CustomProperties["MasterClientActorNumber"];
        //    GameObject boat = PhotonView.Find((int)PhotonNetwork.CurrentRoom.CustomProperties["ShipID"])?.gameObject;

        //    boat.GetComponent<BoatNetworkManager>().UpdateSeatStatus(prevMasterClientActorNr);


        //    Hashtable updatedProperties = new Hashtable();
        //    updatedProperties["MasterClientActorNumber"] = PhotonNetwork.MasterClient.ActorNumber;
        //    PhotonNetwork.CurrentRoom.SetCustomProperties(updatedProperties);
        //}

    }



    // Still useful for masterclient already in room. But implemented in NetworkPlayerSpawner
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        //Debug.Log("A new player joined the room");
        base.OnPlayerEnteredRoom(newPlayer);
    }

    public override void OnLeftRoom()
    {
        PunVoiceClient.Instance.Client.OpLeaveRoom(false);
        PunVoiceClient.Instance.Disconnect();

        StartCoroutine(DestroyPunVoiceClientAfterDisconnect());

        Debug.Log("Room left, returning to Lobby");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.LoadLevel(0);
    }

    System.Collections.IEnumerator DestroyPunVoiceClientAfterDisconnect()
    {
        // Wait until the client is fully disconnected
        while (PunVoiceClient.Instance.ClientState != Photon.Realtime.ClientState.Disconnected)
        {
            yield return null; // Wait for the next frame
        }

        // Safely destroy the PunVoiceClient GameObject
        Destroy(PunVoiceClient.Instance.gameObject);
        Debug.Log("PunVoiceClient destroyed after disconnect.");
    }
}
