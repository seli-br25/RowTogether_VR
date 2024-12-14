using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleAudioManager : MonoBehaviour
{
    public AudioSource audioSource;
    public float offsetTime = 2.0f; // Start playing from 2 seconds into the clip
    [Range(0f,100f)]
    public float delay = 1.0f; // Delay before playback starts

    void Start()
    {
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.time = offsetTime; // Set playback position
            audioSource.PlayScheduled(AudioSettings.dspTime + delay); // Schedule playback
        }
        else
        {
            Debug.LogError("AudioSource or AudioClip is missing!");
        }
    }
}

