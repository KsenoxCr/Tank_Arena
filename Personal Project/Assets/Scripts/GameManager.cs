using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int round;
    public int lastRound = 25;
    public int enemyCount = 0;
    public float roundTime = 0;
    public float startShootingCooldown = 2f;

    public GameObject[] movingPoints = new GameObject[22];
    public Vector3[] movingPointPositions;
    public Vector3[] cornerMovingPoints;

    private SpawnManager spawnManager;

    void Start()
    {
        round = 0;
        spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();

        // saving movingPoints positions to movingPointPositions

        movingPointPositions = new Vector3[movingPoints.Length];

        for (int i = 0; i < movingPoints.Length; i++)
        {
            movingPointPositions[i] = movingPoints[i].transform.position;
        }

        // Saving corner movingPoints

        cornerMovingPoints = new Vector3[4] { movingPointPositions[1], movingPointPositions[3],
            movingPointPositions[5] , movingPointPositions[7] };
        
        StartNewRound();
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