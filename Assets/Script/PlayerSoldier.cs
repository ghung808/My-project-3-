using UnityEngine;
using UnityEngine.UI;

public class PlayerSoldier : MonoBehaviour
{
    [Header("Movement & Attack Settings")]
    public float speed = 3f;
    public float attackRate = 1f;
    public float detectRadius = 4f;

    // Khoảng cách để bắt đầu đánh
    public float attackRange = 1.0f;

    // Khoảng cách thực tế lính sẽ dừng trước mục tiêu
    // Tăng giá trị này để lính không chui vào Boss
    public float stopDistance = 1.2f;

    [Header("Stats")]
    public int maxHp = 20;
    public int currentHp = 20;
    public int damage = 2;

    [Header("UI Health Bar")]
    public Slider healthBarSlider;

    [Header("Positioning")]
    public Vector3 rallyPosition;
    [HideInInspector] public SpawnTower homeTower;

    private float attackCountdown = 0f;
    private Transform targetEnemy;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool hasMovedFromRally = false;

    public void InitializeStats(int hp, int dmg, SpawnTower tower)
    {
        maxHp = hp;
        currentHp = hp;
        damage = dmg;
        homeTower = tower;

        currentHp = maxHp;

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
    // TÌM MỤC TIÊU
    // =========================================================

    void FindEnemy()
    {
        if (targetEnemy != null && targetEnemy.gameObject.activeInHierarchy)
        {
            float distToTarget =
                Vector3.Distance(transform.position, targetEnemy.position);

            if (distToTarget <= detectRadius * 1.5f)
                return;
        }

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        // -----------------------------------------------------
        // TÌM ENEMY THƯỜNG
        // -----------------------------------------------------

        GameObject[] enemies =
            GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy)
                continue;

            float distance =
                Vector3.Distance(transform.position, enemy.transform.position);

            if (distance <= detectRadius &&
                distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestEnemy = enemy.transform;
            }
        }

        // -----------------------------------------------------
        // TÌM BOSS
        // -----------------------------------------------------

        GameObject bossObject =
            GameObject.FindGameObjectWithTag("Boss");

        if (bossObject != null && bossObject.activeInHierarchy)
        {
            float distanceToBoss =
                Vector3.Distance(
                    transform.position,
                    bossObject.transform.position
                );

            if (distanceToBoss <= detectRadius &&
                distanceToBoss < shortestDistance)
            {
                shortestDistance = distanceToBoss;
                nearestEnemy = bossObject.transform;
            }
        }

        targetEnemy = nearestEnemy;
    }

    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        // Nếu mục tiêu đã bị Destroy / inactive
        if (targetEnemy != null &&
            !targetEnemy.gameObject.activeInHierarchy)
        {
            targetEnemy = null;
        }

        // -----------------------------------------------------
        // CÓ MỤC TIÊU
        // -----------------------------------------------------

        if (targetEnemy != null)
        {
            float distanceToEnemy =
                Vector3.Distance(
                    transform.position,
                    targetEnemy.position
                );

            // =================================================
            // CHƯA ĐỦ GẦN → DI CHUYỂN
            // =================================================

            if (distanceToEnemy > stopDistance)
            {
                Vector3 direction =
                    (targetEnemy.position - transform.position).normalized;

                /*
                 * QUAN TRỌNG:
                 *
                 * Lính chỉ tiến tới stopDistance.
                 * Không tiến sát tâm Boss.
                 *
                 * Trước đây:
                 *
                 * attackRange = 1.0
                 *
                 * nên nhiều lính có thể cùng ép vào Boss.
                 *
                 * Bây giờ:
                 *
                 * stopDistance = 1.2
                 *
                 * giúp tạo khoảng cách an toàn.
                 */

                Vector3 stopPosition =
                    targetEnemy.position -
                    direction * stopDistance;

                transform.position =
                    Vector3.MoveTowards(
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
                // Đã đứng đủ xa mục tiêu
                SetMovingAnimation(false);
            }

            // =================================================
            // TẤN CÔNG
            // =================================================

            if (distanceToEnemy <= attackRange)
            {
                Vector3 dirToEnemy =
                    targetEnemy.position - transform.position;

                if (dirToEnemy != Vector3.zero)
                {
                    FlipSprite(dirToEnemy.x);
                }

                if (attackCountdown <= 0f)
                {
                    TriggerAttackAnimation();

                    OnAttackHit();

                    attackCountdown = 1f / attackRate;
                }
            }
        }
        else
        {
            // =================================================
            // KHÔNG CÓ QUÁI
            // =================================================

            attackCountdown = 0f;

            SetMovingAnimation(false);

            if (hasMovedFromRally)
            {
                float distToRally =
                    Vector3.Distance(
                        transform.position,
                        rallyPosition
                    );

                if (distToRally > 0.1f)
                {
                    transform.position =
                        Vector3.MoveTowards(
                            transform.position,
                            rallyPosition,
                            speed * Time.deltaTime
                        );

                    Vector3 moveBackDir =
                        (rallyPosition - transform.position).normalized;

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
    // ANIMATION
    // =========================================================

    void SetMovingAnimation(bool isMoving)
    {
        if (animator != null)
        {
            animator.SetBool("isMoving", isMoving);
        }
    }

    void TriggerAttackAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("AttackTrigger");
        }
    }

    // =========================================================
    // ĐÁNH MỤC TIÊU
    // =========================================================

    public void OnAttackHit()
    {
        if (targetEnemy == null)
            return;

        // -----------------------------------------------------
        // ENEMY THƯỜNG
        // -----------------------------------------------------

        Enemy enemy =
            targetEnemy.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            return;
        }

        // -----------------------------------------------------
        // BOSS CŨ - NẾU CÓ CLASS BOSS
        // -----------------------------------------------------

        Boss boss =
            targetEnemy.GetComponent<Boss>();

        if (boss != null)
        {
            boss.TakeDamage(damage);
            return;
        }

        // -----------------------------------------------------
        // BOSS HIỆN TẠI - BossEnemy
        // -----------------------------------------------------

        BossEnemy bossEnemy =
            targetEnemy.GetComponent<BossEnemy>();

        if (bossEnemy != null)
        {
            bossEnemy.TakeDamage(damage);
            return;
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
            spriteRenderer.flipX = directionX < 0;
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
            animator.SetTrigger("HurtTrigger");
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
    // CHẾT
    // =========================================================

    void Die()
    {
        if (animator != null)
        {
            animator.SetTrigger("DieTrigger");
        }

        if (homeTower != null)
        {
            homeTower.OnSoldierDied(this);
        }

        Destroy(gameObject, 0.3f);
    }
}