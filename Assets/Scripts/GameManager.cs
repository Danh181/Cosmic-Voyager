using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Player player;
    public int lives = 3;
    public float respawnTime = 3.0f;
    public float respawnImmortalTime = 3.0f;
    public int score = 0;
    public float ElapsedTime => elapsedTime;

    [Header("UI")]
    public Text scoreText;
    public Text livesText;
    public GameObject gameOverUI;
    public GameObject gameStartUI;
    public Text timeGame;
    public GameObject GameManual;
    public Text highScoreText;
    public GameObject memberTeam;

    public AudioSource themeMusic;
    public AudioSource destroyAsteroidMusic;
    public AudioClip asteroidHitClip;

    public GameObject leaderboardPanel;
    public Text[] leaderboardTexts;

    public ParticleSystem explosion;

    private bool isPauseGame = false;
    private int lastCheckpoint = 0;
    private float elapsedTime = 0f;
    private bool isGameStarted = false;

    private void Start()
    {
        Time.timeScale = 0f; // dừng game khi chưa bấm enter
        gameStartUI.SetActive(true);
        gameOverUI.SetActive(false);
        GameManual.SetActive(true);
        memberTeam.SetActive(true);

        if (leaderboardPanel != null)
            leaderboardPanel.SetActive(false);

        if (scoreText != null) scoreText.gameObject.SetActive(false);
        if (livesText != null) livesText.gameObject.SetActive(false);
        if (timeGame != null) timeGame.gameObject.SetActive(false);
        if (highScoreText != null) highScoreText.gameObject.SetActive(false);

        if (themeMusic != null && !themeMusic.isPlaying)
        {
            themeMusic.Play();
        }

        // load highscore và hiển thị
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;

        UpdateUI();
    }

    private void Update()
    {

        if (!isGameStarted)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                StartGame();
            }
            return;
        }

        if (lives <= 0 && Input.GetKeyDown(KeyCode.Return))
        {
            NewGame();
        }

        // bật/tắt nhạc
        if (Input.GetKeyDown(KeyCode.T)) // phím T để bật/tắt nhạc
        {
            if (themeMusic != null)
            {
                if (themeMusic.isPlaying)
                    themeMusic.Pause();  // hoặc themeMusic.Stop();
                else
                    themeMusic.Play();
            }
        }

        if (Input.GetKeyDown(KeyCode.P)) // phím P để pause/unpause game
        {
            TriggerPause();
        }



        // Time của game
        if (lives > 0)
        {
            elapsedTime += Time.deltaTime;

            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            if (timeGame != null)
                timeGame.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        //key code để phá hủy toàn bộ TT
        if (isGameStarted && Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.E))
        {
            NukeAllAsteroids();
        }
    }

    private void TriggerPause()
    {
        isPauseGame = !isPauseGame;

        if (isPauseGame)
        {
            Time.timeScale = 0f; // dừng tất cả vật lý, Update, FixedUpdate
            if (themeMusic != null)
                themeMusic.Pause();
        }
        else
        {
            Time.timeScale = 1f; // chạy lại bình thường
            if (themeMusic != null && !themeMusic.isPlaying)
                themeMusic.Play();
        }
    }


    public void AsteroidDestroyed(Asteroid asteroid)
    {
        if (destroyAsteroidMusic != null && asteroidHitClip != null)
        {
            destroyAsteroidMusic.PlayOneShot(asteroidHitClip, 0.3f);
        }

        explosion.transform.position = asteroid.transform.position;
        explosion.Play();

        // tăng điểm khi bắn bể TT
        if (asteroid.size < 0.75f)
        {
            score += 10;
        }
        else if (asteroid.size < 1.0f)
        {
            score += 20;
        }
        else
        {
            score += 30;
        }

        int checkpoint = score / 1000;
        if (checkpoint > lastCheckpoint && lives <3)
        {
            lives++;
        }
        lastCheckpoint = checkpoint;

        UpdateUI();
    }

    public void PlayerDied()
    {
        explosion.transform.position = player.transform.position;
        explosion.Play();

        lives--;
        UpdateUI();

        if (lives == 0)
        {
            GameOver();
        }
        else
        {
            Invoke(nameof(Respawn), respawnTime);
        }
    }

    private void Respawn()
    {
        player.transform.position = Vector3.zero;
        player.gameObject.layer = LayerMask.NameToLayer("Ignore Collisions");
        player.gameObject.SetActive(true);

        Invoke(nameof(TurnOnCollisions), respawnImmortalTime);
    }

    private void TurnOnCollisions()
    {
        player.gameObject.layer = LayerMask.NameToLayer("Player");
    }

    private void GameOver()
    {
        gameOverUI.SetActive(true);
        int currentHighScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > currentHighScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
        }


        SaveScore(score);

        // hiển thị highscore mới
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
            highScoreText.text = "High Score: " + highScore;
    }

    private void StartGame()
    {
        isGameStarted = true;
        gameStartUI.SetActive(false);
        GameManual.SetActive(false);
        memberTeam.SetActive(false);
        Time.timeScale = 1f; // chạy game bình thường
        elapsedTime = 0f; // reset time

        if (scoreText != null) scoreText.gameObject.SetActive(true);
        if (livesText != null) livesText.gameObject.SetActive(true);
        if (timeGame != null) timeGame.gameObject.SetActive(true);
        if (highScoreText != null) highScoreText.gameObject.SetActive(true);
    }


    private void NewGame()
    {
        // xoá tất cả asteroid còn lại
        //Asteroid[] asteroids = FindObjectsOfType<Asteroid>();
        Asteroid[] asteroids = FindObjectsByType<Asteroid>(FindObjectsSortMode.None);
        for (int i = 0; i < asteroids.Length; i++)
        {
            Destroy(asteroids[i].gameObject);
        }

        // ẩn UI game over
        gameOverUI.SetActive(false);

        // reset điểm và mạng
        lives = 3;
        score = 0;
        lastCheckpoint = 0;
        elapsedTime = 0f; // set lại time
        UpdateUI();

        // respawn player
        Respawn();
    }

    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
        if (livesText != null)
            livesText.text = "Lives: " + lives;
    }

    private void SaveScore(int newScore)
    {
        int[] scores = new int[5];
        for (int i = 0; i < 5; i++)
        {
            scores[i] = PlayerPrefs.GetInt("HighScore" + i,0);
        }

        // thêm điểm mới vào mảng
        System.Collections.Generic.List<int> scoreList = new System.Collections.Generic.List<int>(scores);
        scoreList.Add(newScore);

        // sắp xếp điểm giảm dần
        scoreList.Sort((a, b) => b.CompareTo(a));

        for (int i = 0; i < 5; i++)
        {
            PlayerPrefs.SetInt("HighScore" + i, scoreList[i]);
        }

        PlayerPrefs.Save();
    }


    // hiện top 5 điểm
    public void ShowLeaderboard()
    {
        leaderboardPanel.SetActive(true);

        // Load và hiển thị 5 top score
        for (int i = 0; i < 5; i++)
        {
            int score = PlayerPrefs.GetInt("HighScore" + i, 0);
            leaderboardTexts[i].text = $"{i + 1}. {score}";
        }
    }

    // ẩn top 5 điểm
    public void HideLeaderboard()
    {
        leaderboardPanel.SetActive(false);
    }

    // phá hủy toàn bộ TT
    private void NukeAllAsteroids()
    {
        Asteroid[] asteroids = FindObjectsByType<Asteroid>(FindObjectsSortMode.None);

        foreach (Asteroid a in asteroids)
        {
            if (a == null) continue;

            AsteroidDestroyed(a); // cộng điểm
            Destroy(a.gameObject); // xóa thiên thạch
        }
    }

}
