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
    [SerializeField] private float speed = 20f;
    private BoxCollider bc;

    private GameObject shooter;
    //private PlayerController playerControllerScript;
    private Vector3 bulletDirection;

    private GameManager gameManager;
    private SpawnManager spawnManager;

    public delegate void AllEnemiesKilledEventHandler(EnemyEventArgs enemyPos);

    public static event AllEnemiesKilledEventHandler AllEnemiesKilled;

    protected virtual void OnAllEnemiesKilled(Vector3 enemyPos)
    {
        if (AllEnemiesKilled != null)
        {
            AllEnemiesKilled(new EnemyEventArgs() { EnemyPos = enemyPos });
        }
    }

    public void Initialize(GameObject shooter)
    {
        this.shooter = shooter;
    }

    void Start()
    {
        bc = GetComponent<BoxCollider>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
    }

    void Update()
    {
        Debug.DrawLine(transform.position, transform.position + transform.forward, Color.blue);
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Wall") ||
            other.gameObject.CompareTag("Chest") ||
            other.gameObject.CompareTag("Key"))
        {
            Destroy(gameObject);
        }
        else if (other.gameObject.CompareTag("Enemy") && shooter.CompareTag("Player"))
        {
            Vector3 enemyPos = other.gameObject.transform.position;

            Destroy(gameObject);
            Destroy(other.gameObject);
            gameManager.enemyCount--;

            if (gameManager.enemyCount == 0)
            {
                OnAllEnemiesKilled(enemyPos);
            }
        }
        else if (other.gameObject.CompareTag("Player") && shooter.CompareTag("Enemy")) 
         //The object of type 'UnityEngine.GameObject' has been destroyed but you are still trying to access it.
        {
            Destroy(gameObject);
            PlayerController.hp--;
            Debug.Log("Player's hp: " + PlayerController.hp);
        }
    }
}


