using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioClip breakSound;
    public AudioClip itemSound;
    public AudioClip nextSound;

    private AudioSource audioSource;

    private void Awake()
    {
        Instance = this;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void PlaySound(string name)
    {
        switch (name)
        {
            case "Break": audioSource.PlayOneShot(breakSound); break;
            case "Item": audioSource.PlayOneShot(itemSound); break;
            case "Next": audioSource.PlayOneShot(nextSound); break;
        }
    }
}