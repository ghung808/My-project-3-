using UnityEngine;
using System.Collections;

public class WaveSpawnerMap3 : MonoBehaviour
{
    [Header("Wave")]
    public Wave[] waves;

    public float timeBetweenWaves = 5f;
    public float countdown = 3f;

    private int currentWaveIndex = 0;

    // =========================================================
    // SỐ ENEMY ĐANG SỐNG
    // =========================================================

    private int enemiesAliveMap3 = 0;

    // =========================================================
    // WAYPOINT MAP 3
    // =========================================================

    [Header("Waypoint Map3")]
    public WaypointsMap3 waypointsMap3;

    // =========================================================
    // BOSS
    // =========================================================

    [Header("Boss")]
    public int bossWave = 12;
    public GameObject bossPrefab;

    [Header("Boss UI")]
    public GameObject bossWarningPanel;

    private bool waitingForBossStart = false;
    private bool bossStarted = false;


    // =========================================================
    // LẤY WAVE HIỆN TẠI
    // =========================================================

    public int GetCurrentWaveNumber()
    {
        return currentWaveIndex + 1;
    }


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(false);
        }

        if (GameUI.instance != null)
        {
            GameUI.instance.maxWave = 12;
            GameUI.instance.currentWave = 1;
            GameUI.instance.UpdateUI();
        }

        if (waves == null || waves.Length < 12)
        {
            Debug.LogError(
                "❌ MAP 3 CHƯA ĐỦ 12 WAVE! Hiện tại: " +
                (waves == null ? 0 : waves.Length)
            );

            return;
        }

        if (waypointsMap3 == null)
        {
            Debug.LogError(
                "❌ MAP 3 CHƯA GÁN WaypointsMap3!"
            );

            return;
        }

        Debug.Log("================================");
        Debug.Log("🟦 MAP 3 BẮT ĐẦU");
        Debug.Log("🟦 TỔNG SỐ WAVE: 12");
        Debug.Log("================================");

        StartCoroutine(SpawnWaves());
    }


    // =========================================================
    // WAVE 1 → 12
    // =========================================================

    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(2f);

        while (
            currentWaveIndex < waves.Length &&
            currentWaveIndex < 12
        )
        {
            int waveNumber = currentWaveIndex + 1;

            if (GameUI.instance != null)
            {
                GameUI.instance.currentWave = waveNumber;
                GameUI.instance.maxWave = 12;
                GameUI.instance.UpdateUI();
            }

            Debug.Log("================================");
            Debug.Log(
                "⚔️ CHUẨN BỊ WAVE " +
                waveNumber +
                " / 12"
            );
            Debug.Log("================================");

            yield return new WaitForSeconds(countdown);

            enemiesAliveMap3 = 0;

            yield return StartCoroutine(
                SpawnCurrentWave()
            );

            Debug.Log(
                "🟢 SPAWN XONG WAVE " +
                waveNumber +
                " | Enemy đang sống: " +
                enemiesAliveMap3
            );

            yield return new WaitUntil(
                () => enemiesAliveMap3 <= 0
            );

            Debug.Log(
                "✅ WAVE " +
                waveNumber +
                " / 12 HOÀN THÀNH!"
            );

            currentWaveIndex++;

            if (currentWaveIndex >= 12)
            {
                if (GameUI.instance != null)
                {
                    GameUI.instance.currentWave = 12;
                    GameUI.instance.maxWave = 12;
                    GameUI.instance.UpdateUI();
                }

                Debug.Log(
                    "🎉 ĐÃ HOÀN THÀNH TẤT CẢ 12 WAVE!"
                );

                Debug.Log(
                    "💀 CHUẨN BỊ HIỆN BẢNG BOSS!"
                );

                ShowBossPanel();

                yield break;
            }

            int nextWave = currentWaveIndex + 1;

            if (GameUI.instance != null)
            {
                GameUI.instance.currentWave = nextWave;
                GameUI.instance.maxWave = 12;
                GameUI.instance.UpdateUI();
            }

            yield return new WaitForSeconds(
                timeBetweenWaves
            );
        }

        if (currentWaveIndex >= 12)
        {
            if (!bossStarted)
            {
                ShowBossPanel();
            }
        }
    }


    // =========================================================
    // SPAWN WAVE HIỆN TẠI
    // =========================================================

    IEnumerator SpawnCurrentWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            yield break;
        }

        int waveNumber = currentWaveIndex + 1;

        Wave wave = waves[currentWaveIndex];

        Debug.Log(
            "========== BẮT ĐẦU WAVE " +
            waveNumber +
            " / 12 =========="
        );

        if (wave.enemyPrefab == null)
        {
            Debug.LogError(
                "❌ Wave " +
                waveNumber +
                " chưa gán Enemy Prefab!"
            );

            yield break;
        }

        if (wave.count <= 0)
        {
            Debug.LogError(
                "❌ Wave " +
                waveNumber +
                " có Count = 0!"
            );

            yield break;
        }

        Debug.Log(
            "🟢 WAVE " +
            waveNumber +
            " SẼ SPAWN: " +
            wave.count +
            " ENEMY"
        );

        for (
            int i = 0;
            i < wave.count;
            i++
        )
        {
            Transform[] selectedPath =
                GetPathForEnemy(
                    waveNumber,
                    i
                );

            GameObject enemy =
                SpawnEnemy(
                    wave.enemyPrefab,
                    selectedPath
                );

            if (enemy != null)
            {
                enemiesAliveMap3++;

                EnemyMap3Tracker tracker =
                    enemy.GetComponent<EnemyMap3Tracker>();

                if (tracker == null)
                {
                    tracker =
                        enemy.AddComponent<EnemyMap3Tracker>();
                }

                tracker.spawner = this;

                Debug.Log(
                    "🟢 Spawn Enemy " +
                    (i + 1) +
                    "/" +
                    wave.count +
                    " | Wave " +
                    waveNumber
                );
            }

            if (wave.rate > 0f)
            {
                yield return new WaitForSeconds(
                    wave.rate
                );
            }
        }

        Debug.Log(
            "🟢 Spawn xong Wave " +
            waveNumber +
            " | Tổng Enemy: " +
            enemiesAliveMap3
        );
    }


    // =========================================================
    // SPAWN ENEMY
    // =========================================================
    // QUAN TRỌNG:
    // Enemy được tắt trước khi Start() chạy.
    // Sau đó gán customWaypoints rồi mới bật.
    // =========================================================

    GameObject SpawnEnemy(
        GameObject prefab,
        Transform[] path
    )
    {
        if (prefab == null)
        {
            Debug.LogError(
                "❌ Enemy Prefab NULL!"
            );

            return null;
        }

        if (
            path == null ||
            path.Length == 0
        )
        {
            Debug.LogError(
                "❌ Waypoint Map3 bị thiếu!"
            );

            return null;
        }

        // =====================================================
        // TẠO ENEMY
        // =====================================================

        GameObject enemy =
            Instantiate(
                prefab,
                path[0].position,
                Quaternion.identity
            );

        // =====================================================
        // TẮT NGAY
        // =====================================================

        enemy.SetActive(false);

        // =====================================================
        // LẤY SCRIPT
        // =====================================================

        Enemy enemyScript =
            enemy.GetComponent<Enemy>();

        if (enemyScript == null)
        {
            Debug.LogError(
                "❌ Enemy Prefab không có Enemy.cs!"
            );

            Destroy(enemy);

            return null;
        }

        // =====================================================
        // GÁN ĐƯỜNG TRƯỚC KHI BẬT
        // =====================================================

        enemyScript.customWaypoints = path;

        Debug.Log(
            "🛣️ Enemy nhận đường: " +
            path[0].name +
            " → " +
            path[path.Length - 1].name
        );

        // =====================================================
        // BẬT ENEMY
        // =====================================================

        enemy.SetActive(true);

        return enemy;
    }


    // =========================================================
    // CHỌN ĐƯỜNG
    // =========================================================

    Transform[] GetPathForEnemy(
        int waveNumber,
        int enemyIndex
    )
    {
        if (waypointsMap3 == null)
        {
            Debug.LogError(
                "❌ Chưa gán WaypointsMap3!"
            );

            return null;
        }

        // =====================================================
        // WAVE 1
        // CHỈ ĐƯỜNG GIỮA
        // =====================================================

        if (waveNumber == 1)
        {
            return waypointsMap3.middlePoints;
        }

        // =====================================================
        // WAVE 2
        // GIỮA + TRÊN
        // =====================================================

        if (waveNumber == 2)
        {
            if (enemyIndex % 2 == 0)
            {
                return waypointsMap3.middlePoints;
            }

            return waypointsMap3.topPoints;
        }

        // =====================================================
        // WAVE 3 → 12
        // 3 ĐƯỜNG
        // =====================================================

        int lane =
            enemyIndex % 3;

        if (lane == 0)
        {
            return waypointsMap3.middlePoints;
        }

        if (lane == 1)
        {
            return waypointsMap3.topPoints;
        }

        return waypointsMap3.bottomPoints;
    }


    // =========================================================
    // ENEMY MAP 3 CHẾT
    // =========================================================

    public void OnEnemyMap3Died()
    {
        enemiesAliveMap3--;

        if (enemiesAliveMap3 < 0)
        {
            enemiesAliveMap3 = 0;
        }

        Debug.Log(
            "💀 Enemy Map3 chết | Còn lại: " +
            enemiesAliveMap3
        );
    }


    // =========================================================
    // HIỆN BẢNG BOSS
    // =========================================================

    void ShowBossPanel()
    {
        if (bossStarted)
            return;

        if (waitingForBossStart)
            return;

        waitingForBossStart = true;

        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(true);
        }

        Debug.Log(
            "💀 ĐÃ HOÀN THÀNH WAVE 12!"
        );

        Debug.Log(
            "⚠️ BOSS ĐANG CHỜ!"
        );
    }


    // =========================================================
    // NÚT BẮT ĐẦU BOSS
    // =========================================================

    public void StartBoss()
    {
        if (!waitingForBossStart)
        {
            Debug.LogWarning(
                "⚠️ Chưa đến lúc bắt đầu Boss!"
            );

            return;
        }

        if (bossStarted)
            return;

        waitingForBossStart = false;
        bossStarted = true;

        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(false);
        }

        SpawnBoss();
    }


    // =========================================================
    // SPAWN BOSS
    // =========================================================

    void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError(
                "❌ Chưa gán Boss Prefab!"
            );

            return;
        }

        if (waypointsMap3 == null)
        {
            Debug.LogError(
                "❌ Chưa gán WaypointsMap3!"
            );

            return;
        }

        if (
            waypointsMap3.middlePoints == null ||
            waypointsMap3.middlePoints.Length == 0
        )
        {
            Debug.LogError(
                "❌ Middle Points chưa được gán!"
            );

            return;
        }

        Transform[] bossPath =
            waypointsMap3.middlePoints;

        GameObject boss =
            Instantiate(
                bossPrefab,
                bossPath[0].position,
                Quaternion.identity
            );

        BossEnemy bossScript =
            boss.GetComponent<BossEnemy>();

        if (bossScript != null)
        {
            bossScript.customWaypoints =
                bossPath;
        }
        else
        {
            Debug.LogError(
                "❌ Boss Prefab không có BossEnemy.cs!"
            );
        }

        Debug.Log(
            "💀 BOSS ĐI ĐƯỜNG GIỮA!"
        );
    }
}


// =============================================================
// TRACKER ENEMY MAP 3
// =============================================================

public class EnemyMap3Tracker : MonoBehaviour
{
    [HideInInspector]
    public WaveSpawnerMap3 spawner;

    private bool hasReportedDeath = false;

    void OnDestroy()
    {
        if (hasReportedDeath)
            return;

        hasReportedDeath = true;

        if (spawner != null)
        {
            spawner.OnEnemyMap3Died();
        }
    }
}