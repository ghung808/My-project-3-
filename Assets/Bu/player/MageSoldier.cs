using UnityEngine;
using UnityEngine.UI;

public class MageSoldier : MonoBehaviour
{
    [Header("Movement & Attack Settings")]
    public float speed = 2.5f;
    public float attackRate = 1f;
    public float detectRadius = 5f;
    public float attackRange = 3.5f;

    [Header("Stats")]
    public int maxHp = 15;
    public int currentHp = 15;
    public int damage = 3;

    [Header("Ranged Settings")]
    public GameObject magicBulletPrefab;
    public Transform firePoint;

    [Header("UI Health Bar")]
    public Slider healthBarSlider; // Kéo Slider vào đây

    [Header("Positioning")]
    public Vector3 rallyPosition;
    [HideInInspector] public SpawnTower homeTower;

    private float attackCountdown = 0f;
    private Transform targetEnemy;
    private SpriteRenderer spriteRenderer;
    private Animator animator;

    // Các hàm tương thích với SpawnTower.cs
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

        InvokeRepeating("FindEnemy", 0.5f, 0.2f);
    }

    void FindEnemy()
    {
        if (Vector3.Distance(transform.position, rallyPosition) > 0.1f) return;
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

        targetEnemy = (nearestEnemy != null) ? nearestEnemy.transform : null;
    }

    void Update()
    {
        float distanceToRally = Vector3.Distance(transform.position, rallyPosition);
        if (distanceToRally > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, rallyPosition, speed * Time.deltaTime);
            Vector3 moveDir = (rallyPosition - transform.position).normalized;
            if (moveDir != Vector3.zero) FlipSprite(moveDir.x);
            SetMovingAnimation(true);
            return;
        }
        else
        {
            transform.position = rallyPosition;
        }

        if (targetEnemy == null)
        {
            SetMovingAnimation(false);
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
            transform.position = Vector3.MoveTowards(transform.position, targetEnemy.position, speed * Time.deltaTime);
            Vector3 moveDir = (targetEnemy.position - transform.position).normalized;
            if (moveDir != Vector3.zero) FlipSprite(moveDir.x);
            SetMovingAnimation(true);
        }
        else
        {
            SetMovingAnimation(false);
            Vector3 dirToEnemy = targetEnemy.position - transform.position;
            if (dirToEnemy != Vector3.zero) FlipSprite(dirToEnemy.x);

            if (attackCountdown <= 0f)
            {
                TriggerAttackAnimation();
                ShootMagicBullet();
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
        if (animator != null) animator.SetBool("isMoving", isMoving);
    }

    void TriggerAttackAnimation()
    {
        if (animator != null) animator.SetTrigger("AttackTrigger");
    }

    void ShootMagicBullet()
    {
        if (targetEnemy == null || magicBulletPrefab == null) return;

        Vector3 spawnPos = (firePoint != null) ? firePoint.position : transform.position;
        GameObject bulletObj = Instantiate(magicBulletPrefab, spawnPos, Quaternion.identity);

        MagicBullet bullet = bulletObj.GetComponent<MagicBullet>();
        if (bullet != null)
        {
            bullet.Seek(targetEnemy, damage);
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
        Debug.Log(gameObject.name + " nhận " + damageAmount + " sát thương. Máu còn: " + currentHp);

        if (animator != null) animator.SetTrigger("HurtTrigger");
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
        if (animator != null) animator.SetTrigger("DieTrigger");

        if (homeTower != null)
        {
            homeTower.OnSoldierDied(this);
        }

        Destroy(gameObject, 0.3f);
    }
}