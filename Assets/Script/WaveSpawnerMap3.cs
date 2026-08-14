using UnityEngine;
using TMPro;
using System.Collections;

public class WaveSpawnerMap3 : MonoBehaviour
{
    [Header("Wave")]
    public Wave[] waves;

    public float timeBetweenWaves = 5f;
    public float countdown = 3f;

    private int currentWaveIndex = 0;
    private bool isSpawning = false;

    [Header("Waypoint Map3")]
    public WaypointsMap3 waypointsMap3;

    [Header("Boss")]
    public int bossWave = 8;
    public GameObject bossPrefab;

    [Header("Boss UI")]
    public GameObject bossWarningPanel;

    private bool waitingForBossStart = false;
    private bool bossStarted = false;

    void Start()
    {
        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(false);
        }

        StartCoroutine(SpawnWaves());
    }

    IEnumerator SpawnWaves()
    {
        yield return new WaitForSeconds(2f);

        // Chỉ spawn Wave 1 → Wave 7
        while (currentWaveIndex < waves.Length)
        {
            yield return new WaitForSeconds(countdown);

            SpawnCurrentWave();

            // Đợi toàn bộ quái của wave hiện tại chết hết
            yield return new WaitUntil(() => AreAllEnemiesDead());

            Debug.Log("Đã tiêu diệt hết quái Wave " + (currentWaveIndex + 1));

            currentWaveIndex++;

            yield return new WaitForSeconds(timeBetweenWaves);
        }

        // =========================
        // SAU KHI WAVE 7 KẾT THÚC
        // =========================

        if (currentWaveIndex >= 7)
        {
            ShowBossPanel();
        }
    }

    bool AreAllEnemiesDead()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );

        return enemies.Length == 0;
    }

    void SpawnCurrentWave()
    {
        int waveNumber = currentWaveIndex + 1;

        Debug.Log("Bắt đầu Wave " + waveNumber);

        Wave wave = waves[currentWaveIndex];

        StartCoroutine(
            SpawnEnemiesInWave(wave, waveNumber)
        );
    }

    IEnumerator SpawnEnemiesInWave(Wave wave, int waveNumber)
    {
        for (int i = 0; i < wave.count; i++)
        {
            Transform[] selectedPath = GetPathForEnemy(waveNumber, i);

            SpawnEnemy(wave.enemyPrefab, selectedPath);

            yield return new WaitForSeconds(wave.rate);
        }
    }

    void SpawnEnemy(GameObject prefab, Transform[] path)
    {
        if (path == null || path.Length == 0)
        {
            Debug.LogError("Map3 chưa có Waypoint!");
            return;
        }

        GameObject enemy = Instantiate(
            prefab,
            path[0].position,
            Quaternion.identity
        );

        Enemy enemyScript = enemy.GetComponent<Enemy>();

        if (enemyScript != null)
        {
            enemyScript.customWaypoints = path;
        }
    }

    Transform[] GetPathForEnemy(int waveNumber, int enemyIndex)
    {
        // WAVE 1: chỉ đường giữa
        if (waveNumber == 1)
        {
            return waypointsMap3.middlePoints;
        }

        // WAVE 2: đường giữa + đường trên
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

        // WAVE 3 trở đi: cả 3 đường
        int lane = enemyIndex % 3;

        if (lane == 0)
        {
            return waypointsMap3.middlePoints;
        }
        else if (lane == 1)
        {
            return waypointsMap3.topPoints;
        }
        else
        {
            return waypointsMap3.bottomPoints;
        }
    }

    void ShowBossPanel()
    {
        waitingForBossStart = true;

        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(true);
        }

        Debug.Log("WAVE 8 - BOSS ĐANG CHỜ NGƯỜI CHƠI!");
    }

    public void StartBoss()
    {
        if (!waitingForBossStart)
            return;

        if (bossStarted)
            return;

        waitingForBossStart = false;
        bossStarted = true;

        // Đóng bảng
        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(false);
        }

        // Spawn Boss
        SpawnBoss();
    }

    void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Chưa gán Boss Prefab!");
            return;
        }

        if (waypointsMap3 == null)
        {
            Debug.LogError("Chưa gán WaypointsMap3!");
            return;
        }

        if (waypointsMap3.middlePoints == null ||
            waypointsMap3.middlePoints.Length == 0)
        {
            Debug.LogError("Chưa có Middle Waypoints!");
            return;
        }

        Transform[] bossPath = waypointsMap3.middlePoints;

        GameObject boss = Instantiate(
            bossPrefab,
            bossPath[0].position,
            Quaternion.identity
        );

        BossEnemy bossScript = boss.GetComponent<BossEnemy>();

        if (bossScript != null)
        {
            bossScript.customWaypoints = bossPath;
        }

        Debug.Log("BOSS ĐÃ XUẤT HIỆN!");
    }
}