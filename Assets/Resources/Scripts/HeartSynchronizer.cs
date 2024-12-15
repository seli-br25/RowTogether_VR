using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class HeartSynchronizer : MonoBehaviourPun, IPunObservable
{


    private GameObject heartObject;
    public bool isActive;


    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isActive);
        }
        else if (stream.IsReading)
        {
            isActive = (bool)stream.ReceiveNext();
            heartObject.SetActive(isActive);
        }
    }

    void Awake()
    {
        isActive = true;
        heartObject = this.transform.GetChild(0).gameObject;
        heartObject.SetActive(isActive);
    }


    public void SetState(bool state)
    {
        isActive = state;
        heartObject?.SetActive(state);
    }

}
