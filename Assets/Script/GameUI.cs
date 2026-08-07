using TMPro;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public TextMeshProUGUI hpText;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI waveText;

    public void UpdateCastleHP(int hp, int maxHp)
    {
        hpText.text = "❤️ Thành: " + hp + " / " + maxHp;
    }

    public void UpdateGold(int gold)
    {
        goldText.text = "🪙 Vàng: " + gold;
    }

    public void UpdateWave(int wave, int maxWave)
    {
        waveText.text = "🌊 Wave: " + wave + " / " + maxWave;
    }
}