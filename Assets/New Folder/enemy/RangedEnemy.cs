using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class RangedEnemy : MonoBehaviour
{
    [Header("Cấu hình Chỉ số")]
    public float speed = 3f;
    public int maxHp = 20;
    private int hp;
    public int damage = 2;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Phạm vi & Tầm đánh xa")]
    public float detectRadius = 6f;    // Tầm phát hiện lính từ xa
    public float attackRange = 4.5f;   // Tầm đứng bắn xa để quái dừng lại

    [Header("Cấu hình Tấn công Tầm xa (Bắn đạn)")]
    public bool isRangedEnemy = true;
    public GameObject bulletPrefab;
    public Transform firePoint;

    private Transform targetWaypoint;
    private int waypointIndex = 0;

    private bool isDead = false;
    private Transform currentTargetTransform; // Quản lý mục tiêu bất kỳ loại lính nào

    [Header("Thanh máu (UI Slider)")]
    public Slider healthSlider;

    [Header("Cấu hình Rơi Xu")]
    public GameObject coinPrefab;

    private Animator animator;
    private Rigidbody2D rb;
    private Collider2D col;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        hp = maxHp;
        UpdateHealthUI();
        GetNextWaypoint();

        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void Update()
    {
        if (isDead) return;

        // Kiểm tra xem mục tiêu hiện tại còn tồn tại trên bản đồ không
        if (currentTargetTransform != null)
        {
            if (!currentTargetTransform.gameObject.activeInHierarchy)
            {
                currentTargetTransform = null;
            }
        }

        // Nếu chưa có mục tiêu, tiến hành quét tìm tất cả các loại lính
        if (currentTargetTransform == null)
        {
            FindClosestSoldier();
        }

        // Xử lý hành động di chuyển hoặc đứng bắn
        if (currentTargetTransform != null)
        {
            float distanceToTarget = Vector3.Distance(transform.position, currentTargetTransform.position);

            // Quay mặt về phía mục tiêu
            float directionX = currentTargetTransform.position.x - transform.position.x;
            FlipSprite(directionX);

            if (distanceToTarget > attackRange)
            {
                // Nếu lính ở ngoài tầm bắn -> Di chuyển tiến lại gần mục tiêu
                MoveTowardsTarget(currentTargetTransform.position);
            }
            else
            {
                // Đã nằm trong tầm bắn -> Dừng lại và thực hiện bắn
                if (rb != null) rb.linearVelocity = Vector2.zero;
                SetAnimatorBool("isRunning", false);

                CheckTargetAlive();
            }
        }
        else
        {
            // Không thấy lính -> Tiếp tục di chuyển theo hệ thống Waypoint ban đầu
            MoveTowardsWaypoint();
        }
    }

    void FindClosestSoldier()
    {
        float shortestDistance = Mathf.Infinity;
        Transform nearestTarget = null;

        // 1. Quét tìm PlayerSoldier (Đấu sĩ)
        foreach (var w in FindObjectsByType<PlayerSoldier>(FindObjectsSortMode.None))
        {
            if (w == null || !w.gameObject.activeInHierarchy) continue;
            float dist = Vector3.Distance(transform.position, w.transform.position);
            if (dist <= detectRadius && dist < shortestDistance)
            {
                shortestDistance = dist;
                nearestTarget = w.transform;
            }
        }

        // 2. Quét tìm MageSoldier (Pháp sư)
        foreach (var m in FindObjectsByType<MageSoldier>(FindObjectsSortMode.None))
        {
            if (m == null || !m.gameObject.activeInHierarchy) continue;
            float dist = Vector3.Distance(transform.position, m.transform.position);
            if (dist <= detectRadius && dist < shortestDistance)
            {
                shortestDistance = dist;
                nearestTarget = m.transform;
            }
        }

        // 3. Quét tìm ArcherSoldier (Xạ thủ)
        foreach (var a in FindObjectsByType<ArcherSoldier>(FindObjectsSortMode.None))
        {
            if (a == null || !a.gameObject.activeInHierarchy) continue;
            float dist = Vector3.Distance(transform.position, a.transform.position);
            if (dist <= detectRadius && dist < shortestDistance)
            {
                shortestDistance = dist;
                nearestTarget = a.transform;
            }
        }

        currentTargetTransform = nearestTarget;
    }

    void MoveTowardsWaypoint()
    {
        if (targetWaypoint == null) return;

        Vector3 dir = (targetWaypoint.position - transform.position).normalized;

        if (rb != null)
        {
            rb.MovePosition(transform.position + dir * speed * Time.fixedDeltaTime);
        }

        if (dir.x != 0)
        {
            FlipSprite(dir.x);
        }

        SetAnimatorBool("isRunning", true);

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.2f)
        {
            GetNextWaypoint();
        }
    }

    void MoveTowardsTarget(Vector3 targetPos)
    {
        Vector3 dir = (targetPos - transform.position).normalized;

        if (rb != null)
        {
            rb.MovePosition(transform.position + dir * speed * Time.fixedDeltaTime);
        }

        if (dir.x != 0)
        {
            FlipSprite(dir.x);
        }

        SetAnimatorBool("isRunning", true);
    }

    void GetNextWaypoint()
    {
        if (Waypoints.points == null || Waypoints.points.Length == 0) return;

        if (waypointIndex >= Waypoints.points.Length)
        {
            ReachDestination();
            return;
        }

        targetWaypoint = Waypoints.points[waypointIndex];
        waypointIndex++;
    }

    void ReachDestination()
    {
        Destroy(gameObject);
    }

    // --- HÀM NHẬN SÁT THƯƠNG KHI PLAYER ĐÁNH QUÁI ---
    public void TakeDamage(int amt)
    {
        if (isDead) return;

        hp -= amt;
        UpdateHealthUI();

        if (hp > 0)
        {
            SetAnimatorTrigger("Hurt");
        }
        else
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

    void Die()
    {
        isDead = true;

        if (col != null) col.enabled = false;
        if (rb != null) rb.simulated = false;

        SetAnimatorTrigger("Die");

        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(false);
        }

        if (coinPrefab != null)
        {
            Instantiate(coinPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, 1.0f);
    }

    void CheckTargetAlive()
    {
        if (currentTargetTransform == null) return;

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackTarget();
            lastAttackTime = Time.time;
        }
    }

    void AttackTarget()
    {
        if (currentTargetTransform == null) return;

        SetAnimatorTrigger("Attack");

        if (isRangedEnemy)
        {
            ShootBullet();
        }
    }

    void ShootBullet()
    {
        if (bulletPrefab == null || currentTargetTransform == null) return;

        Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;
        GameObject bulletObj = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

        EnemyBullet bullet = bulletObj.GetComponent<EnemyBullet>();
        if (bullet != null)
        {
            bullet.Seek(currentTargetTransform, damage);
        }
    }

    void FlipSprite(float directionX)
    {
        if (spriteRenderer != null && Mathf.Abs(directionX) > 0.01f)
        {
            spriteRenderer.flipX = directionX < 0;
        }
    }

    void SetAnimatorTrigger(string triggerName)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == triggerName && param.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.SetTrigger(triggerName);
                    return;
                }
            }
        }
    }

    void SetAnimatorBool(string boolName, bool value)
    {
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == boolName && param.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(boolName, value);
                    return;
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}