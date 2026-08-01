using UnityEngine;
using System.Collections.Generic;

public class SpawnTower : MonoBehaviour
{
    [Header("Cấu hình Tháp")]
    public int cost = 100;
    public int upgradeCost = 60;
    public int sellValue = 50;

    [Header("Chỉ số Lính của Tháp")]
    public int soldierHp = 20;
    public int soldierDmg = 2;
    public int maxSoldiers = 3;

    [Header("Prefabs & References")]
    public GameObject soldierPrefab;
    public Transform spawnPoint;

    private Vector3 rallyPoint;
    private List<MonoBehaviour> activeSoldiers = new List<MonoBehaviour>();
    private BuildingSpot2D assignedSpot;

    void Start()
    {
        rallyPoint = GetNearestRoadPoint();
        SpawnInitialSoldiers();
    }

    void Update()
    {
        if (activeSoldiers.Count == 0)
        {
            DestroyTowerAndResetSpot();
        }
    }

    public void SetAssignedSpot(BuildingSpot2D spot)
    {
        assignedSpot = spot;
    }

    Vector3 GetNearestRoadPoint()
    {
        if (Waypoints.points == null || Waypoints.points.Length == 0)
        {
            return transform.position;
        }

        Transform nearestPoint = Waypoints.points[0];
        float minDistance = Vector3.Distance(transform.position, nearestPoint.position);

        for (int i = 1; i < Waypoints.points.Length; i++)
        {
            float dist = Vector3.Distance(transform.position, Waypoints.points[i].position);
            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPoint = Waypoints.points[i];
            }
        }

        Vector3 adjustedPoint = nearestPoint.position + new Vector3(0f, 0.8f, 0f);
        return adjustedPoint;
    }

    void SpawnInitialSoldiers()
    {
        for (int i = 0; i < maxSoldiers; i++)
        {
            SpawnSingleSoldier(i);
        }
    }

    void SpawnSingleSoldier(int index)
    {
        if (soldierPrefab == null) return;

        Vector3 spawnPos = (spawnPoint != null) ? spawnPoint.position : transform.position;
        GameObject soldierObj = Instantiate(soldierPrefab, spawnPos, Quaternion.identity);

        PlayerSoldier warrior = soldierObj.GetComponent<PlayerSoldier>();
        MageSoldier mage = soldierObj.GetComponent<MageSoldier>();
        ArcherSoldier archer = soldierObj.GetComponent<ArcherSoldier>();

        Vector3 targetPos = rallyPoint;

        if (archer != null)
        {
            float spacing = 0.6f;
            float startOffset = -((maxSoldiers - 1) * spacing) / 2f;
            targetPos = rallyPoint + new Vector3(startOffset + (index * spacing), -0.8f, 0f);
        }
        else
        {
            float spacing = 0.5f;
            float startOffset = -((maxSoldiers - 1) * spacing) / 2f;
            targetPos = rallyPoint + new Vector3(startOffset + (index * spacing), 0f, 0f);
        }

        if (warrior != null)
        {
            warrior.InitializeStats(soldierHp, soldierDmg, this);
            warrior.SetRallyPosition(targetPos);
            activeSoldiers.Add(warrior);
        }
        else if (mage != null)
        {
            mage.InitializeStats(soldierHp, soldierDmg, this);
            mage.SetRallyPosition(targetPos);
            activeSoldiers.Add(mage);
        }
        else if (archer != null)
        {
            archer.InitializeStats(soldierHp, soldierDmg, this);
            archer.SetRallyPosition(targetPos);
            activeSoldiers.Add(archer);
        }
    }

    public void OnSoldierDied(MonoBehaviour deadSoldier)
    {
        if (activeSoldiers.Contains(deadSoldier))
        {
            activeSoldiers.Remove(deadSoldier);
        }
    }

    void DestroyTowerAndResetSpot()
    {
        if (assignedSpot != null)
        {
            assignedSpot.ClearSpot();
        }
        Destroy(gameObject);
    }

    public void UpgradeTower()
    {
        if (PlayerStats.Money >= upgradeCost)
        {
            PlayerStats.Money -= upgradeCost;
            soldierHp += 10;
            soldierDmg += 2;

            foreach (var soldier in activeSoldiers)
            {
                if (soldier != null)
                {
                    if (soldier is PlayerSoldier w) { w.InitializeStats(soldierHp, soldierDmg, this); w.FullHeal(); }
                    else if (soldier is MageSoldier m) { m.InitializeStats(soldierHp, soldierDmg, this); m.FullHeal(); }
                    else if (soldier is ArcherSoldier a) { a.InitializeStats(soldierHp, soldierDmg, this); a.FullHeal(); }
                }
            }
            Debug.Log("Đã nâng cấp tháp thành công!");
        }
        else
        {
            Debug.Log("Không đủ tiền để nâng cấp!");
        }
    }

    public void SellTower()
    {
        PlayerStats.Money += sellValue;

        if (assignedSpot != null)
        {
            assignedSpot.ClearSpot();
        }

        foreach (var soldier in activeSoldiers)
        {
            if (soldier != null)
            {
                if (soldier is PlayerSoldier w) Destroy(w.gameObject);
                if (soldier is MageSoldier m) Destroy(m.gameObject);
                if (soldier is ArcherSoldier a) Destroy(a.gameObject);
            }
        }

        Destroy(gameObject);
    }

    void OnMouseDown()
    {
        BuildingSpot2D spot = assignedSpot != null ? assignedSpot : GetComponentInParent<BuildingSpot2D>();
        if (spot != null && BuildManager.instance != null)
        {
            BuildManager.instance.SelectSpotToUpgrade(spot);
        }
    }
}