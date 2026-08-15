using UnityEngine;
using UnityEngine.UI;

public class MissionMap3UI : MonoBehaviour
{
    [Header("Bảng nhiệm vụ Map 3")]
    public GameObject missionPanel;

    [Header("Nút bắt đầu")]
    public Button startButton;

    [Header("WaveSpawner của Map 3")]
    public WaveSpawnerMap3 waveSpawner;

    void Start()
    {
        Debug.Log("=== MISSION MAP 3 START ===");

        // Hiện bảng nhiệm vụ khi vừa vào Map 3
        if (missionPanel != null)
        {
            missionPanel.SetActive(true);
            Debug.Log("✅ Đã hiện MissionPanelMap3");
        }
        else
        {
            Debug.LogError("❌ CHƯA GÁN MissionPanelMap3!");
        }

        // Chưa cho WaveSpawner Map 3 chạy
        if (waveSpawner != null)
        {
            waveSpawner.enabled = false;
            Debug.Log("⏸ WaveSpawnerMap3 đã tạm dừng");
        }
        else
        {
            Debug.LogError("❌ CHƯA GÁN WaveSpawnerMap3!");
        }

        // Gán nút BẮT ĐẦU
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartMap3);

            Debug.Log("✅ Đã gán StartButton");
        }
        else
        {
            Debug.LogError("❌ CHƯA GÁN StartButton!");
        }

        // Dừng thời gian game
        Time.timeScale = 0f;
    }

    public void StartMap3()
    {
        Debug.Log("🔥 MAP 3 BẮT ĐẦU!");

        // Ẩn bảng nhiệm vụ
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }

        // Cho WaveSpawner Map 3 chạy
        if (waveSpawner != null)
        {
            waveSpawner.enabled = true;
        }

        // Tiếp tục game
        Time.timeScale = 1f;

        Debug.Log("✅ WaveSpawnerMap3 đã được bật!");
        Debug.Log("✅ Time.timeScale = 1");
    }
}