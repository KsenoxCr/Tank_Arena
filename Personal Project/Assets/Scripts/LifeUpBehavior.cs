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

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && !isUsed)
        {
            isUsed = true;

            audioSource.PlayOneShot(healthUpAudio, 0.3f);

            GetComponentInChildren<MeshRenderer>().enabled = false;

            StartCoroutine(DestroyLifeUpOnAudioEnd());

            // Increase Player's health points by 2

            playerController.hp = playerController.hp <= 2 ? playerController.hp + 2 : 4;

            Debug.Log("Player's Healthpoints: " + playerController.hp);
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
