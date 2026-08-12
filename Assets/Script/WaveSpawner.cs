using UnityEngine;
using System.Collections;
using TMPro; // Add this for TextMeshPro support

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class Wave
    {
        public string waveName;
        public GameObject enemyPrefab;
        public int count;
        public float rate; // Giữ lại để không lỗi giao diện Inspector
    }

    [Header("Cấu hình Wave quái")]
    public Wave[] waves;
    public Transform spawnPoint; // Vị trí quái xuất hiện
    public float timeBetweenWaves = 5f; // Thời gian nghỉ giữa các wave

    [Header("Thời gian xây")]
    public float firstBuildTime = 10f; // Thời gian xây trước Wave 1

    [Header("Boss")]
    public GameObject bossPrefab;
    public bool spawnBossAfterLastWave = true;

    private int currentWaveIndex = 0;
    private float waveCountdown;
    private bool bossSpawned = false;
    private enum SpawnState { SPAWNING, WAITING, COUNTING };
    private SpawnState state = SpawnState.COUNTING;

    void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        waveCountdown = firstBuildTime; // Đếm ngược từ 10 giây

        // Cho phép xây 10 giây trước Wave 1
        BuildingSpot2D.canBuild = true;

        GameUI.instance.maxWave = waves.Length; // Set total waves in GameUI
    }

    void Update()
    {
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

        state = SpawnState.COUNTING;

        // Đã hết Wave → cho phép xây trong thời gian nghỉ
        BuildingSpot2D.canBuild = true;

        waveCountdown = timeBetweenWaves;

        currentWaveIndex++;
        GameUI.instance.currentWave = currentWaveIndex + 1; // Update current wave in GameUI

        if (currentWaveIndex >= waves.Length)
        {
            // Đã hết tất cả Wave → không cho xây nữa
            BuildingSpot2D.canBuild = false;

            if (spawnBossAfterLastWave && !bossSpawned && bossPrefab != null)
            {
                bossSpawned = true;

                Instantiate(
                    bossPrefab,
                    spawnPoint.position,
                    spawnPoint.rotation
                );

                state = SpawnState.WAITING;

                Debug.Log("Boss xuất hiện!");
            }
            else if (bossSpawned && !EnemyIsAlive())
            {
                GameUI.instance.WinGame();
            }

            return;
        }
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

        float spacing = 0.8f; // Khoảng cách giữa các con quái để không bị dính vào nhau

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