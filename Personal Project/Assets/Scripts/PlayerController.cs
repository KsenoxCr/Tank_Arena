using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [Header("Player Settings")]
    public float speed = 7.5f;
    public float rotationSpeed = 150f;
    public static int hp = 4;
    public bool hasKey;
    private float shootingCooldown = 0.5f;
    private float shootingTime = 0f;
    private float damageCooldown = 1.5f;
    private float damageTime = 0f;

    private Rigidbody rb;
    private BoxCollider bc;

    public GameObject bulletPrefab;
    private SpawnManager spawnManagerScript;

    private float horizontalInput;
    private float verticalInput;
    private float reverse;
    public Vector3 eulerAngleVelocity;

    public float raysDistance = 0.8f;
    private Ray frontRay;
    private Ray backRay;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        bc = GetComponent<BoxCollider>();
        spawnManagerScript = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();

        eulerAngleVelocity = new Vector3(0, rotationSpeed, 0);
    }

    void Start()
    {
        hp = 4; // Not needed after development without reloading domain and scene
        hasKey = false;
        Debug.Log("Player's Healthpoints: " + hp);
    }

    void FixedUpdate()
    {
        // Saving Player's movement input

        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        rb.isKinematic = verticalInput == 0 ? true : false;


        StopPlayerOnCollision();

        MovePlayer();
    }
    
    void Update()
    {
        if (hp <= 0)
        {
            // Game Over

            Debug.Log("Game Over");
            Destroy(gameObject);
        }

        // Player Shooting

        if (Input.GetKey(KeyCode.Space) && Time.time - shootingTime >= shootingCooldown)
        {
            spawnManagerScript.ShootBullet(gameObject);
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

            Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.fixedDeltaTime * horizontalInput * reverse);
            rb.MoveRotation(rb.rotation * deltaRotation);
        }
    }

    void StopPlayerOnCollision()
    {
        //Checking for forward hits

        frontRay = new Ray(transform.position, transform.forward);

        RaycastHit frontHit;
        if (Physics.Raycast(frontRay, out frontHit, raysDistance))
        {
            Debug.DrawLine(transform.position, frontHit.point, Color.red);

            // Stop Player's forward movement
            if (verticalInput >= 0 && !frontHit.collider.CompareTag("Projectile"))
            {
                rb.linearVelocity = rb.angularVelocity = Vector3.zero;
                verticalInput = 0;
            }
        }
        else
        {
            // Visualize the raycast in the Scene view
            Debug.DrawLine(transform.position, transform.position + transform.forward * raysDistance, Color.green);
        }


        //Checking for backward hits

        backRay = new Ray(transform.position, -transform.forward);

        RaycastHit backHit;
        if (Physics.Raycast(backRay, out backHit, raysDistance))
        {
            Debug.DrawLine(transform.position, backHit.point, Color.red);

            // Stop Player's backward movement

            if (verticalInput <= 0)
            {
                rb.linearVelocity = rb.angularVelocity = Vector3.zero;
                verticalInput = 0;
            }
        }
        else
        {
            // Visualize the raycast in the Scene view
            Debug.DrawLine(transform.position, transform.position - transform.forward * raysDistance, Color.green);
        }
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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.CompareTag("Key"))
        {
            Destroy(collision.collider.transform.parent.gameObject);
            hasKey = true;
        }
        else if (collision.collider.gameObject.CompareTag("Life Up"))
        {
            // Player hp increased by 2 

            hp = hp <= 2 ? hp + 2 : 4;

            Destroy(collision.collider.gameObject);

            Debug.Log("Player's Healthpoints: " + hp);
        }
    }
}