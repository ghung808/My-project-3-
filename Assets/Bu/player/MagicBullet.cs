using UnityEngine;

public class MagicBullet : MonoBehaviour
{
    private Transform target;
    private int damage;
    public float speed = 8f;

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
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // =========================================
        // ENEMY THƯỜNG
        // =========================================

        Enemy enemy = target.GetComponent<Enemy>();

        if (enemy == null)
        {
            enemy = target.GetComponentInParent<Enemy>();
        }

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // =========================================
        // BOSS CŨ - MAP 1 / MAP 2
        // GIỮ NGUYÊN
        // =========================================

        Boss boss = target.GetComponent<Boss>();

        if (boss == null)
        {
            boss = target.GetComponentInParent<Boss>();
        }

        if (boss != null)
        {
            boss.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // =========================================
        // BOSS MAP 3
        // THÊM MỚI
        // =========================================

        BossEnemy bossEnemy =
            target.GetComponent<BossEnemy>();

        if (bossEnemy == null)
        {
            bossEnemy =
                target.GetComponentInParent<BossEnemy>();
        }

        if (bossEnemy != null)
        {
            Debug.Log(
                "MagicBullet gây " +
                damage +
                " damage cho BossEnemy"
            );

            bossEnemy.TakeDamage(damage);

            Destroy(gameObject);
            return;
        }

        // =========================================
        // KHÔNG TÌM THẤY MỤC TIÊU
        // =========================================

        Debug.LogWarning(
            "MagicBullet chạm mục tiêu nhưng không tìm thấy Enemy, Boss hoặc BossEnemy!"
        );

        Destroy(gameObject);
    }
}