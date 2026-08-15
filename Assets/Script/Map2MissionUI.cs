using UnityEngine;
using UnityEngine.UI;

public class Map2MissionUI : MonoBehaviour
{
    [Header("Bảng nhiệm vụ")]
    public GameObject missionPanel;

    [Header("Nút bắt đầu")]
    public Button startButton;

    [Header("WaveSpawner của Map 2")]
    public WaveSpawner waveSpawner;

    void Start()
    {
        // Hiện bảng nhiệm vụ khi vừa vào Map 2
        if (missionPanel != null)
        {
            missionPanel.SetActive(true);
        }

        // Chưa cho WaveSpawner chạy
        if (waveSpawner != null)
        {
            waveSpawner.enabled = false;
        }

        // Gán nút BẮT ĐẦU
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartMap2);
        }

        // Dừng thời gian game
        Time.timeScale = 0f;
    }

    public void StartMap2()
    {
        // Ẩn bảng nhiệm vụ
        if (missionPanel != null)
        {
            missionPanel.SetActive(false);
        }

        // Cho WaveSpawner chạy
        if (waveSpawner != null)
        {
            waveSpawner.enabled = true;
        }

        // Tiếp tục game
        Time.timeScale = 1f;

        Debug.Log("MAP 2 BẮT ĐẦU!");
    }
}