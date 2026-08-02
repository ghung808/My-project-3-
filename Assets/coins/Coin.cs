using UnityEngine;

public class Coin : MonoBehaviour
{
    public int coinValue = 1;

    void Update()
    {
        // Khi người chơi nhấn phím số 1
        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
        {
            CollectCoin();
        }
    }

    void CollectCoin()
    {
        // Cộng xu trực tiếp vào PlayerStats
        PlayerStats.Money += coinValue;

        // Hủy đồng xu này đi
        Destroy(gameObject);
    }
}