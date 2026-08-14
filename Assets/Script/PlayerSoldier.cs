using UnityEngine;
using UnityEngine.UI;

public class PlayerSoldier : MonoBehaviour
{
    [Header("Movement & Attack Settings")]
    public float speed = 3f;
    public float attackRate = 1f;
    public float detectRadius = 4f;

    [Header("Attack")]
    public float attackRange = 1.7f;
    public float stopDistance = 1.2f;

    [Header("Stats")]
    public int maxHp = 20;
    public int currentHp = 20;
    public int damage = 2;

    [Header("UI Health Bar")]
    public Slider healthBarSlider;

    [Header("Positioning")]
    public Vector3 rallyPosition;

    [HideInInspector]
    public SpawnTower homeTower;

    private float attackCountdown = 0f;
    private Transform targetEnemy;

    private SpriteRenderer spriteRenderer;
    private Animator animator;

    private bool hasMovedFromRally = false;


    // =========================================================
    // KHỞI TẠO
    // =========================================================

    public void InitializeStats(int hp, int dmg, SpawnTower tower)
    {
        maxHp = hp;
        currentHp = hp;
        damage = dmg;
        homeTower = tower;

        UpdateHealthBarUI();
    }

    public void SetRallyPosition(Vector3 newPos)
    {
        rallyPosition = newPos;
        transform.position = rallyPosition;
        hasMovedFromRally = false;
    }

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

        currentHp = maxHp;

        UpdateHealthBarUI();

        if (rallyPosition != Vector3.zero)
        {
            transform.position = rallyPosition;
        }

        InvokeRepeating(nameof(FindEnemy), 0f, 0.2f);
    }


    // =========================================================
    // TÌM ENEMY
    // =========================================================

    void FindEnemy()
    {
        if (targetEnemy != null &&
            targetEnemy.gameObject != null &&
            targetEnemy.gameObject.activeInHierarchy)
        {
            float distance = Vector3.Distance(
                transform.position,
                targetEnemy.position
            );

            if (distance <= detectRadius * 1.5f)
            {
                return;
            }
        }

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;


        // =====================================================
        // TÌM ENEMY
        // =====================================================

        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsSortMode.None
        );

        foreach (Enemy enemy in enemies)
        {
            if (enemy == null)
                continue;

            if (!enemy.gameObject.activeInHierarchy)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                enemy.transform.position
            );

            if (distance <= detectRadius &&
                distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }


        // =====================================================
        // TÌM BOSS ENEMY
        // =====================================================

        BossEnemy[] bossEnemies = FindObjectsByType<BossEnemy>(
            FindObjectsSortMode.None
        );

        foreach (BossEnemy bossEnemy in bossEnemies)
        {
            if (bossEnemy == null)
                continue;

            if (!bossEnemy.gameObject.activeInHierarchy)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                bossEnemy.transform.position
            );

            if (distance <= detectRadius &&
                distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = bossEnemy.transform;
            }
        }


        // =====================================================
        // TÌM BOSS CŨ
        // =====================================================

        Boss[] bosses = FindObjectsByType<Boss>(
            FindObjectsSortMode.None
        );

        foreach (Boss boss in bosses)
        {
            if (boss == null)
                continue;

            if (!boss.gameObject.activeInHierarchy)
                continue;

            float distance = Vector3.Distance(
                transform.position,
                boss.transform.position
            );

            if (distance <= detectRadius &&
                distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = boss.transform;
            }
        }


        targetEnemy = nearestEnemy;
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        // Mục tiêu bị Destroy
        if (targetEnemy != null)
        {
            if (targetEnemy.gameObject == null ||
                !targetEnemy.gameObject.activeInHierarchy)
            {
                targetEnemy = null;
            }
        }


        // =====================================================
        // CÓ MỤC TIÊU
        // =====================================================

        if (targetEnemy != null)
        {
            float distanceToEnemy = Vector3.Distance(
                transform.position,
                targetEnemy.position
            );


            // =================================================
            // DI CHUYỂN TỚI ENEMY
            // =================================================

            if (distanceToEnemy > stopDistance)
            {
                Vector3 direction = (
                    targetEnemy.position -
                    transform.position
                ).normalized;

                Vector3 stopPosition =
                    targetEnemy.position -
                    direction * stopDistance;

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    stopPosition,
                    speed * Time.deltaTime
                );

                if (direction != Vector3.zero)
                {
                    FlipSprite(direction.x);
                }

                SetMovingAnimation(true);

                hasMovedFromRally = true;
            }
            else
            {
                SetMovingAnimation(false);
            }


            // =================================================
            // ĐÁNH
            // =================================================

            if (distanceToEnemy <= attackRange)
            {
                Vector3 direction =
                    targetEnemy.position -
                    transform.position;

                if (direction != Vector3.zero)
                {
                    FlipSprite(direction.x);
                }

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
        }


        // =====================================================
        // KHÔNG CÓ ENEMY
        // =====================================================

        else
        {
            attackCountdown = 0f;

            SetMovingAnimation(false);

            if (hasMovedFromRally)
            {
                float distToRally = Vector3.Distance(
                    transform.position,
                    rallyPosition
                );

                if (distToRally > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(
                        transform.position,
                        rallyPosition,
                        speed * Time.deltaTime
                    );

                    Vector3 moveBackDir =
                        (rallyPosition -
                         transform.position).normalized;

                    if (moveBackDir != Vector3.zero)
                    {
                        FlipSprite(moveBackDir.x);
                    }

                    SetMovingAnimation(true);
                }
                else
                {
                    transform.position = rallyPosition;

                    SetMovingAnimation(false);

                    hasMovedFromRally = false;
                }
            }
            else
            {
                transform.position = rallyPosition;

                SetMovingAnimation(false);
            }
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
    // ĐÁNH ENEMY / BOSS
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

        Debug.LogError(
            "Không tìm thấy Enemy/Boss trên: " +
            targetEnemy.name
        );
    }


    // =========================================================
    // ANIMATION
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
        if (spriteRenderer != null &&
            Mathf.Abs(directionX) > 0.01f)
        {
            spriteRenderer.flipX =
                directionX < 0;
        }
    }


    // =========================================================
    // NHẬN DAMAGE
    // =========================================================

    public void TakeDamage(int damageAmount)
    {
        currentHp -= damageAmount;

        if (animator != null)
        {
            animator.SetTrigger(
                "HurtTrigger"
            );
        }

        UpdateHealthBarUI();

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
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHp;
            healthBarSlider.value = currentHp;
        }
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

        Destroy(gameObject, 0.3f);
    }
}