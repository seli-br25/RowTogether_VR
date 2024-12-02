using UnityEngine;
using UnityEngine.UI;
using System.Collections;

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

    private void Start()
    {
        paddleControllerLeft = paddleLeft.GetComponent<PaddleBoatController>();
        paddleControllerRight = paddleRight.GetComponent<PaddleBoatController>();
        initialForceMultiplier = paddleControllerLeft.forceMultiplier;
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
        Time.timeScale = 0f;
    }

    private void UpdateLivesUI()
    {
        Debug.Log("Lives: " + lives);
        // TODO: add UI for showing lives
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