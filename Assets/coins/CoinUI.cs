using UnityEngine;
using TMPro; // Sử dụng thư viện TextMeshPro (nếu bạn dùng Text thường thì đổi thành using UnityEngine.UI;)

public class CoinUI : MonoBehaviour
{
    public TextMeshProUGUI coinText; // Kéo TextMeshPro vào đây qua Inspector
    // Nếu dùng Text thường thì đổi dòng trên thành: public UnityEngine.UI.Text coinText;

    void Update()
    {
        if (coinText != null)
        {
            // Hiển thị số tiền hiện tại lên giao diện
            coinText.text = "Coins: " + PlayerStats.Money.ToString();
        }
    }
}