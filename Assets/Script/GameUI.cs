using UnityEngine;
using TMPro;

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
        castleText.text = "Thành : " + castleHP + " / " + maxCastleHP;
        goldText.text = "GOLD : " + gold;
        waveText.text = "WAVE : " + currentWave + " / " + maxWave;

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
        totalGoldEarned += amount;
    }

    public void TakeCastleDamage(int damage)
    {
        castleHP -= damage;

        if (castleHP <= 0)
        {
            castleHP = 0;

            if (losePanel != null)
                losePanel.SetActive(true);

            Time.timeScale = 0f;
        }
    }

    public void WinGame()
    {
        if (winPanel != null)
            winPanel.SetActive(true);

        Time.timeScale = 0f;
    }

    public void AddKill()
    {
        enemiesKilled++;
    }
}