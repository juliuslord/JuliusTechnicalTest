using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionNoise : MonoBehaviour
{
    public AudioClip[] audioClips; // Array of audio clips to choose from
    private AudioSource audioSource; // Audio source to play the audio clip

    private void Start()
    {
        // Get the audio source component attached to this game object
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the audio source is already playing a clip
        if (!audioSource.isPlaying)
        {
            // Choose a random audio clip from the array
            AudioClip randomClip = audioClips[Random.Range(0, audioClips.Length)];

            // Play the audio clip through the audio source
            audioSource.clip = randomClip;
            audioSource.Play();
        }
    }
}