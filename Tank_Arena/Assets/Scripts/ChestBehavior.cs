using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class ChestBehavior : MonoBehaviour
{
    private GameManager gameManager;

    private PlayerController playerController;
    private Animator chestAnim;
    private AudioSource audioSource;
    [SerializeField] private AudioClip chestOpenAudio;
    [SerializeField] private AudioClip roundCompleteAudio;
    [SerializeField] private ParticleSystem[] fireworks;

    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        chestAnim = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Player") && playerController.hasKey && gameManager.isGamePlaying)
        {
            audioSource.PlayOneShot(chestOpenAudio, 0.8f);
            chestAnim.Play("Open");

            Invoke("PlayRandomFirework", 0.4f);

            playerController.hasKey = false;

            StartCoroutine(DestroyAndStartRoundOnEnd());
        }
    }

    private void PlayRandomFirework()
    {
        audioSource.PlayOneShot(roundCompleteAudio, 0.2f);

        int randomIndex = Random.Range(0, fireworks.Length);

        if (!fireworks[randomIndex].isPlaying)
        {
            fireworks[randomIndex].Play();
        }
    }

    IEnumerator DestroyAndStartRoundOnEnd()
    {
        while (!chestAnim.GetCurrentAnimatorStateInfo(0).IsName("Close")
               || audioSource.isPlaying)
        {
            yield return null;
        }

        Destroy(gameObject);
        gameManager.StartNewRound();
    }
}
