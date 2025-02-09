using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int round = 0;
    public int lastRound = 25;
    public int enemyCount = 0;
    public float roundTime = 0;
    public float startShootingCooldown = 2f;
    public bool isGamePlaying = false;

    [SerializeField] private GameObject startScreen;
    [SerializeField] private GameObject gameUi;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private Button startButton;

    [SerializeField] private Button restartButton;

    public GameObject[] movingPoints = new GameObject[22];
    public Vector3[] movingPointPositions;
    public Vector3[] cornerMovingPoints;

    private SpawnManager spawnManager;

    void Awake()
    {
        // saving movingPoints positions to movingPointPositions

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
        round = 0;
        spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        startButton.onClick.AddListener(StartNewRound);
        restartButton.onClick.AddListener(RestartGame);
    }

    public void UpdateRound()
    {
        round++;
        roundText.text = "Round: " + round;
    }

    public void StartNewRound()
    {
        if (round == 0)
        {
            isGamePlaying = true;
            startScreen.SetActive(false);
            gameUi.SetActive(true);
        }

        if (round > 0)
        {
            Destroy(GameObject.FindGameObjectWithTag("Life Up"));
            spawnManager.takenSpawnPositions.Clear();
        }

        UpdateRound();

        spawnManager.SpawnEnemyWave(round);
        spawnManager.SpawnChest();

        // Finishing round 50, player wins

        if (round > lastRound)
        {
            Debug.Log("You win!");
        }
    }

    public void GameOver()
    {
        isGamePlaying = false;

        gameOverScreen.SetActive(true);
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    } 
}