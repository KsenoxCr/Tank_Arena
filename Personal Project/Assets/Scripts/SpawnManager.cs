using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

public class SpawnManager : MonoBehaviour
{
    private GameManager gameManager;

    public GameObject enemyPrefab;
    public GameObject chestPrefab;
    public GameObject lifeUpPrefab;
    public GameObject bulletPrefab;
    public GameObject keyPrefab;
    private Vector3 prevChestPosition = new Vector3(0, 0, 0);
    private Vector3 prevLifeUpPosition = new Vector3(0, 0, 0);

    public List<Vector3> takenSpawnPositions = new();

    void Awake()
    {
        // Setting a reference to GameManager

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Start()
    {
        // Subscribing to the AllEnemiesKilled event

        EnemyBehavior.AllEnemiesKilled += OnAllEnemiesKilled;
    }

    void OnDisable()
    {
        // Unsubscribing to the AllEnemiesKilled event

        EnemyBehavior.AllEnemiesKilled -= OnAllEnemiesKilled;
    }

    public void SpawnEnemyWave(int round)
    {
        // Spawning Enemy wave based on round number

        gameManager.roundTime = Time.time;

        if (round >= 1 && round < 3)
        {
            gameManager.enemyCount = 1;
        }
        else if (round >= 3 && round < 10)
        {
            gameManager.enemyCount = 2;
        }
        else if (round >= 10 && round < 15)
        {
            gameManager.enemyCount = 3;
        }
        else if (round >= 15 && round < 21)
        {
            gameManager.enemyCount = 4;
        }

        // Spawn Life up on round 1, 3, 10, 15 and 20

        if (round == 1 || round == 3 || round == 10 || round == 15 || round == 20)
        {
            SpawnLifeUp();
        }

        // Spawning appropriate number of enemies for the current round
        
        //Invariant: i is the amount of enemies spawned
        for (int i = 0; i < gameManager.enemyCount; i++)
        {
            Instantiate(enemyPrefab, RandomCorner(), enemyPrefab.transform.rotation);
        }
    }

    public void SpawnChest()
    {
        // Spawning chest to a random moving point, on layer 2-3

        Vector3 spawningPosition = RandomMovingPoint(2, prevChestPosition);
        Instantiate(chestPrefab, spawningPosition, chestPrefab.transform.rotation);
        prevChestPosition = spawningPosition;
    }

    public void SpawnLifeUp()
    {
        // Spawning life up to a random moving point, on layer 1-3

        Vector3 spawningPosition = RandomMovingPoint(1, prevLifeUpPosition);
        Instantiate(lifeUpPrefab, spawningPosition, lifeUpPrefab.transform.rotation);
        prevLifeUpPosition = spawningPosition;
    }
    void SpawnKey(Vector3 position)
    {
        // Spawning key on the position of the last killed enemy

        Instantiate(keyPrefab, position, keyPrefab.transform.rotation);
    }

    Vector3 RandomMovingPoint(int layer, Vector3 prevPoint)
    {
        // Choosing a new random moving point
        // that not taken yet on the current round

        Vector3 randomMovingPoint;
        int minInclusive = layer == 2 ? 8 : 0;

        do
        {
            int randomIndex = Random.Range(minInclusive, gameManager.movingPointPositions.Length);
            randomMovingPoint = gameManager.movingPointPositions[randomIndex];

        } while (takenSpawnPositions.Contains(randomMovingPoint) || randomMovingPoint == prevPoint);

        takenSpawnPositions.Add(randomMovingPoint);

        return new Vector3(randomMovingPoint.x, 0.85f, randomMovingPoint.z);
    }

    Vector3 RandomCorner()
    {
        // Picking a random corner out of corner moving points 
        // that not taken yet on the current round

        Vector3 randomCorner;

        do
        {
            int randomIndex = Random.Range(0, gameManager.cornerMovingPoints.Length);
            randomCorner = gameManager.cornerMovingPoints[randomIndex];

        } while (takenSpawnPositions.Contains(randomCorner));

        takenSpawnPositions.Add(randomCorner);

        return randomCorner;
    }

    public void ShootBullet(GameObject shooter)
    {
        // Spawning a bullet in front of shooter 
        // with an offset based on the shooter type

        Vector3 offset;

        if (shooter.CompareTag("Enemy"))
        {
            offset = GetEnemyBulletOffset(shooter);
        }
        else
        {
            offset = shooter.transform.forward * 2f + Vector3.up * 1.1f;
        }

        GameObject bullet = Instantiate(bulletPrefab, shooter.transform.position + offset, shooter.transform.rotation);
        bullet.GetComponent<ProjectileBehavior>().Initialize(shooter);
    }

    Vector3 GetEnemyBulletOffset(GameObject gameObj)
    {
        // Choosing offset for enemy's bullet
        // based on which direction enemy is heading

        Vector3 localForward = gameObj.transform.forward;
        Vector3 roundedLocalForward = new Vector3(Mathf.Round(localForward.x), 
                                                  Mathf.Round(localForward.y),
                                                  Mathf.Round(localForward.z));
        Vector3 offset = gameObj.transform.forward * 1.15f;

        if (roundedLocalForward == transform.right)
            offset += new Vector3(0, 0, -0.3f);
        else if (roundedLocalForward == -transform.right)
            offset += new Vector3(0, 0, 0.3f);
        else if (roundedLocalForward == transform.forward)
            offset += new Vector3(0.3f, 0, 0);
        else if (roundedLocalForward == -transform.forward)
            offset += new Vector3(-0.3f, 0, 0);

        return offset;
    }

    void OnAllEnemiesKilled(EnemyEventArgs e)
    {
        SpawnKey(e.EnemyPos);
    }
}
