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
        // -----------------------------------------------------
        // ẨN BẢNG BOSS
        // -----------------------------------------------------

        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(false);
        }


        // -----------------------------------------------------
        // GAME UI
        // -----------------------------------------------------

        if (GameUI.instance != null)
        {
            GameUI.instance.maxWave = 12;
            GameUI.instance.currentWave = 1;
            GameUI.instance.UpdateUI();
        }


        // -----------------------------------------------------
        // KIỂM TRA 12 WAVE
        // -----------------------------------------------------

        if (waves == null || waves.Length < 12)
        {
            Debug.LogError(
                "❌ MAP 3 CHƯA ĐỦ 12 WAVE! " +
                "Hiện tại chỉ có " +
                (waves == null ? 0 : waves.Length) +
                " Wave."
            );

            return;
        }


        // -----------------------------------------------------
        // KIỂM TRA WAYPOINT
        // -----------------------------------------------------

        if (waypointsMap3 == null)
        {
            Debug.LogError(
                "❌ MAP 3 CHƯA GÁN WaypointsMap3!"
            );

            return;
        }


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "🟦 MAP 3 BẮT ĐẦU"
        );

        Debug.Log(
            "🟦 TỔNG SỐ WAVE: 12"
        );

        Debug.Log(
            "================================"
        );


        StartCoroutine(SpawnWaves());
    }


    // =========================================================
    // CHẠY WAVE 1 → 12
    // =========================================================

    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(2f);


        while (
            currentWaveIndex < waves.Length &&
            currentWaveIndex < 12
        )
        {
            int waveNumber =
                currentWaveIndex + 1;


            // =================================================
            // CẬP NHẬT UI
            // =================================================

            if (GameUI.instance != null)
            {
                GameUI.instance.currentWave =
                    waveNumber;

                GameUI.instance.maxWave =
                    12;

                GameUI.instance.UpdateUI();
            }


            Debug.Log(
                "================================"
            );

            Debug.Log(
                "⚔️ CHUẨN BỊ WAVE " +
                waveNumber +
                " / 12"
            );

            Debug.Log(
                "================================"
            );


            // =================================================
            // COUNTDOWN
            // =================================================

            yield return new WaitForSeconds(
                countdown
            );


            // =================================================
            // RESET ENEMY
            // =================================================

            enemiesAliveMap3 = 0;


            // =================================================
            // SPAWN WAVE
            // =================================================

            yield return StartCoroutine(
                SpawnCurrentWave()
            );


            Debug.Log(
                "================================"
            );

            Debug.Log(
                "🟢 SPAWN XONG WAVE " +
                waveNumber
            );

            Debug.Log(
                "🟢 Enemy đang sống: " +
                enemiesAliveMap3
            );

            Debug.Log(
                "================================"
            );


            // =================================================
            // CHỜ ENEMY CHẾT HẾT
            // =================================================

            yield return new WaitUntil(
                () => enemiesAliveMap3 <= 0
            );


            Debug.Log(
                "================================"
            );

            Debug.Log(
                "✅ WAVE " +
                waveNumber +
                " / 12 HOÀN THÀNH!"
            );

            Debug.Log(
                "================================"
            );


            // =================================================
            // TĂNG WAVE
            // =================================================

            currentWaveIndex++;


            // =================================================
            // ĐÃ XONG WAVE 12
            // =================================================

            if (currentWaveIndex >= 12)
            {
                if (GameUI.instance != null)
                {
                    GameUI.instance.currentWave = 12;
                    GameUI.instance.maxWave = 12;
                    GameUI.instance.UpdateUI();
                }


                Debug.Log(
                    "========================================"
                );

                Debug.Log(
                    "🎉 ĐÃ HOÀN THÀNH TẤT CẢ 12 WAVE!"
                );

                Debug.Log(
                    "💀 CHUẨN BỊ HIỆN BẢNG BOSS!"
                );

                Debug.Log(
                    "========================================"
                );


                ShowBossPanel();

                yield break;
            }


            // =================================================
            // WAVE TIẾP THEO
            // =================================================

            int nextWave =
                currentWaveIndex + 1;


            if (GameUI.instance != null)
            {
                GameUI.instance.currentWave =
                    nextWave;

                GameUI.instance.maxWave =
                    12;

                GameUI.instance.UpdateUI();
            }


            Debug.Log(
                "➡️ Wave tiếp theo: " +
                nextWave +
                " / 12"
            );


            Debug.Log(
                "⏳ Wave tiếp theo sau " +
                timeBetweenWaves +
                " giây..."
            );


            yield return new WaitForSeconds(
                timeBetweenWaves
            );
        }


        // =================================================
        // KIỂM TRA CUỐI
        // =================================================

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
        if (
            currentWaveIndex >=
            waves.Length
        )
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
            " / 12 =========="
        );


        // =================================================
        // KIỂM TRA PREFAB
        // =================================================

        if (wave.enemyPrefab == null)
        {
            Debug.LogError(
                "❌ Wave " +
                waveNumber +
                " chưa gán Enemy Prefab!"
            );

            yield break;
        }


        // =================================================
        // KIỂM TRA COUNT
        // =================================================

        if (wave.count <= 0)
        {
            Debug.LogError(
                "❌ Wave " +
                waveNumber +
                " có Count = 0!"
            );

            yield break;
        }


        // =================================================
        // SPAWN TỪNG ENEMY
        // =================================================

        for (
            int i = 0;
            i < wave.count;
            i++
        )
        {
            // -------------------------------------------------
            // CHỌN ĐƯỜNG
            // -------------------------------------------------

            Transform[] selectedPath =
                GetPathForEnemy(
                    waveNumber,
                    i
                );


            // -------------------------------------------------
            // SPAWN
            // -------------------------------------------------

            GameObject enemy =
                SpawnEnemy(
                    wave.enemyPrefab,
                    selectedPath
                );


            if (enemy != null)
            {
                enemiesAliveMap3++;


                // =================================================
                // TRACKER
                // =================================================

                EnemyMap3Tracker tracker =
                    enemy.GetComponent<EnemyMap3Tracker>();


                if (tracker == null)
                {
                    tracker =
                        enemy.AddComponent<EnemyMap3Tracker>();
                }


                tracker.spawner = this;


                Debug.Log(
                    "🟢 Spawn Enemy Map3 " +
                    (i + 1) +
                    "/" +
                    wave.count +
                    " | Wave " +
                    waveNumber
                );
            }


            // -------------------------------------------------
            // DELAY
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
            " / 12" +
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
                "❌ Enemy Prefab NULL!"
            );

            return null;
        }


        // =================================================
        // KIỂM TRA PATH
        // =================================================

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


        // =================================================
        // SPAWN TẠI WAYPOINT ĐẦU TIÊN
        // =================================================

        GameObject enemy =
            Instantiate(
                prefab,
                path[0].position,
                Quaternion.identity
            );


        // =================================================
        // LẤY ENEMY SCRIPT
        // =================================================

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


        // =================================================
        // RẤT QUAN TRỌNG
        // GÁN ĐƯỜNG RIÊNG CHO MAP 3
        // =================================================

        enemyScript.customWaypoints =
            path;


        Debug.Log(
            "🛣️ Enemy Map3 nhận " +
            path.Length +
            " Waypoint."
        );


        return enemy;
    }


    // =========================================================
    // CHỌN ĐƯỜNG ENEMY
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


        // =================================================
        // WAVE 1
        // CHỈ ĐƯỜNG GIỮA
        // =================================================

        if (waveNumber == 1)
        {
            return waypointsMap3.middlePoints;
        }


        // =================================================
        // WAVE 2
        // GIỮA + TRÊN
        // =================================================

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


        // =================================================
        // WAVE 3 → 12
        // CẢ 3 ĐƯỜNG
        // =================================================

        int lane =
            enemyIndex % 3;


        // -------------------------------------------------
        // ĐƯỜNG GIỮA
        // -------------------------------------------------

        if (lane == 0)
        {
            return waypointsMap3.middlePoints;
        }


        // -------------------------------------------------
        // ĐƯỜNG TRÊN
        // -------------------------------------------------

        if (lane == 1)
        {
            return waypointsMap3.topPoints;
        }


        // -------------------------------------------------
        // ĐƯỜNG DƯỚI
        // -------------------------------------------------

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
        {
            return;
        }


        if (waitingForBossStart)
        {
            return;
        }


        waitingForBossStart = true;


        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(true);
        }


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "💀 ĐÃ HOÀN THÀNH WAVE 12!"
        );

        Debug.Log(
            "⚠️ BOSS ĐANG CHỜ!"
        );

        Debug.Log(
            "👉 Bấm nút BẮT ĐẦU để xuất hiện Boss."
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
                "⚠️ Chưa đến lúc bắt đầu Boss!"
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


        Debug.Log(
            "================================"
        );

        Debug.Log(
            "🔥 NGƯỜI CHƠI ĐÃ BẤM BẮT ĐẦU!"
        );

        Debug.Log(
            "💀 BOSS ĐANG XUẤT HIỆN!"
        );

        Debug.Log(
            "================================"
        );


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


        // =================================================
        // BOSS CHỈ ĐI ĐƯỜNG GIỮA
        // =================================================

        Transform[] bossPath =
            waypointsMap3.middlePoints;


        GameObject boss =
            Instantiate(
                bossPrefab,
                bossPath[0].position,
                Quaternion.identity
            );


        // =================================================
        // BOSS SCRIPT
        // =================================================

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
            "================================"
        );

        Debug.Log(
            "💀 BOSS SAU WAVE 12 ĐÃ XUẤT HIỆN!"
        );

        Debug.Log(
            "💀 BOSS ĐI ĐƯỜNG GIỮA!"
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
        {
            return;
        }


        hasReportedDeath = true;


        if (spawner != null)
        {
            spawner.OnEnemyMap3Died();
        }
    }
}