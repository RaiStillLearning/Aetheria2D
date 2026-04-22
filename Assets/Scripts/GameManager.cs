using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    public PlayerController player;
    public PlatformSpawner spawner;
    public Transform cameraTransform;

    [Header("UI")]
    public Text scoreText;
    public Text livesText;
    public GameObject gameOverPanel;
    public Text gameOverScoreText;
    public Button restartButton;

    [Header("Settings")]
    public int maxLives = 3;

    int _lives;
    int _score;
    float _highestY;
    bool _gameOver;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _lives = maxLives;
        _score = 0;
        _gameOver = false;

        if (player == null)
        {
            var go = GameObject.Find("--- PLAYER ---");
            if (go != null) player = go.GetComponent<PlayerController>();
        }

        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;

        _highestY = player ? player.transform.position.y : 0f;

        if (gameOverPanel) gameOverPanel.SetActive(false);
        if (restartButton) restartButton.onClick.AddListener(RestartGame);
        UpdateUI();
    }

    void Update()
    {
        if (_gameOver || player == null) return;

        float py = player.transform.position.y;
        if (py > _highestY)
        {
            _score += Mathf.FloorToInt((py - _highestY) * 10f);
            _highestY = py;
        }

        if (cameraTransform != null)
        {
            float camBottom = cameraTransform.position.y - 7f;
            if (py < camBottom && !player.isDead)
                player.Die();
        }

        UpdateUI();
    }

    public void PlayerDied()
    {
        _lives--;
        UpdateUI();
        if (_lives <= 0)
            StartCoroutine(GameOverRoutine());
        else
            StartCoroutine(RespawnRoutine());
    }

    IEnumerator RespawnRoutine()
    {
        yield return new WaitForSeconds(1.2f);
        if (player == null) yield break;
        float respawnY = cameraTransform ? cameraTransform.position.y : 0f;
        player.ResetPlayer(new Vector3(0f, respawnY + 1f, 0f));
    }

    IEnumerator GameOverRoutine()
    {
        yield return new WaitForSeconds(1.2f);
        _gameOver = true;
        if (gameOverPanel) gameOverPanel.SetActive(true);
        if (gameOverScoreText) gameOverScoreText.text = "Score: " + _score;
    }

    public void RestartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    public void UpdateLives(int newLives) { _lives = newLives; UpdateUI(); }
    public void UpdateScore(int newScore) { if (newScore > _score) { _score = newScore; UpdateUI(); } }
    public void RespawnPlayer(PlayerController p) { StartCoroutine(RespawnRoutine()); }

    void UpdateUI()
    {
        if (scoreText) scoreText.text = "Score: " + _score;
        if (livesText)
        {
            string h = "";
            for (int i = 0; i < maxLives; i++)
                h += (i < _lives) ? "\u2764 " : "\U0001F90E ";
            livesText.text = h;
        }
    }
}
