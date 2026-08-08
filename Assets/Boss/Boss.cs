using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHp = 2000;
    private int currentHp;

    public float moveSpeed = 2f;
    public int armor = 5;

    public int attackDamage = 50;
    public float attackCooldown = 2f;

    [Header("Detection")]
    public float detectRadius = 4f;
    public float attackRange = 1.5f;

    [Header("UI")]
    public Slider healthBar;

    [Header("Projectile")]
    public GameObject fireballPrefab;
    public Transform firePoint;

    private Transform currentWaypoint;
    private int waypointIndex = 0;

    private MonoBehaviour targetSoldier;

    private bool isDead = false;
    private bool rageMode = false;

    private float attackTimer;

    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D col;

    void Start()
    {
        currentHp = maxHp;

        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        UpdateHealthBar();

        GetNextWaypoint();
    }

    void Update()
    {
        if (isDead)
            return;

        UpdateHealthBar();

        if (targetSoldier == null)
        {
            SearchSoldier();
        }

        if (targetSoldier != null && !targetSoldier.gameObject.activeInHierarchy)
        {
            targetSoldier = null;
        }

        if (targetSoldier != null)
        {
            if (Vector2.Distance(transform.position, targetSoldier.transform.position) > detectRadius * 2)
            {
                targetSoldier = null;
                return;
            }

            FaceTarget();

            float dis = Vector2.Distance(
                transform.position,
                targetSoldier.transform.position
            );

            if (dis > attackRange)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    targetSoldier.transform.position,
                    moveSpeed * Time.deltaTime
                );

                Flip(targetSoldier.transform.position.x - transform.position.x);

                if (animator != null)
                {
                    animator.SetBool("isWalking", true);
                }
            }
            else
            {
                AttackBehaviour();
            }

            return;
        }

        MoveWaypoint();
    }

    void MoveWaypoint()
    {
        if (currentWaypoint == null)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            currentWaypoint.position,
            moveSpeed * Time.deltaTime
        );

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
        }

        Flip(currentWaypoint.position.x - transform.position.x);

        if (Vector2.Distance(transform.position, currentWaypoint.position) < 0.15f)
        {
            GetNextWaypoint();
        }
    }

    void GetNextWaypoint()
    {
        if (Waypoints.points == null || Waypoints.points.Length == 0)
            return;

        if (waypointIndex >= Waypoints.points.Length)
        {
            ReachDestination();
            return;
        }

        currentWaypoint = Waypoints.points[waypointIndex];
        waypointIndex++;
    }

    void ReachDestination()
    {
        if (isDead)
            return;

        isDead = true;

        if (GameUI.instance != null)
        {
            GameUI.instance.TakeCastleDamage(GameUI.instance.maxCastleHP);
        }

        Destroy(gameObject);
    }

    void Flip(float x)
    {
        if (Mathf.Abs(x) < 0.01f)
            return;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = x < 0;
        }
    }

    void FaceTarget()
    {
        if (targetSoldier == null)
            return;

        float x = targetSoldier.transform.position.x - transform.position.x;
        Flip(x);
    }

    void UpdateHealthBar()
    {
        if (healthBar == null)
            return;

        healthBar.maxValue = maxHp;
        healthBar.value = currentHp;
    }

    void SearchSoldier()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, detectRadius);

        float nearest = Mathf.Infinity;
        MonoBehaviour bestTarget = null;

        foreach (Collider2D hit in hits)
        {
            PlayerSoldier warrior = hit.GetComponent<PlayerSoldier>();
            ArcherSoldier archer = hit.GetComponent<ArcherSoldier>();
            MageSoldier mage = hit.GetComponent<MageSoldier>();

            MonoBehaviour soldier = null;

            if (warrior != null)
                soldier = warrior;
            else if (archer != null)
                soldier = archer;
            else if (mage != null)
                soldier = mage;

            if (soldier == null)
                continue;

            float d = Vector2.Distance(transform.position, soldier.transform.position);

            if (d < nearest)
            {
                nearest = d;
                bestTarget = soldier;
            }
        }

        targetSoldier = bestTarget;
    }

    void AttackBehaviour()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", false);
        }

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        attackTimer = attackCooldown;

        if (currentHp <= maxHp * 0.3f && !rageMode)
        {
            rageMode = true;
            moveSpeed *= 1.5f;
            attackCooldown *= 0.7f;
            attackDamage += 10;
        }

        float randomSkill = Random.Range(0f, 100f);

        if (randomSkill < 100f)
        {
            FireballAttack();
        }
        else
        {
            NormalAttack();
        }
    }

    void NormalAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("AttackTrig");
        }

        if (targetSoldier == null)
            return;

        if (targetSoldier is ArcherSoldier archer)
        {
            archer.TakeDamage(attackDamage);
        }
        else if (targetSoldier is MageSoldier mage)
        {
            mage.TakeDamage(attackDamage);
        }
        else if (targetSoldier is PlayerSoldier player)
        {
            player.TakeDamage(attackDamage);
        }
    }

    void FireballAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("StrikeTrig");
        }

        if (fireballPrefab == null)
            return;

        if (firePoint == null)
            return;

        if (targetSoldier == null)
            return;

        GameObject obj = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);

        BossProjectile projectile = obj.GetComponent<BossProjectile>();

        if (projectile != null)
        {
            projectile.Initialize(targetSoldier.transform, attackDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        damage -= armor;

        if (damage < 1)
            damage = 1;

        currentHp -= damage;

        if (currentHp < 0)
            currentHp = 0;

        UpdateHealthBar();

        // Hurt trigger removed - no damage animation

        if (currentHp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (col != null)
            col.enabled = false;

        if (animator != null)
        {
            animator.SetBool("isWalking", false);
            animator.SetTrigger("DieTrigger");
        }

        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(false);
        }

        // Cộng số Boss đã giết và vàng
        if (GameUI.instance != null)
        {
            GameUI.instance.AddKill();
            GameUI.instance.AddGold(100);
        }

        Invoke(nameof(WinBattle), 2f);
    }

    void WinBattle()
    {
        if (GameUI.instance != null)
        {
            GameUI.instance.WinGame();
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}   