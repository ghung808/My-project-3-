using UnityEngine;

public class LightningDamage : MonoBehaviour
{
    [Header("Skill Stats")]
    public int damage = 50;           // Lượng sát thương của sét (bạn có thể chỉnh tùy ý trong Inspector)
    public float lifetime = 0.5f;     // Thời gian tự hủy khớp với độ dài animation sét

    void Start()
    {
        // Tự động hủy đối tượng sét sau khi chạy xong animation
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Kiểm tra xem đối tượng va chạm có chứa script Enemy hay không
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Gọi hàm TakeDamage có sẵn trong script Enemy để trừ máu quái
            enemy.TakeDamage(damage);

            Debug.Log("Sét đã đánh trúng quái và gây " + damage + " sát thương!");
        }
    }
}