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

    public GameObject[] movingPoints = new GameObject[22];
    public Vector3[] movingPointPositions;
    private Vector3[] cornerMovingPoints;
    public List<Vector3> takenSpawnPositions = new();

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Start()
    {
        GameObject gameManagerGameObject = GameObject.Find("GameManager");
        //gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        //gameManager = gameManagerObject.GetComponent<GameManager>();

        // subscribing to the projectileBehaviour AllEnemiesKilled event

        ProjectileBehavior.AllEnemiesKilled -= OnAllEnemiesKilled;
        ProjectileBehavior.AllEnemiesKilled += OnAllEnemiesKilled;

        // Adding movingPoints positions to dictionary

        movingPointPositions = new Vector3[movingPoints.Length];

        for (int i = 0; i < movingPoints.Length; i++)
        {
            movingPointPositions[i] = movingPoints[i].transform.position;
            //debugMSG += movingPointPositions[i].ToString() + "\n";
        }

        // Saving corner movingPoints

        cornerMovingPoints = new Vector3[4] { movingPointPositions[1], movingPointPositions[3],
                                              movingPointPositions[5] , movingPointPositions[7] };

        gameManager.StartNewRound();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawCube(cornerMovingPoints[0], new Vector3(1, 1, 1));
    //    Gizmos.DrawCube(cornerMovingPoints[1], new Vector3(1, 1, 1));
    //    Gizmos.DrawCube(cornerMovingPoints[2], new Vector3(1, 1, 1));
    //    Gizmos.DrawCube(cornerMovingPoints[3], new Vector3(1, 1, 1));
    //}

    public void SpawnEnemyWave(int round)
    {
        gameManager.roundTime = Time.time;

        if (round >= 1 && round < 3)
        {
            gameManager.enemyCount = 1;
        }
        else if (round >= 3 && round < 10)
        {
            gameManager.enemyCount = 2;
        }
        else if (round >= 10 && round < 20)
        {
            gameManager.enemyCount = 3;
        }
        else if (round >= 20 && round < 26)
        {
            gameManager.enemyCount = 4;
        }

        // Spawn Life up on round 1, 3, 10, 15 and 20

        if (round == 1 || round == 3 || round == 10 || round == 15 || round == 20)
        {
            SpawnLifeUp();
        }

        for (int i = 0; i < gameManager.enemyCount; i++)
        {
            Instantiate(enemyPrefab, RandomCorner(), enemyPrefab.transform.rotation);
        }
    }

    public void SpawnChest()
    {
        Vector3 spawningPosition = RandomMovingPoint(2, prevChestPosition);
        Instantiate(chestPrefab, spawningPosition, chestPrefab.transform.rotation);
        prevChestPosition = spawningPosition;
    }

    public void SpawnLifeUp()
    {
        Vector3 spawningPosition = RandomMovingPoint(1, prevLifeUpPosition);
        Instantiate(lifeUpPrefab, spawningPosition, lifeUpPrefab.transform.rotation);
        prevLifeUpPosition = spawningPosition;
    }
    void SpawnKey(Vector3 position)
    {
        Instantiate(keyPrefab, position, keyPrefab.transform.rotation);
    }

    Vector3 RandomMovingPoint(int layer, Vector3 prevPoint)
    {
        // Choosing a random moving point

        Vector3 randomMovingPoint;
        int minInclusive = layer == 2 ? 8 : 0;

        do
        {
            int randomIndex = Random.Range(minInclusive, movingPointPositions.Length);
            randomMovingPoint = movingPointPositions[randomIndex];

        } while (takenSpawnPositions.Contains(randomMovingPoint) || randomMovingPoint == prevPoint);

        takenSpawnPositions.Add(randomMovingPoint);

        return new Vector3(randomMovingPoint.x, 0.85f, randomMovingPoint.z);
    }

    Vector3 RandomCorner()
    {
        // Picking a random corner out of moving points

        Vector3 randomCorner;

        do
        {
            int randomIndex = Random.Range(0, cornerMovingPoints.Length);
            randomCorner = cornerMovingPoints[randomIndex];

        } while (takenSpawnPositions.Contains(randomCorner));

        takenSpawnPositions.Add(randomCorner);

        return randomCorner;
    }

    public void ShootBullet(GameObject shooter)
    {
        // Spawning a bullet infront of shooter

        Vector3 offset = shooter.transform.forward * 1.1f;

        if (shooter.CompareTag("Enemy"))
        {
            offset += GetEnemyBulletOffset(shooter);
        }

        GameObject bullet = Instantiate(bulletPrefab, shooter.transform.position + offset, shooter.transform.rotation);
        bullet.GetComponent<ProjectileBehavior>().Initialize(shooter);
    }

    Vector3 GetEnemyBulletOffset(GameObject gameObj)
    {
        // Getting enemy's bullet spawn positions 
        // offset based on which direction enemy is heading

        Vector3 localForward = gameObj.transform.forward;
        Vector3 roundedLocalForward = new Vector3(Mathf.Round(localForward.x), 
                                                  Mathf.Round(localForward.y),
                                                  Mathf.Round(localForward.z));

        if (roundedLocalForward == transform.right)
            return new Vector3(0, 0, -0.3f);
        else if (roundedLocalForward == -transform.right)
            return new Vector3(0, 0, 0.3f);
        else if (roundedLocalForward == transform.forward)
            return new Vector3(0.3f, 0, 0);
        else if (roundedLocalForward == -transform.forward)
            return new Vector3(-0.3f, 0, 0);
        else 
            return Vector3.zero;
    }

    void OnAllEnemiesKilled(EnemyEventArgs e)
    {
        SpawnKey(e.EnemyPos);
    }
}
