using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;

public class GameplayManager : MonoBehaviour
{
    public int lives = 3; // start with 3 lives
    public float immunityTime = 3f;
    private bool isImmune = false;
    private float gameTimer = 0f;

    public PaddleBoatController paddleControllerLeft;
    public PaddleBoatController paddleControllerRight;
    private float initialForceMultiplier;
    private float initialTorqueMultiplier;

    public GameObject heartUI1;
    public GameObject heartUI2;
    public GameObject heartUI3;
    private List<GameObject> heartUIs;
    public Material deactivatedMaterial;
    private Material activatedMaterial;

    private PhotonView photonView;
    public UIManager uiManager;
    public Floater floater1;
    public Floater floater2;
    public Floater floater3;
    public Floater floater4;

    Transform initialBoatTransform;
    private float initialFloaterDepthBeforeSubmerge;
    //private GameObject heartItem1;
    //private GameObject heartItem2;


    // all hearts are created in editor and have a photon view from 200 - 220
    // if new hearts need to be added, assign them a appropriate photon view id in the range and add that id here.
    private List<int> heartPhotonViewIDs = new List<int>( new int[] {200, 201, 202, 203, 204, 205});

    private Rigidbody body;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        body = GetComponent<Rigidbody>();

        heartUIs = new List<GameObject>();
        heartUIs.Add(heartUI1);
        heartUIs.Add(heartUI2);
        heartUIs.Add(heartUI3);
        activatedMaterial = heartUI1.GetComponent<MeshRenderer>().materials[0];
        UpdateLivesUI();

        // instead of recording initial pos/rot at script init, use global already defined ship start location
        // edgecase: in the middle of play, new player joins. has its initial pos/rot somewhere in track middle
        // master client leaves/switched. newplayer can reset, but resets to middle of track
        initialBoatTransform = GameObject.Find("Ship Start Location").transform;
        //heartItem1 = GameObject.Find("Heart_Up1");
        //heartItem2 = GameObject.Find("Heart_Up2");
        initialFloaterDepthBeforeSubmerge = floater1.depthBeforeSubmerged;
        initialForceMultiplier = paddleControllerLeft.GetForceMultiplier();
        initialTorqueMultiplier = paddleControllerLeft.GetTorqueMultiplier();
    }

    public void InitializeUI(UIManager manager)
    {
        uiManager = manager;
        uiManager.RegisterButtonListener(ResetGame, StartGame);
    }

    public void InitializeGameplay()
    {
        //TODO check if not castin works as well
        UpdateRigidBodyConstraints((int)(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ));
    }

    private void Update()
    {
        if (lives > 0)
        {
            gameTimer += Time.deltaTime;
        }
    }
    // because non master clients have their boats set to kinematic and transforms synced via photon transform view, collisions are disabled for them until they become master clients
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && !isImmune)
        {
            LoseLife();

            if (photonView != null && photonView.IsMine)
            {
                photonView.RPC("SynchronizeLoseLife", RpcTarget.Others);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            if (other.CompareTag("HeartItem"))
            {
                GainLife();

                if (photonView != null && photonView.IsMine)
                {
                    photonView.RPC("SynchronizeGainLife", RpcTarget.Others);
                }

                other.gameObject.GetComponentInParent<HeartSynchronizer>().SetState(false);

                //other.gameObject.SetActive(false);
            }
            else if (other.CompareTag("SpeedTrap"))
            {
                paddleControllerLeft.SetForceMultiplier(1f);
                paddleControllerRight.SetForceMultiplier(1f);
                paddleControllerLeft.SetTorqueMultiplier(6f);
                paddleControllerRight.SetTorqueMultiplier(6f);
            }
            else if (other.CompareTag("Goal"))
            {
                // ui manager has access to gameplay, but gameplay does not have access to ui
                uiManager.SetGoalUI(gameTimer);
            }
        }
        
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SpeedTrap"))
        {
            paddleControllerLeft.SetForceMultiplier(initialForceMultiplier);
            paddleControllerRight.SetForceMultiplier(initialForceMultiplier);
            paddleControllerLeft.SetTorqueMultiplier(initialTorqueMultiplier);
            paddleControllerRight.SetTorqueMultiplier(initialTorqueMultiplier);
        }
    }

    public void LoseLife()
    {
        lives--;
        UpdateLivesUI();

        if (lives <= 0)
        {
            GameOver();
        }
        else
        {
            StartCoroutine(ActivateImmunity());
        }
    }

    public void GainLife()
    {
        if (lives < 3)
        {
            lives++;
            UpdateLivesUI();
        }
    }

    private void GameOver()
    {
        // only invoke game over if this boats photonview is mine (masterclients)
        // for non master clients, game over is synched through master client for safer approach agains desync
        if (photonView != null && photonView.IsMine)
        {
            Debug.Log("Game Over!");
            floater1.depthBeforeSubmerged = 4;
            floater2.depthBeforeSubmerged = 4;
            floater3.depthBeforeSubmerged = 4;
            floater4.depthBeforeSubmerged = 4;
            this.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            //
            uiManager.SetGameOverUI();
        }

    }

    public void UpdateLivesUI()
    {
        Debug.Log("Lives: " + lives);
        for (int i = 0; i < heartUIs.Count; i++)
        {
            MeshRenderer renderer = heartUIs[i].GetComponent<MeshRenderer>();
            Material[] materials = renderer.materials;
            if (i < lives)
            {
                materials[0] = activatedMaterial;
            }
            else
            {
                materials[0] = deactivatedMaterial;
            }
            renderer.materials = materials;
        }
                   
    }

    private IEnumerator ActivateImmunity()
    {
        isImmune = true;
        StartCoroutine(BlinkBoat());
        yield return new WaitForSeconds(immunityTime);
        isImmune = false;
    }

    private IEnumerator BlinkBoat()
    {
        Renderer boatRenderer = GetComponent<Renderer>();
        float blinkInterval = 0.15f; // time between blinks

        for (float i = 0; i < immunityTime; i += blinkInterval)
        {
            // toggle the visibility of the boat
            boatRenderer.enabled = !boatRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }

        boatRenderer.enabled = true;
    }

    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            uiManager.StartGame();
            photonView.RPC("StartCountdown", RpcTarget.All);
        }
    }


    // ISSUE WITH COUNTDOWNROUTINE
    // only existing players will be able to sync the countdown
    // new players will keep seeing the UI if joined late
    // on join fetch text. if master text is anything but "" or default text, then keep fetching text from master with 1 sec delay until ""
    public IEnumerator CountdownRoutine()
    {
        uiManager.UpdateUIText("3");
        yield return new WaitForSeconds(1);

        uiManager.UpdateUIText("2");
        yield return new WaitForSeconds(1);

        uiManager.UpdateUIText("1");
        yield return new WaitForSeconds(1);

        uiManager.UpdateUIText("Go!");

        // TODO if master client switched, constraints broken not established for new masterclient
        if (PhotonNetwork.IsMasterClient)
        {
            //boatRigidbody = transform.parent.parent.parent.gameObject.GetComponent<Rigidbody>();
            //boatRigidbody.constraints = RigidbodyConstraints.None;
        }

        // Instead constraints synced for all clients, and also freed for all clients
        // This way existing clients will have constraints freed and new clients joining mid game have correct constraints due to sync trigger request to masater on join
        UpdateRigidBodyConstraints((int)RigidbodyConstraints.None);
        if (PhotonNetwork.IsMasterClient) 
        {
            photonView.RPC("SynchronizeBoatConstraints", RpcTarget.Others, (int)(body.constraints));
        }

        yield return new WaitForSeconds(2);

        uiManager.UpdateUIText("");
        yield return new WaitForSeconds(1);

        // Due to canvas syncing required here at end of countdown for people joining between coroutine start and end. 
        // Cannot implement synchronizecanvas in character.cs photonview
        photonView.RPC("SynchronizeCanvas", RpcTarget.Others, "");
    }



    public IEnumerator FetchMasterCountdown()
    {
        yield return new WaitForSeconds(1);
        photonView.RPC("TriggerSynchronizeCanvas", RpcTarget.MasterClient, PhotonNetwork.LocalPlayer);
    }


    public void ResetAndSyncUI()
    {
        uiManager.ResetUI();
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ResetUI", RpcTarget.Others);
        }
    }

    public void ResetGame()
    {
        //TODO SYNC LIFE UI WITH ALL CLIENTS
        if (PhotonNetwork.IsMasterClient)
        {

            this.transform.position = initialBoatTransform.position;
            this.transform.rotation = initialBoatTransform.rotation;
            
            // reset and resync lives
            lives = 3;
            UpdateLivesUI();
            photonView.RPC("SynchronizeLives", RpcTarget.All, lives);

            gameTimer = 0f;

            ResetAndSyncUI();

            foreach(var id in heartPhotonViewIDs)
            {
                GameObject heartItem = PhotonView.Find(id)?.gameObject;
                if (heartItem != null)
                {
                    heartItem.GetComponent<HeartSynchronizer>().SetState(true);
                }
            }

            //heartItem1.SetActive(true);
            //heartItem2.SetActive(true);
            floater1.depthBeforeSubmerged = initialFloaterDepthBeforeSubmerge;
            floater2.depthBeforeSubmerged = initialFloaterDepthBeforeSubmerge;
            floater3.depthBeforeSubmerged = initialFloaterDepthBeforeSubmerge;
            floater4.depthBeforeSubmerged = initialFloaterDepthBeforeSubmerge;

            // reset boat constraints resync constraints
            UpdateRigidBodyConstraints((int)(RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ));
            photonView.RPC("SynchronizeBoatConstraints", RpcTarget.All, (int)body.constraints);
        }
    }


    public void UpdateRigidBodyConstraints(int constraints)
    {
        if (body == null)
        {
            body = GetComponent<Rigidbody>();
        }
        body.constraints = (RigidbodyConstraints)constraints;

    }
}