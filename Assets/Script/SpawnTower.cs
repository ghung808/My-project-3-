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
    public float respawnTime = 10f;

    [Header("Chỉ Số Lính")]
    public int soldierHp = 20;
    public int soldierDamage = 2;

    [Header("Vị Trí Tập Kết (Rally Point)")]
    public Vector3 rallyPoint;
    public float rallyRadius = 0.5f;

    [HideInInspector] public BuildingSpot2D targetSpot;

    private List<MonoBehaviour> activeSoldiers = new List<MonoBehaviour>();
    private float respawnTimer = 0f;

    void Start()
    {
        if (spawnPoint == null) spawnPoint = transform;

        rallyPoint = GetNearestRoadPoint();
        SpawnAllSoldiers();
    }

    void Update()
    {
        if (activeSoldiers.Count < maxSoldiers)
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer >= respawnTime)
            {
                SpawnSingleSoldier(activeSoldiers.Count);
                respawnTimer = 0f;
            }
        }
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
            warrior.SetRallyPosition(targetPos);
            activeSoldiers.Add(warrior);
        }
        else if (mage != null)
        {
            mage.InitializeStats(soldierHp, soldierDamage, this);
            mage.SetRallyPosition(targetPos);
            activeSoldiers.Add(mage);
        }
        else if (archer != null)
        {
            archer.InitializeStats(soldierHp, soldierDamage, this);
            archer.SetRallyPosition(targetPos);
            activeSoldiers.Add(archer);
        }
    }

    public void OnSoldierDied(MonoBehaviour soldier)
    {
        if (activeSoldiers.Contains(soldier))
        {
            activeSoldiers.Remove(soldier);
        }
    }

    public void UpgradeTower()
    {
        level++;
        soldierHp += 10;
        soldierDamage += 2;

        foreach (var soldier in activeSoldiers)
        {
            if (soldier != null)
            {
                // Dùng chung hàm InitializeStats để nâng cấp chuẩn xác không bị lỗi bảo mật biến
                if (soldier is PlayerSoldier w) { w.InitializeStats(soldierHp, soldierDamage, this); w.FullHeal(); }
                else if (soldier is MageSoldier m) { m.InitializeStats(soldierHp, soldierDamage, this); m.FullHeal(); }
                else if (soldier is ArcherSoldier a) { a.InitializeStats(soldierHp, soldierDamage, this); a.FullHeal(); }
            }
        }

        Debug.Log("Đã nâng cấp Tháp Lính lên cấp " + level);
    }

    public void DestroyTower()
    {
        SellTower();
    }

    public void SellTower()
    {
        PlayerStats.Money += cost / 2;

        foreach (var soldier in activeSoldiers)
        {
            if (soldier != null)
            {
                if (soldier is PlayerSoldier w) Destroy(w.gameObject);
                else if (soldier is MageSoldier m) Destroy(m.gameObject);
                else if (soldier is ArcherSoldier a) Destroy(a.gameObject);
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
        for (int i = 0; i < activeSoldiers.Count; i++)
        {
            if (activeSoldiers[i] != null)
            {
                Vector3 offset = Quaternion.Euler(0, 0, i * (360f / maxSoldiers)) * Vector3.right * rallyRadius;
                Vector3 targetPos = rallyPoint + offset;

                if (activeSoldiers[i] is PlayerSoldier w) w.SetRallyPosition(targetPos);
                else if (activeSoldiers[i] is MageSoldier m) m.SetRallyPosition(targetPos);
                else if (activeSoldiers[i] is ArcherSoldier a) a.SetRallyPosition(targetPos);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(rallyPoint, 0.4f);
        Gizmos.DrawLine(transform.position, rallyPoint);
    }
}