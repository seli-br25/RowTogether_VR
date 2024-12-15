using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.XR.CoreUtils;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SeatManager : MonoBehaviourPunCallbacks, IPunObservable
{
    // Start is called before the first frame update
    [SerializeField]
    private GameObject leftSeat;
    public int leftPlayerViewID = 0;
    [SerializeField]
    private GameObject rightSeat;
    public int rightPlayerViewID = 0;
    [SerializeField]
    private GameObject ghostSeat0;
    public int ghostPlayerViewID0 = 0;
    [SerializeField]
    private GameObject ghostSeat1;
    public int ghostPlayerViewID1 = 0;
    [SerializeField]
    private GameObject ghostSeat2;
    public int ghostPlayerViewID2 = 0;
    [SerializeField]
    private GameObject ghostSeat3;
    public int ghostPlayerViewID3 = 0;
    [SerializeField]
    private GameObject ghostSeat4;
    public int ghostPlayerViewID4 = 0;

    private PhotonView myPhotonView;

    public bool seatsUpdated = false;


    // TODO if have time:
    // implement seat change && ghost player head materials


    // Custom sync logic
    // Whenever a new player joins and instantiates their networked player
    // A rpc call will trigger the masterclient to assign a seat to the player 
    // the left/right/ghostPlayerViewID will change and thus trigger a serializeview update
    // in master clients instance, the networked player is set as a child of the boat 
    // Since this boat is automatically synced via photon view, the new player will be notified to move its networked player and XROrigin to the assigned seat
    // Since this boat is synced, the new player will also find and move all other players networked player to the synced seatings
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {

        if (stream.IsWriting)
        {
            stream.SendNext(leftPlayerViewID);
            stream.SendNext(rightPlayerViewID);

            stream.SendNext(ghostPlayerViewID0);
            stream.SendNext(ghostPlayerViewID1);
            stream.SendNext(ghostPlayerViewID2);
            stream.SendNext(ghostPlayerViewID3);
            stream.SendNext(ghostPlayerViewID4);

        }
        else if (stream.IsReading)
        {
            int tempID;
            // left seat
            tempID = (int)stream.ReceiveNext();
            AssignSeating(tempID, leftPlayerViewID, leftSeat.transform, 1);
            leftPlayerViewID = tempID;


            // right seat
            tempID = (int)stream.ReceiveNext();
            AssignSeating(tempID, rightPlayerViewID, rightSeat.transform, 1);
            rightPlayerViewID = tempID;

            // ghost seats
            tempID = (int)stream.ReceiveNext();
            AssignSeating(tempID, ghostPlayerViewID0, ghostSeat0.transform, 0);
            ghostPlayerViewID0 = tempID;

            tempID = (int)stream.ReceiveNext();
            AssignSeating(tempID, ghostPlayerViewID1, ghostSeat1.transform, 0);
            ghostPlayerViewID1 = tempID;

            tempID = (int)stream.ReceiveNext();
            AssignSeating(tempID, ghostPlayerViewID2, ghostSeat2.transform, 0);
            ghostPlayerViewID2 = tempID;

            tempID = (int)stream.ReceiveNext();
            AssignSeating(tempID, ghostPlayerViewID3, ghostSeat3.transform, 0);
            ghostPlayerViewID3 = tempID;

            tempID = (int)stream.ReceiveNext();
            AssignSeating(tempID, ghostPlayerViewID4, ghostSeat4.transform, 0);
            ghostPlayerViewID4 = tempID;



            seatsUpdated = true;
        }
    }

    private void AssignSeating(int tempID, int playerID, Transform seat, int playerGhost)
    {
        PhotonView tempView;
        GameObject tempPlayer;

        if (tempID != 0 && tempID != playerID)
        {

            seat.gameObject.GetComponentInChildren<Button>().gameObject.SetActive(false);

            tempView = PhotonView.Find(tempID);
            if (tempView != null && tempView.IsMine)
            {
                XROrigin xROrigin = FindObjectOfType<XROrigin>();
                SitDown(xROrigin.transform, seat);
                if (playerGhost == 0)
                {
                    xROrigin.GetComponent<PlayerGhostVisualizer>().SetGhost();
                }
                else if (playerGhost == 1)
                {
                    xROrigin.GetComponent<PlayerGhostVisualizer>().SetPlayer();
                }


                myPhotonView = tempView;
            }

            tempPlayer = tempView?.gameObject;
            if (tempPlayer != null)
            {
                SitDown(tempPlayer.transform, seat);
            }

            if (playerGhost == 0)
            {
                tempPlayer.GetComponent<PlayerGhostVisualizer>().SetGhost();
            }
            else if (playerGhost == 1)
            {
                tempPlayer.GetComponent<PlayerGhostVisualizer>().SetPlayer();
            }
            

        } else if (tempID == 0)
        {
            seat.gameObject.GetComponentInChildren<Button>(true).gameObject.SetActive(true);
        }
    }



    private void SitDown(Transform player, Transform parent)
    {
        player.transform.SetParent(parent);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;
    }
    

    // only seat if the tags are matching and the seat is empty, which it should anyways because player should not be able to request occupied seats

    [PunRPC]
    public void ReqestSeatTransfer(int viewID, string seatTag)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (seatTag != null && viewID != 0)
            {
                if (seatTag == leftSeat.tag && leftPlayerViewID == 0)
                {
                    AssignSeating(viewID, leftPlayerViewID, leftSeat.transform, 1);
                    leftPlayerViewID = viewID;
                }
                else if (seatTag == rightSeat.tag && rightPlayerViewID == 0)
                {
                    AssignSeating(viewID, rightPlayerViewID, rightSeat.transform, 1);
                    rightPlayerViewID = viewID;
                }
                else if (seatTag == ghostSeat0.tag && ghostPlayerViewID0 == 0)
                {
                    AssignSeating(viewID, ghostPlayerViewID0, ghostSeat0.transform, 0);
                    ghostPlayerViewID0 = viewID;
                }
                else if (seatTag == ghostSeat1.tag && ghostPlayerViewID1 == 0)
                {
                    AssignSeating(viewID, ghostPlayerViewID1, ghostSeat1.transform, 0);
                    ghostPlayerViewID1 = viewID;
                }
                else if (seatTag == ghostSeat2.tag && ghostPlayerViewID2 == 0)
                {
                    AssignSeating(viewID, ghostPlayerViewID2, ghostSeat2.transform, 0);
                    ghostPlayerViewID2 = viewID;
                }
                else if (seatTag == ghostSeat3.tag && ghostPlayerViewID3 == 0)
                {
                    AssignSeating(viewID, ghostPlayerViewID3, ghostSeat3.transform, 0);
                    ghostPlayerViewID3 = viewID;
                }
                else if (seatTag == ghostSeat4.tag && ghostPlayerViewID4 == 0)
                {
                    AssignSeating(viewID, ghostPlayerViewID4, ghostSeat4.transform, 0);
                    ghostPlayerViewID4 = viewID;
                }

            }
        }
    }

    [PunRPC]
    public void SeatSpawnedPlayer(int viewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {

            if (leftPlayerViewID == 0)
            {
                AssignSeating(viewID, leftPlayerViewID, leftSeat.transform, 1);
                leftPlayerViewID = viewID;

            }
            else if (rightPlayerViewID == 0)
            {
                AssignSeating(viewID, rightPlayerViewID, rightSeat.transform, 1);
                rightPlayerViewID = viewID;
            }
            else if (ghostPlayerViewID0 == 0)
            {
                AssignSeating(viewID, ghostPlayerViewID0, ghostSeat0.transform, 0);
                ghostPlayerViewID0 = viewID;
            }
            else if (ghostPlayerViewID1 == 0)
            {
                AssignSeating(viewID, ghostPlayerViewID1, ghostSeat1.transform, 0);
                ghostPlayerViewID1 = viewID;

            }
            else if (ghostPlayerViewID2 == 0)
            {
                AssignSeating(viewID, ghostPlayerViewID2, ghostSeat2.transform, 0);
                ghostPlayerViewID2 = viewID;

            }
            else if (ghostPlayerViewID3 == 0)
            {
                AssignSeating(viewID, ghostPlayerViewID3, ghostSeat3.transform, 0);
                ghostPlayerViewID3 = viewID;
            }
            else if (ghostPlayerViewID4 == 0)
            {
                AssignSeating(viewID, ghostPlayerViewID4, ghostSeat4.transform, 0);
                ghostPlayerViewID4 = viewID;
            }
        }
    }


    public void UpdateSeatStatus()
    {

        if (leftSeat.transform.childCount == 1)
        {
            leftPlayerViewID = 0;
            leftSeat.GetComponentInChildren<Button>(true).gameObject.SetActive(true);
        }

        if (rightSeat.transform.childCount == 1)
        {
            rightPlayerViewID = 0;
            rightSeat.GetComponentInChildren<Button>(true).gameObject.SetActive(true);
        }

        if (ghostSeat0.transform.childCount == 1)
        {
            ghostPlayerViewID0 = 0;
            ghostSeat0.GetComponentInChildren<Button>(true).gameObject.SetActive(true);
        }

        if (ghostSeat1.transform.childCount == 1)
        {
            ghostPlayerViewID1 = 0;
            ghostSeat1.GetComponentInChildren<Button>(true).gameObject.SetActive(true);
        }

        if (ghostSeat2.transform.childCount == 1)
        {
            ghostPlayerViewID2 = 0;
            ghostSeat2.GetComponentInChildren<Button>(true).gameObject.SetActive(true);
        }

        if (ghostSeat3.transform.childCount == 1)
        {
            ghostPlayerViewID3 = 0;
            ghostSeat3.GetComponentInChildren<Button>(true).gameObject.SetActive(true);
        }

        if (ghostSeat4.transform.childCount == 1)
        {
            ghostPlayerViewID4 = 0;
            ghostSeat4.GetComponentInChildren<Button>(true).gameObject.SetActive(true);
        }

    }

    public void OnButtonClick(GameObject buttonPressInvokedParent)
    {
        if (buttonPressInvokedParent != null)
        {
            photonView.RPC("ReqestSeatTransfer", RpcTarget.MasterClient, myPhotonView.ViewID, buttonPressInvokedParent.tag);
        }
    }


}
