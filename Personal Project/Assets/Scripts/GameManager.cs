using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [Header("Game Parameters")]
    [SerializeField] int round;
    private readonly int lastRound = 25;
    public int enemyCount = 0;
    public float roundTime = 0;
    public readonly float startShootingCooldown = 2f;
    public bool isGamePlaying;

    [Header("UI Elements")]
    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject gameUi;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private GameObject winScreen;

    [SerializeField] private TextMeshProUGUI roundCountText;
    [SerializeField] private TextMeshProUGUI newRoundText;

    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button backToTitleButton;


    public GameObject[] movingPoints = new GameObject[22];
    public Vector3[] movingPointPositions;
    public Vector3[] cornerMovingPoints;

    private AudioSource audioSource;
    private SpawnManager spawnManager;

    void Awake()
    {
        // saving movingPoints Vector3 positions to movingPointPositions

        movingPointPositions = new Vector3[movingPoints.Length];

        for (int i = 0; i < movingPoints.Length; i++)
        {
            movingPointPositions[i] = movingPoints[i].transform.position;
        }

        // Saving corner movingPoints

        cornerMovingPoints = new Vector3[4] { movingPointPositions[1], movingPointPositions[3],
            movingPointPositions[5] , movingPointPositions[7] };
    }

    void Start()
    {
        // Setting up the game

        audioSource = GetComponent<AudioSource>();
        spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();

        newRoundText.alpha = 0; 
        round = 0;

        startButton.onClick.AddListener(StartNewRound);
        restartButton.onClick.AddListener(RestartGame);
        backToTitleButton.onClick.AddListener(RestartGame);
    }

    void Update()
    {
        // Let player start and restart game by pressing Enter

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (startScreen.activeSelf)
            {
                StartNewRound();
            }
            else if (gameOverScreen.activeSelf || winScreen.activeSelf)
            {
                RestartGame();
            }
        }
    }

    public void UpdateRound()
    {
        // Updating round number and UI text

        round++;
        roundCountText.text = "Round: " + round;
    }

    public void StartNewRound()
    {
        // Starting a new round

        UpdateRound();

        if (round == 1)
        {
            // Starting the game

            audioSource.Play();
            isGamePlaying = true;
            startScreen.SetActive(false);
            gameUi.SetActive(true);
            newRoundText.gameObject.SetActive(true);
        }
        else if (round > lastRound)
        {
            // Player wins the game

            gameUi.SetActive(false);
            winScreen.SetActive(true);

            return;
        }

        if (round > 1)
        {
            // Destroying previous round's life up 
            // and clearing taken spawn positions

            Destroy(GameObject.FindGameObjectWithTag("Life Up"));
            spawnManager.takenSpawnPositions.Clear();
        }


        // Fade the new round text in and out

        StartCoroutine(NewRoundTextFade(1, round));

        // Spawning enemies and chest

        spawnManager.SpawnEnemyWave(round);
        spawnManager.SpawnChest();
    }

    public void GameOver()
    {
        // Stop game from playing and show game over screen

        isGamePlaying = false;
        audioSource.Stop();

        newRoundText.gameObject.SetActive(false);
        gameOverScreen.SetActive(true);
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator NewRoundTextFade(int step, int round)
    {
        // Fading new round text in or out based on step value

        bool isFadeIn = step > 0;

        if (isFadeIn)
        {
            while (newRoundText.alpha != 0)
            {
                yield return null;
            }

            newRoundText.text = "Round: " + round;
        }

        int startOfRange = isFadeIn ? 0 : 100;
        int endOfRange = isFadeIn ? 101 : -1;

        for (float a = startOfRange; a != endOfRange; a += step)
        {
            newRoundText.alpha = a / 100;
            yield return null;
        }

        if (isFadeIn)
        {
            // Starting the fade out after 3 seconds

            yield return new WaitForSeconds(3);

            StartCoroutine(NewRoundTextFade(-1, round));
        }
    }
}