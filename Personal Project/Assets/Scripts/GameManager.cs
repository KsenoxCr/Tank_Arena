using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int round;
    public int lastRound = 25;
    public int enemyCount = 0;
    public float roundTime = 0;
    public float startShootingCooldown = 2f;

    private SpawnManager spawnManager;

    void Start()
    {
        round = 0;
        spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
    }

    void Update()
    {
    }

    public void StartNewRound()
    {
        if (round > 0)
        {
            Destroy(GameObject.FindGameObjectWithTag("Life Up"));
            spawnManager.takenSpawnPositions.Clear();
        }
        round++;

        spawnManager.SpawnEnemyWave(round);
        spawnManager.SpawnChest();


        Debug.Log("Round: " + round);

        // Finishing round 50, player wins

        if (round > lastRound)
        {
            Debug.Log("You win!");
        }
    }
}