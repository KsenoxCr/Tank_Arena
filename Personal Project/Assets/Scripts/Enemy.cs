using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using Random = UnityEngine.Random;
using Color = UnityEngine.Color;
using Unity.VisualScripting;

public class Enemy : MonoBehaviour
{
    [Header("Enemy's Settings")]
    [SerializeField] private readonly float movementSpeed = 5f;
    private bool canMove = true;
    private bool canShoot = true;

    private readonly float shootingCooldown = 0.5f;
    private float shootingTime = 0f;

    private Rigidbody rb;

    [SerializeField] private ParticleSystem bloodSplatter;
    [SerializeField] private ParticleSystem dirtSplatter;
    private ParticleSystem.EmissionModule dirtSplatterEm;

    private AudioSource audioSource;
    [SerializeField] private AudioClip shootingAudio;
    [SerializeField] private AudioClip deathAudio;

    private GameManager gameManager;
    private SpawnManager spawnManager;
    private Animator enemyAnimator;

    private Vector3[] movingPoints;
    private Dictionary<int, Vector3[]> movingPossibilities;
    private Vector3 currentMovingPoint;
    private Vector3 lastMovingPoint;
    private Vector3 nextMovingPoint;

    [SerializeField] private Vector3 rbVelocity; //Debug

    private Vector3 lastPosition;

    public delegate void AllEnemiesKilledEventHandler(EnemyEventArgs enemyPos);

    public static event AllEnemiesKilledEventHandler AllEnemiesKilled;

    protected virtual void OnAllEnemiesKilled(Vector3 enemyPos)
    {
        if (AllEnemiesKilled != null)
        {
            AllEnemiesKilled(new EnemyEventArgs() { EnemyPos = enemyPos });
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        enemyAnimator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();

        rb.constraints = RigidbodyConstraints.FreezePosition
                       | RigidbodyConstraints.FreezeRotationZ
                       | RigidbodyConstraints.FreezeRotationX;
    }

    void Start()
    {
        dirtSplatterEm = dirtSplatter.emission;
        dirtSplatterEm.enabled = true;

        // Saving movingPoints from SpawnManager

        movingPoints = gameManager.movingPointPositions;

        SetupMovingPossibilities();

        FindFirstPointAndStartMoving();
    }

    void FixedUpdate()
    {
        // Moving enemy towards next movingPoint

        if (canMove)
        {
            MoveToNextMovingPoint(currentMovingPoint, nextMovingPoint);
        }

        ChangeAnimation();

        Debug.DrawLine(transform.position, transform.position + transform.forward, Color.cyan);

        if (gameManager.isGamePlaying && canShoot && Time.time - gameManager.roundTime >= gameManager.startShootingCooldown
            && Time.time - shootingTime >= shootingCooldown)
        {
            audioSource.PlayOneShot(shootingAudio, 0.1f);
            //enemyAnimator.SetBool("isShooting", true);
            spawnManager.ShootBullet(gameObject);
            //StartCoroutine(StopShootingAnimation());
            shootingTime = Time.time;
        }
    }

    void ChangeAnimation()
    {
        if (transform.position == lastPosition)
        {
            enemyAnimator.SetBool("isMoving", false);
        }
        else
        {
            enemyAnimator.SetBool("isMoving", true);
        }
        lastPosition = transform.position;
    }

    void FindFirstPointAndStartMoving()
    {
        // Checking what the first movingPoint is, where enemy spawned

        currentMovingPoint = transform.position;
        nextMovingPoint = GetNextMovingPoint(movingPossibilities[Array.IndexOf(movingPoints, currentMovingPoint)]);

        MoveToNextMovingPoint(currentMovingPoint, nextMovingPoint);
    }

    void MoveToNextMovingPoint(Vector3 currentPoint, Vector3 nextPoint)
    {
        // Checking the direction of movement and locking rigidbodys position on opposite axis 
        // (For making sure that player can't push enemy out of it's trajectory

        Vector3 direction = (currentPoint - nextPoint).normalized;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            rb.constraints = RigidbodyConstraints.FreezePositionZ | ~RigidbodyConstraints.FreezePositionX;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezePositionX | ~RigidbodyConstraints.FreezePositionZ;
        }

        rb.MoveRotation(Quaternion.LookRotation(nextPoint - currentPoint, Vector3.up));
        rb.MovePosition(GetInterpolatedPosition(transform.position, nextPoint));

        // Check if enemy is on the next movingPoint (or close enough to it),
        // get a new next movingPoint and start moving to it

        if (Vector3.Distance(transform.position, nextPoint) < 0.1f //There should be a better way than use distance 0.1 but there's too much floating point fluctuation
            || rb.linearVelocity == Vector3.zero && Vector3.Distance(transform.position, nextPoint) < 0.3f)
        {
            lastMovingPoint = currentMovingPoint;
            currentMovingPoint = nextPoint;
            nextMovingPoint = GetNextMovingPoint(movingPossibilities[Array.IndexOf(movingPoints, currentMovingPoint)]);

            MoveToNextMovingPoint(currentMovingPoint, nextMovingPoint);
        }
    }

    Vector3 GetInterpolatedPosition(Vector3 currentPos, Vector3 newPos)
    {
        //float distanceToTarget = Vector3.Distance(currentPos, newPos);

        //float travelTime = distanceToTarget / speed;

        //float interpolationRatio = (float)elapsedFrames / movingFramesCount;

        //Vector3 interpolatedPosition = Vector3.Lerp(currentPos, newPos, interpolationRatio);
        //elapsedFrames = (elapsedFrames + 1) % (movingFramesCount + 1);

        var step = movementSpeed * Time.fixedDeltaTime;

        Vector3 interpolatedPos = Vector3.MoveTowards(currentPos, newPos, step);

        return interpolatedPos;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(currentMovingPoint, 0.5f);
        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(transform.position, transform.position + transform.forward); // Debugging GetEnemyBulletOffset
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(nextMovingPoint, 0.5f);

    }

    Vector3 GetNextMovingPoint(Vector3[] possibilities)
    {
        Vector3 newPoint = Vector3.zero;

        int attemptCount = 0;
        int maxAttempts = 3;

        do
        {
            int randomIndex = Random.Range(0, possibilities.Length);
            newPoint = possibilities[randomIndex];

            if (newPoint != lastMovingPoint && newPoint != Vector3.zero)
            {
                break;
            }

            attemptCount++;

        } while (newPoint == Vector3.zero || attemptCount < maxAttempts);

        if (newPoint == Vector3.zero)
        {
            return lastMovingPoint;
        }

        return newPoint;
    }

    void SetupMovingPossibilities()
    {
        // Saving movingPossibilities

        movingPossibilities = new Dictionary<int, Vector3[]>();

        movingPossibilities.Add(0, new Vector3[3] { movingPoints[1], movingPoints[8], movingPoints[7] });
        movingPossibilities.Add(1, new Vector3[3] { movingPoints[2], movingPoints[0], Vector3.zero });
        movingPossibilities.Add(2, new Vector3[3] { movingPoints[3], movingPoints[1], Vector3.zero });
        movingPossibilities.Add(3, new Vector3[3] { movingPoints[4], movingPoints[2], Vector3.zero });
        movingPossibilities.Add(4, new Vector3[3] { movingPoints[5], movingPoints[13], movingPoints[3] });
        movingPossibilities.Add(5, new Vector3[3] { movingPoints[6], movingPoints[4], Vector3.zero });
        movingPossibilities.Add(6, new Vector3[3] { movingPoints[7], movingPoints[16], movingPoints[5] });
        movingPossibilities.Add(7, new Vector3[3] { movingPoints[0], movingPoints[6], Vector3.zero });
        movingPossibilities.Add(8, new Vector3[3] { movingPoints[9], movingPoints[0], movingPoints[17] });
        movingPossibilities.Add(9, new Vector3[3] { movingPoints[10], movingPoints[8], movingPoints[18] });
        movingPossibilities.Add(10, new Vector3[3] { movingPoints[11], movingPoints[9], Vector3.zero });
        movingPossibilities.Add(11, new Vector3[3] { movingPoints[12], movingPoints[2], movingPoints[10] });
        movingPossibilities.Add(12, new Vector3[3] { movingPoints[13], movingPoints[11], Vector3.zero });
        movingPossibilities.Add(13, new Vector3[3] { movingPoints[14], movingPoints[4], movingPoints[12] });
        movingPossibilities.Add(14, new Vector3[3] { movingPoints[15], movingPoints[20], movingPoints[13] });
        movingPossibilities.Add(15, new Vector3[3] { movingPoints[16], movingPoints[14], Vector3.zero });
        movingPossibilities.Add(16, new Vector3[3] { movingPoints[17], movingPoints[6], movingPoints[15] });
        movingPossibilities.Add(17, new Vector3[3] { movingPoints[8], movingPoints[16], Vector3.zero });
        movingPossibilities.Add(18, new Vector3[3] { movingPoints[19], movingPoints[9], movingPoints[21] });
        movingPossibilities.Add(19, new Vector3[3] { movingPoints[20], movingPoints[18], Vector3.zero });
        movingPossibilities.Add(20, new Vector3[3] { movingPoints[19], movingPoints[14], movingPoints[21] });
        movingPossibilities.Add(21, new Vector3[3] { movingPoints[20], movingPoints[18], Vector3.zero });
    }

    public void Kill()
    {
        canMove = false;
        canShoot = false;
        dirtSplatterEm.enabled = false;
        gameObject.tag = "Untagged";

        if (!bloodSplatter.isPlaying)
        {
            bloodSplatter.Play();
        }

        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        Destroy(rb);

        audioSource.PlayOneShot(deathAudio, 0.8f);

        gameManager.enemyCount--;

        StartCoroutine(DestroyEnemyOnEnd());

        //if (--gameManager.enemyCount == 0)
        if (gameManager.enemyCount == 0)
        {
            OnAllEnemiesKilled(transform.position);
        }
    }

    IEnumerator DestroyEnemyOnEnd()
    {
        while (audioSource.isPlaying || bloodSplatter.isPlaying)
        {
            yield return new WaitForSeconds(0);
        }

        Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Chest") || 
            collision.collider.gameObject.CompareTag("Life Up") ||
            collision.collider.gameObject.CompareTag("Enemy"))
        {
            // Turning away from other enemy or chest or life up

            Vector3 nextPoint = currentMovingPoint;
            currentMovingPoint = nextMovingPoint;
            nextMovingPoint = nextPoint;
        }
        else if (collision.collider.gameObject.CompareTag("Player"))
        {
            // Stop enemy movement when colliding with player 

            canMove = false;
            dirtSplatterEm.enabled = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Player"))
        {
            // Allow enemy movement when stopping colliding with player 

            canMove = true;
            dirtSplatterEm.enabled = true;
        }
    }
}