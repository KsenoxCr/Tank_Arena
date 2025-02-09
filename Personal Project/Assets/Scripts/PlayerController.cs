using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    private readonly float frontRaysDistance = 1.15f;
    private readonly float backRaysDistance = 1.2f;
    private Ray frontRay;
    private Ray backRay;

    [SerializeField] private TextMeshProUGUI healthText;

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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        audioSource = GetComponent<AudioSource>();
        turret = transform.Find("Tank/TurretPivotPoint").gameObject;
        turretRb = turret.GetComponent<Rigidbody>();
        turretRb.automaticCenterOfMass = false;
        //turretRb.centerOfMass = Vector3.zero; //Doesn't change to 0,0,0 for some reason, had to change in inspector
        
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
        UpdateHealth(4);
    }

    void FixedUpdate()
    {
        // Saving Player's movement input

        bodyHorizontalInput = Input.GetAxis("Body_Horizontal");
        turretHorizontalInput = Input.GetAxis("Turret_Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        rb.isKinematic = verticalInput == 0 ? true : false;

        StopPlayerOnCollision();

        if (gameManager.isGamePlaying)
        {
            MoveTurret();
            MovePlayer();
        }
    }
    
    void Update()
    {
        if (gameManager.isGamePlaying)
        {
            EnableTireSmokeParticles();

            // Player Shooting

            if (Input.GetKey(KeyCode.Space) && Time.time - shootingTime >= shootingCooldown)
            {
                audioSource.PlayOneShot(shootingAudio, 0.6f);
                spawnManager.ShootBullet(turret);
                shootingTime = Time.time;
            }
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

    public void UpdateHealth(int hpDelta)
    {
        hp += hpDelta;

        if (hp > 4)
        {
            hp = 4;
        }
        else if (hp < 0)
        {
            hp = 0;
        }

        if (hp == 0 && gameManager.isGamePlaying)
        {
            // Game Over

            hp = 0;

            //gameManager.isGameOver = true; // Is this better here or in gameManager.GameOver? Is there a big enough delay before we reach GameOver() so that game events can still happen or not

            if (!deathExplosion.isPlaying)
            {
                deathExplosion.Play();
            }

            StopDisplayingPlayer();

            audioSource.PlayOneShot(deathAudio, 1.2f);

            StartCoroutine(DestroyPlayerOnEnd());

            gameManager.GameOver();
        }

        healthText.text = "Health: " + hp;
    }

    void StopPlayerOnCollision()
    {
        //Checking for forward hits

        frontRay = new Ray(transform.position, transform.forward);

        RaycastHit frontHit;
        if (Physics.Raycast(frontRay, out frontHit, frontRaysDistance))
        {
            Debug.DrawLine(transform.position, frontHit.point, Color.red);

            // Stop Player's forward movement
            if (verticalInput > 0 && frontHit.collider.CompareTag("Wall")
                                  && frontHit.collider.CompareTag("Enemy")
                                  && frontHit.collider.CompareTag("Chest"))
            {
                rb.linearVelocity = rb.angularVelocity = Vector3.zero;
                verticalInput = 0;
            }
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position + transform.forward * frontRaysDistance, Color.green);
        }


        //Checking for backward hits

        backRay = new Ray(transform.position, -transform.forward);

        RaycastHit backHit;
        if (Physics.Raycast(backRay, out backHit, backRaysDistance))
        {
            Debug.DrawLine(transform.position, backHit.point, Color.red);

            // Stop Player's backward movement

            if (verticalInput < 0 && frontHit.collider.CompareTag("Wall") 
                                  && frontHit.collider.CompareTag("Enemy")
                                  && frontHit.collider.CompareTag("Chest"))
            {
                rb.linearVelocity = rb.angularVelocity = Vector3.zero;
                verticalInput = 0;
            }
        }
        else
        {
            Debug.DrawLine(transform.position, transform.position - transform.forward * backRaysDistance, Color.green);
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

            UpdateHealth(-1);

            damageTime = Time.time;
        }
    }
}