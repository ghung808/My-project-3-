using UnityEngine;
using UnityEngine.UI;

public class PlayerSoldier : MonoBehaviour
{
    // =========================================================
    // MOVEMENT
    // =========================================================

    [Header("Di chuyển")]
    public float speed = 3f;

    // =========================================================
    // ATTACK
    // =========================================================

    [Header("Tấn công")]
    public float attackRate = 1f;
    public float detectRadius = 4f;
    public float attackRange = 1.4f;

    // =========================================================
    // STATS
    // =========================================================

    [Header("Chỉ số")]
    public int maxHp = 20;
    public int currentHp = 20;
    public int damage = 2;

    // =========================================================
    // HEALTH BAR
    // =========================================================

    [Header("Thanh máu")]
    public Slider healthBarSlider;

    // =========================================================
    // RALLY
    // =========================================================

    [Header("Vị trí tập kết")]
    public Vector3 rallyPosition;

    [HideInInspector]
    public SpawnTower homeTower;

    // =========================================================
    // PRIVATE
    // =========================================================

    private float attackCountdown = 0f;

    private Transform targetEnemy;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool hasMovedFromRally = false;

    // =========================================================
    // INITIALIZE
    // =========================================================

    public void InitializeStats(
        int hp,
        int dmg,
        SpawnTower tower
    )
    {
        maxHp = Mathf.Max(1, hp);
        currentHp = maxHp;

        damage = Mathf.Max(1, dmg);

        homeTower = tower;

        UpdateHealthBarUI();
    }

    // =========================================================
    // SET RALLY
    // =========================================================

    public void SetRallyPosition(Vector3 newPos)
    {
        rallyPosition = newPos;

        transform.position = rallyPosition;

        hasMovedFromRally = false;

        targetEnemy = null;

        attackCountdown = 0f;
    }

    // =========================================================
    // FULL HEAL
    // =========================================================

    public void FullHeal()
    {
        currentHp = maxHp;

        UpdateHealthBarUI();
    }

    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        if (currentHp <= 0)
        {
            currentHp = maxHp;
        }

        UpdateHealthBarUI();

        if (rallyPosition == Vector3.zero)
        {
            rallyPosition = transform.position;
        }

        transform.position = rallyPosition;

        InvokeRepeating(
            nameof(FindEnemy),
            0f,
            0.2f
        );
    }

    // =========================================================
    // FIND ENEMY
    // =========================================================

    void FindEnemy()
    {
        // -----------------------------------------------------
        // Nếu mục tiêu hiện tại vẫn hợp lệ
        // -----------------------------------------------------

        if (IsTargetValid(targetEnemy))
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    targetEnemy.position
                );

            // Giữ mục tiêu hiện tại nếu vẫn còn trong
            // phạm vi phát hiện mở rộng.
            if (distance <= detectRadius * 1.5f)
            {
                return;
            }
        }

        // -----------------------------------------------------
        // Tìm mục tiêu mới
        // -----------------------------------------------------

        float shortestDistance = Mathf.Infinity;

        Transform nearestTarget = null;

        // =====================================================
        // ENEMY THƯỜNG
        // =====================================================

        Enemy[] enemies =
            FindObjectsByType<Enemy>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null)
                continue;

            if (!enemy.gameObject.activeInHierarchy)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    enemy.transform.position
                );

            if (distance <= detectRadius &&
                distance < shortestDistance)
            {
                shortestDistance = distance;

                nearestTarget = enemy.transform;
            }
        }

        // =====================================================
        // BOSS ENEMY
        // =====================================================

        BossEnemy[] bossEnemies =
            FindObjectsByType<BossEnemy>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (BossEnemy bossEnemy in bossEnemies)
        {
            if (bossEnemy == null)
                continue;

            if (!bossEnemy.gameObject.activeInHierarchy)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    bossEnemy.transform.position
                );

            if (distance <= detectRadius &&
                distance < shortestDistance)
            {
                shortestDistance = distance;

                nearestTarget = bossEnemy.transform;
            }
        }

        // =====================================================
        // BOSS CŨ
        // =====================================================

        Boss[] bosses =
            FindObjectsByType<Boss>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            );

        foreach (Boss boss in bosses)
        {
            if (boss == null)
                continue;

            if (!boss.gameObject.activeInHierarchy)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    boss.transform.position
                );

            if (distance <= detectRadius &&
                distance < shortestDistance)
            {
                shortestDistance = distance;

                nearestTarget = boss.transform;
            }
        }

        targetEnemy = nearestTarget;
    }

    // =========================================================
    // CHECK TARGET
    // =========================================================

    bool IsTargetValid(Transform target)
    {
        if (target == null)
            return false;

        if (!target.gameObject.activeInHierarchy)
            return false;

        return true;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        // -----------------------------------------------------
        // Target chết / bị Destroy
        // -----------------------------------------------------

        if (!IsTargetValid(targetEnemy))
        {
            targetEnemy = null;
        }

        // =====================================================
        // CÓ MỤC TIÊU
        // =====================================================

        if (targetEnemy != null)
        {
            HandleCombat();
        }
        else
        {
            ReturnToRally();
        }

        // =====================================================
        // COOLDOWN
        // =====================================================

        if (attackCountdown > 0f)
        {
            attackCountdown -= Time.deltaTime;
        }
    }

    // =========================================================
    // COMBAT
    // =========================================================

    void HandleCombat()
    {
        if (targetEnemy == null)
            return;

        float distanceToEnemy =
            Vector3.Distance(
                transform.position,
                targetEnemy.position
            );

        // -----------------------------------------------------
        // ENEMY CÒN XA
        // -----------------------------------------------------

        if (distanceToEnemy > attackRange)
        {
            MoveToEnemy();

            return;
        }

        // -----------------------------------------------------
        // ĐỦ GẦN → DỪNG
        // -----------------------------------------------------

        SetMovingAnimation(false);

        FaceTarget();

        // -----------------------------------------------------
        // TẤN CÔNG
        // -----------------------------------------------------

        if (attackCountdown <= 0f)
        {
            TriggerAttackAnimation();

            OnAttackHit();

            attackCountdown =
                1f / Mathf.Max(
                    attackRate,
                    0.01f
                );
        }
    }

    // =========================================================
    // MOVE TO ENEMY
    // =========================================================

    void MoveToEnemy()
    {
        if (targetEnemy == null)
            return;

        Vector3 direction =
            (
                targetEnemy.position -
                transform.position
            ).normalized;

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                targetEnemy.position,
                speed * Time.deltaTime
            );

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            FlipSprite(direction.x);
        }

        SetMovingAnimation(true);

        hasMovedFromRally = true;
    }

    // =========================================================
    // RETURN TO RALLY
    // =========================================================

    void ReturnToRally()
    {
        attackCountdown = 0f;

        if (!hasMovedFromRally)
        {
            transform.position = rallyPosition;

            SetMovingAnimation(false);

            return;
        }

        float distanceToRally =
            Vector3.Distance(
                transform.position,
                rallyPosition
            );

        // -----------------------------------------------------
        // Đã về đến rally
        // -----------------------------------------------------

        if (distanceToRally <= 0.05f)
        {
            transform.position = rallyPosition;

            hasMovedFromRally = false;

            SetMovingAnimation(false);

            return;
        }

        // -----------------------------------------------------
        // Di chuyển về rally
        // -----------------------------------------------------

        Vector3 direction =
            (
                rallyPosition -
                transform.position
            ).normalized;

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                rallyPosition,
                speed * Time.deltaTime
            );

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            FlipSprite(direction.x);
        }

        SetMovingAnimation(true);
    }

    // =========================================================
    // FACE TARGET
    // =========================================================

    void FaceTarget()
    {
        if (targetEnemy == null)
            return;

        Vector3 direction =
            targetEnemy.position -
            transform.position;

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            FlipSprite(direction.x);
        }
    }

    // =========================================================
    // ATTACK
    // =========================================================

    public void OnAttackHit()
    {
        if (targetEnemy == null)
            return;

        // =====================================================
        // ENEMY
        // =====================================================

        Enemy enemy =
            targetEnemy.GetComponent<Enemy>();

        if (enemy == null)
        {
            enemy =
                targetEnemy.GetComponentInParent<Enemy>();
        }

        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            return;
        }

        // =====================================================
        // BOSS ENEMY
        // =====================================================

        BossEnemy bossEnemy =
            targetEnemy.GetComponent<BossEnemy>();

        if (bossEnemy == null)
        {
            bossEnemy =
                targetEnemy.GetComponentInParent<BossEnemy>();
        }

        if (bossEnemy != null)
        {
            bossEnemy.TakeDamage(damage);

            return;
        }

        // =====================================================
        // BOSS CŨ
        // =====================================================

        Boss boss =
            targetEnemy.GetComponent<Boss>();

        if (boss == null)
        {
            boss =
                targetEnemy.GetComponentInParent<Boss>();
        }

        if (boss != null)
        {
            boss.TakeDamage(damage);

            return;
        }

        // Target không còn hợp lệ
        targetEnemy = null;
    }

    // =========================================================
    // MOVING ANIMATION
    // =========================================================

    void SetMovingAnimation(bool isMoving)
    {
        if (animator != null)
        {
            animator.SetBool(
                "isMoving",
                isMoving
            );
        }
    }

    // =========================================================
    // ATTACK ANIMATION
    // =========================================================

    void TriggerAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger(
                "AttackTrigger"
            );
        }
    }

    // =========================================================
    // FLIP
    // =========================================================

    void FlipSprite(float directionX)
    {
        if (spriteRenderer == null)
            return;

        if (Mathf.Abs(directionX) <= 0.01f)
            return;

        spriteRenderer.flipX =
            directionX < 0f;
    }

    // =========================================================
    // TAKE DAMAGE
    // =========================================================

    public void TakeDamage(int damageAmount)
    {
        if (currentHp <= 0)
            return;

        currentHp -= damageAmount;

        currentHp =
            Mathf.Max(
                currentHp,
                0
            );

        UpdateHealthBarUI();

        if (animator != null)
        {
            animator.SetTrigger(
                "HurtTrigger"
            );
        }

        if (currentHp <= 0)
        {
            Die();
        }
    }

    // =========================================================
    // HEALTH BAR
    // =========================================================

    void UpdateHealthBarUI()
    {
        if (healthBarSlider == null)
            return;

        healthBarSlider.maxValue = maxHp;

        healthBarSlider.value = currentHp;
    }

    // =========================================================
    // DIE
    // =========================================================

    void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger(
                "DieTrigger"
            );
        }

        if (homeTower != null)
        {
            homeTower.OnSoldierDied(this);
        }

        CancelInvoke(nameof(FindEnemy));

        Destroy(
            gameObject,
            0.3f
        );
    }
}