using UnityEngine;

public class LightningDamage : MonoBehaviour
{
    [Header("Skill Stats")]
    public int damage = 50;           // Lượng sát thương gây ra
    public float lifetime = 0.5f;     // Thời gian tự hủy của hiệu ứng sét (tính bằng giây, khớp với độ dài animation)

    void Start()
    {
        // Tự động hủy đối tượng sét sau khi chạy xong animation để tránh nặng game
        Destroy(gameObject, lifetime);
    }

    // Khi có một đối tượng đi vào vùng Trigger của sét
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem đối tượng va chạm có tag là "Enemy" (Quái vật) không
        if (collision.CompareTag("Enemy"))
        {
            // Tìm script quản lý máu của quái (ví dụ đặt tên là EnemyHealth)
            // Bạn cần thay "EnemyHealth" bằng tên script máu thực tế của quái trong game bạn
            /* 
            EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
            */

            Debug.Log("Sét đánh trúng quái: " + collision.gameObject.name + " - Gây " + damage + " sát thương!");

            // Nếu quái của bạn có hàm trừ máu trực tiếp đơn giản, có thể gọi tại đây.
        }
    }
}