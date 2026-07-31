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
        public float rate; // Tốc độ spawn giữa các con quái trong cùng 1 wave
    }

    [Header("Cấu hình Wave quái")]
    public Wave[] waves;
    public Transform spawnPoint; // Vị trí quái xuất hiện (nếu để trống sẽ lấy vị trí GameObject này)
    public float timeBetweenWaves = 5f; // Thời gian nghỉ giữa các wave

    private int currentWaveIndex = 0;
    private bool isSpawning = false;
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
                // Bắt đầu chuyển sang wave tiếp theo
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
                // Bắt đầu spawn wave tiếp theo
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
            // Đã qua hết tất cả các wave
            currentWaveIndex = waves.Length - 1;
            Debug.Log("Đã qua tất cả các wave! Chiến thắng!");
            // Bạn có thể thêm logic hiển thị màn hình chiến thắng ở đây nếu muốn
        }
    }

    bool EnemyIsAlive()
    {
        // Kiểm tra trong Scene còn đối tượng nào mang component Enemy đang sống không
        Enemy[] enemies = Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        return enemies.Length > 0;
    }

    IEnumerator SpawnWave(Wave _wave)
    {
        state = SpawnState.SPAWNING;

        for (int i = 0; i < _wave.count; i++)
        {
            SpawnEnemy(_wave.enemyPrefab);
            yield return new WaitForSeconds(1f / _wave.rate);
        }

        state = SpawnState.WAITING;
        yield break;
    }

    void SpawnEnemy(GameObject _enemy)
    {
        if (_enemy != null && spawnPoint != null)
        {
            Instantiate(_enemy, spawnPoint.position, spawnPoint.rotation);
        }
    }
}