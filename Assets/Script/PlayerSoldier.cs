using UnityEngine;
using UnityEngine.UI;

public class PlayerSoldier : MonoBehaviour
{
    [Header("Movement & Attack Settings")]
    public float speed = 3f;
    public float attackRate = 1f;
    public float detectRadius = 4f;
    public float attackRange = 0.8f;

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
    private bool hasMovedFromRally = false; // Đánh dấu xem lính đã rời vị trí tập kết để đi đánh quái chưa

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

        InvokeRepeating("FindEnemy", 0f, 0.2f);
    }

    void FindEnemy()
    {
        if (targetEnemy != null && targetEnemy.gameObject.activeInHierarchy)
        {
            float distToTarget = Vector3.Distance(transform.position, targetEnemy.position);
            if (distToTarget <= detectRadius * 1.5f) return;
        }

        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

        // Tìm Enemy thường
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;

            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy <= detectRadius && distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy.transform;
            }
        }

        // Tìm Boss
        GameObject boss = GameObject.FindGameObjectWithTag("Boss");

        if (boss != null && boss.activeInHierarchy)
        {
            float distanceToBoss = Vector3.Distance(transform.position, boss.transform.position);

            if (distanceToBoss <= detectRadius && distanceToBoss < shortestDistance)
            {
                shortestDistance = distanceToBoss;
                nearestEnemy = boss.transform;
            }
        }

        targetEnemy = nearestEnemy;
    }

    void Update()
    {
        if (targetEnemy != null && !targetEnemy.gameObject.activeInHierarchy)
        {
            targetEnemy = null;
        }

        if (targetEnemy != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.position);

            if (distanceToEnemy > attackRange)
            {
                // Lao vào quái mượt mà
                transform.position = Vector3.MoveTowards(transform.position, targetEnemy.position, speed * Time.deltaTime);
                Vector3 moveDir = (targetEnemy.position - transform.position).normalized;
                if (moveDir != Vector3.zero) FlipSprite(moveDir.x);
                SetMovingAnimation(true);
                hasMovedFromRally = true; // Đánh dấu đã rời vị trí
            }
            else
            {
                // Đã áp sát tầm đánh -> Đứng im đánh quái
                SetMovingAnimation(false);
            }

            // --- XỬ LÝ TẤN CÔNG CẬN CHIẾN ---
            if (distanceToEnemy <= attackRange)
            {
                Vector3 dirToEnemy = targetEnemy.position - transform.position;
                if (dirToEnemy != Vector3.zero) FlipSprite(dirToEnemy.x);

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
            // --- KHI HẾT QUÁI (QUÁI ĐÃ CHẾT HẾT) ---
            attackCountdown = 0f; // Reset thời gian tấn công để không bị treo trạng thái
            SetMovingAnimation(false);

            if (hasMovedFromRally)
            {
                // Nếu đã từng rời tháp đi đánh, bây giờ hết quái sẽ tự động quay trở về tháp
                float distToRally = Vector3.Distance(transform.position, rallyPosition);
                if (distToRally > 0.1f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, rallyPosition, speed * Time.deltaTime);
                    Vector3 moveBackDir = (rallyPosition - transform.position).normalized;
                    if (moveBackDir != Vector3.zero) FlipSprite(moveBackDir.x);
                    SetMovingAnimation(true); // Bật animation chạy về
                }
                else
                {
                    // Đã về tới vị trí tháp -> Đứng yên (idle) và reset trạng thái
                    transform.position = rallyPosition;
                    SetMovingAnimation(false);
                    hasMovedFromRally = false;
                }
            }
            else
            {
                // Đang ở sẵn tháp thì đứng yên tại chỗ
                transform.position = rallyPosition;
                SetMovingAnimation(false);
            }
        }

        if (attackCountdown > 0f)
        {
            attackCountdown -= Time.deltaTime;
        }
    }

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

    public void OnAttackHit()
    {
        if (targetEnemy == null) return;

        Enemy enemy = targetEnemy.GetComponent<Enemy>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);
            return;
        }

        Boss boss = targetEnemy.GetComponent<Boss>();

        if (boss != null)
        {
            boss.TakeDamage(damage);
        }
    }

    void FlipSprite(float directionX)
    {
        if (spriteRenderer != null && Mathf.Abs(directionX) > 0.01f)
        {
            spriteRenderer.flipX = directionX < 0;
        }
    }

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

    void UpdateHealthBarUI()
    {
        if (healthBarSlider != null)
        {
            healthBarSlider.maxValue = maxHp;
            healthBarSlider.value = currentHp;
        }
    }

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