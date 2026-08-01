using UnityEngine;

public class Arrow : MonoBehaviour
{
    private Transform target;
    private int damage;
    public float speed = 10f; // Tốc độ bay của mũi tên

    public void Seek(Transform _target, int _damage)
    {
        target = _target;
        damage = _damage;
    }

    void Update()
    {
        // Nếu quái mục tiêu đã biến mất (chết), hủy mũi tên luôn
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // Di chuyển mũi tên hướng về phía quái
        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);

        // Xoay đầu mũi tên hướng về phía quái (tùy chọn)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        // Kiểm tra nếu mũi tên bay đến rất gần quái thì gây sát thương và biến mất
        if (dir.magnitude <= 0.4f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        Enemy enemyScript = target.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}