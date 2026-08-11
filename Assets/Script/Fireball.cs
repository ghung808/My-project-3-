using UnityEngine;

public class FireballEffect : MonoBehaviour
{
    [Header("Cấu hình Sát thương")]
    public int directDamage = 10;          // Sát thương tức thời khi cầu lửa rơi xuống
    public float burnDamagePerSecond = 5f; // Sát thương đốt mỗi giây
    public float burnDuration = 3f;        // Thời gian hiệu ứng đốt kéo dài (giây)

    public float destroyTime = 1f;         // Thời gian tự hủy sau khi chạy animation

    void Start()
    {
        // Tự động xóa đối tượng sau khi chạy xong hiệu ứng
        Destroy(gameObject, destroyTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Gây sát thương tức thì và sát thương đốt dựa trên giá trị chỉnh trong Inspector
            enemy.TakeDamage(directDamage);
            enemy.StartBurn(burnDamagePerSecond, burnDuration);
        }
    }
}