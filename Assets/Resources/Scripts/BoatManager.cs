using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class BoatManager : MonoBehaviourPunCallbacks
{
    public Rigidbody body;
    [SerializeField]
    private GameplayManager boatGameplay;

    public void Start()
    {
        body = GetComponent<Rigidbody>();
        if (!PhotonNetwork.IsMasterClient)
        {
            body.isKinematic = true;
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        { 
            body.isKinematic = false;
        }
    }

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


    // On new player spawned, they will request from the master client to have master client rpc synch the boat status. 
    // TODO investigate why PhotonNetwork.localplayer (with eg viewid2) works in this instance.
    [PunRPC]
    public void TriggerBoatSync(Player targetPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("SynchronizeLives", targetPlayer, boatGameplay.lives);
            photonView.RPC("SynchronizeBoatConstraints", targetPlayer, (int)body.constraints);
            photonView.RPC("SyncGameTimer", targetPlayer, (float) boatGameplay.GetGameTimer());

            Debug.Log("MasterClient attempting to sync boat status");
        }
    }

    [PunRPC]
    public void SynchronizeBoatConstraints(int constraints)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            body.constraints = (RigidbodyConstraints)constraints;
        }
        Debug.Log("Boat Constraints updated");
    }


    [PunRPC]
    public void SynchronizeGainLife()
    {
        boatGameplay.GainLife();
    }

    [PunRPC]
    public void SynchronizeLoseLife()
    {
        boatGameplay.LoseLife();
    }

    

    // Only synch on non master clients
    [PunRPC]
    public void SynchronizeLives(int currentLives)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            boatGameplay.lives = currentLives;
            boatGameplay.UpdateLivesUI();
        }

        Debug.Log("Current boat status from master synchronized");
    }


    [PunRPC]
    public void SyncGameTimer(float time)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            boatGameplay.SetGameTimer(time);
        }
    }



    // Called after spawning networked player prefab. Triggering master to make an rpc to the requested player.
    [PunRPC]
    public void TriggerSynchronizeCanvas(Player targetPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("MasterClient attempting to sync canvas status");
            photonView.RPC("SynchronizeCanvas", targetPlayer, (string)boatGameplay.uiManager.textMesh.text);

        }
    }

    // Checks if the master client text is master text, meaninig has not started game yet
    // otherwise fetch text via coroutine if not ""
    // Seemingly sometimes this, but functionalities work....
    // NullReferenceException: Object reference not set to an instance of an object
    // BoatManager.SynchronizeCanvas(System.String t) (at Assets/Resources/Scripts/BoatManager.cs:136)
    [PunRPC]
    public void SynchronizeCanvas(string t)
    {
        if (!PhotonNetwork.IsMasterClient)
        {
            string masterText = boatGameplay.uiManager.masterClientText;
            string gameOverText = boatGameplay.uiManager.gameOverText;
            string goalText = boatGameplay.uiManager.goalText;


            if (t != masterText)
            {
                boatGameplay.uiManager.UpdateUIText(t);

                if (t == gameOverText)
                {
                    boatGameplay.SetGameOverAndSyncUI();
                } 
                else if (t.Contains(goalText))
                {
                    boatGameplay.SetGoalAndSyncUI(); 
                }
                // do the countdown fetching only if the text is not one of these from above
                else if (t != "")
                {
                    Debug.Log("Fetching UI from Master");
                    StartCoroutine(FetchMasterCountdown());
                }
            }
        }
    }


    public System.Collections.IEnumerator FetchMasterCountdown()
    {
        yield return new WaitForSeconds(1);
        photonView.RPC("TriggerSynchronizeCanvas", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }


    private string NormalizeText(string input)
    {
        return input.Replace("\r\n", "\n").Replace("\r", "\n").Trim();
    }





    [PunRPC]
    public void StartCountdown()
    {
        StartCoroutine(boatGameplay.CountdownRoutine());
    }


    [PunRPC]
    public void SetGoalUI(float timer)
    {
        boatGameplay.SetGameTimer(timer);
        boatGameplay.SetGoalAndSyncUI();
    }

    [PunRPC]
    public void SetGameOverUI()
    {
        boatGameplay.SetGameOverAndSyncUI();
    }


    [PunRPC]
    public void ResetUI()
    {
        boatGameplay.ResetAndSyncUI();
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