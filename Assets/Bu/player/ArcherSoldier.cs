using UnityEngine;
using UnityEngine.UI;

public class ArcherSoldier : MonoBehaviour
{
    private int maxHp;
    private int hp;
    private int damage;
    private SpawnTower towerRef;
    private Vector3 rallyPosition;

    [Header("Cấu hình Tấn công Xạ thủ")]
    public float attackRange = 5f;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Tốc độ")]
    public float speed = 3f;

    public GameObject arrowPrefab;
    public Transform shootPoint;
    private Transform targetEnemy;
    private Animator anim;

    [Header("Thanh máu (UI Slider)")]
    public Slider healthSlider;

    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void InitializeStats(int _hp, int _dmg, SpawnTower _tower)
    {
        maxHp = _hp;
        hp = _hp;
        damage = _dmg;
        towerRef = _tower;
        UpdateHealthUI();
    }

    public void SetRallyPosition(Vector3 pos)
    {
        rallyPosition = pos;
        transform.position = rallyPosition;
    }

    void Update()
    {
        FindClosestEnemy();

        if (targetEnemy != null)
        {
            float distToEnemy = Vector3.Distance(transform.position, targetEnemy.position);
            if (distToEnemy <= attackRange)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    ShootAtEnemy();
                    lastAttackTime = Time.time;
                }
            }
        }
    }

    void FindClosestEnemy()
    {
        float shortestDist = Mathf.Infinity;
        Transform nearestTarget = null;

        // =========================
        // TÌM ENEMY
        // =========================

        Enemy[] enemies =
            Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null || !enemy.gameObject.activeInHierarchy)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance < shortestDist)
            {
                shortestDist = distance;
                nearestTarget = enemy.transform;
            }
        }

        // =========================
        // TÌM BOSS
        // =========================

        Boss[] bosses =
            Object.FindObjectsByType<Boss>(FindObjectsSortMode.None);

        foreach (Boss boss in bosses)
        {
            if (boss == null || !boss.gameObject.activeInHierarchy)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                boss.transform.position
            );

            if (distance < shortestDist)
            {
                shortestDist = distance;
                nearestTarget = boss.transform;
            }
        }

        // =========================
        // TÌM BOSS MAP 3
        // =========================

        BossEnemy[] bossEnemies =
            Object.FindObjectsByType<BossEnemy>(
                FindObjectsSortMode.None
            );

        foreach (BossEnemy bossEnemy in bossEnemies)
        {
            if (bossEnemy == null ||
                !bossEnemy.gameObject.activeInHierarchy)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                bossEnemy.transform.position
            );

            if (distance < shortestDist)
            {
                shortestDist = distance;
                nearestTarget = bossEnemy.transform;
            }
        }

        // =========================
        // GÁN MỤC TIÊU
        // =========================

        if (nearestTarget != null && shortestDist <= attackRange)
        {
            targetEnemy = nearestTarget;
        }
        else
        {
            targetEnemy = null;
        }
    }

    void ShootAtEnemy()
    {
        // Kích hoạt chính xác Trigger "AttackTrig" từ Animator của Xạ Thủ
        if (anim != null)
        {
            anim.SetTrigger("AttackTrigger");
        }

        if (arrowPrefab != null && targetEnemy != null)
        {
            Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position;
            GameObject arrowObj = Instantiate(arrowPrefab, spawnPos, Quaternion.identity);
            Arrow arrowScript = arrowObj.GetComponent<Arrow>();

            if (arrowScript != null)
            {
                arrowScript.Seek(targetEnemy, damage);
            }
        }
    }

    public void TakeDamage(int amt)
    {
        hp -= amt;
        UpdateHealthUI();

        if (anim != null)
        {
            anim.SetTrigger("HurtTrigger");
        }

        if (hp <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHp;
            healthSlider.value = hp;
        }
    }

    public void FullHeal()
    {
        hp = maxHp;
        UpdateHealthUI();
    }

    void Die()
    {
        if (anim != null)
        {
            anim.SetTrigger("DieTrigger");
        }

        if (towerRef != null)
        {
            towerRef.OnSoldierDied(this);
        }

        Destroy(gameObject, 0.2f);
    }
}