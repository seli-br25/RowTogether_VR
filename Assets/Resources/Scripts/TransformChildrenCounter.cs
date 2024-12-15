using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformChildrenCounter : MonoBehaviourPun
{
    public SeatManager seatManager;

    private void OnTransformChildrenChanged()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Transform changed");
            seatManager.UpdateSeatStatus();
        }
    }
}
