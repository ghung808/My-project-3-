using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Transform target;
    private int damage;
    public float speed = 10f;

    public void Seek(Transform _target, int _damage)
    {
        target = _target;
        damage = _damage;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        if (target != null)
        {
            // Cách tổng quát: Tự động tìm tất cả các component trên mục tiêu và gọi hàm TakeDamage nếu có
            // Giúp tránh việc lính dùng tên script khác mà đạn không nhận diện được

            PlayerSoldier w = target.GetComponent<PlayerSoldier>();
            if (w != null) { w.TakeDamage(damage); Destroy(gameObject); return; }

            MageSoldier m = target.GetComponent<MageSoldier>();
            if (m != null) { m.TakeDamage(damage); Destroy(gameObject); return; }

            ArcherSoldier a = target.GetComponent<ArcherSoldier>();
            if (a != null) { a.TakeDamage(damage); Destroy(gameObject); return; }

            // Dự phòng trường hợp lính của bạn dùng một script chung khác có chứa hàm TakeDamage
            // Bạn có thể gắn một interface hoặc gọiSendMessage nếu cần thiết
            target.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
        }

        Destroy(gameObject);
    }
}