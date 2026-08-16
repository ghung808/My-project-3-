using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class Enemy : MonoBehaviour
{
    [Header("Cấu hình Chỉ số")]
    public float speed = 2.5f;
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

    // =========================================================
    // BURN
    // =========================================================

    [Header("Hiệu ứng Trạng thái (Đốt)")]
    private bool isBurning = false;
    private Coroutine burnCoroutine;

    // =========================================================
    // SLOW
    // =========================================================

    [Header("Hiệu ứng Trạng thái (Làm chậm)")]
    private float originalSpeed;
    private bool isSlowed = false;
    private Coroutine slowCoroutine;

    // =========================================================
    // COMPONENT
    // =========================================================

    private Animator animator;
    private Collider2D col;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // =========================================================
    // MAP 3 WAYPOINT
    // =========================================================

    [HideInInspector]
    public Transform[] customWaypoints;

    // =========================================================
    // KIỂM TRA MAP 3
    // =========================================================

    bool IsMap3()
    {
        return SceneManager.GetActiveScene().name == "dh";
    }

    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }

        // =====================================================
        // MAP 3
        // =====================================================

        if (IsMap3())
        {
            ApplyMap3Difficulty();
        }
        else
        {
            // MAP 1 + MAP 2
            hp = maxHp;
            originalSpeed = speed;
        }

        UpdateHealthUI();

        // =====================================================
        // QUAN TRỌNG:
        // NẾU ĐÃ ĐƯỢC GÁN CUSTOM WAYPOINT
        // THÌ KHÔNG ĐƯỢC DÙNG Waypoints.points
        // =====================================================

        if (customWaypoints != null &&
            customWaypoints.Length > 0)
        {
            waypointIndex = 1;

            if (customWaypoints.Length > 1)
            {
                targetWaypoint =
                    customWaypoints[1];
            }
            else
            {
                targetWaypoint =
                    customWaypoints[0];
            }

            Debug.Log(
                "🛣️ ENEMY MAP 3 KHỞI TẠO | " +
                "Spawn: " +
                customWaypoints[0].name +
                " | Next: " +
                targetWaypoint.name
            );
        }
        else
        {
            // MAP 1 + MAP 2
            GetNextWaypoint();
        }
    }

    // =========================================================
    // KHỞI TẠO ĐƯỜNG MAP 3
    // =========================================================

    public void InitializeMap3Path()
    {
        if (customWaypoints == null ||
            customWaypoints.Length == 0)
        {
            Debug.LogError(
                "❌ ENEMY MAP 3 KHÔNG CÓ WAYPOINT!"
            );

            return;
        }

        waypointIndex = 1;

        if (customWaypoints.Length > 1)
        {
            targetWaypoint =
                customWaypoints[1];
        }
        else
        {
            targetWaypoint =
                customWaypoints[0];
        }

        isEngaged = false;
        isDead = false;

        Debug.Log(
            "✅ ENEMY MAP 3 NHẬN ĐƯỜNG | " +
            "Spawn: " +
            customWaypoints[0].name +
            " | Next: " +
            targetWaypoint.name
        );
    }

    // =========================================================
    // CÂN BẰNG ENEMY MAP 3
    // =========================================================

    void ApplyMap3Difficulty()
    {
        WaveSpawnerMap3 spawner =
            FindFirstObjectByType<WaveSpawnerMap3>();

        int waveNumber = 1;

        if (spawner != null)
        {
            waveNumber =
                spawner.GetCurrentWaveNumber();
        }

        switch (waveNumber)
        {
            case 1:

                speed = 2.0f;
                maxHp = 20;
                damage = 2;
                attackCooldown = 1.2f;

                break;

            case 2:

                speed = 2.05f;
                maxHp = 22;
                damage = 2;
                attackCooldown = 1.15f;

                break;

            case 3:

                speed = 2.1f;
                maxHp = 25;
                damage = 3;
                attackCooldown = 1.1f;

                break;

            case 4:

                speed = 2.15f;
                maxHp = 28;
                damage = 3;
                attackCooldown = 1.05f;

                break;

            case 5:

                speed = 2.2f;
                maxHp = 32;
                damage = 3;
                attackCooldown = 1.0f;

                break;

            case 6:

                speed = 2.25f;
                maxHp = 36;
                damage = 4;
                attackCooldown = 1.0f;

                break;

            case 7:

                speed = 2.3f;
                maxHp = 40;
                damage = 4;
                attackCooldown = 0.95f;

                break;

            case 8:

                speed = 2.35f;
                maxHp = 45;
                damage = 5;
                attackCooldown = 0.95f;

                break;

            case 9:

                speed = 2.4f;
                maxHp = 50;
                damage = 5;
                attackCooldown = 0.9f;

                break;

            case 10:

                speed = 2.45f;
                maxHp = 56;
                damage = 6;
                attackCooldown = 0.9f;

                break;

            case 11:

                speed = 2.5f;
                maxHp = 62;
                damage = 6;
                attackCooldown = 0.85f;

                break;

            case 12:

                speed = 2.55f;
                maxHp = 70;
                damage = 7;
                attackCooldown = 0.85f;

                break;

            default:

                speed = 2.55f;
                maxHp = 70;
                damage = 7;
                attackCooldown = 0.85f;

                break;
        }

        hp = maxHp;
        originalSpeed = speed;

        Debug.Log(
            "🔥 MAP 3 - WAVE " +
            waveNumber +
            " | HP: " +
            maxHp +
            " | Damage: " +
            damage +
            " | Speed: " +
            speed +
            " | Cooldown: " +
            attackCooldown
        );
    }

    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (isDead)
            return;

        if (isEngaged)
        {
            CheckSoldierAlive();
            return;
        }

        CheckForSoldiersAhead();

        if (isEngaged)
            return;

        MoveTowardsWaypoint();
    }

    // =========================================================
    // KIỂM TRA LÍNH
    // =========================================================

    void CheckForSoldiersAhead()
    {
        Collider2D[] hits =
            Physics2D.OverlapCircleAll(
                transform.position,
                checkRadius
            );

        PlayerSoldier warrior = null;
        MageSoldier mage = null;
        ArcherSoldier archer = null;

        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;

            if (hit.transform == transform)
                continue;

            PlayerSoldier w =
                hit.GetComponent<PlayerSoldier>();

            if (w != null)
            {
                warrior = w;
            }

            MageSoldier m =
                hit.GetComponent<MageSoldier>();

            if (m != null)
            {
                mage = m;
            }

            ArcherSoldier a =
                hit.GetComponent<ArcherSoldier>();

            if (a != null)
            {
                archer = a;
            }
        }

        if (warrior != null)
        {
            EngageTarget(warrior);
            return;
        }

        if (mage != null)
        {
            EngageTarget(mage);
            return;
        }

        if (archer != null)
        {
            EngageTarget(archer);
            return;
        }
    }

    // =========================================================
    // BẮT ĐẦU ĐÁNH
    // =========================================================

    void EngageTarget(MonoBehaviour target)
    {
        if (target == null)
            return;

        currentTargetSoldier = target;

        isEngaged = true;

        SetAnimatorBool(
            "isRunning",
            false
        );

        float directionX =
            target.transform.position.x -
            transform.position.x;

        FlipSprite(directionX);

        Debug.Log(
            "⚔️ ENEMY DỪNG LẠI ĐÁNH: " +
            target.gameObject.name
        );
    }

    // =========================================================
    // BURN
    // =========================================================

    public void StartBurn(
        float dps,
        float duration
    )
    {
        if (isDead)
            return;

        if (
            isBurning &&
            burnCoroutine != null
        )
        {
            StopCoroutine(
                burnCoroutine
            );
        }

        burnCoroutine =
            StartCoroutine(
                BurnEffectRoutine(
                    dps,
                    duration
                )
            );
    }

    IEnumerator BurnEffectRoutine(
        float dps,
        float duration
    )
    {
        isBurning = true;

        float elapsed = 0f;

        int damagePerTick =
            Mathf.Max(
                1,
                Mathf.RoundToInt(dps)
            );

        while (
            elapsed < duration &&
            !isDead
        )
        {
            yield return new WaitForSeconds(1f);

            elapsed += 1f;

            if (isDead)
                break;

            TakeDamage(
                damagePerTick
            );
        }

        isBurning = false;
    }

    // =========================================================
    // SLOW
    // =========================================================

    public void ApplySlow(
        float slowPercent,
        float duration
    )
    {
        if (isDead)
            return;

        if (!isSlowed)
        {
            originalSpeed = speed;
        }

        if (slowCoroutine != null)
        {
            StopCoroutine(
                slowCoroutine
            );
        }

        slowCoroutine =
            StartCoroutine(
                SlowEffectRoutine(
                    slowPercent,
                    duration
                )
            );
    }

    IEnumerator SlowEffectRoutine(
        float slowPercent,
        float duration
    )
    {
        isSlowed = true;

        speed =
            originalSpeed *
            slowPercent;

        yield return new WaitForSeconds(
            duration
        );

        if (!isDead)
        {
            speed = originalSpeed;
        }

        isSlowed = false;
    }

    // =========================================================
    // WAYPOINT
    // =========================================================

    void GetNextWaypoint()
    {
        Transform[] currentWaypoints =
            customWaypoints != null &&
            customWaypoints.Length > 0
                ? customWaypoints
                : Waypoints.points;

        if (
            currentWaypoints == null ||
            currentWaypoints.Length == 0
        )
        {
            Debug.LogError(
                "❌ ENEMY KHÔNG CÓ WAYPOINT!"
            );

            return;
        }

        if (
            waypointIndex >=
            currentWaypoints.Length
        )
        {
            ReachDestination();
            return;
        }

        targetWaypoint =
            currentWaypoints[
                waypointIndex
            ];

        waypointIndex++;
    }

    // =========================================================
    // DI CHUYỂN
    // =========================================================

    void MoveTowardsWaypoint()
    {
        if (targetWaypoint == null)
        {
            Debug.LogWarning(
                "⚠️ Enemy không có Target Waypoint!"
            );

            return;
        }

        Vector3 dir =
            targetWaypoint.position -
            transform.position;

        transform.Translate(
            dir.normalized *
            speed *
            Time.deltaTime,
            Space.World
        );

        if (Mathf.Abs(dir.x) > 0.01f)
        {
            FlipSprite(dir.x);
        }

        SetAnimatorBool(
            "isRunning",
            true
        );

        if (
            Vector3.Distance(
                transform.position,
                targetWaypoint.position
            ) < 0.2f
        )
        {
            GetNextWaypoint();
        }
    }

    // =========================================================
    // ĐẾN CASTLE
    // =========================================================

    void ReachDestination()
    {
        GameUI ui =
            FindFirstObjectByType<GameUI>();

        if (ui != null)
        {
            ui.TakeCastleDamage(1);
        }

        Destroy(gameObject);
    }

    // =========================================================
    // NHẬN DAMAGE
    // =========================================================

    public void TakeDamage(int amt)
    {
        if (isDead)
            return;

        hp -= amt;

        hp =
            Mathf.Max(
                hp,
                0
            );

        UpdateHealthUI();

        if (hp > 0)
        {
            SetAnimatorTrigger(
                "Hurt"
            );
        }
        else
        {
            Die();
        }
    }

    // =========================================================
    // HEALTH UI
    // =========================================================

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue =
                maxHp;

            healthSlider.value =
                hp;
        }
    }

    // =========================================================
    // CHẾT
    // =========================================================

    void Die()
    {
        if (isDead)
            return;

        isDead = true;

        currentTargetSoldier = null;
        isEngaged = false;

        if (GameUI.instance != null)
        {
            GameUI.instance.enemiesKilled++;
        }

        if (col != null)
        {
            col.enabled = false;
        }

        if (rb != null)
        {
            rb.simulated = false;
        }

        SetAnimatorTrigger(
            "Die"
        );

        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(false);
        }

        if (coinPrefab != null)
        {
            Instantiate(
                coinPrefab,
                transform.position,
                Quaternion.identity
            );
        }

        if (GameUI.instance != null)
        {
            GameUI.instance.AddGold(1);
        }

        Destroy(
            gameObject,
            1f
        );
    }

    // =========================================================
    // KIỂM TRA MỤC TIÊU
    // =========================================================

    void CheckSoldierAlive()
    {
        if (currentTargetSoldier == null)
        {
            ResumeMoving();
            return;
        }

        if (
            !currentTargetSoldier.gameObject
                .activeInHierarchy
        )
        {
            ResumeMoving();
            return;
        }

        bool targetAlive = false;

        if (
            currentTargetSoldier
            is PlayerSoldier
        )
        {
            targetAlive = true;
        }

        if (
            currentTargetSoldier
            is MageSoldier
        )
        {
            targetAlive = true;
        }

        if (
            currentTargetSoldier
            is ArcherSoldier
        )
        {
            targetAlive = true;
        }

        if (!targetAlive)
        {
            ResumeMoving();
            return;
        }

        float directionX =
            currentTargetSoldier.transform.position.x -
            transform.position.x;

        FlipSprite(directionX);

        if (
            Time.time >=
            lastAttackTime +
            attackCooldown
        )
        {
            AttackSoldier();

            lastAttackTime =
                Time.time;
        }
    }

    // =========================================================
    // QUAY LẠI CHẠY
    // =========================================================

    void ResumeMoving()
    {
        currentTargetSoldier = null;

        isEngaged = false;

        SetAnimatorBool(
            "isRunning",
            true
        );
    }

    // =========================================================
    // ĐÁNH LÍNH
    // =========================================================

    void AttackSoldier()
    {
        if (currentTargetSoldier == null)
            return;

        SetAnimatorTrigger(
            "Attack"
        );

        if (
            currentTargetSoldier
            is PlayerSoldier warrior
        )
        {
            warrior.TakeDamage(damage);
            return;
        }

        if (
            currentTargetSoldier
            is MageSoldier mage
        )
        {
            mage.TakeDamage(damage);
            return;
        }

        if (
            currentTargetSoldier
            is ArcherSoldier archer
        )
        {
            archer.TakeDamage(damage);
        }
    }

    // =========================================================
    // FLIP
    // =========================================================

    void FlipSprite(
        float directionX
    )
    {
        if (
            spriteRenderer != null &&
            Mathf.Abs(directionX) > 0.01f
        )
        {
            spriteRenderer.flipX =
                directionX < 0;
        }
    }

    // =========================================================
    // ANIMATION TRIGGER
    // =========================================================

    void SetAnimatorTrigger(
        string triggerName
    )
    {
        if (
            animator != null &&
            animator.runtimeAnimatorController != null
        )
        {
            foreach (
                AnimatorControllerParameter param
                in animator.parameters
            )
            {
                if (
                    param.name == triggerName &&
                    param.type ==
                    AnimatorControllerParameterType.Trigger
                )
                {
                    animator.SetTrigger(
                        triggerName
                    );

                    return;
                }
            }
        }
    }

    // =========================================================
    // ANIMATION BOOL
    // =========================================================

    void SetAnimatorBool(
        string boolName,
        bool value
    )
    {
        if (
            animator != null &&
            animator.runtimeAnimatorController != null
        )
        {
            foreach (
                AnimatorControllerParameter param
                in animator.parameters
            )
            {
                if (
                    param.name == boolName &&
                    param.type ==
                    AnimatorControllerParameterType.Bool
                )
                {
                    animator.SetBool(
                        boolName,
                        value
                    );

                    return;
                }
            }
        }
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            checkRadius
        );
    }
}