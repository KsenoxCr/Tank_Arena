using System.Collections;
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
    [SerializeField] private GameObject winScreen;

    [SerializeField] private TextMeshProUGUI roundCountText;
    [SerializeField] private TextMeshProUGUI newRoundText;

    [SerializeField] private Button startButton;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button backToTitleButton;

    [SerializeField] private AudioSource audioSource;

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
        newRoundText.alpha = 0; 
        round = 0;
        spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
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
        round++;
        roundCountText.text = "Round: " + round;
        newRoundText.text = roundCountText.text;
    }

    public void StartNewRound()
    {
        UpdateRound();

        if (round == 1)
        {
            audioSource.Play();
            isGamePlaying = true;
            startScreen.SetActive(false);
            gameUi.SetActive(true);
            newRoundText.gameObject.SetActive(true);
        }
        else if (round > lastRound)
        {
            gameUi.SetActive(false);
            winScreen.SetActive(true);

            return;
        }

        if (round > 1)
        {
            Destroy(GameObject.FindGameObjectWithTag("Life Up"));
            spawnManager.takenSpawnPositions.Clear();
        }


        // Add if statement for is new round already showing

        StartCoroutine(NewRoundTextFade(1));
        Invoke("NewRoundFadeOut", 3);


        spawnManager.SpawnEnemyWave(round);
        spawnManager.SpawnChest();
    }

    public void GameOver()
    {
        isGamePlaying = false;
        audioSource.Stop();

        newRoundText.gameObject.SetActive(false);
        gameOverScreen.SetActive(true);
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void NewRoundFadeOut()
    {
        StartCoroutine(NewRoundTextFade(-1));
    }

    IEnumerator NewRoundTextFade(float step)
    {
        float startOfRange = step > 0 ? 0 : 100;
        float endOfRange = step > 0 ? 101 : -1f;

        for (float a = startOfRange; a != endOfRange; a += step)
        {
            newRoundText.alpha = a / 100;
            yield return null;
        }
    }
}