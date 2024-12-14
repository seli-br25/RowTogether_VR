using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Unity.XR.CoreUtils;
using ExitGames.Client.Photon;


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
    [SerializeField] 
    private UIManager uiManager;

    private int previousMasterClientActorNr;

    // executed by every client in room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("Player: " + newPlayer.NickName + " entered the room");
    }



    System.Collections.IEnumerator Start()
    {
        yield return new WaitUntil(() => PhotonNetwork.IsConnected && PhotonNetwork.InRoom);
        StartCoroutine(SpawnGameobjects());
    }

    private System.Collections.IEnumerator SpawnGameobjects()
    {
        yield return new WaitForEndOfFrame();

        InitializeShip();

        SpawnPlayer();

    }

    private void SpawnPlayer()
    {
        spawnedPlayer = PhotonNetwork.Instantiate(pathToPrefabs + spawnedPlayerPrefab.name, XROrigin.transform.position, XROrigin.transform.rotation);


        // use the SeatManagers PhotonView to invoke RPC for the master to assign seating.
        // should combat concurrency issue of multiple people wanting to sit down as seat distribution is handled by master instead of local now.
        spawnedShip.GetComponentInChildren<SeatManager>().gameObject.GetPhotonView().
            RPC("SeatSpawnedPlayer",
                RpcTarget.MasterClient,
                spawnedPlayer.GetPhotonView().ViewID
                );

    }

    public override void OnLeftRoom()
    {
        PhotonNetwork.Destroy(spawnedPlayer);
    }

  
    private void InitializeShip()
    {

        if (PhotonNetwork.IsMasterClient)
        {
            spawnedShip = PhotonNetwork.InstantiateRoomObject(pathToPrefabs + spawnedShipPrefab.name, startingShipLocation.transform.position, startingShipLocation.transform.rotation);


            Hashtable prop = new Hashtable()
            {
                {"ShipID", spawnedShip.GetPhotonView().ViewID }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(prop);

            Debug.Log($"Ship Id '{spawnedShip.GetPhotonView().ViewID}' set for room");

            spawnedShip.GetComponent<GameplayManager>().InitializeGameplay();
            spawnedShip.GetComponent<GameplayManager>().InitializeUI(uiManager);
        } 
        else
        {
            int id = (int)PhotonNetwork.CurrentRoom.CustomProperties["ShipID"];
            PhotonView shipPhotonView = PhotonView.Find(id);
            spawnedShip = shipPhotonView?.gameObject;
            spawnedShip.GetComponent<GameplayManager>().InitializeUI(uiManager);

            shipPhotonView.RPC("TriggerBoatSync", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
            shipPhotonView.RPC("TriggerSynchronizeCanvas", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
        }
    }
    public GameObject getSpawnedShip()
    {
        return spawnedShip;
    }

}
