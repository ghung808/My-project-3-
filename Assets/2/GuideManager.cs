using UnityEngine;
using UnityEngine.UI;

public class GuideManager : MonoBehaviour
{
    public static GuideManager instance;

    [Header("Guide UI")]
    public GameObject guidePanel;
    public GameObject guideArrow;

    [Header("Button")]
    public Button startGuideButton;
    public Button continueButton;

    [Header("Target")]
    public Transform target;
    public Transform towerButtonTarget;

    [Header("Wave 1 Guide")]
    public Transform archerBuildingSpot;

    [Header("Wave Complete")]
    public GameObject waveCompletePanel;
    public Button waveCompleteContinueButton;

    [Header("Fighter Info")]
    public GameObject fighterInfoPanel;

    [Header("Guide State")]
    public bool fighterGuideCompleted = false;
    public bool tutorialFinished = false;
    private bool waitingForArcherGuide = false;
    private bool waitingForMageGuide = false;

    [Header("Wave")]
    public WaveSpawner waveSpawner;

    [Header("Next Guide")]
    public Transform archerButtonTargetOld;
    public Transform archerBuildingSpotOld;

    [Header("Archer Info")]
    public GameObject archerInfoPanel;
    public Button archerContinueButton;

    [Header("Wave 3 Mission")]
    public GameObject wave3MissionPanel;
    public Button wave3MissionContinueButton;

    [Header("Boss Warning")]
    public GameObject bossWarningPanel;
    public Button bossContinueButton;

    [Header("Mage Guide")]
    public Transform mageBuildingSpot;
    public Transform mageButtonTarget;

    [Header("Mage Info")]
    public GameObject mageInfoPanel;
    public Button mageContinueButton;

    [Header("Mage Unlock")]
    public GameObject mageUnlockPanel;
    public Button mageUnlockContinueButton;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Tắt bảng hoàn thành Wave 1 khi mới vào game
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(false);
        }

        // Hiện bảng hướng dẫn khi mới vào Map 1
        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
        }

        // Chưa hiện mũi tên
        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        // Gán sự kiện cho nút Bắt đầu
        if (startGuideButton != null)
        {
            startGuideButton.onClick.AddListener(StartGuide);
        }

        // Tắt bảng thông tin Đấu Sĩ
        if (fighterInfoPanel != null)
        {
            fighterInfoPanel.SetActive(false);
        }

        // Gán sự kiện cho nút Tiếp tục
        if (continueButton != null)
        {
            continueButton.onClick.AddListener(ContinueGuide);
        }

        // Tắt bảng thông tin Cung Thủ
        if (archerInfoPanel != null)
        {
            archerInfoPanel.SetActive(false);
        }

        // Gán nút Tiếp tục của Cung Thủ
        if (archerContinueButton != null)
        {
            archerContinueButton.onClick.AddListener(CloseArcherInfo);
        }

        // Tắt bảng nhiệm vụ Wave 3
        if (wave3MissionPanel != null)
        {
            wave3MissionPanel.SetActive(false);
        }

        // Gán nút Tiếp tục của nhiệm vụ Wave 3
        if (wave3MissionContinueButton != null)
        {
            wave3MissionContinueButton.onClick.AddListener(ContinueAfterWave3Mission);
        }

        // Tắt bảng cảnh báo Boss
        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(false);
        }

        // Gán nút Tiếp tục của cảnh báo Boss
        if (bossContinueButton != null)
        {
            bossContinueButton.onClick.AddListener(ContinueToBoss);
        }

        // Tắt bảng thông tin Pháp Sư
        if (mageInfoPanel != null)
        {
            mageInfoPanel.SetActive(false);
        }

        // Gán nút Tiếp tục của Pháp Sư
        if (mageContinueButton != null)
        {
            mageContinueButton.onClick.AddListener(CloseMageInfo);
        }

        // Tắt bảng mở khóa Pháp Sư
        if (mageUnlockPanel != null)
        {
            mageUnlockPanel.SetActive(false);
        }

        // Gán nút Tiếp tục của bảng mở khóa Pháp Sư
        if (mageUnlockContinueButton != null)
        {
            mageUnlockContinueButton.onClick.AddListener(ContinueAfterWave2);
        }

        // Gán nút Tiếp tục sau khi hoàn thành Wave 1
        if (waveCompleteContinueButton != null)
        {
            waveCompleteContinueButton.onClick.AddListener(ContinueAfterWave1);
        }
    }

    public void StartGuide()
    {
        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }

        if (guideArrow != null)
        {
            guideArrow.SetActive(true);
        }

        if (target != null)
        {
            UpdateArrowPosition();
        }

        Debug.Log("Mũi tên đã xuất hiện và chỉ vào mục tiêu đầu tiên!");
    }

    public void BuildingSpotClicked()
    {
        // Wave 4 trở đi không còn hướng dẫn
        if (tutorialFinished)
            return;

        // PHÁP SƯ
        if (waitingForMageGuide)
        {
            ShowMageButtonGuide();

            Debug.Log("🔮 Đã bấm ô xây Pháp Sư - mũi tên chuyển sang nút Pháp Sư!");

            return;
        }

        // CUNG THỦ
        if (waitingForArcherGuide)
        {
            ShowArcherGuide();

            Debug.Log("🏹 Đã bấm ô xây Cung Thủ - mũi tên chuyển sang Cung Thủ!");

            return;
        }

        // ĐẤU SĨ
        if (towerButtonTarget == null)
            return;

        target = towerButtonTarget;

        if (guideArrow != null)
        {
            guideArrow.SetActive(true);
        }

        UpdateArrowPosition();

        Debug.Log("⚔️ Đã bấm BuildingSpot - Mũi tên chuyển sang Đấu Sĩ!");
    }

    public void ShowArcherBuildingGuide()
    {
        if (archerBuildingSpot == null)
            return;

        target = archerBuildingSpot;

        if (guideArrow != null)
        {
            guideArrow.SetActive(true);
        }

        UpdateArrowPosition();

        Debug.Log("Mũi tên đang chỉ vào ô xây Cung thủ!");
    }

    public void ShowArcherBuildingSpotGuide()
    {
        if (tutorialFinished)
            return;

        if (guideArrow == null || archerBuildingSpotOld == null)
            return;

        waitingForArcherGuide = true;

        target = archerBuildingSpotOld;

        guideArrow.SetActive(true);

        UpdateArrowPosition();

        Debug.Log("Wave 1 xong - hay bam BuildingSpot de xay Cung Thu!");
    }

    public void FighterButtonClicked()
    {
        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        target = null;

        Debug.Log("Đã bấm Đấu Sĩ - Kết thúc bước hướng dẫn!");
    }

    public void ShowFighterInfo()
    {
        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        if (fighterInfoPanel != null)
        {
            fighterInfoPanel.SetActive(true);
        }

        Debug.Log("Da mo bang thong tin Dau Si!");
    }

    public void ContinueGuide()
    {
        if (fighterInfoPanel != null)
        {
            fighterInfoPanel.SetActive(false);
        }

        fighterGuideCompleted = true;

        if (waveSpawner != null)
        {
            waveSpawner.StartBattleAfterGuide();
        }

        Debug.Log("Đã hoàn thành hướng dẫn Đấu Sĩ - Wave 1 bắt đầu!");
    }

    public void ShowArcherGuide()
    {
        if (tutorialFinished)
            return;

        if (guideArrow == null || archerButtonTargetOld == null)
            return;

        target = archerButtonTargetOld;

        guideArrow.SetActive(true);

        UpdateArrowPosition();

        Debug.Log("🏹 Mũi tên đang chỉ vào nút Cung Thủ!");
    }

    public void ArcherButtonClicked()
    {
        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        target = null;

        Debug.Log("Đã bấm nút Cung Thủ!");
    }

    public void ShowArcherInfo()
    {
        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        waitingForArcherGuide = false;

        if (archerInfoPanel != null)
        {
            archerInfoPanel.SetActive(true);
        }

        Debug.Log("Đã xây Cung Thủ - hiện thông tin Cung Thủ!");
    }

    public void CloseArcherInfo()
    {
        if (archerInfoPanel != null)
        {
            archerInfoPanel.SetActive(false);
        }

        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        target = null;

        if (waveSpawner != null)
        {
            waveSpawner.StartWave2AfterGuide();
        }

        Debug.Log("Đã hoàn thành hướng dẫn Cung Thủ - Wave 2 bắt đầu!");
    }

    public void ShowWaveCompletePanel()
    {
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(true);
        }

        Debug.Log("Hiển thị bảng chúc mừng hoàn thành Wave 1!");
    }

    public void CloseWaveCompletePanel()
    {
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(false);
        }

        ShowArcherBuildingSpotGuide();

        Debug.Log("Đã đóng bảng chúc mừng - Bắt đầu hướng dẫn Cung Thủ!");
    }

    public void ShowWave1Complete()
    {
        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        target = null;

        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(true);
        }

        Debug.Log("🎉 WAVE 1 ĐÃ HOÀN THÀNH!");
    }

    public void ContinueAfterWave1()
    {
        if (waveCompletePanel != null)
        {
            waveCompletePanel.SetActive(false);
        }

        ShowArcherBuildingSpotGuide();

        Debug.Log("Đã tiếp tục sau Wave 1 - hãy bấm ô xây Cung Thủ!");
    }

    public void ShowMageUnlockPanel()
    {
        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        target = null;

        if (mageUnlockPanel != null)
        {
            mageUnlockPanel.SetActive(true);
        }

        Debug.Log("🔮 PHÁP SƯ ĐÃ ĐƯỢC MỞ KHÓA!");
    }

    public void ContinueAfterWave2()
    {
        if (mageUnlockPanel != null)
        {
            mageUnlockPanel.SetActive(false);
        }

        ShowMageBuildingGuide();

        Debug.Log("Đã tiếp tục sau Wave 2 - hãy chọn ô xây Pháp Sư!");
    }

    public void ShowMageBuildingGuide()
    {
        if (tutorialFinished)
            return;

        if (mageBuildingSpot == null)
        {
            Debug.LogWarning("Chưa gán Mage Building Spot!");
            return;
        }

        waitingForMageGuide = true;

        target = mageBuildingSpot;

        if (guideArrow != null)
        {
            guideArrow.SetActive(true);
        }

        UpdateArrowPosition();

        Debug.Log("🔮 Mũi tên đang chỉ vào ô xây Pháp Sư!");
    }

    public void ShowMageButtonGuide()
    {
        if (tutorialFinished)
            return;

        if (mageButtonTarget == null)
        {
            Debug.LogWarning("Chưa gán Mage Button Target!");
            return;
        }

        target = mageButtonTarget;

        if (guideArrow != null)
        {
            guideArrow.SetActive(true);
        }

        UpdateArrowPosition();

        Debug.Log("🔮 Mũi tên đang chỉ vào nút Pháp Sư!");
    }

    public void MageButtonClicked()
    {
        waitingForMageGuide = false;

        target = null;

        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        // Hiện bảng thông tin Pháp Sư
        ShowMageInfo();

        Debug.Log("🔮 Đã bấm nút Pháp Sư - hiện thông tin Pháp Sư!");
    }

    public void ShowMageInfo()
    {
        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        target = null;

        if (mageInfoPanel != null)
        {
            mageInfoPanel.SetActive(true);
        }

        Debug.Log("📖 Đã mở bảng thông tin Pháp Sư!");
    }

    public void CloseMageInfo()
    {
        if (mageInfoPanel != null)
        {
            mageInfoPanel.SetActive(false);
        }

        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        target = null;

        if (waveSpawner != null)
        {
            waveSpawner.StartWave3AfterGuide();
        }

        Debug.Log("🔮 Đã xem xong thông tin Pháp Sư - Wave 3 bắt đầu!");
    }

    public void ShowWave3Mission()
    {
        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        target = null;

        if (wave3MissionPanel != null)
        {
            wave3MissionPanel.SetActive(true);
        }

        Debug.Log("🎉 Wave 3 hoàn thành - hiện bảng nhiệm vụ!");
    }

    public void ContinueAfterWave3Mission()
    {
        if (wave3MissionPanel != null)
        {
            wave3MissionPanel.SetActive(false);
        }

        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        target = null;

        // TỪ ĐÂY TRỞ ĐI KHÔNG HIỆN HƯỚNG DẪN NHÂN VẬT NỮA
        tutorialFinished = true;

        if (wave3MissionContinueButton != null)
        {
            wave3MissionContinueButton.interactable = false;
        }

        if (waveSpawner != null)
        {
            waveSpawner.ContinueAfterWave3Mission();
        }

        Debug.Log("Đã nhận nhiệm vụ - từ Wave 4 trở đi tắt toàn bộ hướng dẫn!");
    }

    public void ShowBossWarning()
    {
        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

        target = null;

        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(true);
        }

        Debug.Log("⚠️ BOSS CUỐI SẮP XUẤT HIỆN!");
    }

    public void ContinueToBoss()
    {
        if (bossWarningPanel != null)
        {
            bossWarningPanel.SetActive(false);
        }

        if (waveSpawner != null)
        {
            waveSpawner.SpawnFinalBoss();
        }

        Debug.Log("💀 Người chơi đã sẵn sàng - Boss xuất hiện!");
    }

    void UpdateArrowPosition()
    {
        if (guideArrow == null || target == null)
            return;

        RectTransform arrowRect =
            guideArrow.GetComponent<RectTransform>();

        if (arrowRect == null)
            return;

        RectTransform targetRect =
            target.GetComponent<RectTransform>();

        if (targetRect != null)
        {
            arrowRect.position =
                targetRect.position + new Vector3(0f, 100f, 0f);

            return;
        }

        if (Camera.main != null)
        {
            Vector3 screenPosition =
                Camera.main.WorldToScreenPoint(target.position);

            arrowRect.position = screenPosition;
        }
    }
}