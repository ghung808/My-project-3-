using System.Collections.Generic;
using UnityEngine;

public class SpawnTower : MonoBehaviour
{
    [Header("Giá Cả & Nâng Cấp")]
    public int cost = 100;
    public int upgradeCost = 50;
    public int upgradeCostLevel2 = 4;
    public int upgradeCostLevel3 = 7;
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

    // =========================================================
    // MAP 3
    // =========================================================

    private WaypointsMap3 map3Waypoints;

    private enum Map3Lane
    {
        Middle,
        Top,
        Bottom
    }

    private Map3Lane selectedMap3Lane = Map3Lane.Middle;

    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        if (spawnPoint == null)
            spawnPoint = transform;

        // Tự tìm WaypointsMap3 trong Scene.
        // Map 1 và Map 2 không có WaypointsMap3 nên không bị ảnh hưởng.
        map3Waypoints = FindFirstObjectByType<WaypointsMap3>();

        if (map3Waypoints != null)
        {
            // Đây là Map 3
            rallyPoint = GetMap3NearestRoadPoint();
        }
        else
        {
            // Map 1 / Map 2 dùng hệ thống cũ
            rallyPoint = GetNearestRoadPoint();
        }

        SpawnAllSoldiers();
    }

    // =========================================================
    // SPAWN LÍNH
    // =========================================================

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
            Debug.LogError(
                "LỖI: Chưa gán Soldier Prefab vào SpawnTower trên GameObject: "
                + gameObject.name
            );

            return;
        }

        // =====================================================
        // XÁC ĐỊNH VỊ TRÍ SPAWN
        // =====================================================

        Vector3 soldierSpawnPosition;

        if (map3Waypoints != null)
        {
            // MAP 3:
            // Spawn trực tiếp trên đường đã xác định.
            soldierSpawnPosition = rallyPoint;
        }
        else
        {
            // MAP 1 / MAP 2:
            // Giữ nguyên cách spawn cũ.
            soldierSpawnPosition = spawnPoint.position;
        }

        GameObject soldierGO = Instantiate(
            soldierPrefab,
            soldierSpawnPosition,
            Quaternion.identity
        );

        // =====================================================
        // VỊ TRÍ RALLY
        // =====================================================

        Vector3 targetPos;

        if (map3Waypoints != null)
        {
            // Map 3:
            // Cho 3 lính đứng quanh waypoint nhưng vẫn ở trên đường.
            Vector3 offset = GetMap3SoldierOffset(index);

            targetPos = rallyPoint + offset;
        }
        else
        {
            // Map 1 / Map 2:
            // Giữ nguyên logic cũ.
            Vector3 offset =
                Quaternion.Euler(
                    0,
                    0,
                    index * (360f / maxSoldiers)
                ) *
                Vector3.right *
                rallyRadius;

            targetPos = rallyPoint + offset;
        }

        // =====================================================
        // LẤY SCRIPT LÍNH
        // =====================================================

        PlayerSoldier warrior =
            soldierGO.GetComponent<PlayerSoldier>();

        MageSoldier mage =
            soldierGO.GetComponent<MageSoldier>();

        ArcherSoldier archer =
            soldierGO.GetComponent<ArcherSoldier>();

        // =====================================================
        // KHỞI TẠO LÍNH
        // =====================================================

        if (warrior != null)
        {
            warrior.InitializeStats(
                soldierHp,
                soldierDamage,
                this
            );

            warrior.speed = soldierSpeed;
            warrior.SetRallyPosition(targetPos);
        }
        else if (mage != null)
        {
            mage.InitializeStats(
                soldierHp,
                soldierDamage,
                this
            );

            mage.speed = soldierSpeed;
            mage.SetRallyPosition(targetPos);
        }
        else if (archer != null)
        {
            archer.InitializeStats(
                soldierHp,
                soldierDamage,
                this
            );

            archer.speed = soldierSpeed;
            archer.SetRallyPosition(targetPos);
        }

        activeSoldiers.Add(soldierGO);
    }

    // =========================================================
    // MAP 3 - TÌM ĐƯỜNG GẦN NHẤT
    // =========================================================

    Vector3 GetMap3NearestRoadPoint()
    {
        if (map3Waypoints == null)
        {
            Debug.LogError(
                "SpawnTower: Không tìm thấy WaypointsMap3!"
            );

            return transform.position;
        }

        float middleDistance = GetDistanceToPath(
            map3Waypoints.middlePoints
        );

        float topDistance = GetDistanceToPath(
            map3Waypoints.topPoints
        );

        float bottomDistance = GetDistanceToPath(
            map3Waypoints.bottomPoints
        );

        // =====================================================
        // CHỌN ĐƯỜNG GẦN NHẤT
        // =====================================================

        float smallestDistance = middleDistance;

        selectedMap3Lane = Map3Lane.Middle;

        if (topDistance < smallestDistance)
        {
            smallestDistance = topDistance;
            selectedMap3Lane = Map3Lane.Top;
        }

        if (bottomDistance < smallestDistance)
        {
            smallestDistance = bottomDistance;
            selectedMap3Lane = Map3Lane.Bottom;
        }

        // =====================================================
        // LẤY WAYPOINT GẦN NHẤT CỦA ĐƯỜNG ĐÃ CHỌN
        // =====================================================

        Transform nearestWaypoint = null;

        switch (selectedMap3Lane)
        {
            case Map3Lane.Top:

                nearestWaypoint =
                    GetNearestWaypoint(
                        map3Waypoints.topPoints
                    );

                break;

            case Map3Lane.Bottom:

                nearestWaypoint =
                    GetNearestWaypoint(
                        map3Waypoints.bottomPoints
                    );

                break;

            case Map3Lane.Middle:

                nearestWaypoint =
                    GetNearestWaypoint(
                        map3Waypoints.middlePoints
                    );

                break;
        }

        if (nearestWaypoint == null)
        {
            Debug.LogWarning(
                "SpawnTower: Không tìm thấy waypoint Map 3."
            );

            return transform.position;
        }

        Debug.Log(
            "MAP 3 - Tháp " +
            gameObject.name +
            " chọn đường: " +
            selectedMap3Lane +
            " | Waypoint: " +
            nearestWaypoint.name
        );

        return nearestWaypoint.position;
    }

    // =========================================================
    // TÍNH KHOẢNG CÁCH TỚI 1 ĐƯỜNG
    // =========================================================

    float GetDistanceToPath(Transform[] points)
    {
        if (points == null || points.Length == 0)
            return Mathf.Infinity;

        float minDistance = Mathf.Infinity;

        foreach (Transform point in points)
        {
            if (point == null)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    point.position
                );

            if (distance < minDistance)
            {
                minDistance = distance;
            }
        }

        return minDistance;
    }

    // =========================================================
    // LẤY WAYPOINT GẦN NHẤT
    // =========================================================

    Transform GetNearestWaypoint(Transform[] points)
    {
        if (points == null || points.Length == 0)
            return null;

        float minDistance = Mathf.Infinity;
        Transform nearest = null;

        foreach (Transform point in points)
        {
            if (point == null)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    point.position
                );

            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = point;
            }
        }

        return nearest;
    }

    // =========================================================
    // VỊ TRÍ 3 LÍNH TRÊN ĐƯỜNG MAP 3
    // =========================================================

    Vector3 GetMap3SoldierOffset(int index)
    {
        // Cho lính đứng thành hàng nhỏ trên đường.
        // Không đẩy quá xa khỏi đường.

        float spacing = 0.35f;

        if (index == 0)
            return Vector3.zero;

        if (index == 1)
            return Vector3.left * spacing;

        if (index == 2)
            return Vector3.right * spacing;

        // Nếu nâng cấp có thêm lính
        int extraIndex = index - 3;

        float row = (extraIndex / 3) + 1;
        int position = extraIndex % 3;

        float xOffset =
            (position - 1) * spacing;

        float yOffset =
            -row * 0.25f;

        return new Vector3(
            xOffset,
            yOffset,
            0
        );
    }

    // =========================================================
    // KHI LÍNH CHẾT
    // =========================================================

    public void OnSoldierDied(MonoBehaviour soldier)
    {
        if (soldier != null &&
            activeSoldiers.Contains(soldier.gameObject))
        {
            activeSoldiers.Remove(
                soldier.gameObject
            );
        }
    }

    // =========================================================
    // NÂNG CẤP THÁP
    // =========================================================

    public void UpgradeTower()
    {
        if (level >= 3)
        {
            Debug.Log("⚠️ Tháp đã đạt Level 3!");
            return;
        }

        if (GameUI.instance == null)
        {
            Debug.LogError(
                "❌ Không tìm thấy GameUI.instance!"
            );

            return;
        }

        int currentUpgradeCost =
            GetUpgradeCost();

        if (GameUI.instance.gold < currentUpgradeCost)
        {
            Debug.Log(
                "❌ Không đủ vàng nâng cấp! " +
                "Cần: " + currentUpgradeCost +
                " | Có: " + GameUI.instance.gold
            );

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

        GameUI.instance.AddGold(
            -currentUpgradeCost
        );

        if (UpgradeInfoUI.instance != null)
        {
            UpgradeInfoUI.instance.ShowInfo(this);
        }

        Debug.Log(
            "⬆️ Nâng cấp Lv." +
            level +
            " | -" +
            currentUpgradeCost +
            " vàng" +
            " | Còn: " +
            GameUI.instance.gold +
            " | Lính: " +
            activeSoldiers.Count +
            " | HP: " +
            soldierHp +
            " | Damage: " +
            soldierDamage +
            " | Speed: " +
            soldierSpeed
        );
    }

    // =========================================================
    // SPAWN THÊM LÍNH KHI NÂNG CẤP
    // =========================================================

    void SpawnAdditionalSoldiers(int amount)
    {
        int startIndex =
            activeSoldiers.Count;

        for (int i = 0; i < amount; i++)
        {
            SpawnSingleSoldier(
                startIndex + i
            );
        }
    }

    // =========================================================
    // CẬP NHẬT CHỈ SỐ LÍNH
    // =========================================================

    void UpdateAllSoldierStats()
    {
        foreach (var soldierGO in activeSoldiers)
        {
            if (soldierGO == null)
                continue;

            PlayerSoldier w =
                soldierGO.GetComponent<PlayerSoldier>();

            MageSoldier m =
                soldierGO.GetComponent<MageSoldier>();

            ArcherSoldier a =
                soldierGO.GetComponent<ArcherSoldier>();

            if (w != null)
            {
                w.InitializeStats(
                    soldierHp,
                    soldierDamage,
                    this
                );

                w.speed = soldierSpeed;
                w.FullHeal();
            }
            else if (m != null)
            {
                m.InitializeStats(
                    soldierHp,
                    soldierDamage,
                    this
                );

                m.speed = soldierSpeed;
                m.FullHeal();
            }
            else if (a != null)
            {
                a.InitializeStats(
                    soldierHp,
                    soldierDamage,
                    this
                );

                a.speed = soldierSpeed;
                a.FullHeal();
            }
        }
    }

    // =========================================================
    // XÓA THÁP
    // =========================================================

    public void DestroyTower()
    {
        SellTower();
    }

    public void SellTower()
    {
        if (GameUI.instance != null)
        {
            GameUI.instance.AddGold(
                cost / 2
            );
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

    // =========================================================
    // MAP 1 / MAP 2 - HỆ THỐNG CŨ
    // =========================================================

    public Vector3 GetNearestRoadPoint()
    {
        if (Waypoints.points == null ||
            Waypoints.points.Length == 0)
        {
            return transform.position;
        }

        float minDistance = Mathf.Infinity;

        Vector3 nearestPoint =
            transform.position;

        foreach (Transform wp in Waypoints.points)
        {
            if (wp == null)
                continue;

            float dist =
                Vector3.Distance(
                    transform.position,
                    wp.position
                );

            if (dist < minDistance)
            {
                minDistance = dist;
                nearestPoint = wp.position;
            }
        }

        return nearestPoint;
    }

    // =========================================================
    // ĐỔI RALLY POINT
    // =========================================================

    public void SetNewRallyPoint(
        Vector3 newPoint
    )
    {
        rallyPoint = newPoint;
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;

        Gizmos.DrawWireSphere(
            rallyPoint,
            0.4f
        );

        Gizmos.DrawLine(
            transform.position,
            rallyPoint
        );
    }

    // =========================================================
    // GIÁ NÂNG CẤP
    // =========================================================

    public int GetUpgradeCost()
    {
        if (level == 1)
            return upgradeCostLevel2;

        if (level == 2)
            return upgradeCostLevel3;

        return 0;
    }
}