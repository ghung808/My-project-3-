using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static int Lives;
    public int startLives = 20;

    public static int Money;
    public int startMoney = 6; // Số tiền khởi đầu

    void Awake()
    {
        Lives = startLives;
        Money = startMoney;
    }
}