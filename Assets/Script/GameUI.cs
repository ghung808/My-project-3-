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
        // =====================================================
        // RESET THỐNG KÊ KHI BẮT ĐẦU MAP
        // =====================================================

        enemiesKilled = 0;
        totalGoldEarned = 0;
        playTime = 0f;

        gameEnded = false;

        // =====================================================
        // UPDATE UI
        // =====================================================

        UpdateUI();

        // =====================================================
        // ẨN WIN / LOSE PANEL
        // =====================================================

        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        // =====================================================
        // NÚT TIẾP TỤC
        // =====================================================

        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(LoadNextMap);
        }

        // =====================================================
        // NÚT VỀ MENU - WIN
        // =====================================================

        if (winMenuButton != null)
        {
            winMenuButton.onClick.RemoveAllListeners();
            winMenuButton.onClick.AddListener(BackToMenu);
        }

        // =====================================================
        // NÚT CHƠI LẠI - LOSE
        // =====================================================

        if (loseReplayButton != null)
        {
            loseReplayButton.onClick.RemoveAllListeners();
            loseReplayButton.onClick.AddListener(RestartCurrentMap);
        }

        // =====================================================
        // NÚT VỀ MENU - LOSE
        // =====================================================

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

    // =========================================================
    // UPDATE UI
    // =========================================================

    public void UpdateUI()
    {
        if (castleText != null)
        {
            castleText.text =
                castleHP + " / " + maxCastleHP;
        }

        if (goldText != null)
        {
            goldText.text =
                gold.ToString();
        }

        if (waveText != null)
        {
            waveText.text =
                "Wave " + currentWave + " / " + maxWave;
        }
    }

    // =========================================================
    // CASTLE DAMAGE
    // =========================================================

    public void TakeCastleDamage(int damage)
    {
        if (gameEnded)
            return;

        castleHP -= damage;

        if (castleHP < 0)
            castleHP = 0;

        UpdateUI();

        if (castleHP <= 0)
        {
            LoseGame();
        }
    }

    // =========================================================
    // GOLD
    // =========================================================

    public void AddGold(int amount)
    {
        gold += amount;

        if (amount > 0)
        {
            totalGoldEarned += amount;
        }

        UpdateUI();
    }

    // =========================================================
    // ENEMY KILL
    // =========================================================

    public void AddKill()
    {
        enemiesKilled++;

        // Mỗi quái chết +1 vàng
        AddGold(1);

        Debug.Log(
            "💀 Enemy Killed: " + enemiesKilled +
            " | Gold Earned: " + totalGoldEarned +
            " | Current Gold: " + gold
        );
    }

    // =========================================================
    // UPDATE BẢNG THỐNG KÊ
    // =========================================================

    void UpdateBattleStatsUI()
    {
        int seconds = Mathf.FloorToInt(playTime);

        int minutes = seconds / 60;
        int remainingSeconds = seconds % 60;

        string timeText;

        if (minutes > 0)
        {
            timeText =
                "Time: " +
                minutes +
                "m " +
                remainingSeconds +
                "s";
        }
        else
        {
            timeText =
                "Time: " +
                remainingSeconds +
                "s";
        }

        // =====================================================
        // WIN STATS
        // =====================================================

        if (enemiesKilledText != null)
        {
            enemiesKilledText.text =
                "Enemy Killed: " + enemiesKilled;
        }

        if (totalGoldEarnedText != null)
        {
            totalGoldEarnedText.text =
                "Gold Earned: " + totalGoldEarned;
        }

        if (playTimeText != null)
        {
            playTimeText.text =
                timeText;
        }

        // =====================================================
        // LOSE STATS
        // =====================================================

        if (loseEnemiesKilledText != null)
        {
            loseEnemiesKilledText.text =
                "Enemy Killed: " + enemiesKilled;
        }

        if (loseTotalGoldEarnedText != null)
        {
            loseTotalGoldEarnedText.text =
                "Gold Earned: " + totalGoldEarned;
        }

        if (losePlayTimeText != null)
        {
            losePlayTimeText.text =
                timeText;
        }
    }

    // =========================================================
    // WIN GAME
    // =========================================================

    public void WinGame()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        // Cập nhật thống kê trước khi hiện bảng Win
        UpdateBattleStatsUI();

        Debug.Log("🎉 THẮNG MAP!");
        Debug.Log("💀 Enemy Killed: " + enemiesKilled);
        Debug.Log("🪙 Gold Earned: " + totalGoldEarned);
        Debug.Log("⏱ Time: " + playTime);

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        string currentScene =
            SceneManager.GetActiveScene().name;

        Debug.Log(
            "Map hiện tại: " +
            currentScene
        );

        if (currentScene == "Vht")
        {
            Debug.Log(
                "🏆 Đã thắng MAP 1 - Vht"
            );

            Debug.Log(
                "➡️ Nút TIẾP TỤC sẽ sang MAP 2 - hgt"
            );
        }
        else if (currentScene == "hgt")
        {
            Debug.Log(
                "🏆 Đã thắng MAP 2 - hgt"
            );

            Debug.Log(
                "➡️ Nút TIẾP TỤC sẽ sang MAP 3 - dh"
            );
        }
        else if (currentScene == "dh")
        {
            Debug.Log(
                "🏆 Đã thắng MAP 3 - dh"
            );

            Debug.Log(
                "🎉 ĐÃ HOÀN THÀNH TOÀN BỘ 3 MAP!"
            );
        }
    }

    // =========================================================
    // NÚT TIẾP TỤC
    // =========================================================

    public void LoadNextMap()
    {
        string currentScene =
            SceneManager.GetActiveScene().name;

        Debug.Log(
            "Đang chuyển map từ: " +
            currentScene
        );

        // MAP 1 → MAP 2
        if (currentScene == "Vht")
        {
            Debug.Log(
                "➡️ Chuyển từ MAP 1 Vht → MAP 2 hgt"
            );

            SceneManager.LoadScene("hgt");
            return;
        }

        // MAP 2 → MAP 3
        if (currentScene == "hgt")
        {
            Debug.Log(
                "➡️ Chuyển từ MAP 2 hgt → MAP 3 dh"
            );

            SceneManager.LoadScene("dh");
            return;
        }

        // MAP 3
        if (currentScene == "dh")
        {
            Debug.Log(
                "🎉 Đã hoàn thành MAP 3 - hết game!"
            );

            return;
        }

        Debug.LogWarning(
            "⚠️ Scene hiện tại không phải Vht, hgt hoặc dh: " +
            currentScene
        );
    }

    // =========================================================
    // LOSE GAME
    // =========================================================

    public void LoseGame()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        // Cập nhật thống kê khi thua
        UpdateBattleStatsUI();

        Debug.Log("💀 THUA GAME!");

        Debug.Log(
            "Enemy Killed: " +
            enemiesKilled +
            " | Gold Earned: " +
            totalGoldEarned +
            " | Time: " +
            playTime
        );

        if (losePanel != null)
        {
            losePanel.SetActive(true);
        }
    }

    // =========================================================
    // CHƠI LẠI MAP HIỆN TẠI
    // =========================================================

    public void RestartCurrentMap()
    {
        string currentScene =
            SceneManager.GetActiveScene().name;

        Debug.Log(
            "🔄 Chơi lại map hiện tại: " +
            currentScene
        );

        SceneManager.LoadScene(currentScene);
    }

    // =========================================================
    // VỀ SẢNH
    // =========================================================

    public void BackToMenu()
    {
        Debug.Log(
            "🏠 Đang quay về Sảnh..."
        );

        SceneManager.LoadScene("Sảnh");
    }
}