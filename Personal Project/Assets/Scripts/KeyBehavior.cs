using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class KeyBehavior : MonoBehaviour
{
    private BoxCollider bc;
    private PlayerController playerController;

    private AudioSource audioSource;
    [SerializeField] private AudioClip gotKeyAudio;
    [SerializeField] private ParticleSystem keySpark;

    private bool isUsed = false;

    void Start()
    {
        bc = GetComponentInChildren<BoxCollider>();
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        //keySpark = GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isUsed)
        {
            isUsed = true;
            playerController.hasKey = true;

            if (!keySpark.isPlaying)
            {
                keySpark.Play();
            }

            GetComponentInChildren<MeshRenderer>().enabled = false;

            audioSource.PlayOneShot(gotKeyAudio, 0.3f);

            StartCoroutine(DestroyKeyOnEnd());
        }
    }

    IEnumerator DestroyKeyOnEnd()
    {
        while (audioSource.isPlaying || keySpark.isPlaying)
        {
            yield return new WaitForSeconds(0f);
        }

        Destroy(gameObject);
    }
}
