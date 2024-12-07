using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

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

    private void Start()
    {
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Obstacle") && !isImmune)
        {
            LoseLife();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HeartItem"))
        {
            GainLife();
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

    private void LoseLife()
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

    private void GainLife()
    {
        if (lives < 3)
        {
            lives++;
            UpdateLivesUI();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over!");
        // TODO: implement game over
    }

    private void UpdateLivesUI()
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