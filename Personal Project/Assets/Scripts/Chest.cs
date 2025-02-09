using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Chest : MonoBehaviour
{
    private GameManager gameManager;

    private PlayerController playerController;

    private Animator chestAnim;
    private AudioSource audioSource;
    [SerializeField] private AudioClip chestOpenAudio;
    [SerializeField] private AudioClip roundCompleteAudio;
    [SerializeField] private ParticleSystem[] fireworks;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        playerController = GameObject.Find("Player").GetComponent<PlayerController>();
        chestAnim = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Player") && playerController.hasKey && gameManager.isGamePlaying)
        {
            audioSource.PlayOneShot(chestOpenAudio, 0.8f);
            chestAnim.Play("Open");

            Invoke("PlayRandomFirework", 0.4f);

            playerController.hasKey = false;

            StartCoroutine(DestroyOnEnd());
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

    IEnumerator DestroyOnEnd()
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
