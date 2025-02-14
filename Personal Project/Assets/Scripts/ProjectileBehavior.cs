using System;
using System.IO.Pipes;
using TMPro.EditorUtilities;
using UnityEngine;

public class EnemyEventArgs : EventArgs
{
    public Vector3 EnemyPos { get; set; }
}

public class ProjectileBehavior : MonoBehaviour
{
    [Header("Bullet's Settings")]
    [SerializeField] private readonly float speed = 20f;

    private GameObject shooter;
    private Vector3 bulletDirection;

    [SerializeField] private GameManager gameManager;

    public void Initialize(GameObject shooter)
    {
        // Assigning the shooter to the projectile

        this.shooter = shooter;
    }

    void Update()
    {
         MoveBullet();
    }

    void MoveBullet()
    {
        // Moving bullet at shooter's forward direction

        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall") ||
            other.gameObject.CompareTag("Chest") ||
            other.gameObject.CompareTag("Key"))
        {
            // Destroying the bullet 

            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Enemy") && shooter.CompareTag("Player"))
        {
            // Destroying the bullet and killing the enemy

            Destroy(gameObject);

            EnemyBehavior enemyBehavior = other.gameObject.GetComponent<EnemyBehavior>();
            enemyBehavior.Kill();
        }
        else if (other.gameObject.CompareTag("Player") && shooter.CompareTag("Enemy")) 
        {
            // Destroying the bullet and damaging the player

            Destroy(gameObject);

            PlayerController playerController = other.gameObject.GetComponent<PlayerController>();
            playerController.UpdateHealth(-1);
        }
    }
}


