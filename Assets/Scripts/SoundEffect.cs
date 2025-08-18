using UnityEngine;

public class SoundEffect : MonoBehaviour
{
    public AudioClip dashSound;
    public AudioClip jumpSound;
    public AudioClip landSound;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayDashSound()
    {
        audioSource.PlayOneShot(dashSound);
    }

    public void PlayJumpSound()
    {
        audioSource.PlayOneShot(jumpSound);
    }

    public void PlayLandSound()
    {
        audioSource.PlayOneShot(landSound);
    }
}
