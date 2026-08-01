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
    public float checkRadius = 0.8f; // Tăng nhẹ tầm quét để quái dễ bắt mục tiêu hơn

    private Transform targetWaypoint;
    private int waypointIndex = 0;

    private bool isEngaged = false;
    private MonoBehaviour currentTargetSoldier;

    [Header("Thanh máu (UI Slider)")]
    public Slider healthSlider;

    void Start()
    {
        hp = maxHp;
        UpdateHealthUI();
        GetNextWaypoint();
    }

    void Update()
    {
        // Nếu đang đánh nhau với lính, tuyệt đối KHÔNG di chuyển, chỉ đứng lại đánh cho đến khi lính chết
        if (isEngaged)
        {
            CheckSoldierAlive();
            return;
        }

        // Nếu chưa vướng lính, liên tục quét xem có lính nào trong tầm chặn đường không
        CheckForSoldiersAhead();

        // Nếu vẫn không vướng lính thì mới di chuyển theo đường Waypoints
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

        Vector3 dir = targetWaypoint.position - transform.position;
        transform.Translate(dir.normalized * speed * Time.deltaTime, Space.World);

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.2f)
        {
            GetNextWaypoint();
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
        Destroy(gameObject);
    }

    void CheckSoldierAlive()
    {
        // Kiểm tra xem lính còn tồn tại không (nếu lính đã bị tiêu diệt và biến mất)
        if (currentTargetSoldier == null)
        {
            isEngaged = false; // Lính đã chết hoàn toàn, mở khóa cho quái đi tiếp
            return;
        }

        // Tiến hành đấm/bắn lính theo nhịp thời gian
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