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

    // Không còn hàm Update đếm giờ hồi sinh nữa

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
            warrior.SetRallyPosition(targetPos);
        }
        else if (mage != null)
        {
            mage.InitializeStats(soldierHp, soldierDamage, this);
            mage.SetRallyPosition(targetPos);
        }
        else if (archer != null)
        {
            archer.InitializeStats(soldierHp, soldierDamage, this);
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
        level++;
        soldierHp += 10;
        soldierDamage += 2;

        foreach (var soldierGO in activeSoldiers)
        {
            if (soldierGO != null)
            {
                PlayerSoldier w = soldierGO.GetComponent<PlayerSoldier>();
                MageSoldier m = soldierGO.GetComponent<MageSoldier>();
                ArcherSoldier a = soldierGO.GetComponent<ArcherSoldier>();

                if (w != null) { w.InitializeStats(soldierHp, soldierDamage, this); w.FullHeal(); }
                else if (m != null) { m.InitializeStats(soldierHp, soldierDamage, this); m.FullHeal(); }
                else if (a != null) { a.InitializeStats(soldierHp, soldierDamage, this); a.FullHeal(); }
            }
        }
    }

    public void DestroyTower()
    {
        SellTower();
    }

    public void SellTower()
    {
        PlayerStats.Money += cost / 2;

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