using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class PlayerController : MonoBehaviour
{
    [Header("Player Settings")]
    [SerializeField] float speed = 7.5f;
    [SerializeField] float rotationSpeed = 150f;
    public int hp = 4;
    public bool hasKey;
    private float shootingCooldown = 0.5f;
    private float shootingTime = 0f;
    private float damageCooldown = 1.5f;
    private float damageTime = 0f;

    private Rigidbody rb;
    private BoxCollider bc;

    private GameManager gameManager;
    private SpawnManager spawnManager;

    private GameObject turret;
    private Rigidbody turretRb;

    [SerializeField] private ParticleSystem leftRearTireSmoke;
    [SerializeField] private ParticleSystem rightRearTireSmoke;
    [SerializeField] private ParticleSystem leftFrontTireSmoke;
    [SerializeField] private ParticleSystem rightFrontTireSmoke;

    private ParticleSystem.EmissionModule leftRearEm;
    private ParticleSystem.EmissionModule rightRearEm;
    private ParticleSystem.EmissionModule leftFrontEm;
    private ParticleSystem.EmissionModule rightFrontEm;

    [SerializeField] private ParticleSystem keySpark;
    [SerializeField] private ParticleSystem deathExplosion;

    private AudioSource audioSource;
    [SerializeField] private AudioClip shootingAudio;
    [SerializeField] private AudioClip deathAudio;

    private float bodyHorizontalInput;
    private float turretHorizontalInput;
    [SerializeField] private float verticalInput;
    private float reverse;
    private Vector3 eulerAngleVelocity;

    public float raysDistance = 0.8f;
    private Ray frontRay;
    private Ray backRay;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        audioSource = GetComponent<AudioSource>();
        turret = transform.Find("Tank/TurretPivotPoint").gameObject;
        turretRb = turret.GetComponent<Rigidbody>();

        eulerAngleVelocity = new Vector3(0, rotationSpeed, 0);
    }

    void Start()
    {
        leftRearEm = leftRearTireSmoke.emission;
        rightRearEm = rightRearTireSmoke.emission;
        leftFrontEm = leftFrontTireSmoke.emission;
        rightFrontEm = rightFrontTireSmoke.emission;

        leftFrontEm.enabled = false;
        rightFrontEm.enabled = false;
        leftRearEm.enabled = false;
        rightRearEm.enabled = false;

        hp = 4; // Not needed after development without reloading domain and scene
        hasKey = false;
        Debug.Log("Player's Healthpoints: " + hp);
    }

    void FixedUpdate()
    {
        // Saving Player's movement input

        bodyHorizontalInput = Input.GetAxis("Body_Horizontal");
        turretHorizontalInput = Input.GetAxis("Turret_Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        rb.isKinematic = verticalInput == 0 ? true : false;

        StopPlayerOnCollision();

        if (!gameManager.isGameOver)
        {
            MoveTurret();
            MovePlayer();
        }
    }
    
    void Update()
    {
        EnableTireSmokeParticles();

        if (hp <= 0 && !gameManager.isGameOver)
        {
            // Game Over

            gameManager.isGameOver = true;

            if (!deathExplosion.isPlaying)
            {
                deathExplosion.Play();
            }

            StopDisplayingPlayer();

            audioSource.PlayOneShot(deathAudio, 0.8f);

            StartCoroutine(DestroyPlayerOnEnd());
        }

        // Player Shooting

        if (Input.GetKey(KeyCode.Space) && !gameManager.isGameOver && Time.time - shootingTime >= shootingCooldown)
        {
            audioSource.PlayOneShot(shootingAudio, 0.3f);
            spawnManager.ShootBullet(turret);
            shootingTime = Time.time;
        }
    }

    void MovePlayer()
    {
        // Player's movement

        rb.MovePosition(transform.position + transform.forward * verticalInput * Time.fixedDeltaTime * speed);

        if (verticalInput != 0)
        {
            reverse = verticalInput > 0 ? 1 : -1;

            Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.fixedDeltaTime * bodyHorizontalInput * reverse);
            rb.MoveRotation(rb.rotation * deltaRotation);
            turretRb.MoveRotation(turretRb.rotation * deltaRotation);
        }
    }

    void MoveTurret()
    {
        // Turret Movement

        Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.fixedDeltaTime * turretHorizontalInput);
        turretRb.MoveRotation(turretRb.rotation * deltaRotation);
    }

    void StopPlayerOnCollision()
    {
        //Checking for forward hits

        frontRay = new Ray(transform.position, transform.forward);

        RaycastHit frontHit;
        if (Physics.Raycast(frontRay, out frontHit, raysDistance))
        {
            //Debug.DrawLine(transform.position, frontHit.point, Color.red);

            // Stop Player's forward movement
            if (verticalInput >= 0 && !frontHit.collider.CompareTag("Projectile"))
            {
                rb.linearVelocity = rb.angularVelocity = Vector3.zero;
                verticalInput = 0;
            }
        }
        else
        {
            //Debug.DrawLine(transform.position, transform.position + transform.forward * raysDistance, Color.green);
        }


        //Checking for backward hits

        backRay = new Ray(transform.position, -transform.forward);

        RaycastHit backHit;
        if (Physics.Raycast(backRay, out backHit, raysDistance))
        {
            //Debug.DrawLine(transform.position, backHit.point, Color.red);

            // Stop Player's backward movement

            if (verticalInput <= 0)
            {
                rb.linearVelocity = rb.angularVelocity = Vector3.zero;
                verticalInput = 0;
            }
        }
        else
        {
            //Debug.DrawLine(transform.position, transform.position - transform.forward * raysDistance, Color.green);
        }
    }

    void StopDisplayingPlayer()
    {
        MeshRenderer[] meshRenderers = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer renderer in meshRenderers)
        {
            renderer.enabled = false;
        }
    }

    void EnableTireSmokeParticles()
    {
        leftRearEm.enabled = verticalInput > 0 ? true : false;
        rightRearEm.enabled = verticalInput > 0 ? true : false;

        leftFrontEm.enabled = verticalInput < 0 ? true : false;
        rightFrontEm.enabled = verticalInput < 0 ? true : false;
    }

    IEnumerator DestroyPlayerOnEnd()
    {
        while (audioSource.isPlaying || deathExplosion.isPlaying)
        {
            yield return new WaitForSeconds(0f);
        }

        Destroy(gameObject);
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Enemy") && Time.time - damageTime >= damageCooldown)
        {
            // Player takes damage

            hp--;

            Debug.Log("Player's Healthpoints: " + hp);

            damageTime = Time.time;
        }
    }
}