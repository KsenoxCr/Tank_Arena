using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class LifeUpBehavior : MonoBehaviour
{
    private PlayerController playerController;
    private AudioSource audioSource;
    [SerializeField] private AudioClip healthUpAudio;

    private bool isUsed = false;

    void Start()
    {
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Player") && !isUsed)
        {
            isUsed = true;

            audioSource.PlayOneShot(healthUpAudio, 0.6f);

            GetComponentInChildren<MeshRenderer>().enabled = false;

            StartCoroutine(DestroyLifeUpOnAudioEnd());

            // Increase Player's health points by 2

            playerController.UpdateHealth(2);
        }
    }

    IEnumerator DestroyLifeUpOnAudioEnd()
    {
        while (audioSource.isPlaying)
        {
            yield return new WaitForSeconds(0);
        }

        Destroy(gameObject);
    }
}
