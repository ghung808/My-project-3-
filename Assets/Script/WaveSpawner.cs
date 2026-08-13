using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public GameObject enemyPrefab;
        public int count;
        public float rate;
    }

    [Header("Cấu hình Wave quái")]
    public Wave[] waves;
    public Transform spawnPoint;
    public float timeBetweenWaves = 5f;

    [Header("Thời gian xây")]
    public float firstBuildTime = 10f;

    [Header("Boss")]
    public GameObject bossPrefab;
    public bool spawnBossAfterLastWave = true;

    [Header("Boss Warning UI - CHỈ DÙNG CHO MAP 2")]
    public BossWarningUI bossWarningUI;

    private int currentWaveIndex = 0;
    private float waveCountdown;
    private bool bossSpawned = false;
    private bool guideReady = false;

    // Map 1 - Vht
    private bool waitingForWave1Guide = false;
    private bool waitingForWave2Guide = false;
    private bool waitingForWave3Mission = false;

    // Dùng chung cho lúc chờ xác nhận Boss
    private bool waitingForBossWarning = false;

    private enum SpawnState
    {
        SPAWNING,
        WAITING,
        COUNTING
    }

    private SpawnState state = SpawnState.COUNTING;

    // =========================================================
    // KIỂM TRA MAP
    // =========================================================

    bool IsMap1()
    {
        return SceneManager.GetActiveScene().name == "Vht";
    }

    // =========================================================
    // START
    // =========================================================

    void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        // =====================================================
        // MAP 1
        // =====================================================

        if (IsMap1())
        {
            guideReady = false;

            // MAP 1 TUYỆT ĐỐI KHÔNG TỰ DÙNG BossWarningUI MỚI
            bossWarningUI = null;

            Debug.Log("🟦 MAP 1 (Vht) - Dùng hệ thống GuideManager cũ.");
        }
        else
        {
            // =================================================
            // MAP 2 / MAP 3
            // =================================================

            guideReady = true;

            // Chỉ Map 2/3 mới tìm BossWarningUI
            if (bossWarningUI == null)
            {
                bossWarningUI = FindFirstObjectByType<BossWarningUI>();
            }

            if (bossWarningUI != null)
            {
                Debug.Log("🟩 MAP 2/3 - Đã kết nối BossWarningUI.");
            }
            else
            {
                Debug.LogWarning("⚠️ Map 2/3 không tìm thấy BossWarningUI.");
            }
        }

        waveCountdown = 0f;

        BuildingSpot2D.canBuild = true;

        if (GameUI.instance != null)
        {
            GameUI.instance.maxWave = waves.Length;
            GameUI.instance.currentWave = 1;
            GameUI.instance.UpdateUI();
        }
    }

    // =========================================================
    // UPDATE
    // =========================================================

    void Update()
    {
        if (!guideReady)
            return;

        if (waitingForWave2Guide)
            return;

        if (waitingForWave1Guide)
            return;

        if (waitingForWave3Mission)
            return;

        if (waitingForBossWarning)
            return;

        if (state == SpawnState.WAITING)
        {
            if (!EnemyIsAlive())
            {
                WaveCompleted();
                return;
            }
            else
            {
                return;
            }
        }

        if (waveCountdown <= 0)
        {
            if (state != SpawnState.SPAWNING &&
                currentWaveIndex < waves.Length)
            {
                // Bắt đầu Wave mới → khóa xây
                BuildingSpot2D.canBuild = false;

                // =================================================
                // CHỈ MAP 1 - HƯỚNG DẪN NÂNG CẤP WAVE 4
                // =================================================

                if (currentWaveIndex == 3 && IsMap1())
                {
                    if (GuideManager.instance != null)
                    {
                        GuideManager.instance.StartWave4UpgradeGuide();
                    }
                }

                StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            }
        }
        else
        {
            waveCountdown -= Time.deltaTime;
        }
    }

    // =========================================================
    // WAVE COMPLETED
    // =========================================================

    void WaveCompleted()
    {
        Debug.Log("Wave " + (currentWaveIndex + 1) + " đã hoàn thành!");

        // Cho phép xây trong thời gian nghỉ
        BuildingSpot2D.canBuild = true;

        // Tăng số Wave
        currentWaveIndex++;

        // =====================================================
        // MAP 1 - SAU WAVE 1
        // =====================================================

        if (currentWaveIndex == 1 && IsMap1())
        {
            waitingForWave1Guide = true;

            state = SpawnState.COUNTING;
            waveCountdown = 0f;

            if (GuideManager.instance != null)
            {
                GuideManager.instance.ShowWave1Complete();
            }

            Debug.Log("🎉 Map 1 - Wave 1 hoàn thành, tiếp tục tutorial!");

            return;
        }

        // =====================================================
        // MAP 1 - SAU WAVE 2
        // =====================================================

        if (currentWaveIndex == 2 && IsMap1())
        {
            waitingForWave2Guide = true;

            state = SpawnState.COUNTING;
            BuildingSpot2D.canBuild = true;
            waveCountdown = 0f;

            if (GuideManager.instance != null)
            {
                GuideManager.instance.ShowMageUnlockPanel();
            }

            Debug.Log("🎉 Map 1 - Wave 2 hoàn thành, tiếp tục tutorial!");

            return;
        }

        // =====================================================
        // MAP 1 - SAU WAVE 3
        // =====================================================

        if (currentWaveIndex == 3 && IsMap1())
        {
            waitingForWave3Mission = true;

            state = SpawnState.COUNTING;
            waveCountdown = 0f;
            BuildingSpot2D.canBuild = true;

            if (GuideManager.instance != null)
            {
                GuideManager.instance.ShowWave3Mission();
            }

            Debug.Log("🎉 Map 1 - Wave 3 hoàn thành, tiếp tục tutorial!");

            return;
        }

        // =====================================================
        // SAU WAVE 7
        // =====================================================

        if (currentWaveIndex == 7)
        {
            waitingForBossWarning = true;

            state = SpawnState.COUNTING;
            waveCountdown = 0f;

            BuildingSpot2D.canBuild = true;

            // =================================================
            // MAP 1 → GUIDE MANAGER CŨ
            // =================================================

            if (IsMap1())
            {
                if (GuideManager.instance != null)
                {
                    GuideManager.instance.ShowBossWarning();

                    Debug.Log(
                        "⚠️ MAP 1 - WAVE 7 HOÀN THÀNH - HIỆN BOSS WARNING CŨ!"
                    );
                }
                else
                {
                    Debug.LogError(
                        "❌ MAP 1 KHÔNG TÌM THẤY GUIDEMANAGER!"
                    );
                }

                return;
            }

            // =================================================
            // MAP 2 / MAP 3 → BOSSWARNINGUI
            // =================================================

            if (bossWarningUI != null)
            {
                bossWarningUI.ShowWarning();

                Debug.Log(
                    "⚠️ MAP 2/3 - WAVE 7 HOÀN THÀNH - HIỆN BOSS WARNING MỚI!"
                );
            }
            else
            {
                Debug.LogError(
                    "❌ MAP 2/3 KHÔNG TÌM THẤY BOSSWARNINGUI!"
                );
            }

            return;
        }

        // =====================================================
        // CÁC WAVE SAU
        // =====================================================

        state = SpawnState.COUNTING;
        waveCountdown = timeBetweenWaves;

        if (GameUI.instance != null)
        {
            GameUI.instance.currentWave = currentWaveIndex + 1;
        }

        // =====================================================
        // ĐÃ HẾT TẤT CẢ WAVE
        // =====================================================

        if (currentWaveIndex >= waves.Length)
        {
            BuildingSpot2D.canBuild = false;

            if (spawnBossAfterLastWave && !bossSpawned)
            {
                state = SpawnState.COUNTING;
                waveCountdown = 0f;

                // =============================================
                // MAP 1 → GUIDE MANAGER CŨ
                // =============================================

                if (IsMap1())
                {
                    waitingForBossWarning = true;

                    if (GuideManager.instance != null)
                    {
                        GuideManager.instance.ShowBossWarning();

                        Debug.Log(
                            "⚠️ MAP 1 - TẤT CẢ WAVE HOÀN THÀNH - CHỜ BOSS!"
                        );
                    }

                    return;
                }

                // =============================================
                // MAP 2/3 → BOSSWARNINGUI
                // =============================================

                waitingForBossWarning = true;

                if (bossWarningUI != null)
                {
                    bossWarningUI.ShowWarning();

                    Debug.Log(
                        "⚠️ MAP 2/3 - TẤT CẢ WAVE HOÀN THÀNH - CHỜ BOSS!"
                    );
                }

                return;
            }

            return;
        }
    }

    // =========================================================
    // MAP 1 - GUIDE
    // =========================================================

    public void StartBattleAfterGuide()
    {
        guideReady = true;
        waveCountdown = 0f;

        Debug.Log("Hướng dẫn xong - Wave 1 bắt đầu!");
    }

    public void ContinueAfterArcherGuide()
    {
        waitingForWave1Guide = false;
        waveCountdown = timeBetweenWaves;
        state = SpawnState.COUNTING;

        Debug.Log(
            "✅ Đã hoàn thành hướng dẫn Cung Thủ - Wave 2 sẽ bắt đầu sau "
            + timeBetweenWaves + " giây!"
        );
    }

    public void StartWave2AfterGuide()
    {
        waitingForWave1Guide = false;

        state = SpawnState.COUNTING;
        waveCountdown = 0f;

        BuildingSpot2D.canBuild = false;

        Debug.Log(
            "🔥 Hướng dẫn Cung Thủ hoàn tất - Wave 2 bắt đầu!"
        );
    }

    public void StartWave3AfterGuide()
    {
        waitingForWave2Guide = false;

        guideReady = true;

        state = SpawnState.COUNTING;
        waveCountdown = 0f;

        BuildingSpot2D.canBuild = false;

        Debug.Log(
            "🔮 Hướng dẫn Pháp Sư hoàn thành - Wave 3 bắt đầu!"
        );
    }

    public void ContinueAfterWave3Mission()
    {
        waitingForWave3Mission = false;

        state = SpawnState.COUNTING;
        waveCountdown = timeBetweenWaves;

        BuildingSpot2D.canBuild = true;

        Debug.Log(
            "🔥 Đã hoàn thành nhiệm vụ - Wave 4 sẽ bắt đầu sau "
            + timeBetweenWaves + " giây!"
        );
    }

    // =========================================================
    // BẮT ĐẦU BOSS
    // =========================================================

    public void ContinueToBossWave()
    {
        waitingForBossWarning = false;

        BuildingSpot2D.canBuild = false;

        Debug.Log(
            "🔥 Người chơi đã bấm BẮT ĐẦU - Boss xuất hiện!"
        );

        SpawnFinalBoss();
    }

    // =========================================================
    // SPAWN BOSS
    // =========================================================

    public void SpawnFinalBoss()
    {
        if (bossSpawned)
            return;

        if (bossPrefab == null)
        {
            Debug.LogError("❌ Chưa gán Boss Prefab!");
            return;
        }

        bossSpawned = true;

        BuildingSpot2D.canBuild = false;

        Instantiate(
            bossPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        state = SpawnState.WAITING;

        Debug.Log("💀 BOSS CUỐI ĐÃ XUẤT HIỆN!");
    }

    // =========================================================
    // KIỂM TRA QUÁI / BOSS
    // =========================================================

    bool EnemyIsAlive()
    {
        Enemy[] enemies =
            Object.FindObjectsByType<Enemy>(
                FindObjectsSortMode.None
            );

        Boss[] bosses =
            Object.FindObjectsByType<Boss>(
                FindObjectsSortMode.None
            );

        return enemies.Length > 0 || bosses.Length > 0;
    }

    // =========================================================
    // SPAWN WAVE
    // =========================================================

    IEnumerator SpawnWave(Wave _wave)
    {
        state = SpawnState.SPAWNING;

        float spacing = 0.8f;

        for (int i = 0; i < _wave.count; i++)
        {
            Vector3 spawnPos =
                spawnPoint.position +
                new Vector3(i * spacing, 0f, 0f);

            if (_wave.enemyPrefab != null)
            {
                Instantiate(
                    _wave.enemyPrefab,
                    spawnPos,
                    spawnPoint.rotation
                );
            }
        }

        state = SpawnState.WAITING;

        yield break;
    }
}