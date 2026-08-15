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

    // Mục tiêu hiện tại
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

        GetNextWaypoint();
    }


    // =========================================================
    // ĐỘ KHÓ MAP 3
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


        // =====================================================
        // WAVE 1
        // NHẸ HƠN
        // =====================================================

        switch (waveNumber)
        {
            case 1:

                speed = 2.0f;
                maxHp = 18;
                damage = 2;
                attackCooldown = 1.2f;

                break;


            case 2:

                speed = 2.1f;
                maxHp = 20;
                damage = 3;
                attackCooldown = 1.1f;

                break;


            case 3:

                speed = 2.2f;
                maxHp = 23;
                damage = 3;
                attackCooldown = 1.1f;

                break;


            case 4:

                speed = 2.3f;
                maxHp = 26;
                damage = 4;
                attackCooldown = 1.0f;

                break;


            case 5:

                speed = 2.4f;
                maxHp = 30;
                damage = 4;
                attackCooldown = 1.0f;

                break;


            case 6:

                speed = 2.5f;
                maxHp = 34;
                damage = 5;
                attackCooldown = 1.0f;

                break;


            case 7:

                speed = 2.6f;
                maxHp = 38;
                damage = 5;
                attackCooldown = 0.95f;

                break;


            case 8:

                speed = 2.7f;
                maxHp = 42;
                damage = 6;
                attackCooldown = 0.95f;

                break;


            case 9:

                speed = 2.8f;
                maxHp = 46;
                damage = 6;
                attackCooldown = 0.9f;

                break;


            case 10:

                speed = 2.9f;
                maxHp = 50;
                damage = 7;
                attackCooldown = 0.9f;

                break;


            case 11:

                speed = 3.0f;
                maxHp = 55;
                damage = 7;
                attackCooldown = 0.85f;

                break;


            case 12:

                speed = 3.1f;
                maxHp = 60;
                damage = 8;
                attackCooldown = 0.85f;

                break;
        }


        hp = maxHp;

        originalSpeed = speed;


        Debug.Log(
            "🔥 MAP 3 - WAVE " +
            waveNumber +
            " | Speed: " +
            speed +
            " | HP: " +
            maxHp +
            " | Damage: " +
            damage
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (isDead)
            return;


        // =====================================================
        // ĐANG ĐÁNH LÍNH
        // =====================================================

        if (isEngaged)
        {
            CheckSoldierAlive();

            return;
        }


        // =====================================================
        // KIỂM TRA LÍNH PHÍA TRƯỚC
        // =====================================================

        CheckForSoldiersAhead();


        // =====================================================
        // NẾU VỪA PHÁT HIỆN LÍNH
        // KHÔNG ĐƯỢC CHẠY TIẾP
        // =====================================================

        if (isEngaged)
        {
            return;
        }


        // =====================================================
        // KHÔNG CÓ LÍNH → TIẾP TỤC ĐI
        // =====================================================

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


        // =====================================================
        // TÌM MỤC TIÊU
        // =====================================================

        PlayerSoldier warrior = null;
        MageSoldier mage = null;
        ArcherSoldier archer = null;


        foreach (Collider2D hit in hits)
        {
            if (hit == null)
                continue;


            // Không lấy chính enemy
            if (hit.transform == transform)
                continue;


            // =================================================
            // TÌM ĐẤU SĨ
            // =================================================

            PlayerSoldier w =
                hit.GetComponent<PlayerSoldier>();

            if (w != null)
            {
                warrior = w;
            }


            // =================================================
            // TÌM PHÁP SƯ
            // =================================================

            MageSoldier m =
                hit.GetComponent<MageSoldier>();

            if (m != null)
            {
                mage = m;
            }


            // =================================================
            // TÌM CUNG THỦ
            // =================================================

            ArcherSoldier a =
                hit.GetComponent<ArcherSoldier>();

            if (a != null)
            {
                archer = a;
            }
        }


        // =====================================================
        // MAP 3
        // ƯU TIÊN ĐẤU SĨ
        // SAU ĐÓ PHÁP SƯ
        // SAU ĐÓ CUNG THỦ
        // =====================================================

        if (IsMap3())
        {
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


            return;
        }


        // =====================================================
        // MAP 1 + MAP 2
        // GIỮ NGUYÊN CƠ CHẾ CŨ
        // =====================================================

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
    // BẮT ĐẦU ĐÁNH MỤC TIÊU
    // =========================================================

    void EngageTarget(MonoBehaviour target)
    {
        if (target == null)
            return;


        currentTargetSoldier = target;

        isEngaged = true;


        // Dừng animation chạy
        SetAnimatorBool(
            "isRunning",
            false
        );


        // Flip về phía mục tiêu
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
            Mathf.RoundToInt(dps);


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


        speed = originalSpeed;

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


        if (dir.x != 0)
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


        // Xóa mục tiêu
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
    // KIỂM TRA MỤC TIÊU CÒN SỐNG
    // =========================================================

    void CheckSoldierAlive()
    {
        // Không còn mục tiêu
        if (currentTargetSoldier == null)
        {
            ResumeMoving();

            return;
        }


        // GameObject bị tắt
        if (
            !currentTargetSoldier.gameObject
                .activeInHierarchy
        )
        {
            ResumeMoving();

            return;
        }


        // =====================================================
        // KIỂM TRA TỪNG LOẠI LÍNH
        // =====================================================

        PlayerSoldier warrior =
            currentTargetSoldier
            as PlayerSoldier;


        MageSoldier mage =
            currentTargetSoldier
            as MageSoldier;


        ArcherSoldier archer =
            currentTargetSoldier
            as ArcherSoldier;


        bool targetAlive = false;


        if (warrior != null)
        {
            targetAlive = true;
        }


        if (mage != null)
        {
            targetAlive = true;
        }


        if (archer != null)
        {
            targetAlive = true;
        }


        // Không còn đúng loại mục tiêu
        if (!targetAlive)
        {
            ResumeMoving();

            return;
        }


        // =====================================================
        // FLIP VỀ PHÍA MỤC TIÊU
        // =====================================================

        float directionX =
            currentTargetSoldier.transform.position.x -
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


        // =====================================================
        // ĐẤU SĨ
        // =====================================================

        PlayerSoldier warrior =
            currentTargetSoldier
            as PlayerSoldier;


        if (warrior != null)
        {
            warrior.TakeDamage(
                damage
            );

            return;
        }


        // =====================================================
        // PHÁP SƯ
        // =====================================================

        MageSoldier mage =
            currentTargetSoldier
            as MageSoldier;


        if (mage != null)
        {
            mage.TakeDamage(
                damage
            );

            return;
        }


        // =====================================================
        // CUNG THỦ
        // =====================================================

        ArcherSoldier archer =
            currentTargetSoldier
            as ArcherSoldier;


        if (archer != null)
        {
            archer.TakeDamage(
                damage
            );

            return;
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