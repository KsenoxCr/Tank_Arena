using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Chest : MonoBehaviour
{
    private GameObject player;
    private PlayerController playerController;

    private Animator chestAnim;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
        playerController = player.GetComponent<PlayerController>();
        chestAnim = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Player") && playerController.hasKey)
        {
            // Next Round starts
            playerController.hasKey = false;

            //chestAnimator.SetBool("isOpen", true);
            chestAnim.Play("Open");
            Debug.Log("SetBool should set isOpen to true here");
            StartCoroutine(WaitForAnimationEnd(gameObject));
        }
    }

    IEnumerator WaitForAnimationEnd(GameObject gameObj)
    {
        while (!chestAnim.GetCurrentAnimatorStateInfo(0).IsName("Close"))
            //while (!chestAnim.GetCurrentAnimatorStateInfo(0).IsName("Close") &&
            //       chestAnim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1 &&
            //       chestAnim.IsInTransition(0)) // Might not be needed
        {
            yield return null;
        }

        Destroy(gameObj);
        playerController.StartNewRound();
    }
}
