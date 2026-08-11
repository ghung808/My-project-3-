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

    [Header("Fighter Info")]
    public GameObject fighterInfoPanel;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
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
    }

    public void StartGuide()
    {
        // Ẩn bảng chào mừng
        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }

        // Hiện mũi tên
        if (guideArrow != null)
        {
            guideArrow.SetActive(true);
        }

        // Ban đầu mũi tên chỉ vào BuildingSpot
        UpdateArrowPosition();
    }

    public void BuildingSpotClicked()
    {
        // Đổi mục tiêu sang nút Đấu Sĩ
        if (towerButtonTarget == null)
            return;

        target = towerButtonTarget;

        // Hiện mũi tên
        if (guideArrow != null)
        {
            guideArrow.SetActive(true);
        }

        // Di chuyển mũi tên tới nút Đấu Sĩ
        UpdateArrowPosition();

        Debug.Log("Đã bấm BuildingSpot - Mũi tên chuyển sang Đấu Sĩ!");
    }

    public void FighterButtonClicked()
    {
        // Ẩn mũi tên
        if (guideArrow != null)
        {
            guideArrow.SetActive(false);
        }

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

        Debug.Log("Da bam TIEP TUC - bat dau chien dau!");
    }

    void UpdateArrowPosition()
    {
        if (guideArrow == null || target == null)
            return;

        RectTransform arrowRect =
            guideArrow.GetComponent<RectTransform>();

        RectTransform targetRect =
            target.GetComponent<RectTransform>();

        if (arrowRect == null)
            return;

        if (targetRect != null)
        {
            // Đưa mũi tên lên phía trên nút Đấu Sĩ
            arrowRect.position = targetRect.position + new Vector3(0f, 100f, 0f);
            return;
        }

        if (Camera.main != null)
        {
            Vector3 screenPosition =
                Camera.main.WorldToScreenPoint(target.position);

            arrowRect.position =
                screenPosition + new Vector3(0f, 55f, 0f);
        }
    }
}