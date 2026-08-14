using UnityEngine;
using System.Collections;

public class WaveSpawnerMap3 : MonoBehaviour
{
    [Header("Wave")]
    public Wave[] waves;

    public float timeBetweenWaves = 5f;
    public float countdown = 3f;

    private int currentWaveIndex = 0;

    // Số enemy đang sống của Map3
    private int enemiesAliveMap3 = 0;

    [Header("Waypoint Map3")]
    public WaypointsMap3 waypointsMap3;

    [Header("Boss")]
    public int bossWave = 8;
    public GameObject bossPrefab;

    [Header("Boss UI")]
    public GameObject bossWarningPanel;

    private bool waitingForBossStart = false;
    private bool bossStarted = false;


    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(false);
        }

        StartCoroutine(SpawnWaves());
    }


    // =========================================================
    // CHẠY WAVE 1 -> WAVE 7
    // =========================================================

    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(2f);

        while (currentWaveIndex < waves.Length &&
               currentWaveIndex < 7)
        {
            int waveNumber = currentWaveIndex + 1;

            Debug.Log(
                "=============================="
            );

            Debug.Log(
                "CHUẨN BỊ WAVE " + waveNumber
            );

            Debug.Log(
                "=============================="
            );


            // -------------------------------------------------
            // COUNTDOWN
            // -------------------------------------------------

            yield return new WaitForSeconds(countdown);


            // -------------------------------------------------
            // RESET SỐ ENEMY
            // -------------------------------------------------

            enemiesAliveMap3 = 0;


            // -------------------------------------------------
            // SPAWN WAVE
            // -------------------------------------------------

            yield return StartCoroutine(
                SpawnCurrentWave()
            );


            Debug.Log(
                "Wave " +
                waveNumber +
                " đã spawn xong."
            );

            Debug.Log(
                "Enemy đang sống: " +
                enemiesAliveMap3
            );


            // -------------------------------------------------
            // CHỜ TẤT CẢ ENEMY CHẾT
            // -------------------------------------------------

            yield return new WaitUntil(
                () => enemiesAliveMap3 <= 0
            );


            Debug.Log(
                "=============================="
            );

            Debug.Log(
                "WAVE " +
                waveNumber +
                " ĐÃ HOÀN THÀNH!"
            );

            Debug.Log(
                "=============================="
            );


            // -------------------------------------------------
            // TĂNG WAVE
            // -------------------------------------------------

            currentWaveIndex++;


            // -------------------------------------------------
            // NẾU CHƯA PHẢI WAVE 7
            // -------------------------------------------------

            if (currentWaveIndex < 7 &&
                currentWaveIndex < waves.Length)
            {
                Debug.Log(
                    "Wave tiếp theo sau " +
                    timeBetweenWaves +
                    " giây..."
                );

                yield return new WaitForSeconds(
                    timeBetweenWaves
                );
            }
        }


        // =====================================================
        // HOÀN THÀNH WAVE 7
        // =====================================================

        if (currentWaveIndex >= 7)
        {
            Debug.Log(
                "================================"
            );

            Debug.Log(
                "ĐÃ HOÀN THÀNH WAVE 7"
            );

            Debug.Log(
                "HIỆN BẢNG BOSS"
            );

            Debug.Log(
                "================================"
            );

            ShowBossPanel();
        }
    }


    // =========================================================
    // SPAWN WAVE
    // =========================================================

    IEnumerator SpawnCurrentWave()
    {
        if (currentWaveIndex >= waves.Length)
        {
            yield break;
        }


        int waveNumber =
            currentWaveIndex + 1;

        Wave wave =
            waves[currentWaveIndex];


        Debug.Log(
            "========== BẮT ĐẦU WAVE " +
            waveNumber +
            " =========="
        );


        // -------------------------------------------------
        // KIỂM TRA PREFAB
        // -------------------------------------------------

        if (wave.enemyPrefab == null)
        {
            Debug.LogError(
                "Wave " +
                waveNumber +
                " chưa gán Enemy Prefab!"
            );

            yield break;
        }


        // -------------------------------------------------
        // KIỂM TRA COUNT
        // -------------------------------------------------

        if (wave.count <= 0)
        {
            Debug.LogError(
                "Wave " +
                waveNumber +
                " có Count = 0!"
            );

            yield break;
        }


        // -------------------------------------------------
        // SPAWN TỪNG ENEMY
        // -------------------------------------------------

        for (int i = 0; i < wave.count; i++)
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

                // Gắn callback theo dõi enemy chết
                EnemyMap3Tracker tracker =
                    enemy.GetComponent<EnemyMap3Tracker>();

                if (tracker == null)
                {
                    tracker =
                        enemy.AddComponent<EnemyMap3Tracker>();
                }

                tracker.spawner = this;
            }


            // -------------------------------------------------
            // DELAY GIỮA CÁC ENEMY
            // -------------------------------------------------

            if (wave.rate > 0f)
            {
                yield return new WaitForSeconds(
                    wave.rate
                );
            }
        }


        Debug.Log(
            "Spawn xong Wave " +
            waveNumber +
            " | Tổng Enemy: " +
            enemiesAliveMap3
        );
    }


    // =========================================================
    // SPAWN ENEMY
    // =========================================================

    GameObject SpawnEnemy(
        GameObject prefab,
        Transform[] path
    )
    {
        if (prefab == null)
        {
            Debug.LogError(
                "Enemy Prefab NULL!"
            );

            return null;
        }


        if (path == null ||
            path.Length == 0)
        {
            Debug.LogError(
                "Waypoint Map3 bị thiếu!"
            );

            return null;
        }


        GameObject enemy =
            Instantiate(
                prefab,
                path[0].position,
                Quaternion.identity
            );


        Enemy enemyScript =
            enemy.GetComponent<Enemy>();


        if (enemyScript == null)
        {
            Debug.LogError(
                "Enemy Prefab không có Enemy.cs!"
            );

            Destroy(enemy);

            return null;
        }


        // Gán đường đi riêng cho Map3
        enemyScript.customWaypoints =
            path;


        return enemy;
    }


    // =========================================================
    // CHỌN ĐƯỜNG CHO ENEMY
    // =========================================================

    Transform[] GetPathForEnemy(
        int waveNumber,
        int enemyIndex
    )
    {
        if (waypointsMap3 == null)
        {
            Debug.LogError(
                "Chưa gán WaypointsMap3!"
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
            else
            {
                return waypointsMap3.topPoints;
            }
        }


        // =====================================================
        // WAVE 3 -> WAVE 7
        // CẢ 3 ĐƯỜNG
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
    // ENEMY CHẾT
    // =========================================================

    public void OnEnemyMap3Died()
    {
        enemiesAliveMap3--;

        if (enemiesAliveMap3 < 0)
        {
            enemiesAliveMap3 = 0;
        }


        Debug.Log(
            "Enemy Map3 chết. Còn lại: " +
            enemiesAliveMap3
        );
    }


    // =========================================================
    // BOSS WARNING PANEL
    // =========================================================

    void ShowBossPanel()
    {
        waitingForBossStart = true;


        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(true);
        }


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "BOSS WAVE 8 ĐANG CHỜ!"
        );

        Debug.Log(
            "Bấm nút BẮT ĐẦU để xuất hiện Boss."
        );

        Debug.Log(
            "================================"
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
                "Chưa đến lúc bắt đầu Boss!"
            );

            return;
        }


        if (bossStarted)
        {
            return;
        }


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
                "Chưa gán Boss Prefab!"
            );

            return;
        }


        if (waypointsMap3 == null)
        {
            Debug.LogError(
                "Chưa gán WaypointsMap3!"
            );

            return;
        }


        if (waypointsMap3.middlePoints == null ||
            waypointsMap3.middlePoints.Length == 0)
        {
            Debug.LogError(
                "Middle Points chưa được gán!"
            );

            return;
        }


        // Boss chỉ đi đường giữa
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
                "Boss Prefab không có BossEnemy.cs!"
            );
        }


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "BOSS WAVE 8 ĐÃ XUẤT HIỆN!"
        );

        Debug.Log(
            "================================"
        );
    }
}


// =============================================================
// TRACKER CHO ENEMY MAP3
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