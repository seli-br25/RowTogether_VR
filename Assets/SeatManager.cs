using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Unity.XR.CoreUtils;
using Photon.Realtime;

public class SeatManager : MonoBehaviourPunCallbacks, IPunObservable
{
    // Start is called before the first frame update
    [SerializeField]
    private GameObject leftSeat;
    private int leftPlayerViewID = 0;
    [SerializeField]
    private GameObject rightSeat;
    private int rightPlayerViewID = 0;
    [SerializeField]
    private GameObject ghostSeat;
    private List<int> ghostPlayerViewIDs = new List<int>();
    public bool seatsUpdated = false;


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


            stream.SendNext(ghostPlayerViewIDs.Count);
            foreach (int viewID in ghostPlayerViewIDs)
            {
                stream.SendNext(viewID);
            }

        }
        else if (stream.IsReading)
        {
            int tempLeftPlayerViewID = (int)stream.ReceiveNext();

            if (tempLeftPlayerViewID != 0 && tempLeftPlayerViewID != leftPlayerViewID)
            {
                PhotonView leftPlayerPhotonView = PhotonView.Find(tempLeftPlayerViewID);
                if (leftPlayerPhotonView != null && leftPlayerPhotonView.IsMine)
                {
                    XROrigin xROrigin = FindObjectOfType<XROrigin>();
                    SitDown(xROrigin.transform, leftSeat.transform);
                }

                GameObject leftPlayer = leftPlayerPhotonView?.gameObject;
                if (leftPlayer != null)
                {
                    SitDown(leftPlayer.transform, leftSeat.transform);
                } 

            }
            leftPlayerViewID = tempLeftPlayerViewID;


            int tempRightPlayerViewID = (int)stream.ReceiveNext();

            if (tempRightPlayerViewID != 0 && tempRightPlayerViewID != rightPlayerViewID)
            {

                PhotonView rightPlayerPhotonView = PhotonView.Find(tempRightPlayerViewID);
                if (rightPlayerPhotonView != null && rightPlayerPhotonView.IsMine)
                {
                    XROrigin xROrigin = FindObjectOfType<XROrigin>();
                    SitDown(xROrigin.transform, rightSeat.transform);
                }

                GameObject rightPlayer = rightPlayerPhotonView?.gameObject;
                if (rightPlayer != null)
                {
                    SitDown(rightPlayer.transform, rightSeat.transform);
                }

            }
            rightPlayerViewID = tempRightPlayerViewID;



            int ghostCount = (int)stream.ReceiveNext();
            ghostPlayerViewIDs.Clear();
            for (int i = 0; i < ghostCount; i++)
            {
                int viewID = (int)stream.ReceiveNext();
                ghostPlayerViewIDs.Add(viewID);
            }



            seatsUpdated = true;
        }
    }

    private IEnumerator WaitForPhotonView(int viewID, Transform targetSeat, float timeout = 2f)
    {
        GameObject player = null;
        float elapsedTime = 0f;

        while (player == null && elapsedTime < timeout)
        {
            PhotonView photonView = PhotonView.Find(viewID);
            if (photonView != null)
            {
                player = photonView.gameObject;
                SitDown(player.transform, targetSeat);
                yield break; // Exit the coroutine once the player is seated
            }
            elapsedTime += Time.deltaTime;
            yield return null; // Wait for the next frame
        }
    }

    private void SitDown(Transform player, Transform parent)
    {
        player.transform.SetParent(parent);
        player.transform.localPosition = Vector3.zero;
        player.transform.localRotation = Quaternion.identity;
    }

    // TODO everytime player leaves, update seat status
    [PunRPC]
    public void SeatSpawnedPlayer(int viewID)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonView networkedPhotonView = PhotonView.Find(viewID);
            GameObject networkedPlayer = networkedPhotonView?.gameObject;

            if (leftPlayerViewID == 0)
            {
                if (networkedPhotonView.IsMine)
                {
                    XROrigin xROrigin = FindObjectOfType<XROrigin>();
                    SitDown(xROrigin.transform, leftSeat.transform);
                }
                SitDown(networkedPlayer.transform, leftSeat.transform);
                leftPlayerViewID = viewID;
            }
            else if (rightPlayerViewID == 0) 
            {
                if (networkedPhotonView.IsMine)
                {
                    XROrigin xROrigin = FindObjectOfType<XROrigin>();
                    SitDown(xROrigin.transform, rightSeat.transform);
                }
                SitDown(networkedPlayer.transform, rightSeat.transform);
                rightPlayerViewID = viewID;
            }
            else
            {
                // impossible for Masterclient to end up spawning into ghost seat
                if (networkedPhotonView.IsMine)
                {    
                }
                SitDown(networkedPlayer.transform, ghostSeat.transform);
                ghostPlayerViewIDs.Add(viewID);
            }
        }
    }


    public void UpdateSeatStatus()
    {

        if (leftSeat.transform.childCount == 0)
        {
            leftPlayerViewID = 0;
        }

        if (rightSeat.transform.childCount == 0)
        {
            rightPlayerViewID = 0;
        }

        // Update the ghost seats
        for (int i = ghostPlayerViewIDs.Count - 1; i >= 0; i--)
        {
            int viewID = ghostPlayerViewIDs[i];
            PhotonView ghostPlayerView = PhotonView.Find(viewID);
            if (ghostPlayerView == null || ghostPlayerView.transform.parent != ghostSeat.transform)
            {
                ghostPlayerViewIDs.RemoveAt(i);
                Debug.Log($"Removed ghost player with ViewID {viewID} from ghost seats.");
            }
        }
    }




}
