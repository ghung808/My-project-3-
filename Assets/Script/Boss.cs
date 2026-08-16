using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BossEnemy : MonoBehaviour
{
    [Header("--- CẤU HÌNH BOSS ---")]
    public string bossName = "Chúa Tể Orc";

    // =========================================================
    // CHỈ SỐ CÂN BẰNG
    // =========================================================

    [Header("--- CHỈ SỐ CÂN BẰNG ---")]

    // Boss chậm hơn Enemy thường
    // nhưng HP cao hơn rất nhiều
    public float speed = 1.6f;

    // HP Boss
    public int maxHp = 500;
    private int hp;

    // Damage vừa phải
    public int damage = 6;

    // Cứ 1.5 giây đánh 1 lần
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    [Header("Phạm vi phát hiện lính")]
    public float checkRadius = 1.4f;

    [Header("Khoảng cách duy trì đánh")]
    public float attackRange = 1.3f;

    private Transform targetWaypoint;
    private int waypointIndex = 0;

    private bool isEngaged = false;
    private bool isDead = false;

    private MonoBehaviour currentTargetSoldier;

    [Header("--- THANH MÁU BOSS (UI) ---")]
    public Slider healthSlider;

    [Header("--- PHẦN THƯỞNG ---")]
    public GameObject coinPrefab;

    // Boss chết được 10 vàng
    public int goldReward = 10;

    // =========================================================
    // BURN
    // =========================================================

    [Header("--- HIỆU ỨNG ĐỐT ---")]

    private bool isBurning = false;
    private Coroutine burnCoroutine;

    // =========================================================
    // SLOW
    // =========================================================

    [Header("--- HIỆU ỨNG LÀM CHẬM ---")]

    private float originalSpeed;
    private bool isSlowed = false;
    private Coroutine slowCoroutine;

    // =========================================================
    // RAGE
    // =========================================================

    [Header("--- RAGE PHASE ---")]

    // Boss bắt đầu Rage khi còn 40% HP
    [Range(0.1f, 0.9f)]
    public float rageThreshold = 0.4f;

    private bool isRaging = false;

    // Rage tăng tốc 20%
    public float rageSpeedMultiplier = 1.2f;

    // Rage tăng damage 20%
    public float rageDamageMultiplier = 1.2f;

    private int baseDamage;

    // =========================================================
    // COMPONENT
    // =========================================================

    private Animator animator;
    private Collider2D col;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // =========================================================
    // WAYPOINT MAP 3
    // =========================================================

    [HideInInspector]
    public Transform[] customWaypoints;

    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Boss không chịu trọng lực
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
        }

        // Khởi tạo HP
        hp = maxHp;

        // Lưu speed gốc
        originalSpeed = speed;

        // Lưu damage gốc
        baseDamage = damage;

        // Cập nhật thanh máu
        UpdateHealthUI();

        // Lấy waypoint đầu tiên
        GetNextWaypoint();

        Debug.Log(
            "👑 BOSS SPAWN | " +
            bossName +
            " | HP: " +
            maxHp +
            " | Damage: " +
            damage +
            " | Speed: " +
            speed +
            " | AttackCooldown: " +
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

        // Kiểm tra Rage
        CheckRagePhase();

        // =====================================================
        // ĐANG ĐÁNH LÍNH
        // =====================================================

        if (isEngaged)
        {
            CheckSoldierAlive();
            return;
        }

        // =====================================================
        // TÌM LÍNH
        // =====================================================

        CheckForSoldiersAhead();

        // Nếu tìm thấy lính thì dừng
        if (isEngaged)
            return;

        // Không có lính → tiếp tục đi
        MoveTowardsWaypoint();
    }

    // =========================================================
    // RAGE
    // =========================================================

    void CheckRagePhase()
    {
        // Đã Rage thì không Rage lần nữa
        if (isRaging)
            return;

        // Còn 40% HP → Rage
        if (hp <= maxHp * rageThreshold)
        {
            isRaging = true;

            // Tăng tốc 20%
            speed =
                originalSpeed *
                rageSpeedMultiplier;

            // Tăng damage 20%
            damage =
                Mathf.RoundToInt(
                    baseDamage *
                    rageDamageMultiplier
                );

            Debug.Log(
                "🔥 " +
                bossName +
                " NỔI GIẬN! " +
                "HP còn " +
                hp +
                "/" +
                maxHp +
                " | Damage: " +
                damage +
                " | Speed: " +
                speed
            );

            SetAnimatorTrigger("Rage");
        }
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

        // Nếu đang slow thì reset coroutine
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

        float rageMultiplier =
            isRaging
                ? rageSpeedMultiplier
                : 1f;

        speed =
            originalSpeed *
            rageMultiplier *
            slowPercent;

        yield return new WaitForSeconds(
            duration
        );

        if (!isDead)
        {
            float currentRageMultiplier =
                isRaging
                    ? rageSpeedMultiplier
                    : 1f;

            speed =
                originalSpeed *
                currentRageMultiplier;
        }

        isSlowed = false;
    }

    // =========================================================
    // PHÁT HIỆN LÍNH
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

            // =================================================
            // ĐẤU SĨ
            // =================================================

            PlayerSoldier w =
                hit.GetComponent<PlayerSoldier>();

            if (w != null)
            {
                warrior = w;
                continue;
            }

            // =================================================
            // PHÁP SƯ
            // =================================================

            MageSoldier m =
                hit.GetComponent<MageSoldier>();

            if (m != null)
            {
                mage = m;
                continue;
            }

            // =================================================
            // CUNG THỦ
            // =================================================

            ArcherSoldier a =
                hit.GetComponent<ArcherSoldier>();

            if (a != null)
            {
                archer = a;
            }
        }

        // =====================================================
        // ƯU TIÊN ĐẤU SĨ
        // =====================================================

        if (warrior != null)
        {
            EngageTarget(warrior);
            return;
        }

        // =====================================================
        // SAU ĐÓ PHÁP SƯ
        // =====================================================

        if (mage != null)
        {
            EngageTarget(mage);
            return;
        }

        // =====================================================
        // CUỐI CÙNG CUNG THỦ
        // =====================================================

        if (archer != null)
        {
            EngageTarget(archer);
        }
    }

    // =========================================================
    // BẮT ĐẦU ĐÁNH
    // =========================================================

    void EngageTarget(
        MonoBehaviour target
    )
    {
        if (target == null)
            return;

        currentTargetSoldier =
            target;

        isEngaged = true;

        SetAnimatorBool(
            "isRunning",
            false
        );

        float directionX =
            target.transform.position.x -
            transform.position.x;

        FlipSprite(directionX);
    }

    // =========================================================
    // KIỂM TRA LÍNH
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

        // =====================================================
        // KIỂM TRA ĐÚNG LOẠI LÍNH
        // =====================================================

        bool validTarget = false;

        if (
            currentTargetSoldier
            is PlayerSoldier
        )
        {
            validTarget = true;
        }

        if (
            currentTargetSoldier
            is MageSoldier
        )
        {
            validTarget = true;
        }

        if (
            currentTargetSoldier
            is ArcherSoldier
        )
        {
            validTarget = true;
        }

        if (!validTarget)
        {
            ResumeMoving();
            return;
        }

        // =====================================================
        // KIỂM TRA KHOẢNG CÁCH
        // =====================================================

        float distance =
            Vector2.Distance(
                transform.position,
                currentTargetSoldier
                    .transform.position
            );

        // Lính chạy ra xa → Boss tiếp tục di chuyển
        if (distance > attackRange)
        {
            ResumeMoving();
            return;
        }

        // =====================================================
        // QUAY VỀ PHÍA LÍNH
        // =====================================================

        float directionX =
            currentTargetSoldier
                .transform.position.x -
            transform.position.x;

        FlipSprite(directionX);

        // =====================================================
        // ĐÁNH
        // =====================================================

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
    // TIẾP TỤC DI CHUYỂN
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

        // =====================================================
        // ĐẤU SĨ
        // =====================================================

        if (
            currentTargetSoldier
            is PlayerSoldier warrior
        )
        {
            warrior.TakeDamage(
                damage
            );

            return;
        }

        // =====================================================
        // PHÁP SƯ
        // =====================================================

        if (
            currentTargetSoldier
            is MageSoldier mage
        )
        {
            mage.TakeDamage(
                damage
            );

            return;
        }

        // =====================================================
        // CUNG THỦ
        // =====================================================

        if (
            currentTargetSoldier
            is ArcherSoldier archer
        )
        {
            archer.TakeDamage(
                damage
            );
        }
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
                bossName +
                ": Không có Waypoint!"
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
            return;

        Vector3 dir =
            targetWaypoint.position -
            transform.position;

        transform.Translate(
            dir.normalized *
            speed *
            Time.deltaTime,
            Space.World
        );

        if (
            Mathf.Abs(dir.x) >
            0.01f
        )
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
            // Boss đập vào Castle gây 5 damage
            ui.TakeCastleDamage(5);
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

        amt =
            Mathf.Max(
                0,
                amt
            );

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

        Debug.Log(
            "👑 BOSS ĐÃ BỊ TIÊU DIỆT!"
        );

        // =====================================================
        // TĂNG SỐ QUÁI GIẾT
        // =====================================================

        if (GameUI.instance != null)
        {
            GameUI.instance.enemiesKilled++;
        }

        // =====================================================
        // TẮT COLLIDER
        // =====================================================

        if (col != null)
        {
            col.enabled = false;
        }

        // =====================================================
        // TẮT PHYSICS
        // =====================================================

        if (rb != null)
        {
            rb.simulated = false;
        }

        // =====================================================
        // ANIMATION
        // =====================================================

        SetAnimatorTrigger(
            "Die"
        );

        // =====================================================
        // TẮT THANH MÁU
        // =====================================================

        if (healthSlider != null)
        {
            healthSlider.gameObject.SetActive(
                false
            );
        }

        // =====================================================
        // COIN
        // =====================================================

        if (coinPrefab != null)
        {
            for (int i = 0; i < 3; i++)
            {
                Instantiate(
                    coinPrefab,
                    transform.position +
                    (Vector3)
                    Random.insideUnitCircle *
                    0.5f,
                    Quaternion.identity
                );
            }
        }

        // =====================================================
        // GOLD
        // =====================================================

        if (GameUI.instance != null)
        {
            GameUI.instance.AddGold(
                goldReward
            );
        }

        // =====================================================
        // THẮNG GAME
        // =====================================================

        if (GameUI.instance != null)
        {
            GameUI.instance.WinGame();
        }

        // =====================================================
        // HỦY BOSS
        // =====================================================

        Destroy(
            gameObject,
            2f
        );
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
            Mathf.Abs(directionX) >
            0.01f
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
        // Vùng phát hiện
        Gizmos.color =
            Color.red;

        Gizmos.DrawWireSphere(
            transform.position,
            checkRadius
        );

        // Vùng đánh
        Gizmos.color =
            Color.yellow;

        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}