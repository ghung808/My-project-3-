using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHp = 10;
    public int currentHp = 10;
    public int damage = 2;
    public float attackRate = 1f;
    public float attackRange = 1.2f; // Tầm đánh để dừng lại đánh lính
    public float speed = 2f;

    [Header("Waypoint Path")]
    private List<Transform> waypoints = new List<Transform>();
    private int currentWaypointIndex = 0;

    [Header("UI Health Bar")]
    public Slider enemyHealthSlider;

    private float attackCountdown = 0f;
    private Transform targetPlayer; // Lính đang chặn đường
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentHp = maxHp;
        UpdateHealthBarUI();

        // Tự động tìm đường đi (Waypoints) trong Scene
        GameObject waypointObj = GameObject.Find("Waypoints");
        if (waypointObj != null)
        {
            foreach (Transform child in waypointObj.transform)
            {
                waypoints.Add(child);
            }
        }
    }

    void Update()
    {
        // 1. Kiểm tra xem có lính nào đứng gần trong tầm chặn đường không
        FindBlockingPlayer();

        // 2. Nếu có lính đứng chặn -> Dừng lại đứng đánh lính
        if (targetPlayer != null && targetPlayer.gameObject.activeInHierarchy)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, targetPlayer.position);

            if (distanceToPlayer <= attackRange)
            {
                if (animator != null) animator.SetBool("isRunning", false);

                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = targetPlayer.position.x < transform.position.x;
                }

                if (attackCountdown <= 0f)
                {
                    AttackPlayer();
                    attackCountdown = 1f / attackRate;
                }
            }
            else
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPlayer.position, speed * Time.deltaTime);
                if (animator != null) animator.SetBool("isRunning", true);
                if (spriteRenderer != null)
                {
                    spriteRenderer.flipX = targetPlayer.position.x < transform.position.x;
                }
            }
        }
        else // 3. Không có lính chặn -> Đi theo Waypoints
        {
            targetPlayer = null;
            MoveAlongWaypoints();
        }

        if (attackCountdown > 0f)
        {
            attackCountdown -= Time.deltaTime;
        }
    }

    void MoveAlongWaypoints()
    {
        if (waypoints.Count == 0 || currentWaypointIndex >= waypoints.Count)
        {
            if (animator != null) animator.SetBool("isRunning", false);
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        transform.position = Vector3.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = targetWaypoint.position.x < transform.position.x;
        }

        if (animator != null) animator.SetBool("isRunning", true);

        if (Vector3.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex++;
        }
    }

    void FindBlockingPlayer()
    {
        if (targetPlayer != null && targetPlayer.gameObject.activeInHierarchy)
        {
            float dist = Vector3.Distance(transform.position, targetPlayer.position);
            if (dist <= 2.5f) return;
        }

        Transform closest = null;
        float shortestDistance = 2.5f;

        // Tìm kiếm Pháp sư (MageSoldier)
        MageSoldier[] mages = Object.FindObjectsByType<MageSoldier>(FindObjectsSortMode.None);
        foreach (MageSoldier mage in mages)
        {
            if (mage == null || !mage.gameObject.activeInHierarchy) continue;
            float dist = Vector3.Distance(transform.position, mage.transform.position);
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                closest = mage.transform;
            }
        }

        // Tìm kiếm Đấu sĩ (PlayerSoldier)
        PlayerSoldier[] playerSoldiers = Object.FindObjectsByType<PlayerSoldier>(FindObjectsSortMode.None);
        foreach (PlayerSoldier soldier in playerSoldiers)
        {
            if (soldier == null || !soldier.gameObject.activeInHierarchy) continue;
            float dist = Vector3.Distance(transform.position, soldier.transform.position);
            if (dist < shortestDistance)
            {
                shortestDistance = dist;
                closest = soldier.transform;
            }
        }

        targetPlayer = closest;
    }

    void AttackPlayer()
    {
        if (animator != null)
        {
            animator.SetTrigger("AttackTrigger"); // Khớp với Parameter sẵn có trong Animator của quái
        }

        if (targetPlayer != null)
        {
            MageSoldier mage = targetPlayer.GetComponent<MageSoldier>();
            if (mage != null)
            {
                mage.TakeDamage(damage);
                return;
            }

            PlayerSoldier soldier = targetPlayer.GetComponent<PlayerSoldier>();
            if (soldier != null)
            {
                soldier.TakeDamage(damage);
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHp -= damageAmount;
        Debug.Log("Quái nhận " + damageAmount + " sát thương. Máu quái còn: " + currentHp);

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
        if (enemyHealthSlider != null)
        {
            enemyHealthSlider.maxValue = maxHp;
            enemyHealthSlider.value = currentHp;
        }
    }

    void Die()
    {
        if (animator != null) animator.SetTrigger("DieTrigger");
        Destroy(gameObject, 0.2f);
    }
}