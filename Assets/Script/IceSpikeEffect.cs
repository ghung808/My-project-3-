using UnityEngine;
using System.Collections;

public class IceSpikeEffect : MonoBehaviour
{
    [Header("Cấu hình Sát thương & Hiệu ứng")]
    public int damage = 10;
    public float slowMultiplier = 0.5f; // Giảm tốc độ còn 50% (0.5 = 50%)
    public float slowDuration = 3f;    // Thời gian làm chậm (giây)
    public float destroyTime = 1f;     // Thời gian tự hủy sau khi animation chạy xong

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Gây sát thương tức thì
            enemy.TakeDamage(damage);

            // Gọi hàm làm chậm
            enemy.ApplySlow(slowMultiplier, slowDuration);
        }
    }
}