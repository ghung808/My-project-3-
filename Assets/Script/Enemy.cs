using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    [Header("Cấu hình Chỉ số")]
    public float speed = 3f;
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
    private MonoBehaviour currentTargetSoldier;

    [Header("Thanh máu (UI Slider)")]
    public Slider healthSlider;

    [Header("Cấu hình Rơi Xu")]
    public GameObject coinPrefab;

    private SpriteRenderer spriteRenderer; // Biến quản lý lật mặt sprite

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        hp = maxHp;
        UpdateHealthUI();
        GetNextWaypoint();
    }

    void Update()
    {
        // Nếu đang đánh nhau với lính
        if (isEngaged)
        {
            // Quay mặt nhìn thẳng về phía con lính đang đánh nhau
            if (currentTargetSoldier != null)
            {
                float dirX = currentTargetSoldier.transform.position.x - transform.position.x;
                FlipSprite(dirX);
            }

            CheckSoldierAlive();
            return;
        }

        // Nếu chưa vướng lính, quét tìm lính
        CheckForSoldiersAhead();

        // Di chuyển theo Waypoints và lật mặt theo hướng di chuyển
        MoveTowardsWaypoint();
    }

    void CheckForSoldiersAhead()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, checkRadius);

        foreach (var hit in hits)
        {
            PlayerSoldier warrior = hit.GetComponent<PlayerSoldier>();
            MageSoldier mage = hit.GetComponent<MageSoldier>();
            ArcherSoldier archer = hit.GetComponent<ArcherSoldier>();

            if (warrior != null)
            {
                isEngaged = true;
                currentTargetSoldier = warrior;
                break;
            }
            else if (mage != null)
            {
                isEngaged = true;
                currentTargetSoldier = mage;
                break;
            }
            else if (archer != null)
            {
                isEngaged = true;
                currentTargetSoldier = archer;
                break;
            }
        }
    }

    void GetNextWaypoint()
    {
        if (Waypoints.points == null || Waypoints.points.Length == 0) return;

        if (waypointIndex >= Waypoints.points.Length)
        {
            ReachDestination();
            return;
        }

        targetWaypoint = Waypoints.points[waypointIndex];
        waypointIndex++;
    }

    void MoveTowardsWaypoint()
    {
        if (targetWaypoint == null) return;

        Vector3 currentPos = transform.position;
        Vector3 targetPos = targetWaypoint.position;

        // Tính hướng di chuyển ngang (để lật mặt trái/phải)
        float moveDirX = targetPos.x - currentPos.x;
        if (Mathf.Abs(moveDirX) > 0.01f)
        {
            FlipSprite(moveDirX);
        }

        // Di chuyển quái
        transform.position = Vector3.MoveTowards(currentPos, targetPos, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPos) < 0.2f)
        {
            GetNextWaypoint();
        }
    }

    // Hàm lật mặt Sprite (Nếu đi/nhìn sang trái thì lật, sang phải thì giữ nguyên hoặc ngược lại tùy asset gốc)
    void FlipSprite(float directionX)
    {
        if (spriteRenderer != null && Mathf.Abs(directionX) > 0.01f)
        {
            // Nếu asset gốc của bạn mặc định quay mặt sang TRÁI, hãy đổi thành: directionX > 0
            // Nếu asset gốc mặc định quay mặt sang PHẢI, giữ nguyên: directionX < 0
            spriteRenderer.flipX = directionX < 0;
        }
    }

    void ReachDestination()
    {
        Destroy(gameObject);
    }

    public void TakeDamage(int amt)
    {
        hp -= amt;
        UpdateHealthUI();

        if (hp <= 0)
        {
            Die();
        }
    }

    void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHp;
            healthSlider.value = hp;
        }
    }

    void Die()
    {
        if (coinPrefab != null)
        {
            Instantiate(coinPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    void CheckSoldierAlive()
    {
        if (currentTargetSoldier == null)
        {
            isEngaged = false;
            return;
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackSoldier();
            lastAttackTime = Time.time;
        }
    }

    void AttackSoldier()
    {
        if (currentTargetSoldier == null) return;

        if (currentTargetSoldier is PlayerSoldier w)
        {
            w.TakeDamage(damage);
        }
        else if (currentTargetSoldier is MageSoldier m)
        {
            m.TakeDamage(damage);
        }
        else if (currentTargetSoldier is ArcherSoldier a)
        {
            a.TakeDamage(damage);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, checkRadius);
    }
}