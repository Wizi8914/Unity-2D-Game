using System.Collections.Generic;
using UnityEngine;

public class FootStepSound : MonoBehaviour
{
    AudioSource audioSource; // Reference to the AudioSource component
    public List<AudioClip> footstepSounds; // Array of footstep sound clips

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>(); // Get the AudioSource component
    }

    // Play a random footstep sound
    public void PlayFootstepSound()
    {
        if (footstepSounds.Count > 0)
        {
            AudioClip clip = footstepSounds[Random.Range(0, footstepSounds.Count)];

            audioSource.pitch = Random.Range(0.8f, 1.2f);

            audioSource.PlayOneShot(clip);
        }
    }
}
