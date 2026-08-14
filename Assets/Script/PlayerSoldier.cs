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

    public void InitializeStats(
        int hp,
        int dmg,
        SpawnTower tower
    )
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
        spriteRenderer =
            GetComponent<SpriteRenderer>();

        animator =
            GetComponent<Animator>();

        currentHp = maxHp;

        UpdateHealthBarUI();


        if (rallyPosition != Vector3.zero)
        {
            transform.position =
                rallyPosition;
        }


        // Tìm enemy liên tục
        InvokeRepeating(
            nameof(FindEnemy),
            0f,
            0.2f
        );
    }


    // =========================================================
    // TÌM ENEMY
    // =========================================================

    void FindEnemy()
    {
        // Nếu target hiện tại vẫn còn và còn đủ gần
        if (targetEnemy != null &&
            targetEnemy.gameObject != null &&
            targetEnemy.gameObject.activeInHierarchy)
        {
            float distance =
                Vector3.Distance(
                    transform.position,
                    targetEnemy.position
                );

            if (distance <= detectRadius * 1.5f)
            {
                return;
            }
        }


        float shortestDistance =
            Mathf.Infinity;

        Transform nearestEnemy = null;


        // =====================================================
        // TÌM ENEMY THƯỜNG
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
                shortestDistance =
                    distance;

                nearestEnemy =
                    enemy.transform;
            }
        }


        // =====================================================
        // TÌM BOSS ENEMY
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
                shortestDistance =
                    distance;

                nearestEnemy =
                    bossEnemy.transform;
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
                shortestDistance =
                    distance;

                nearestEnemy =
                    boss.transform;
            }
        }


        targetEnemy =
            nearestEnemy;


        // Debug để kiểm tra
        if (targetEnemy != null)
        {
            Debug.Log(
                gameObject.name +
                " đang nhắm: " +
                targetEnemy.name
            );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        // Target đã bị Destroy
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
            float distanceToEnemy =
                Vector3.Distance(
                    transform.position,
                    targetEnemy.position
                );


            // =================================================
            // CÒN XA → CHẠY TỚI
            // =================================================

            if (distanceToEnemy > attackRange)
            {
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
                    FlipSprite(
                        direction.x
                    );
                }


                SetMovingAnimation(true);

                hasMovedFromRally = true;


                // Không đánh khi còn đang chạy
                return;
            }


            // =================================================
            // ĐỦ GẦN → DỪNG
            // =================================================

            SetMovingAnimation(false);


            Vector3 attackDirection =
                targetEnemy.position -
                transform.position;


            if (Mathf.Abs(attackDirection.x) > 0.01f)
            {
                FlipSprite(
                    attackDirection.x
                );
            }


            // =================================================
            // ĐÁNH
            // =================================================

            if (attackCountdown <= 0f)
            {
                Debug.Log(
                    gameObject.name +
                    " ĐANG ĐÁNH " +
                    targetEnemy.name
                );


                TriggerAttackAnimation();


                // Gây damage
                OnAttackHit();


                // Cooldown
                attackCountdown =
                    1f /
                    Mathf.Max(
                        attackRate,
                        0.01f
                    );
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
                float distanceToRally =
                    Vector3.Distance(
                        transform.position,
                        rallyPosition
                    );


                if (distanceToRally > 0.1f)
                {
                    transform.position =
                        Vector3.MoveTowards(
                            transform.position,
                            rallyPosition,
                            speed * Time.deltaTime
                        );


                    Vector3 moveBackDirection =
                        (
                            rallyPosition -
                            transform.position
                        ).normalized;


                    if (Mathf.Abs(moveBackDirection.x) > 0.01f)
                    {
                        FlipSprite(
                            moveBackDirection.x
                        );
                    }


                    SetMovingAnimation(true);
                }
                else
                {
                    transform.position =
                        rallyPosition;

                    SetMovingAnimation(false);

                    hasMovedFromRally = false;
                }
            }
            else
            {
                transform.position =
                    rallyPosition;

                SetMovingAnimation(false);
            }
        }


        // =====================================================
        // COOLDOWN
        // =====================================================

        if (attackCountdown > 0f)
        {
            attackCountdown -=
                Time.deltaTime;
        }
    }


    // =========================================================
    // ĐÁNH ENEMY / BOSS
    // =========================================================

    public void OnAttackHit()
    {
        if (targetEnemy == null)
        {
            Debug.LogWarning(
                gameObject.name +
                ": Không có target!"
            );

            return;
        }


        // =====================================================
        // ENEMY THƯỜNG
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
            Debug.Log(
                gameObject.name +
                " gây " +
                damage +
                " damage cho Enemy"
            );


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
            Debug.Log(
                gameObject.name +
                " gây " +
                damage +
                " damage cho BossEnemy"
            );


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
            Debug.Log(
                gameObject.name +
                " gây " +
                damage +
                " damage cho Boss"
            );


            boss.TakeDamage(damage);

            return;
        }


        Debug.LogError(
            "TARGET KHÔNG CÓ Enemy/Boss: " +
            targetEnemy.name
        );
    }


    // =========================================================
    // ANIMATION DI CHUYỂN
    // =========================================================

    void SetMovingAnimation(
        bool isMoving
    )
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
    // ANIMATION ATTACK
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

    void FlipSprite(
        float directionX
    )
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

    public void TakeDamage(
        int damageAmount
    )
    {
        currentHp -=
            damageAmount;


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
            healthBarSlider.maxValue =
                maxHp;

            healthBarSlider.value =
                currentHp;
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
            homeTower.OnSoldierDied(
                this
            );
        }


        Destroy(
            gameObject,
            0.3f
        );
    }
}