using UnityEngine;
using UnityEngine.UI;

public class ArcherSoldier : MonoBehaviour
{
    [Header("Tấn công")]
    public float attackRange = 5f;
    public float attackCooldown = 1.2f;

    [Header("Tốc độ")]
    public float speed = 3f;

    [Header("Đạn")]
    public GameObject arrowPrefab;
    public Transform shootPoint;

    [Header("Thanh máu")]
    public Slider healthSlider;

    private int maxHp;
    private int hp;
    private int damage;

    private SpawnTower towerRef;
    private Vector3 rallyPosition;

    private Transform targetEnemy;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private float attackTimer = 0f;


    // =========================================================
    // KHỞI TẠO
    // =========================================================

    public void InitializeStats(
        int _hp,
        int _dmg,
        SpawnTower _tower
    )
    {
        maxHp = _hp;
        hp = _hp;
        damage = _dmg;
        towerRef = _tower;

        UpdateHealthUI();
    }


    // =========================================================
    // RALLY
    // =========================================================

    public void SetRallyPosition(Vector3 pos)
    {
        rallyPosition = pos;

        // Xạ thủ đứng cố định tại vị trí tập kết.
        transform.position = rallyPosition;
    }


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        anim = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (maxHp <= 0)
        {
            maxHp = 20;
            hp = maxHp;
        }

        UpdateHealthUI();

        attackTimer = 0f;

        InvokeRepeating(
            nameof(FindClosestEnemy),
            0f,
            0.15f
        );
    }


    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        // -----------------------------------------
        // XÓA TARGET CHẾT
        // -----------------------------------------

        if (targetEnemy != null)
        {
            if (!targetEnemy.gameObject.activeInHierarchy)
            {
                targetEnemy = null;
            }
        }


        // -----------------------------------------
        // XẠ THỦ LUÔN ĐỨNG YÊN
        // -----------------------------------------

        transform.position = rallyPosition;


        // -----------------------------------------
        // COOLDOWN
        // -----------------------------------------

        if (attackTimer > 0f)
        {
            attackTimer -= Time.deltaTime;
        }


        // -----------------------------------------
        // KHÔNG CÓ TARGET
        // -----------------------------------------

        if (targetEnemy == null)
        {
            SetMovingAnimation(false);
            return;
        }


        // -----------------------------------------
        // KIỂM TRA KHOẢNG CÁCH
        // -----------------------------------------

        float distance =
            Vector3.Distance(
                transform.position,
                targetEnemy.position
            );


        // -----------------------------------------
        // TARGET RA NGOÀI TẦM
        // -----------------------------------------

        if (distance > attackRange)
        {
            SetMovingAnimation(false);
            return;
        }


        // -----------------------------------------
        // QUAY MẶT
        // -----------------------------------------

        Vector3 direction =
            targetEnemy.position -
            transform.position;

        if (Mathf.Abs(direction.x) > 0.01f)
        {
            FlipSprite(direction.x);
        }


        // -----------------------------------------
        // ĐÁNH
        // -----------------------------------------

        if (attackTimer <= 0f)
        {
            ShootAtEnemy();

            attackTimer = attackCooldown;
        }
    }


    // =========================================================
    // TÌM ENEMY GẦN NHẤT
    // =========================================================

    void FindClosestEnemy()
    {
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

            if (distance <= attackRange &&
                distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTarget = enemy.transform;
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

            if (distance <= attackRange &&
                distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTarget = boss.transform;
            }
        }


        // =====================================================
        // BOSS MAP 3
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

            if (distance <= attackRange &&
                distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestTarget = bossEnemy.transform;
            }
        }


        targetEnemy = nearestTarget;
    }


    // =========================================================
    // BẮN
    // =========================================================

    void ShootAtEnemy()
    {
        if (targetEnemy == null)
            return;


        // Animation
        if (anim != null)
        {
            anim.SetTrigger("AttackTrigger");
        }


        // Không có arrow prefab
        if (arrowPrefab == null)
        {
            Debug.LogWarning(
                gameObject.name +
                ": Chưa gán Arrow Prefab!"
            );

            return;
        }


        Vector3 spawnPosition =
            shootPoint != null
            ? shootPoint.position
            : transform.position;


        GameObject arrowObject =
            Instantiate(
                arrowPrefab,
                spawnPosition,
                Quaternion.identity
            );


        Arrow arrow =
            arrowObject.GetComponent<Arrow>();


        if (arrow != null)
        {
            arrow.Seek(
                targetEnemy,
                damage
            );
        }
        else
        {
            Debug.LogWarning(
                "Arrow Prefab không có script Arrow!"
            );
        }
    }


    // =========================================================
    // ANIMATION
    // =========================================================

    void SetMovingAnimation(bool isMoving)
    {
        if (anim != null)
        {
            // Xạ thủ không di chuyển.
            anim.SetBool("isMoving", false);
        }
    }


    // =========================================================
    // FLIP
    // =========================================================

    void FlipSprite(float directionX)
    {
        if (spriteRenderer == null)
            return;

        if (Mathf.Abs(directionX) > 0.01f)
        {
            spriteRenderer.flipX =
                directionX < 0;
        }
    }


    // =========================================================
    // NHẬN DAMAGE
    // =========================================================

    public void TakeDamage(int amount)
    {
        hp -= amount;

        UpdateHealthUI();


        if (anim != null)
        {
            anim.SetTrigger("HurtTrigger");
        }


        if (hp <= 0)
        {
            Die();
        }
    }


    // =========================================================
    // HỒI ĐẦY MÁU
    // =========================================================

    public void FullHeal()
    {
        hp = maxHp;

        UpdateHealthUI();
    }


    // =========================================================
    // HEALTH BAR
    // =========================================================

    void UpdateHealthUI()
    {
        if (healthSlider == null)
            return;

        healthSlider.maxValue = maxHp;
        healthSlider.value = hp;
    }


    // =========================================================
    // CHẾT
    // =========================================================

    void Die()
    {
        if (anim != null)
        {
            anim.SetTrigger("DieTrigger");
        }


        if (towerRef != null)
        {
            towerRef.OnSoldierDied(this);
        }


        Destroy(
            gameObject,
            0.3f
        );
    }
}