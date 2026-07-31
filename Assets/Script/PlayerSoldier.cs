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
    public Slider healthBarSlider; // Kéo Slider thanh máu của đấu sĩ vào đây trong Inspector

    [Header("Positioning")]
    public Vector3 rallyPosition;
    [HideInInspector] public SpawnTower homeTower;

    private float attackCountdown = 0f;
    private Transform targetEnemy;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

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
        targetEnemy = null;
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

        if (rallyPosition == Vector3.zero)
        {
            rallyPosition = transform.position;
        }

        InvokeRepeating("FindEnemy", 0f, 0.2f);
    }

    void FindEnemy()
    {
        if (targetEnemy != null && targetEnemy.gameObject.activeInHierarchy) return;

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        GameObject nearestEnemy = null;

        foreach (GameObject enemy in enemies)
        {
            if (enemy == null || !enemy.activeInHierarchy) continue;

            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy <= detectRadius && distanceToEnemy < shortestDistance)
            {
                shortestDistance = distanceToEnemy;
                nearestEnemy = enemy;
            }
        }

        if (nearestEnemy != null)
        {
            targetEnemy = nearestEnemy.transform;
        }
        else
        {
            targetEnemy = null;
        }
    }

    void Update()
    {
        if (targetEnemy == null)
        {
            float distanceToRally = Vector3.Distance(transform.position, rallyPosition);

            if (distanceToRally > 0.15f)
            {
                Vector3 moveDir = (rallyPosition - transform.position).normalized;
                transform.Translate(moveDir * speed * Time.deltaTime, Space.World);

                FlipSprite(moveDir.x);
                SetMovingAnimation(true);
            }
            else
            {
                transform.position = rallyPosition;
                SetMovingAnimation(false);
            }
            return;
        }

        if (!targetEnemy.gameObject.activeInHierarchy)
        {
            targetEnemy = null;
            SetMovingAnimation(false);
            return;
        }

        float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.position);

        if (distanceToEnemy > attackRange)
        {
            Vector3 moveDir = (targetEnemy.position - transform.position).normalized;
            transform.Translate(moveDir * speed * Time.deltaTime, Space.World);

            FlipSprite(moveDir.x);
            SetMovingAnimation(true);
        }
        else
        {
            SetMovingAnimation(false);

            Vector3 dirToEnemy = targetEnemy.position - transform.position;
            FlipSprite(dirToEnemy.x);

            if (attackCountdown <= 0f)
            {
                TriggerAttackAnimation();
                OnAttackHit();
                attackCountdown = 1f / attackRate;
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
        Debug.Log(gameObject.name + " (Đấu sĩ) nhận " + damageAmount + " sát thương. Máu còn: " + currentHp);

        if (animator != null)
        {
            animator.SetTrigger("HurtTrigger");
        }

        UpdateHealthBarUI(); // Cập nhật thanh máu trực quan khi nhận sát thương

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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(rallyPosition, 0.3f);
        Gizmos.DrawLine(transform.position, rallyPosition);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectRadius);
    }
}