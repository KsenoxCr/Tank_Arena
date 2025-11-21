using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class KeyBehavior : MonoBehaviour
{
    private PlayerController playerController;
    private GameManager gameManager;

    private AudioSource audioSource;
    [SerializeField] private AudioClip gotKeyAudio;
    [SerializeField] private ParticleSystem keySpark;

    private bool isUsed;

    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        audioSource = GetComponent<AudioSource>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isUsed && gameManager.isGamePlaying)
        {
            isUsed = true;
            playerController.hasKey = true;

            if (!keySpark.isPlaying)
            {
                keySpark.Play();
            }

            GetComponentInChildren<MeshRenderer>().enabled = false;

            audioSource.PlayOneShot(gotKeyAudio, 0.6f);

            StartCoroutine(DestroyKeyOnEnd());
        }
    }

    IEnumerator DestroyKeyOnEnd()
    {
        while (audioSource.isPlaying || keySpark.isPlaying)
        {
            yield return null;
        }

        Destroy(gameObject);
    }
}
