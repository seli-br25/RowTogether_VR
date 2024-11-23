using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;

public class BoatManager : MonoBehaviourPunCallbacks
{
    [PunRPC]
    private void UpdateSeatAvailability(int seatNr, int playerId)
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
            updatedProperties["LeftFree"] = playerId;
        }
        else if (seatNr == 1)
        {
            updatedProperties["RightFree"] = playerId;
        }

        PhotonNetwork.CurrentRoom.SetCustomProperties(updatedProperties);

        Debug.Log($"Seat '{seatNr}' used by id: '{playerId}'.");

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