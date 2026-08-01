using UnityEngine;
using System.Collections;

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
    public float timeBetweenWaves = 3f; // Thời gian nghỉ giữa các wave

    private int currentWaveIndex = 0;
    private float waveCountdown;
    private enum SpawnState { SPAWNING, WAITING, COUNTING };
    private SpawnState state = SpawnState.COUNTING;

    void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        waveCountdown = timeBetweenWaves;
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
            if (state != SpawnState.SPAWNING)
            {
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
        waveCountdown = timeBetweenWaves;

        currentWaveIndex++;
        if (currentWaveIndex > waves.Length - 1)
        {
            currentWaveIndex = waves.Length - 1;
            Debug.Log("Đã qua tất cả các wave!");
        }
    }

    bool EnemyIsAlive()
    {
        // Kiểm tra trong Scene còn con quái nào đang sống không
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        return enemies.Length > 0;
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