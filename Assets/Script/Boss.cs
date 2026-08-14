using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BossEnemy : MonoBehaviour
{
    [Header("--- CẤU HÌNH BOSS ---")]
    public string bossName = "Chúa Tể Orc";
    public float speed = 2f;
    public int maxHp = 200;
    private int hp;
    public int damage = 10;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    [Header("Phạm vi phát hiện lính")]
    public float checkRadius = 1.2f;

    private Transform targetWaypoint;
    private int waypointIndex = 0;

    private bool isEngaged = false;
    private bool isDead = false;
    private MonoBehaviour currentTargetSoldier;

    [Header("--- THANH MÁU BOSS (UI) ---")]
    public Slider healthSlider;

    [Header("--- CẤU HÌNH PHẦN THƯỞNG ---")]
    public GameObject coinPrefab;
    public int goldReward = 10;

    [Header("--- HIỆU ỨNG TRẠNG THÁI ---")]
    private bool isBurning = false;
    private Coroutine burnCoroutine;

    private float originalSpeed;
    private bool isSlowed = false;
    private Coroutine slowCoroutine;

    [Header("--- CƠ CHẾ NỔI GIẬN (RAGE PHASE) ---")]
    [Range(0.1f, 0.9f)]
    public float rageThreshold = 0.5f;
    private bool isRaging = false;
    public float rageSpeedMultiplier = 1.3f;
    public float rageDamageMultiplier = 1.5f;

    private Animator animator;
    private Collider2D col;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // --- Biến quản lý waypoint tùy chỉnh cho Boss ---
    [HideInInspector]
    public Transform[] customWaypoints;

    void Start()
    {
        hp = maxHp;
        originalSpeed = speed;
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

        CheckRagePhase();

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
        // Đã xóa ApplySeparation() để Boss không bị đẩy cưỡng bức
    }

    void CheckRagePhase()
    {
        if (!isRaging && hp <= maxHp * rageThreshold)
        {
            isRaging = true;
            if (!isSlowed)
            {
                originalSpeed *= rageSpeedMultiplier;
                speed = originalSpeed;
            }
            damage = Mathf.RoundToInt(damage * rageDamageMultiplier);

            Debug.Log(bossName + " ĐÃ NỔI GIẬN! Tốc độ và sát thương tăng mạnh!");
            SetAnimatorTrigger("Rage");
        }
    }

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
            Debug.Log(bossName + " bị thiêu đốt, nhận " + damagePerTick + " sát thương.");
        }

        isBurning = false;
    }

    public void ApplySlow(float slowPercent, float duration)
    {
        if (isDead) return;

        float baseSpd = isRaging ? (originalSpeed / rageSpeedMultiplier) : originalSpeed;

        if (!isSlowed)
        {
            originalSpeed = baseSpd;
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
        speed = (isRaging ? originalSpeed * rageSpeedMultiplier : originalSpeed) * slowPercent;
        Debug.Log(bossName + " bị làm chậm!");

        yield return new WaitForSeconds(duration);

        speed = isRaging ? originalSpeed * rageSpeedMultiplier : originalSpeed;
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
            ui.TakeCastleDamage(5);
        }

        Destroy(gameObject);
    }

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
            for (int i = 0; i < 3; i++)
            {
                Instantiate(coinPrefab, transform.position + (Vector3)Random.insideUnitCircle * 0.5f, Quaternion.identity);
            }
        }

        if (GameUI.instance != null)
        {
            GameUI.instance.AddGold(goldReward);
        }

        Destroy(gameObject, 2.0f);
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
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}