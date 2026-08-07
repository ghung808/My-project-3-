using UnityEngine;

public class Coin : MonoBehaviour
{
    [Header("Cài đặt xu")]
    public int coinValue = 1;      // Giá trị của đồng xu
    public float moveSpeed = 15f;  // Tốc độ bay lên góc màn hình

    private Vector3 targetWorldPosition;
    private bool isFlying = false;

    void Start()
    {
        // Chuyển đổi vị trí góc trên bên trái màn hình (Viewport) sang tọa độ trong game (World Space)
        // (0, 1) là góc trên bên trái. Chúng ta đặt (0.05, 0.95) để nó thụt vào trong màn hình một chút cho đẹp.
        targetWorldPosition = Camera.main.ViewportToWorldPoint(new Vector3(0.05f, 0.95f, Camera.main.nearClipPlane + 5f));

        // Bắt đầu bay ngay khi xuất hiện
        isFlying = true;
    }

    void Update()
    {
        if (isFlying)
        {
            // Di chuyển đồng xu liên tục về hướng góc trên bên trái
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetWorldPosition,
                moveSpeed * Time.deltaTime
            );

            // Khi bay đến gần vị trí đích, tiến hành cộng tiền và xóa đồng xu
            if (Vector3.Distance(transform.position, targetWorldPosition) < 0.2f)
            {
                // Cộng tiền vào PlayerStats
                PlayerStats.Money += coinValue;

                // Hủy đồng xu
                Destroy(gameObject);
            }
        }
    }
}