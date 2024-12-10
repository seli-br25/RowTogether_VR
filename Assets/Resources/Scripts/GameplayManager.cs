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

    public GameObject paddleLeft;
    public GameObject paddleRight;
    private PaddleBoatController paddleControllerLeft;
    private PaddleBoatController paddleControllerRight;
    private float initialForceMultiplier;

    public GameObject heartUI1;
    public GameObject heartUI2;
    public GameObject heartUI3;
    private List<GameObject> heartUIs;
    public Material deactivatedMaterial;
    private Material activatedMaterial;

    private PhotonView photonView;

    private void Start()
    {
        photonView = GetComponent<PhotonView>();
        paddleControllerLeft = paddleLeft.GetComponent<PaddleBoatController>();
        paddleControllerRight = paddleRight.GetComponent<PaddleBoatController>();
        initialForceMultiplier = paddleControllerLeft.forceMultiplier;

        heartUIs = new List<GameObject>();
        heartUIs.Add(heartUI1);
        heartUIs.Add(heartUI2);
        heartUIs.Add(heartUI3);
        activatedMaterial = heartUI1.GetComponent<MeshRenderer>().materials[0];
        UpdateLivesUI();
    }

    private void Update()
    {
        if (lives > 0)
        {
            gameTimer += Time.deltaTime;
            UpdateTimerUI();
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
            paddleControllerLeft.forceMultiplier = 4f;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("SpeedTrap"))
        {
            paddleControllerLeft.forceMultiplier = initialForceMultiplier;
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

            // Maybe just disable all floaters so the boat sinks down, but the view when sunk is empty/ transparent ...
            // TODO: implement game over
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

    private void UpdateTimerUI()
    {
        // format and display the elapsed time in the UI
        int minutes = Mathf.FloorToInt(gameTimer / 60F);
        int seconds = Mathf.FloorToInt(gameTimer % 60F);
        //Debug.Log(string.Format("{0:00}:{1:00}", minutes, seconds));
        // TODO: add UI for showing time
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
}