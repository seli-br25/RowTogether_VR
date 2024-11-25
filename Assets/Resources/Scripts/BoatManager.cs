using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using UnityEngine.XR.Interaction.Toolkit;

public class BoatManager : MonoBehaviourPunCallbacks
{




    public void UpdateSeatStatus(int actorNumber)
    {
        Debug.Log($"Updating Seat status with ActorNumber '{actorNumber}'");
        int leftSeat = (int)PhotonNetwork.CurrentRoom.CustomProperties["LeftFree"];
        int rightSeat = (int)PhotonNetwork.CurrentRoom.CustomProperties["RightFree"];



        // check who is seated
        if ((int)PhotonNetwork.CurrentRoom.CustomProperties[$"{actorNumber}"] == leftSeat)
        {
            photonView.RPC("UpdateSeatAvailability", RpcTarget.MasterClient, 0, 0, actorNumber);

        }
        else if ((int)PhotonNetwork.CurrentRoom.CustomProperties[$"{actorNumber}"] == rightSeat)
        {
            photonView.RPC("UpdateSeatAvailability", RpcTarget.MasterClient, 1, 0, actorNumber);
        }

        leftSeat = (int)PhotonNetwork.CurrentRoom.CustomProperties["LeftFree"];
        rightSeat = (int)PhotonNetwork.CurrentRoom.CustomProperties["RightFree"];

        Debug.Log($"Current leftSeat {leftSeat}, rightSeat {rightSeat}");
    }

    // the playerViewId is used to store the spawned and synched photonview of the player rig. helps synchronize which photonview is currently sitting in the boat
    // the playerActorNumber is used in the hashtable to identify which player left and which photonview belonged to that player.
    [PunRPC]
    private void UpdateSeatAvailability(int seatNr, int playerViewId, int playerActorNumber)
    {

        // Non master client should not update room properties
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }

        //! CASE already handled by Networkmanager on leave
        // TODO fetch list of all active players
        // if playerId <= first 2 indexes, then update seat
        Hashtable updatedProperties = new Hashtable();

        if (seatNr == 0)
        {
            updatedProperties["LeftFree"] = playerViewId;
        }
        else if (seatNr == 1)
        {
            updatedProperties["RightFree"] = playerViewId;
        }
        updatedProperties[$"{playerActorNumber}"] = playerViewId;

        PhotonNetwork.CurrentRoom.SetCustomProperties(updatedProperties);


        Debug.Log($"Seat '{seatNr}' used by id: '{playerViewId}' belonging to '{playerActorNumber}'.");


        //Hashtable roomProperties = PhotonNetwork.CurrentRoom.CustomProperties;

        //foreach (var key in roomProperties.Keys)
        //{
        //    Debug.Log($"Key: {key}, Value: {roomProperties[key]}");
        //}
        // TODO implement prompt for player joining in seat
    }



    [PunRPC]
    private void SetShipID()
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            return;
        }
        Hashtable prop = new Hashtable()
        {
            {"ShipID", GetComponent<PhotonView>().ViewID }
        };
        PhotonNetwork.CurrentRoom.SetCustomProperties(prop);

        Debug.Log($"Ship Id '{GetComponent<PhotonView>().ViewID}' set for room");
    }



}