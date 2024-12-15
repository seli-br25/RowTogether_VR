using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PaddleAudioManager : MonoBehaviour
{
    public AudioSource audioSource;          // The single AudioSource
    public List<AudioClip> weak;       // List of all available audio clips
    public List<AudioClip> strong;
    public bool allowSoundOverwrite = false; // If true, allows interrupting the currently playing sound

    private bool isPlaying = false; // Flag to track if a sound is playing

    // Function to play an audio clip by name


    public void PlayRandomAudio(int clips)
    {
        List<AudioClip> audioClips = new List<AudioClip>();
        if (clips == 0)
        {
            audioClips = strong;
        }
        else if (clips == 1)
        {
            audioClips = weak;
        }

        if (audioClips == null || audioClips.Count == 0)
        {
            Debug.LogWarning("AudioManager: No audio clips available to play.");
            return;
        }


        int randomIndex = Random.Range(0, audioClips.Count);

        // Select the random audio clip
        AudioClip randomClip = audioClips[randomIndex];

        audioSource.pitch = Random.Range(0.9f, 1.1f);

        if (isPlaying && !allowSoundOverwrite)
        {
            // dont overwrite clip
            return;
        }

        // Stop the currently playing clip if overwrite is allowed
        if (allowSoundOverwrite && audioSource.isPlaying)
        {
            audioSource.Stop();
        }
        StartCoroutine(PlayClip(randomClip));

        //audioSource.clip = randomClip;
        //audioSource.Play();

    }


    private IEnumerator PlayClip(AudioClip clip)
    {
        isPlaying = true;
        audioSource.clip = clip;
        audioSource.Play();

        yield return new WaitForSeconds(clip.length);

        isPlaying = false;
    }
}
