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

    [Header("Chỉ Số Chung")]
    public float soldierSpeed = 3f;

    // =========================================================
    // CÂN BẰNG 3 LOẠI LÍNH
    // =========================================================

    [Header("ĐẤU SĨ - TANK")]
    public int warriorHp = 35;
    public int warriorDamage = 2;

    [Header("CUNG THỦ - DAMAGE CAO")]
    public int archerHp = 18;
    public int archerDamage = 5;

    [Header("PHÁP SƯ - DAMAGE CAO NHẤT")]
    public int mageHp = 14;
    public int mageDamage = 7;

    [Header("Vị Trí Tập Kết (Rally Point)")]
    public Vector3 rallyPoint;
    public float rallyRadius = 0.5f;

    [HideInInspector] public BuildingSpot2D targetSpot;

    private List<GameObject> activeSoldiers =
        new List<GameObject>();

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

    private Map3Lane selectedMap3Lane =
        Map3Lane.Middle;

    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        if (spawnPoint == null)
            spawnPoint = transform;

        // Tự tìm WaypointsMap3 trong Scene.
        // Map 1 / Map 2 không có thì dùng hệ thống cũ.
        map3Waypoints =
            FindFirstObjectByType<WaypointsMap3>();

        if (map3Waypoints != null)
        {
            // MAP 3
            rallyPoint =
                GetMap3NearestRoadPoint();
        }
        else
        {
            // MAP 1 / MAP 2
            rallyPoint =
                GetNearestRoadPoint();
        }

        SpawnAllSoldiers();
    }

    // =========================================================
    // SPAWN TẤT CẢ LÍNH
    // =========================================================

    void SpawnAllSoldiers()
    {
        for (int i = 0; i < maxSoldiers; i++)
        {
            SpawnSingleSoldier(i);
        }
    }

    // =========================================================
    // SPAWN 1 LÍNH
    // =========================================================

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
        // VỊ TRÍ SPAWN
        // =====================================================

        Vector3 soldierSpawnPosition;

        if (map3Waypoints != null)
        {
            // MAP 3
            soldierSpawnPosition =
                rallyPoint;
        }
        else
        {
            // MAP 1 / MAP 2
            soldierSpawnPosition =
                spawnPoint.position;
        }

        GameObject soldierGO =
            Instantiate(
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
            Vector3 offset =
                GetMap3SoldierOffset(index);

            targetPos =
                rallyPoint + offset;
        }
        else
        {
            Vector3 offset =
                Quaternion.Euler(
                    0,
                    0,
                    index * (360f / maxSoldiers)
                ) *
                Vector3.right *
                rallyRadius;

            targetPos =
                rallyPoint + offset;
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
        // KHỞI TẠO ĐẤU SĨ
        // =====================================================

        if (warrior != null)
        {
            warrior.InitializeStats(
                warriorHp,
                warriorDamage,
                this
            );

            warrior.speed =
                soldierSpeed;

            warrior.SetRallyPosition(
                targetPos
            );
        }

        // =====================================================
        // KHỞI TẠO PHÁP SƯ
        // =====================================================

        else if (mage != null)
        {
            mage.InitializeStats(
                mageHp,
                mageDamage,
                this
            );

            mage.speed =
                soldierSpeed;

            mage.SetRallyPosition(
                targetPos
            );
        }

        // =====================================================
        // KHỞI TẠO CUNG THỦ
        // =====================================================

        else if (archer != null)
        {
            archer.InitializeStats(
                archerHp,
                archerDamage,
                this
            );

            archer.speed =
                soldierSpeed;

            archer.SetRallyPosition(
                targetPos
            );
        }

        else
        {
            Debug.LogError(
                "❌ Soldier Prefab không có " +
                "PlayerSoldier / MageSoldier / ArcherSoldier: "
                + soldierGO.name
            );
        }

        activeSoldiers.Add(
            soldierGO
        );
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

        float middleDistance =
            GetDistanceToPath(
                map3Waypoints.middlePoints
            );

        float topDistance =
            GetDistanceToPath(
                map3Waypoints.topPoints
            );

        float bottomDistance =
            GetDistanceToPath(
                map3Waypoints.bottomPoints
            );

        // =====================================================
        // CHỌN ĐƯỜNG GẦN NHẤT
        // =====================================================

        float smallestDistance =
            middleDistance;

        selectedMap3Lane =
            Map3Lane.Middle;

        if (topDistance < smallestDistance)
        {
            smallestDistance =
                topDistance;

            selectedMap3Lane =
                Map3Lane.Top;
        }

        if (bottomDistance < smallestDistance)
        {
            smallestDistance =
                bottomDistance;

            selectedMap3Lane =
                Map3Lane.Bottom;
        }

        // =====================================================
        // TÌM WAYPOINT
        // =====================================================

        Transform nearestWaypoint =
            null;

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
    // TÍNH KHOẢNG CÁCH TỚI ĐƯỜNG
    // =========================================================

    float GetDistanceToPath(
        Transform[] points
    )
    {
        if (points == null ||
            points.Length == 0)
        {
            return Mathf.Infinity;
        }

        float minDistance =
            Mathf.Infinity;

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
                minDistance =
                    distance;
            }
        }

        return minDistance;
    }

    // =========================================================
    // LẤY WAYPOINT GẦN NHẤT
    // =========================================================

    Transform GetNearestWaypoint(
        Transform[] points
    )
    {
        if (points == null ||
            points.Length == 0)
        {
            return null;
        }

        float minDistance =
            Mathf.Infinity;

        Transform nearest =
            null;

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
                minDistance =
                    distance;

                nearest =
                    point;
            }
        }

        return nearest;
    }

    // =========================================================
    // VỊ TRÍ LÍNH MAP 3
    // =========================================================

    Vector3 GetMap3SoldierOffset(
        int index
    )
    {
        float spacing =
            0.35f;

        if (index == 0)
            return Vector3.zero;

        if (index == 1)
            return Vector3.left * spacing;

        if (index == 2)
            return Vector3.right * spacing;

        int extraIndex =
            index - 3;

        float row =
            (extraIndex / 3) + 1;

        int position =
            extraIndex % 3;

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

    public void OnSoldierDied(
        MonoBehaviour soldier
    )
    {
        if (soldier != null &&
            activeSoldiers.Contains(
                soldier.gameObject
            ))
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
            Debug.Log(
                "⚠️ Tháp đã đạt Level 3!"
            );

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

        if (GameUI.instance.gold <
            currentUpgradeCost)
        {
            Debug.Log(
                "❌ Không đủ vàng nâng cấp! " +
                "Cần: " +
                currentUpgradeCost +
                " | Có: " +
                GameUI.instance.gold
            );

            return;
        }

        level++;

        // =====================================================
        // LEVEL 2
        // =====================================================

        if (level == 2)
        {
            // ĐẤU SĨ
            warriorHp += 10;
            warriorDamage += 1;

            // CUNG THỦ
            archerHp += 6;
            archerDamage += 2;

            // PHÁP SƯ
            mageHp += 4;
            mageDamage += 2;

            soldierSpeed += 1f;

            maxSoldiers += 2;

            SpawnAdditionalSoldiers(2);
        }

        // =====================================================
        // LEVEL 3
        // =====================================================

        else if (level == 3)
        {
            // ĐẤU SĨ
            warriorHp += 10;
            warriorDamage += 1;

            // CUNG THỦ
            archerHp += 5;
            archerDamage += 2;

            // PHÁP SƯ
            mageHp += 4;
            mageDamage += 2;

            soldierSpeed += 1f;

            maxSoldiers += 3;

            SpawnAdditionalSoldiers(3);
        }

        // =====================================================
        // CẬP NHẬT LÍNH
        // =====================================================

        UpdateAllSoldierStats();

        // =====================================================
        // TRỪ VÀNG
        // =====================================================

        GameUI.instance.AddGold(
            -currentUpgradeCost
        );

        if (UpgradeInfoUI.instance != null)
        {
            UpgradeInfoUI.instance.ShowInfo(
                this
            );
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
            activeSoldiers.Count
        );
    }

    // =========================================================
    // SPAWN THÊM LÍNH
    // =========================================================

    void SpawnAdditionalSoldiers(
        int amount
    )
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
    // CẬP NHẬT CHỈ SỐ TẤT CẢ LÍNH
    // =========================================================

    void UpdateAllSoldierStats()
    {
        foreach (
            var soldierGO in activeSoldiers
        )
        {
            if (soldierGO == null)
                continue;

            PlayerSoldier warrior =
                soldierGO.GetComponent<PlayerSoldier>();

            MageSoldier mage =
                soldierGO.GetComponent<MageSoldier>();

            ArcherSoldier archer =
                soldierGO.GetComponent<ArcherSoldier>();

            // =================================================
            // ĐẤU SĨ
            // =================================================

            if (warrior != null)
            {
                warrior.InitializeStats(
                    warriorHp,
                    warriorDamage,
                    this
                );

                warrior.speed =
                    soldierSpeed;

                warrior.FullHeal();
            }

            // =================================================
            // PHÁP SƯ
            // =================================================

            else if (mage != null)
            {
                mage.InitializeStats(
                    mageHp,
                    mageDamage,
                    this
                );

                mage.speed =
                    soldierSpeed;

                mage.FullHeal();
            }

            // =================================================
            // CUNG THỦ
            // =================================================

            else if (archer != null)
            {
                archer.InitializeStats(
                    archerHp,
                    archerDamage,
                    this
                );

                archer.speed =
                    soldierSpeed;

                archer.FullHeal();
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

        foreach (
            var soldierGO in activeSoldiers
        )
        {
            if (soldierGO != null)
            {
                Destroy(
                    soldierGO
                );
            }
        }

        activeSoldiers.Clear();

        if (targetSpot != null)
        {
            targetSpot.ClearSpot();
        }

        Destroy(
            gameObject
        );
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

        float minDistance =
            Mathf.Infinity;

        Vector3 nearestPoint =
            transform.position;

        foreach (
            Transform wp in Waypoints.points
        )
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
                minDistance =
                    dist;

                nearestPoint =
                    wp.position;
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
        rallyPoint =
            newPoint;
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    void OnDrawGizmosSelected()
    {
        Gizmos.color =
            Color.cyan;

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