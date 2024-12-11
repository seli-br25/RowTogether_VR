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
    private UIManager uiManager;
    public Floater floater1;
    public Floater floater2;
    public Floater floater3;
    public Floater floater4;

    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float initialFloaterDepthBeforeSubmerge;
    private GameObject heartItem1;
    private GameObject heartItem2;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();

        heartUIs = new List<GameObject>();
        heartUIs.Add(heartUI1);
        heartUIs.Add(heartUI2);
        heartUIs.Add(heartUI3);
        activatedMaterial = heartUI1.GetComponent<MeshRenderer>().materials[0];
        UpdateLivesUI();

        uiManager = transform.Find("Seats/Left Seat/XR Origin (XR Rig)").GetComponent<UIManager>();
        initialPosition = this.transform.position;
        initialRotation = this.transform.rotation;
        heartItem1 = GameObject.Find("Heart_Up1");
        heartItem2 = GameObject.Find("Heart_Up2");
        initialFloaterDepthBeforeSubmerge = floater1.depthBeforeSubmerged;
        initialForceMultiplier = paddleControllerLeft.GetForceMultiplier();
        initialTorqueMultiplier = paddleControllerLeft.GetTorqueMultiplier();
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
        if (other.CompareTag("HeartItem"))
        {
            GainLife();

            if (photonView != null && photonView.IsMine)
            {
                photonView.RPC("SynchronizeGainLife", RpcTarget.Others);
            }
            
            other.gameObject.SetActive(false);
        } else  if (other.CompareTag("SpeedTrap"))
        {
            paddleControllerLeft.SetForceMultiplier(1f);
            paddleControllerRight.SetForceMultiplier(1f);
            paddleControllerLeft.SetTorqueMultiplier(6f);
            paddleControllerRight.SetTorqueMultiplier(6f);
        } else if (other.CompareTag("Goal"))
        {
            uiManager.SetGoalUI(gameTimer);
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

    public void ResetGame()
    {
        this.transform.position = initialPosition;
        this.transform.rotation = initialRotation;
        lives = 3;
        UpdateLivesUI();
        gameTimer = 0f;
        uiManager.ResetUI();
        heartItem1.SetActive(true);
        heartItem2.SetActive(true);
        floater1.depthBeforeSubmerged = initialFloaterDepthBeforeSubmerge;
        floater2.depthBeforeSubmerged = initialFloaterDepthBeforeSubmerge;
        floater3.depthBeforeSubmerged = initialFloaterDepthBeforeSubmerge;
        floater4.depthBeforeSubmerged = initialFloaterDepthBeforeSubmerge;
    }
}