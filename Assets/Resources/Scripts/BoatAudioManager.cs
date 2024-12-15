using System.Collections.Generic;
using UnityEngine;

public class BoatAudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip winClip;
    public AudioClip gameOverClip;
    public List<AudioClip> boatCrackings;
    // Start is called before the first frame update


    public void Start()
    {
        Debug.Log(boatCrackings);
    }
    public void PlayBoatCrack()
    {

        
        if (boatCrackings == null || boatCrackings.Count == 0)
        {
            Debug.Log("Boat no cracking sounds registered");
            return;
        }

        int randomIndex = Random.Range(0, boatCrackings.Count);

        // Select the random audio clip
        AudioClip randomClip = boatCrackings[randomIndex];

        audioSource.pitch = Random.Range(0.9f, 1.1f);


        // Stop the currently playing clip if overwrite is allowed
        Debug.Log(randomClip.name);
        audioSource.clip = randomClip;
        audioSource.Play();
    }

}
