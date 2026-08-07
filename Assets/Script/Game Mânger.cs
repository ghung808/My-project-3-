using UnityEngine;
using System.Collections;
using TMPro; // Add this line for TextMeshPro support

public class GoldManager : MonoBehaviour
{
    public int currentGold = 1250;
    public TextMeshProUGUI goldText; // Kéo TextMeshPro vào đây trong Inspector

    void Update()
    {
        // Cập nhật hiển thị số vàng lên UI
        if (goldText != null)
        {
            goldText.text = "GOLD: " + currentGold.ToString("N0");
        }
    }

    // Hàm gọi khi người chơi nhặt thêm vàng
    public void AddGold(int amount)
    {
        currentGold += amount;
    }
}