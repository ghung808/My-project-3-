using System.Collections.Generic;
using UnityEngine;

public class SpawnTower : MonoBehaviour
{
    [Header("Giá Cả & Nâng Cấp")]
    public int cost = 100;
    public int upgradeCost = 50;
    public int level = 1;

    [Header("Cấu Hình Lính")]
    public GameObject soldierPrefab;
    public Transform spawnPoint;
    public int maxSoldiers = 3;

    [Header("Chỉ Số Lính")]
    public int soldierHp = 20;
    public int soldierDamage = 2;
    public float soldierSpeed = 3f;

    [Header("Vị Trí Tập Kết (Rally Point)")]
    public Vector3 rallyPoint;
    public float rallyRadius = 0.5f;

    [HideInInspector] public BuildingSpot2D targetSpot;

    private List<GameObject> activeSoldiers = new List<GameObject>();

    void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;

        rallyPoint = GetNearestRoadPoint();
        SpawnAllSoldiers();
    }

    void SpawnAllSoldiers()
    {
        for (int i = 0; i < maxSoldiers; i++)
        {
            SpawnSingleSoldier(i);
        }
    }

    void SpawnSingleSoldier(int index)
    {
        if (soldierPrefab == null)
        {
            Debug.LogError("LỖI: Chưa gán Soldier Prefab vào SpawnTower trên GameObject: " + gameObject.name);
            return;
        }

        GameObject soldierGO = Instantiate(soldierPrefab, spawnPoint.position, Quaternion.identity);

        PlayerSoldier warrior = soldierGO.GetComponent<PlayerSoldier>();
        MageSoldier mage = soldierGO.GetComponent<MageSoldier>();
        ArcherSoldier archer = soldierGO.GetComponent<ArcherSoldier>();

        Vector3 offset = Quaternion.Euler(0, 0, index * (360f / maxSoldiers)) * Vector3.right * rallyRadius;
        Vector3 targetPos = rallyPoint + offset;

        if (warrior != null)
        {
            warrior.InitializeStats(soldierHp, soldierDamage, this);
            warrior.speed = soldierSpeed;
            warrior.SetRallyPosition(targetPos);
        }
        else if (mage != null)
        {
            mage.InitializeStats(soldierHp, soldierDamage, this);
            mage.speed = soldierSpeed;
            mage.SetRallyPosition(targetPos);
        }
        else if (archer != null)
        {
            archer.InitializeStats(soldierHp, soldierDamage, this);
            archer.speed = soldierSpeed;
            archer.SetRallyPosition(targetPos);
        }

        activeSoldiers.Add(soldierGO);
    }

    // Khi lính chết, xóa khỏi danh sách và KHÔNG BAO GIỜ sinh lại nữa
    public void OnSoldierDied(MonoBehaviour soldier)
    {
        if (soldier != null && activeSoldiers.Contains(soldier.gameObject))
        {
            activeSoldiers.Remove(soldier.gameObject);
        }
    }

    public void UpgradeTower()
    {
        if (level >= 3)
        {
            Debug.Log("Tháp đã đạt Level 3!");
            return;
        }

        level++;

        if (level == 2)
        {
            soldierHp += 10;
            soldierDamage += 1;
            soldierSpeed += 1f;

            maxSoldiers += 2;

            SpawnAdditionalSoldiers(2);
        }
        else if (level == 3)
        {
            soldierHp += 5;
            soldierDamage += 1;
            soldierSpeed += 1f;

            maxSoldiers += 3;

            SpawnAdditionalSoldiers(3);
        }

        UpdateAllSoldierStats();

        Debug.Log(
            "Nâng cấp Lv." + level +
            " | Lính: " + activeSoldiers.Count +
            " | HP: " + soldierHp +
            " | Damage: " + soldierDamage +
            " | Speed: " + soldierSpeed
        );
    }

    void SpawnAdditionalSoldiers(int amount)
    {
        int startIndex = activeSoldiers.Count;

        for (int i = 0; i < amount; i++)
        {
            SpawnSingleSoldier(startIndex + i);
        }
    }

    void UpdateAllSoldierStats()
    {
        foreach (var soldierGO in activeSoldiers)
        {
            if (soldierGO == null)
                continue;

            PlayerSoldier w = soldierGO.GetComponent<PlayerSoldier>();
            MageSoldier m = soldierGO.GetComponent<MageSoldier>();
            ArcherSoldier a = soldierGO.GetComponent<ArcherSoldier>();

            if (w != null)
            {
                w.InitializeStats(soldierHp, soldierDamage, this);
                w.speed = soldierSpeed;
                w.FullHeal();
            }
            else if (m != null)
            {
                m.InitializeStats(soldierHp, soldierDamage, this);
                m.speed = soldierSpeed;
                m.FullHeal();
            }
            else if (a != null)
            {
                a.InitializeStats(soldierHp, soldierDamage, this);
                a.speed = soldierSpeed;
                a.FullHeal();
            }
        }
    }

    public void DestroyTower()
    {
        SellTower();
    }

    public void SellTower()
    {
        if (GameUI.instance != null)
        {
            GameUI.instance.AddGold(cost / 2);
        }

        foreach (var soldierGO in activeSoldiers)
        {
            if (soldierGO != null)
            {
                Destroy(soldierGO);
            }
        }

        activeSoldiers.Clear();

        if (targetSpot != null)
        {
            targetSpot.ClearSpot();
        }

        Destroy(gameObject);
    }

    public Vector3 GetNearestRoadPoint()
    {
        if (Waypoints.points == null || Waypoints.points.Length == 0)
        {
            return transform.position;
        }

        float minDistance = Mathf.Infinity;
        Vector3 nearestPoint = transform.position;

        foreach (Transform wp in Waypoints.points)
        {
            if (wp == null) continue;
            float dist = Vector3.Distance(transform.position, wp.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPoint = wp.position;
            }
        }

        return nearestPoint;
    }

    public void SetNewRallyPoint(Vector3 newPoint)
    {
        rallyPoint = newPoint;
        // Chỉ dời vị trí khi cần thiết
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(rallyPoint, 0.4f);
        Gizmos.DrawLine(transform.position, rallyPoint);
    }
}