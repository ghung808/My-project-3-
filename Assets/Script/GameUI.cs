using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    public static GameUI instance;

    public int castleHP = 50; // Changed from castleHp to castleHP for consistency
    public int maxCastleHP = 50;

    public int gold = 100;

    public int currentWave = 1;
    public int maxWave = 8;

    public TextMeshProUGUI castleText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI waveText;

    void Awake()
    {
        instance = this;
    }

    void Update()
    {
        castleText.text = "Thành : " + castleHP + " / " + maxCastleHP;
        goldText.text = "GOLD : " + gold;
        waveText.text = "WAVE : " + currentWave + " / " + maxWave;
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
    }

    // New method added below
    public void TakeCastleDamage(int damage)
    {
        castleHP -= damage;

        if (castleHP < 0)
            castleHP = 0;
    }
}