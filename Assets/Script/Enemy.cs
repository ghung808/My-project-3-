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

    // --- Biến quản lý hiệu ứng Đốt (Burn DOT) ---
    [Header("Hiệu ứng Trạng thái (Đốt)")]
    private bool isBurning = false;
    private Coroutine burnCoroutine;

    // --- BỔ SUNG: Biến quản lý hiệu ứng Làm chậm (Slow) ---
    [Header("Hiệu ứng Trạng thái (Làm chậm)")]
    private float originalSpeed;
    private bool isSlowed = false;
    private Coroutine slowCoroutine;

    private Animator animator;
    private Collider2D col;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // --- Biến quản lý waypoint tùy chỉnh cho Map3 ---
    [HideInInspector]
    public Transform[] customWaypoints;

    void Start()
    {
        hp = maxHp;
        UpdateHealthUI();
        GetNextWaypoint();

        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        if (isDead) return;

        if (isEngaged)
        {
            if (currentTargetSoldier != null)
            {
                float directionX = currentTargetSoldier.transform.position.x - transform.position.x;
                FlipSprite(directionX);
            }

            CheckSoldierAlive();
            return;
        }

        CheckForSoldiersAhead();
        MoveTowardsWaypoint();
    }

    // --- Hàm kích hoạt hiệu ứng Đốt từ Cầu Lửa ---
    public void StartBurn(float dps, float duration)
    {
        if (isDead) return;

        if (isBurning && burnCoroutine != null)
        {
            StopCoroutine(burnCoroutine);
        }

        burnCoroutine = StartCoroutine(BurnEffectRoutine(dps, duration));
    }

    IEnumerator BurnEffectRoutine(float dps, float duration)
    {
        isBurning = true;
        float elapsed = 0f;
        int damagePerTick = Mathf.RoundToInt(dps);

        while (elapsed < duration && !isDead)
        {
            yield return new WaitForSeconds(1f);
            elapsed += 1f;

            if (isDead) break;

            TakeDamage(damagePerTick);
            Debug.Log("Quái bị thiêu đốt, nhận " + damagePerTick + " sát thương.");
        }

        isBurning = false;
    }

    // --- BỔ SUNG: Hàm kích hoạt hiệu ứng Làm chậm từ Kỹ năng Băng ---
    public void ApplySlow(float slowPercent, float duration)
    {
        if (isDead) return;

        if (!isSlowed)
        {
            originalSpeed = speed;
        }

        if (slowCoroutine != null)
        {
            StopCoroutine(slowCoroutine);
        }

        slowCoroutine = StartCoroutine(SlowEffectRoutine(slowPercent, duration));
    }

    IEnumerator SlowEffectRoutine(float slowPercent, float duration)
    {
        isSlowed = true;
        speed = originalSpeed * slowPercent; // Giảm tốc độ theo tỉ lệ
        Debug.Log("Quái bị làm chậm!");

        yield return new WaitForSeconds(duration);

        speed = originalSpeed; // Hết giờ, hồi lại tốc độ cũ
        isSlowed = false;
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

                SetAnimatorBool("isRunning", false);
                break;
            }
        }
    }

    void GetNextWaypoint()
    {
        // Lấy danh sách waypoint hiện tại
        Transform[] currentWaypoints = customWaypoints != null && customWaypoints.Length > 0
            ? customWaypoints
            : Waypoints.points;

        if (currentWaypoints == null || currentWaypoints.Length == 0) return;

        if (waypointIndex >= currentWaypoints.Length)
        {
            ReachDestination();
            return;
        }

        targetWaypoint = currentWaypoints[waypointIndex];
        waypointIndex++;
    }

    void MoveTowardsWaypoint()
    {
        if (targetWaypoint == null) return;

        // Lấy danh sách waypoint hiện tại
        Transform[] currentWaypoints = customWaypoints != null && customWaypoints.Length > 0
            ? customWaypoints
            : Waypoints.points;

        Vector3 dir = targetWaypoint.position - transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

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

    void ReachDestination()
    {
        GameUI ui = FindFirstObjectByType<GameUI>();

        if (ui != null)
        {
            ui.TakeCastleDamage(1);
        }

        Destroy(gameObject);
    }

    public void TakeDamage(int amt)
    {
        Debug.Log(
            "🔥 ENEMY NHẬN DAMAGE: " +
            amt +
            " | HP trước: " +
            hp
        );

        if (isDead)
            return;

        hp -= amt;

        Debug.Log(
            "🔥 ENEMY HP SAU KHI BỊ ĐÁNH: " +
            hp
        );

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
        if (GameUI.instance != null)
        {
            GameUI.instance.enemiesKilled++;
        }

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

        if (GameUI.instance != null)
        {
            GameUI.instance.AddGold(1);
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

        SetAnimatorTrigger("Attack");

        if (currentTargetSoldier is PlayerSoldier w) w.TakeDamage(damage);
        else if (currentTargetSoldier is MageSoldier m) m.TakeDamage(damage);
        else if (currentTargetSoldier is ArcherSoldier a) a.TakeDamage(damage);
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
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}