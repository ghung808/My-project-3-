using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI instance;

    [Header("Castle")]
    public int castleHP = 10;
    public int maxCastleHP = 10;

    [Header("Gold")]
    public int gold = 10;

    [Header("Wave")]
    public int currentWave = 1;
    public int maxWave = 8;

    [Header("Battle Stats")]
    public int enemiesKilled = 0;
    public int totalGoldEarned = 0;
    public float playTime = 0f;

    [Header("UI Text")]
    public TextMeshProUGUI castleText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI waveText;

    [Header("Win / Lose Panel")]
    public GameObject winPanel;
    public GameObject losePanel;

    [Header("Win Stats")]
    public TextMeshProUGUI enemiesKilledText;
    public TextMeshProUGUI totalGoldEarnedText;
    public TextMeshProUGUI playTimeText;

    [Header("Lose Stats")]
    public TextMeshProUGUI loseEnemiesKilledText;
    public TextMeshProUGUI loseTotalGoldEarnedText;
    public TextMeshProUGUI losePlayTimeText;

    [Header("Win Button")]
    public Button continueButton;

    [Header("Win / Lose Navigation Buttons")]
    public Button winMenuButton;
    public Button loseReplayButton;
    public Button loseMenuButton;

    private bool gameEnded = false;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        // RESET THỐNG KÊ KHI BẮT ĐẦU MAP
        enemiesKilled = 0;
        totalGoldEarned = 0;
        playTime = 0f;
        gameEnded = false;

        // UPDATE UI
        UpdateUI();

        // ẨN WIN / LOSE PANEL
        if (winPanel != null) winPanel.SetActive(false);
        if (losePanel != null) losePanel.SetActive(false);

        // NÚT TIẾP TỤC
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(LoadNextMap);
        }

        // NÚT VỀ MENU - WIN
        if (winMenuButton != null)
        {
            winMenuButton.onClick.RemoveAllListeners();
            winMenuButton.onClick.AddListener(BackToMenu);
        }

        // NÚT CHƠI LẠI - LOSE
        if (loseReplayButton != null)
        {
            loseReplayButton.onClick.RemoveAllListeners();
            loseReplayButton.onClick.AddListener(RestartCurrentMap);
        }

        // NÚT VỀ MENU - LOSE
        if (loseMenuButton != null)
        {
            loseMenuButton.onClick.RemoveAllListeners();
            loseMenuButton.onClick.AddListener(BackToMenu);
        }
    }

    void Update()
    {
        if (!gameEnded)
        {
            playTime += Time.deltaTime;
        }
    }

    public void UpdateUI()
    {
        if (castleText != null) castleText.text = castleHP + " / " + maxCastleHP;
        if (goldText != null) goldText.text = gold.ToString();
        if (waveText != null) waveText.text = "Wave " + currentWave + " / " + maxWave;
    }

    public void TakeCastleDamage(int damage)
    {
        if (gameEnded) return;

        castleHP -= damage;
        if (castleHP < 0) castleHP = 0;

        UpdateUI();

        if (castleHP <= 0)
        {
            LoseGame();
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        if (amount > 0) totalGoldEarned += amount;
        UpdateUI();
    }

    public void AddKill()
    {
        enemiesKilled++;
        AddGold(1);
    }

    void UpdateBattleStatsUI()
    {
        int seconds = Mathf.FloorToInt(playTime);
        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        string timeText = (minutes > 0)
            ? "Time: " + minutes + "m " + remainingSeconds + "s"
            : "Time: " + remainingSeconds + "s";

        // WIN STATS
        if (enemiesKilledText != null) enemiesKilledText.text = "Enemy Killed: " + enemiesKilled;
        if (totalGoldEarnedText != null) totalGoldEarnedText.text = "Gold Earned: " + totalGoldEarned;
        if (playTimeText != null) playTimeText.text = timeText;

        // LOSE STATS
        if (loseEnemiesKilledText != null) loseEnemiesKilledText.text = "Enemy Killed: " + enemiesKilled;
        if (loseTotalGoldEarnedText != null) loseTotalGoldEarnedText.text = "Gold Earned: " + totalGoldEarned;
        if (losePlayTimeText != null) losePlayTimeText.text = timeText;
    }

    // =========================================================
    // XỬ LÝ MỞ KHÓA MAP KHI THẮNG GAME
    // =========================================================

    public void WinGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        UpdateBattleStatsUI();

        if (winPanel != null) winPanel.SetActive(true);

        string currentScene = SceneManager.GetActiveScene().name;

        // Xử lý mở khóa theo từng Map
        if (currentScene == "Vht")
        {
            PlayerPrefs.SetInt("Map2_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log("🏆 Thắng MAP 1 - Đã mở khóa MAP 2 (hgt)");
        }
        else if (currentScene == "hgt")
        {
            PlayerPrefs.SetInt("Map3_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log("🏆 Thắng MAP 2 - Đã mở khóa MAP 3 (dh)");
        }
        else if (currentScene == "dh")
        {
            // Thắng Map 3 -> Đảm bảo mở hết toàn bộ Map
            PlayerPrefs.SetInt("Map2_Unlocked", 1);
            PlayerPrefs.SetInt("Map3_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log("🏆 Thắng MAP 3 - Đã mở khóa toàn bộ Map!");
        }
    }

    public void LoadNextMap()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "Vht")
        {
            SceneManager.LoadScene("hgt");
            return;
        }

        if (currentScene == "hgt")
        {
            SceneManager.LoadScene("dh");
            return;
        }

        if (currentScene == "dh")
        {
            // Nếu ở Map 3 bấm tiếp tục thì quay về Sảnh
            BackToMenu();
            return;
        }
    }

    public void LoseGame()
    {
        if (gameEnded) return;

        gameEnded = true;
        UpdateBattleStatsUI();

        if (losePanel != null) losePanel.SetActive(true);
    }

    public void RestartCurrentMap()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(currentScene);
    }

    // =========================================================
    // VỀ SẢNH (Xử lý thoát khi ở Map 3)
    // =========================================================

    public void BackToMenu()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Nếu người chơi thoát từ Map 3 ra Sảnh -> Mở khóa toàn bộ các Map
        if (currentScene == "dh")
        {
            PlayerPrefs.SetInt("Map2_Unlocked", 1);
            PlayerPrefs.SetInt("Map3_Unlocked", 1);
            PlayerPrefs.Save();
            Debug.Log("🚪 Thoát từ MAP 3 - Đã mở khóa toàn bộ Map!");
        }

        // Chuyển về Scene "Sanh" (Sử dụng tên không dấu đồng bộ)
        SceneManager.LoadScene("Sanh");
    }
}