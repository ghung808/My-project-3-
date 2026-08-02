using UnityEngine;
using UnityEngine.UI;

public class MageSoldier : MonoBehaviour
{
    [Header("Attack Settings")]
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
    public Slider healthBarSlider;

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
        rallyPosition = GetNearestRoadPoint(newPos);
        transform.position = rallyPosition; // Cố định vị trí luôn
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

        // Đưa pháp sư ra đường lớn và KHÓA CỨNG TỌA ĐỘ TẠI ĐÓ, không bao giờ dịch chuyển nữa
        rallyPosition = GetNearestRoadPoint(rallyPosition);
        transform.position = rallyPosition;

        InvokeRepeating("FindEnemy", 0f, 0.2f);
    }

    Vector3 GetNearestRoadPoint(Vector3 fromPosition)
    {
        if (Waypoints.points == null || Waypoints.points.Length == 0)
        {
            return fromPosition;
        }

        float minDistance = Mathf.Infinity;
        Vector3 nearestPoint = fromPosition;

        foreach (Transform wp in Waypoints.points)
        {
            if (wp == null) continue;
            float dist = Vector3.Distance(fromPosition, wp.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPoint = wp.position;
            }
        }

        return nearestPoint;
    }

    void FindEnemy()
    {
        if (targetEnemy != null && targetEnemy.gameObject.activeInHierarchy)
        {
            float distToTarget = Vector3.Distance(transform.position, targetEnemy.position);
            if (distToTarget <= detectRadius * 1.5f) return;
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float shortestDistance = Mathf.Infinity;
        Transform nearestEnemy = null;

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

        targetEnemy = nearestEnemy;
    }

    void Update()
    {
        if (targetEnemy != null && !targetEnemy.gameObject.activeInHierarchy)
        {
            targetEnemy = null;
        }

        // Pháp sư đứng yên một chỗ, KHÔNG DÙNG MoveTowards nữa nên hoàn toàn không thể bị giật
        SetMovingAnimation(false);

        // --- XỬ LÝ TẤN CÔNG BẮN ĐẠN KHI QUÁI VÀO TẦM ---
        if (targetEnemy != null)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, targetEnemy.position);

            // Quay mặt về phía quái đang bắn
            Vector3 dirToEnemy = targetEnemy.position - transform.position;
            if (dirToEnemy != Vector3.zero) FlipSprite(dirToEnemy.x);

            if (distanceToEnemy <= attackRange)
            {
                if (attackCountdown <= 0f)
                {
                    TriggerAttackAnimation();
                    ShootMagicBullet();
                    attackCountdown = 1f / attackRate;
                }
            }
        }

        if (attackCountdown > 0f)
        {
            attackCountdown -= Time.deltaTime;
        }
    }

    void SetMovingAnimation(bool isMoving)
    {
        if (animator != null) animator.SetBool("isMoving", false);
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