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

public class EnemyBehavior : MonoBehaviour
{
    [Header("Enemy's Settings")]
    [SerializeField] private readonly float movementSpeed = 5f;
    [SerializeField] private float velocity;
    private bool canMove = true;
    private bool canShoot = true;
    private readonly float shootingCooldown = 0.5f;
    private float shootingTime = 0f;

    [Header("Enemy's Particles")]
    [SerializeField] private ParticleSystem bloodSplatter;
    [SerializeField] private ParticleSystem dirtSplatter;
    private ParticleSystem.EmissionModule dirtSplatterEm;

    [Header("Enemy's Audio")]
    [SerializeField] private AudioClip shootingAudio;
    [SerializeField] private AudioClip deathAudio;

    private AudioSource audioSource;
    private Rigidbody rb;
    private GameManager gameManager;
    private SpawnManager spawnManager;
    private Animator enemyAnimator;

    private Dictionary<int, Vector3[]> movingPossibilities;
    private Vector3 currentMovingPoint;
    private Vector3 lastMovingPoint;
    private Vector3 nextMovingPoint;

    private Vector3 prevDirection;
    private Vector3 prevPos;
    private Vector3 lastPosition;

    public delegate void AllEnemiesKilledEventHandler(EnemyEventArgs enemyPos);

    public static event AllEnemiesKilledEventHandler AllEnemiesKilled;

    protected virtual void OnAllEnemiesKilled(Vector3 enemyPos)
    {
        //if (AllEnemiesKilled != null)
        //{
        //    AllEnemiesKilled(new EnemyEventArgs() { EnemyPos = enemyPos });
        //}

        AllEnemiesKilled?.Invoke(new EnemyEventArgs() { EnemyPos = enemyPos });
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
        // Enabling dirt splatter particles and
        // Setting up moving points and starting enemy movement

        dirtSplatterEm = dirtSplatter.emission;
        dirtSplatterEm.enabled = true;

        SetupMovingPossibilities(ref gameManager.movingPointPositions);

        //FindFirstPointAndStartMoving();
        MoveToNextMovingPoint(transform.position, transform.position);
    }

    void FixedUpdate()
    {
        // Moving enemy towards next movingPoint

        if (canMove)
        {
            MoveToNextMovingPoint(currentMovingPoint, nextMovingPoint);
        }

        prevPos = transform.position;

        Debug.DrawLine(transform.position, transform.position + transform.forward, Color.cyan);

        // Enemy shooting bullets on interval based on shooting cooldown

        if (gameManager.isGamePlaying && canShoot && Time.time - gameManager.roundTime >= gameManager.startShootingCooldown
            && Time.time - shootingTime >= shootingCooldown)
        {
            audioSource.PlayOneShot(shootingAudio, 0.1f);
            spawnManager.ShootBullet(gameObject);
            shootingTime = Time.time;
        }
    }

    void FindFirstPointAndStartMoving()
    {
        // Checking which moving point enemy spawned on
        // and setting up next moving point to start moving

        currentMovingPoint = transform.position;
        nextMovingPoint = GetNextMovingPoint(movingPossibilities[Array.IndexOf(gameManager.movingPointPositions, currentMovingPoint)]);

        MoveToNextMovingPoint(currentMovingPoint, nextMovingPoint);
    }

    void MoveToNextMovingPoint(Vector3 currentPoint, Vector3 nextPoint)
    {
        // Moving enemy towards next movingPoint


        // Calculating velocity to check if enemy is stuck (and for changing animation)

        velocity = (transform.position - prevPos).sqrMagnitude;

        // Getting a new point to move to when
        // enemy reaches the point it was previously was moving to

        if (Vector3.Distance(transform.position, nextPoint) < 0.1f //There should be a better way than use distance 0.1 but there's too much floating point fluctuation (Try (vector3 - vector3).sqrMagnitude < 0.1f)
            || (velocity == 0 && Vector3.Distance(transform.position, nextPoint) < 0.5f))
        {
            lastMovingPoint = currentMovingPoint;
            currentMovingPoint = nextPoint;
            nextMovingPoint = GetNextMovingPoint(movingPossibilities[Array.IndexOf(gameManager.movingPointPositions, currentMovingPoint)]);
        }
        // Checking the direction of enemy's movement and locking its rigidbody's position on opposite axis 
        // to make sure that player can't push the enemy out of its trajectory

        Vector3 direction = (currentPoint - nextPoint).normalized;

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.z))
        {
            rb.constraints = RigidbodyConstraints.FreezePositionZ | ~RigidbodyConstraints.FreezePositionX;
        }
        else
        {
            rb.constraints = RigidbodyConstraints.FreezePositionX | ~RigidbodyConstraints.FreezePositionZ;
        }

        // Moving enemy towards next movingPoint and rotating it to face the point

        if (direction != prevDirection)
            rb.MoveRotation(Quaternion.LookRotation(nextPoint - currentPoint, Vector3.up));
        rb.MovePosition(GetInterpolatedPosition(transform.position, nextPoint));

        prevDirection = direction;
    }

    Vector3 GetInterpolatedPosition(Vector3 currentPos, Vector3 newPos)
    {
        // Interpolating enemy's position to move it smoothly 

        var step = movementSpeed * Time.fixedDeltaTime;

        Vector3 interpolatedPos = Vector3.MoveTowards(currentPos, newPos, step);

        return interpolatedPos;
    }


    Vector3 GetNextMovingPoint(Vector3[] possibilities)
    {
        // Getting a random point to move to from the possibilities
        // Preferring new point over the previous moving point
        // = Moving forward more often than moving back

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

    void SetupMovingPossibilities(ref Vector3[] movingPoints)
    {
        // Saving movingPossibilities

        movingPossibilities = new Dictionary<int, Vector3[]>
        {
            { 0, new Vector3[3] { movingPoints[1], movingPoints[8], movingPoints[7] } },
            { 1, new Vector3[3] { movingPoints[2], movingPoints[0], Vector3.zero } },
            { 2, new Vector3[3] { movingPoints[3], movingPoints[1], Vector3.zero } },
            { 3, new Vector3[3] { movingPoints[4], movingPoints[2], Vector3.zero } },
            { 4, new Vector3[3] { movingPoints[5], movingPoints[13], movingPoints[3] } },
            { 5, new Vector3[3] { movingPoints[6], movingPoints[4], Vector3.zero } },
            { 6, new Vector3[3] { movingPoints[7], movingPoints[16], movingPoints[5] } },
            { 7, new Vector3[3] { movingPoints[0], movingPoints[6], Vector3.zero } },
            { 8, new Vector3[3] { movingPoints[9], movingPoints[0], movingPoints[17] } },
            { 9, new Vector3[3] { movingPoints[10], movingPoints[8], movingPoints[18] } },
            { 10, new Vector3[3] { movingPoints[11], movingPoints[9], Vector3.zero } },
            { 11, new Vector3[3] { movingPoints[12], movingPoints[2], movingPoints[10] } },
            { 12, new Vector3[3] { movingPoints[13], movingPoints[11], Vector3.zero } },
            { 13, new Vector3[3] { movingPoints[14], movingPoints[4], movingPoints[12] } },
            { 14, new Vector3[3] { movingPoints[15], movingPoints[20], movingPoints[13] } },
            { 15, new Vector3[3] { movingPoints[16], movingPoints[14], Vector3.zero } },
            { 16, new Vector3[3] { movingPoints[17], movingPoints[6], movingPoints[15] } },
            { 17, new Vector3[3] { movingPoints[8], movingPoints[16], Vector3.zero } },
            { 18, new Vector3[3] { movingPoints[19], movingPoints[9], movingPoints[21] } },
            { 19, new Vector3[3] { movingPoints[20], movingPoints[18], Vector3.zero } },
            { 20, new Vector3[3] { movingPoints[19], movingPoints[14], movingPoints[21] } },
            { 21, new Vector3[3] { movingPoints[20], movingPoints[18], Vector3.zero } }
        };
    }

    public void Kill()
    {
        // Killing enemy: playing blood splatter particles
        // and death audio before destroying it

        canMove = false;
        canShoot = false;
        dirtSplatterEm.enabled = false;
        gameObject.tag = "Untagged";

        if (!bloodSplatter.isPlaying)
        {
            bloodSplatter.Play();
        }

        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        rb.detectCollisions = false;

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
        // Destroying enemy after blood splatter
        // and death audio are done playing

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
            // Turning away from another enemy or chest or life up

            (currentMovingPoint, nextMovingPoint) = (nextMovingPoint, currentMovingPoint);
        }
        else if (collision.collider.gameObject.CompareTag("Player"))
        {
            // Stop enemy movement when colliding with player

            canMove = false;
            enemyAnimator.SetBool("isMoving", false);
            dirtSplatterEm.enabled = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Player"))
        {
            // Allow enemy movement when stopping colliding with player 

            canMove = true;
            enemyAnimator.SetBool("isMoving", true);
            dirtSplatterEm.enabled = true;
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawSphere(currentMovingPoint, 0.5f);
    //    //Gizmos.color = Color.red;
    //    //Gizmos.DrawLine(transform.position, transform.position + transform.forward); // Debugging GetEnemyBulletOffset
    //    Gizmos.color = Color.blue;
    //    Gizmos.DrawSphere(nextMovingPoint, 0.5f);
    //}
}