using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Cấu hình Chỉ số")]
    public float speed = 3f;
    public int maxHp = 20;
    private int hp;
    public int damage = 2;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Phạm vi phát hiện và chặn lính")]
    public float checkRadius = 0.8f;

    private Transform targetWaypoint;
    private int waypointIndex = 0;

    private bool isEngaged = false;
    private bool isDead = false;
    private MonoBehaviour currentTargetSoldier;

    [Header("Thanh máu (UI Slider)")]
    public Slider healthSlider;

    [Header("Cấu hình Rơi Xu")]
    public GameObject coinPrefab;

    private Animator animator;
    private Collider2D col;

    void Start()
    {
        hp = maxHp;
        UpdateHealthUI();
        GetNextWaypoint();

        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        if (isDead) return;

        if (isEngaged)
        {
            CheckSoldierAlive();
            return;
        }

        CheckForSoldiersAhead();
        MoveTowardsWaypoint();
    }

    void CheckForSoldiersAhead()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, checkRadius);

        foreach (var hit in hits)
        {
            PlayerSoldier warrior = hit.GetComponent<PlayerSoldier>();
            MageSoldier mage = hit.GetComponent<MageSoldier>();
            ArcherSoldier archer = hit.GetComponent<ArcherSoldier>();

            if (warrior != null || mage != null || archer != null)
            {
                isEngaged = true;
                currentTargetSoldier = hit.GetComponent<MonoBehaviour>();

                // Chuyển sang trạng thái đứng yên (eidle) khi gặp lính
                SetAnimatorBool("isRunning", false);
                break;
            }
        }
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

    void MoveTowardsWaypoint()
    {
        if (targetWaypoint == null) return;

        Vector3 dir = targetWaypoint.position - transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

        // Chuyển sang trạng thái đi bộ (ewalk)
        SetAnimatorBool("isRunning", true);

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.2f)
        {
            GetNextWaypoint();
        }
    }

    void ReachDestination()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(int amt)
    {
        if (isDead) return;

        hp -= amt;
        UpdateHealthUI();
        if (hp > 0)
        {
            // Kích hoạt animation bị thương (ehurt)
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

        // Kích hoạt animation chết (edealth)
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

    void CheckSoldierAlive()
    {
        if (currentTargetSoldier == null)
        {
            isEngaged = false;
            return;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackSoldier();
            lastAttackTime = Time.time;
        }
    }

    void AttackSoldier()
    {
        if (currentTargetSoldier == null) return;

        // Kích hoạt animation tấn công (eattack)
        SetAnimatorTrigger("Attack");

        if (currentTargetSoldier is PlayerSoldier w) w.TakeDamage(damage);
        else if (currentTargetSoldier is MageSoldier m) m.TakeDamage(damage);
        else if (currentTargetSoldier is ArcherSoldier a) a.TakeDamage(damage);
    }

    // --- HÀM HỖ TRỢ AN TOÀN TRÁNH LỖI ANIMATOR ---
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
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}