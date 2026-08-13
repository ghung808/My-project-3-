using UnityEngine;
using UnityEngine.UI;
using System;

public class BossWarningUI : MonoBehaviour
{
    [Header("Boss Warning UI")]
    public GameObject bossWarningPanel;
    public Button bossContinueButton;

    private WaveSpawner waveSpawner;
    private bool warningShown = false;

    void Awake()
    {
        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(false);
        }
    }

    void Start()
    {
        if (bossContinueButton != null)
        {
            bossContinueButton.onClick.RemoveListener(CloseWarning);
            bossContinueButton.onClick.AddListener(CloseWarning);
        }
    }

    public void ShowWarning()
    {
        if (warningShown)
            return;

        warningShown = true;

        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(true);
        }

        Debug.Log("⚠️ BOSS SẮP XUẤT HIỆN!");
    }

    public void CloseWarning()
    {
        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(false);
        }

        Debug.Log("🔥 ĐÃ BẤM BẮT ĐẦU BOSS!");

        if (waveSpawner == null)
        {
            waveSpawner = FindFirstObjectByType<WaveSpawner>();
        }

        if (waveSpawner != null)
        {
            Debug.Log("✅ Đã tìm thấy WaveSpawner → gọi Boss!");

            waveSpawner.ContinueToBossWave();
        }
        else
        {
            Debug.LogError("❌ KHÔNG TÌM THẤY WaveSpawner!");
        }
    }
}