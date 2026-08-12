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
        if (isDead) return;

        UpdateHealthBar();

        if (targetSoldier == null) SearchSoldier();
        if (targetSoldier != null && !targetSoldier.gameObject.activeInHierarchy) targetSoldier = null;

        // Nếu có lính trong tầm đánh -> Dừng lại và quay mặt về phía lính để tấn công
        if (targetSoldier != null)
        {
            float dis = Vector2.Distance(transform.position, targetSoldier.transform.position);
            if (dis <= attackRange)
            {
                FaceTarget();
                AttackBehaviour();
                return;
            }
        }

        // Nếu không đánh lính -> Di chuyển theo Waypoint và lật mặt theo hướng đi
        MoveWaypoint();
    }

    void MoveWaypoint()
    {
        if (currentWaypoint == null) return;

        // Lấy hướng từ vị trí hiện tại đến Waypoint
        Vector3 dir = currentWaypoint.position - transform.position;

        // Di chuyển Boss
        transform.position = Vector3.MoveTowards(transform.position, currentWaypoint.position, moveSpeed * Time.deltaTime);

        if (animator != null) animator.SetBool("isWalking", true);

        // Lật mặt theo hướng di chuyển (dir.x)
        if (Mathf.Abs(dir.x) > 0.001f)
        {
            Flip(dir.x);
        }

        // Kiểm tra xem đã đến Waypoint chưa
        if (Vector2.Distance(transform.position, currentWaypoint.position) < 0.15f)
        {
            GetNextWaypoint();
        }
    }

    void Flip(float x)
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = x < 0;
        }
    }

    void FaceTarget()
    {
        if (targetSoldier == null) return;
        float x = targetSoldier.transform.position.x - transform.position.x;
        Flip(x);
    }

    void GetNextWaypoint()
    {
        if (Waypoints.points == null || Waypoints.points.Length == 0) return;
        if (waypointIndex >= Waypoints.points.Length) { ReachDestination(); return; }
        currentWaypoint = Waypoints.points[waypointIndex];
        waypointIndex++;
    }

    void ReachDestination()
    {
        if (isDead) return;
        isDead = true;
        if (GameUI.instance != null) GameUI.instance.TakeCastleDamage(GameUI.instance.maxCastleHP);
        Destroy(gameObject);
    }

    void UpdateHealthBar()
    {
        if (healthBar == null) return;
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
            MonoBehaviour soldier = hit.GetComponent<PlayerSoldier>() ?? (MonoBehaviour)hit.GetComponent<ArcherSoldier>() ?? hit.GetComponent<MageSoldier>();
            if (soldier != null)
            {
                float d = Vector2.Distance(transform.position, soldier.transform.position);
                if (d < nearest) { nearest = d; bestTarget = soldier; }
            }
        }
        targetSoldier = bestTarget;
    }

    void AttackBehaviour()
    {
        if (animator != null) animator.SetBool("isWalking", false);
        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;
        attackTimer = attackCooldown;
        if (currentHp <= maxHp * 0.3f && !rageMode) { rageMode = true; moveSpeed *= 1.5f; attackCooldown *= 0.7f; attackDamage += 10; }
        FireballAttack();
    }

    void FireballAttack()
    {
        if (animator != null) animator.SetTrigger("StrikeTrig");
        if (fireballPrefab != null && firePoint != null && targetSoldier != null)
        {
            GameObject obj = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
            obj.GetComponent<BossProjectile>()?.Initialize(targetSoldier.transform, attackDamage);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHp = Mathf.Max(0, currentHp - Mathf.Max(1, damage - armor));
        UpdateHealthBar();
        if (currentHp <= 0) Die();
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        if (col != null) col.enabled = false;
        if (animator != null) { animator.SetBool("isWalking", false); animator.SetTrigger("DieTrigger"); }
        if (healthBar != null) healthBar.gameObject.SetActive(false);
        if (GameUI.instance != null) { GameUI.instance.AddKill(); GameUI.instance.AddGold(100); }
        Invoke(nameof(WinBattle), 2f);
    }

    void WinBattle()
    {
        if (GameUI.instance != null) GameUI.instance.WinGame();
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, detectRadius);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}