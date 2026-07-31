using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public static int Money;
    public int startMoney = 200;

    public static int Lives;
    public int startLives = 20;

    void Start()
    {
        Money = startMoney;
        Lives = startLives;
    }

    void Update()
    {
        // Nhấn phím M để kiểm tra nhanh chỉ số trên Console
        if (Input.GetKeyDown(KeyCode.M))
        {
            Debug.Log("Tiền: $" + Money + " | Máu: " + Lives);
        }
    }
}