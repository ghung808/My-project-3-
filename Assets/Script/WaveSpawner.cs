using UnityEngine;
using System.Collections;
using TMPro;

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

    private int currentWaveIndex = 0;
    private float waveCountdown;
    private bool bossSpawned = false;
    private bool guideReady = false; // KHÓA WAVE KHI MỚI VÀO GAME

    // Tạm dừng sau Wave 1
    private bool waitingForWave1Guide = false;

    // Tạm dừng sau Wave 2
    private bool waitingForWave2Guide = false;

    // Tạm dừng sau Wave 3
    private bool waitingForWave3Mission = false;

    private enum SpawnState { SPAWNING, WAITING, COUNTING };
    private SpawnState state = SpawnState.COUNTING;

    void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        // KHÓA WAVE KHI MỚI VÀO GAME
        guideReady = false;

        waveCountdown = 0f;

        // Vẫn cho phép người chơi xây tháp
        BuildingSpot2D.canBuild = true;

        if (GameUI.instance != null)
        {
            GameUI.instance.maxWave = waves.Length;
        }
    }

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

        if (state == SpawnState.WAITING)
        {
            // Kiểm tra xem quái đã chết hết chưa
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
            if (state != SpawnState.SPAWNING && currentWaveIndex < waves.Length)
            {
                // Bắt đầu Wave mới → khóa xây
                BuildingSpot2D.canBuild = false;

                StartCoroutine(SpawnWave(waves[currentWaveIndex]));
            }
        }
        else
        {
            waveCountdown -= Time.deltaTime;
        }
    }

    void WaveCompleted()
    {
        Debug.Log("Wave " + (currentWaveIndex + 1) + " đã hoàn thành!");

        // Cho phép xây trong thời gian nghỉ
        BuildingSpot2D.canBuild = true;

        // Tăng số Wave
        currentWaveIndex++;

        // ================================
        // SAU KHI HOÀN THÀNH WAVE 1
        // ================================
        if (currentWaveIndex == 1)
        {
            // Dừng WaveSpawner
            waitingForWave1Guide = true;

            // Không cho tự động chạy Wave 2
            state = SpawnState.COUNTING;

            // Không chạy countdown
            waveCountdown = 0f;

            // Hiện bảng chúc mừng
            if (GuideManager.instance != null)
            {
                GuideManager.instance.ShowWave1Complete();
            }

            Debug.Log("🎉 Wave 1 hoàn thành - đang chờ hướng dẫn Cung Thủ!");

            return;
        }

        // ================================
        // SAU KHI HOÀN THÀNH WAVE 2
        // ================================
        if (currentWaveIndex == 2)
        {
            waitingForWave2Guide = true;

            state = SpawnState.COUNTING;

            BuildingSpot2D.canBuild = true;

            waveCountdown = 0f;

            if (GuideManager.instance != null)
            {
                GuideManager.instance.ShowMageUnlockPanel();
            }

            Debug.Log("🎉 Wave 2 hoàn thành - chờ hướng dẫn Pháp Sư!");

            return;
        }

        // ================================
        // SAU KHI HOÀN THÀNH WAVE 3
        // ================================
        if (currentWaveIndex == 3)
        {
            waitingForWave3Mission = true;

            state = SpawnState.COUNTING;

            waveCountdown = 0f;

            BuildingSpot2D.canBuild = true;

            if (GuideManager.instance != null)
            {
                GuideManager.instance.ShowWave3Mission();
            }

            Debug.Log("🎉 Wave 3 hoàn thành - hiển thị nhiệm vụ!");

            return;
        }

        // ================================
        // CÁC WAVE SAU
        // ================================
        state = SpawnState.COUNTING;

        waveCountdown = timeBetweenWaves;

        if (GameUI.instance != null)
        {
            GameUI.instance.currentWave = currentWaveIndex + 1;
        }

        // ================================
        // ĐÃ HẾT TẤT CẢ WAVE
        // ================================
        if (currentWaveIndex >= waves.Length)
        {
            BuildingSpot2D.canBuild = false;

            if (spawnBossAfterLastWave && !bossSpawned)
            {
                state = SpawnState.COUNTING;

                waveCountdown = 0f;

                if (GuideManager.instance != null)
                {
                    GuideManager.instance.ShowBossWarning();
                }

                Debug.Log("⚠️ Tất cả Wave đã hoàn thành - chờ người chơi xác nhận Boss!");

                return;
            }

            return;
        }
    }

    // Hàm cho GuideManager gọi khi hướng dẫn xong
    // ĐÂY LÀ CỬA DUY NHẤT MỞ WAVE 1
    public void StartBattleAfterGuide()
    {
        guideReady = true;
        waveCountdown = 0f;

        Debug.Log("Hướng dẫn xong - Wave 1 bắt đầu!");
    }

    // Hàm để tiếp tục sau khi hoàn thành hướng dẫn Cung Thủ
    public void ContinueAfterArcherGuide()
    {
        waitingForWave1Guide = false;
        waveCountdown = timeBetweenWaves;
        state = SpawnState.COUNTING;

        Debug.Log("✅ Đã hoàn thành hướng dẫn Cung Thủ - Wave 2 sẽ bắt đầu sau " + timeBetweenWaves + " giây!");
    }

    public void StartWave2AfterGuide()
    {
        // Mở khóa WaveSpawner
        waitingForWave1Guide = false;

        // Cho phép bắt đầu Wave 2 ngay lập tức
        state = SpawnState.COUNTING;
        waveCountdown = 0f;

        // Khi Wave 2 bắt đầu thì khóa xây
        BuildingSpot2D.canBuild = false;

        Debug.Log("🔥 Hướng dẫn Cung Thủ hoàn tất - Wave 2 bắt đầu!");
    }

    public void StartWave3AfterGuide()
    {
        // Mở khóa sau hướng dẫn Wave 2
        waitingForWave2Guide = false;

        // Cho phép WaveSpawner chạy tiếp
        guideReady = true;

        // Đưa trạng thái về đếm Wave
        state = SpawnState.COUNTING;

        // Bắt đầu Wave 3 ngay lập tức
        waveCountdown = 0f;

        // Khi Wave 3 bắt đầu thì khóa xây
        BuildingSpot2D.canBuild = false;

        Debug.Log("🔮 Hướng dẫn Pháp Sư hoàn thành - Wave 3 bắt đầu!");
    }

    public void ContinueAfterWave3Mission()
    {
        waitingForWave3Mission = false;

        state = SpawnState.COUNTING;

        waveCountdown = timeBetweenWaves;

        BuildingSpot2D.canBuild = true;

        Debug.Log("🔥 Đã hoàn thành nhiệm vụ - Wave 4 sẽ bắt đầu sau " + timeBetweenWaves + " giây!");
    }

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

    bool EnemyIsAlive()
    {
        // Kiểm tra trong Scene còn con quái nào đang sống không
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        Boss[] bosses = Object.FindObjectsByType<Boss>(FindObjectsSortMode.None);

        return enemies.Length > 0 || bosses.Length > 0;
    }

    IEnumerator SpawnWave(Wave _wave)
    {
        state = SpawnState.SPAWNING;

        float spacing = 0.8f;

        // Spawn toàn bộ số lượng quái trong wave CÙNG MỘT LÚC
        for (int i = 0; i < _wave.count; i++)
        {
            Vector3 spawnPos = spawnPoint.position + new Vector3(i * spacing, 0f, 0f);
            if (_wave.enemyPrefab != null)
            {
                Instantiate(_wave.enemyPrefab, spawnPos, spawnPoint.rotation);
            }
        }

        state = SpawnState.WAITING;
        yield break;
    }
}