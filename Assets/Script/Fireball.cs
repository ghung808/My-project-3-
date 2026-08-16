using UnityEngine;

public class FireballEffect : MonoBehaviour
{
    [Header("Cấu hình Sát thương")]
    public int directDamage = 10;
    public float burnDamagePerSecond = 5f;
    public float burnDuration = 3f;
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
            enemy.TakeDamage(directDamage);
            enemy.StartBurn(burnDamagePerSecond, burnDuration);
            return;
        }

        // 2. BossEnemy (Boss dạng 1)
        BossEnemy bossEnemy = collision.GetComponent<BossEnemy>();
        if (bossEnemy != null)
        {
            bossEnemy.TakeDamage(directDamage);
            bossEnemy.StartBurn(burnDamagePerSecond, burnDuration);
            return;
        }

        // 3. Boss (Boss dạng 2)
        Boss boss = collision.GetComponent<Boss>();
        if (boss != null)
        {
            boss.TakeDamage(directDamage);
        }
    }
}