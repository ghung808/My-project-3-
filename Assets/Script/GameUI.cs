using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameUI : MonoBehaviour
{
    public static GameUI instance;

    public int castleHP = 50;
    public int maxCastleHP = 50;

    public int gold = 100;

    public int currentWave = 1;
    public int maxWave = 8;

    [Header("Battle Statistics")]
    public int enemiesKilled = 0;
    public int totalGoldEarned = 0;
    public float playTime = 0;

    public TextMeshProUGUI castleText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI waveText;

    [Header("Panel Win/Lose")]
    public GameObject winPanel;
    public GameObject losePanel;

    // =========================
    // WIN STATISTICS
    // =========================

    [Header("Win Statistics")]
    public TextMeshProUGUI enemyKilledText;
    public TextMeshProUGUI goldEarnedText;
    public TextMeshProUGUI timeText;

    // =========================
    // LOSE STATISTICS
    // =========================

    [Header("Lose Statistics")]
    public TextMeshProUGUI loseEnemyKilledText;
    public TextMeshProUGUI loseGoldEarnedText;
    public TextMeshProUGUI loseTimeText;


    void Awake()
    {
        instance = this;

        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);
    }


    void Update()
    {
        if (castleText != null)
            castleText.text = "Thành : " + castleHP + " / " + maxCastleHP;

        if (goldText != null)
            goldText.text = "GOLD : " + gold;

        if (waveText != null)
            waveText.text = "WAVE : " + currentWave + " / " + maxWave;

        // Tính thời gian chơi
        playTime += Time.deltaTime;
    }


    public void DamageCastle(int damage)
    {
        castleHP -= damage;

        if (castleHP < 0)
            castleHP = 0;
    }


    public void AddGold(int amount)
    {
        gold += amount;

        // Tổng vàng kiếm được trong trận
        totalGoldEarned += amount;
    }


    public void TakeCastleDamage(int damage)
    {
        castleHP -= damage;

        if (castleHP <= 0)
        {
            castleHP = 0;

            // Cập nhật thống kê trước khi dừng game
            UpdateBattleStatistics();

            if (losePanel != null)
                losePanel.SetActive(true);

            Time.timeScale = 0f;
        }
    }


    public void WinGame()
    {
        // GIỮ NGUYÊN LOGIC WIN
        UpdateBattleStatistics();

        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }


    public void AddKill()
    {
        enemiesKilled++;
    }


    void UpdateBattleStatistics()
    {
        // =========================
        // WIN PANEL
        // =========================

        if (enemyKilledText != null)
            enemyKilledText.text =
                "Quái đã tiêu diệt : " + enemiesKilled;

        if (goldEarnedText != null)
            goldEarnedText.text =
                "Vàng đã nhận : " + totalGoldEarned;

        if (timeText != null)
        {
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);

            timeText.text =
                "Thời gian : " +
                minutes.ToString("00") +
                ":" +
                seconds.ToString("00");
        }


        // =========================
        // LOSE PANEL
        // =========================

        if (loseEnemyKilledText != null)
            loseEnemyKilledText.text =
                "Quái đã tiêu diệt : " + enemiesKilled;

        if (loseGoldEarnedText != null)
            loseGoldEarnedText.text =
                "Vàng đã nhận : " + totalGoldEarned;

        if (loseTimeText != null)
        {
            int minutes = Mathf.FloorToInt(playTime / 60f);
            int seconds = Mathf.FloorToInt(playTime % 60f);

            loseTimeText.text =
                "Thời gian : " +
                minutes.ToString("00") +
                ":" +
                seconds.ToString("00");
        }
    }

    // =========================
    // HÀM RESTART GAME
    // =========================

    public void RestartGame()
    {
        // Reset time scale về bình thường
        Time.timeScale = 1f;

        // Load lại scene hiện tại
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // =========================
    // HÀM VỀ SẢNH
    // =========================

    public void BackToHall()
    {
        // Reset time scale về bình thường
        Time.timeScale = 1f;

        // Load scene "Sảnh"
        SceneManager.LoadScene("Sảnh");
    }

    // =========================
    // HÀM CHƠI LẠI
    // =========================

    public void ReplayGame()
    {
        // Reset time scale về bình thường
        Time.timeScale = 1f;

        // Load lại scene hiện tại
        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
}