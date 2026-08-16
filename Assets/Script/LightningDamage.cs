using UnityEngine;

public class LightningDamage : MonoBehaviour
{
    [Header("Skill Stats")]
    public int damage = 50;
    public float lifetime = 0.5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Enemy thường
        Enemy enemy = collision.GetComponent<Enemy>();
        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            return;
        }

        // 2. BossEnemy (Boss dạng 1)
        BossEnemy bossEnemy = collision.GetComponent<BossEnemy>();
        if (bossEnemy != null)
        {
            bossEnemy.TakeDamage(damage);
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