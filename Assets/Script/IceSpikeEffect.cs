using UnityEngine;

public class IceSpikeEffect : MonoBehaviour
{
    [Header("Cấu hình Sát thương & Hiệu ứng")]
    public int damage = 10;
    public float slowMultiplier = 0.5f;
    public float slowDuration = 3f;
    public float destroyTime = 1f;

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Enemy thường
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            enemy.ApplySlow(slowMultiplier, slowDuration);
            return;
        }

        // 2. BossEnemy (Boss dạng 1)
        BossEnemy bossEnemy = collision.GetComponent<BossEnemy>();
        if (bossEnemy != null)
        {
            bossEnemy.TakeDamage(damage);
            bossEnemy.ApplySlow(slowMultiplier, slowDuration);
            return;
        }

        // 3. Boss (Boss dạng 2)
        Boss boss = collision.GetComponent<Boss>();
        if (boss != null)
        {
            boss.TakeDamage(damage);
        }
    }
}