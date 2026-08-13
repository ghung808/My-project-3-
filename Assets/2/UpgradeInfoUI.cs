using UnityEngine;
using UnityEngine.SceneManagement;

public class UpgradeInfoUI : MonoBehaviour
{
    public static UpgradeInfoUI instance;

    [Header("Fighter")]
    public GameObject fighterInfoPanel;
    public GameObject fighterLevel2Image;
    public GameObject fighterLevel3Image;

    [Header("Archer")]
    public GameObject archerInfoPanel;
    public GameObject archerLevel2Image;
    public GameObject archerLevel3Image;

    [Header("Mage")]
    public GameObject mageInfoPanel;
    public GameObject mageLevel2Image;
    public GameObject mageLevel3Image;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        HideAllPanels();
    }

    public void ShowInfo(SpawnTower tower)
    {
        // Chỉ hoạt động ở Map 1
        if (SceneManager.GetActiveScene().name != "Vht")
        {
            return;
        }

        if (tower == null)
        {
            Debug.LogWarning("UpgradeInfoUI: Tower = null!");
            return;
        }

        HideAllPanels();

        if (tower.soldierPrefab == null)
        {
            Debug.LogWarning(
                "UpgradeInfoUI: " +
                tower.gameObject.name +
                " chưa có Soldier Prefab!"
            );

            return;
        }

        // =========================
        // ĐẤU SĨ
        // =========================
        if (tower.soldierPrefab.GetComponent<PlayerSoldier>() != null)
        {
            fighterInfoPanel.SetActive(true);

            if (tower.level == 2)
            {
                if (fighterLevel2Image != null)
                    fighterLevel2Image.SetActive(true);

                Debug.Log("⚔️ Đấu sĩ - Hiện bảng CẤP 2");
            }
            else if (tower.level == 3)
            {
                if (fighterLevel3Image != null)
                    fighterLevel3Image.SetActive(true);

                Debug.Log("⚔️ Đấu sĩ - Hiện bảng CẤP 3");
            }
        }

        // =========================
        // CUNG THỦ
        // =========================
        else if (tower.soldierPrefab.GetComponent<ArcherSoldier>() != null)
        {
            archerInfoPanel.SetActive(true);

            if (tower.level == 2)
            {
                if (archerLevel2Image != null)
                    archerLevel2Image.SetActive(true);

                Debug.Log("🏹 Cung thủ - Hiện bảng CẤP 2");
            }
            else if (tower.level == 3)
            {
                if (archerLevel3Image != null)
                    archerLevel3Image.SetActive(true);

                Debug.Log("🏹 Cung thủ - Hiện bảng CẤP 3");
            }
        }

        // =========================
        // PHÁP SƯ
        // =========================
        else if (tower.soldierPrefab.GetComponent<MageSoldier>() != null)
        {
            mageInfoPanel.SetActive(true);

            if (tower.level == 2)
            {
                if (mageLevel2Image != null)
                    mageLevel2Image.SetActive(true);

                Debug.Log("🔮 Pháp sư - Hiện bảng CẤP 2");
            }
            else if (tower.level == 3)
            {
                if (mageLevel3Image != null)
                    mageLevel3Image.SetActive(true);

                Debug.Log("🔮 Pháp sư - Hiện bảng CẤP 3");
            }
        }
    }

    public void HideAllPanels()
    {
        if (fighterInfoPanel != null)
            fighterInfoPanel.SetActive(false);

        if (archerInfoPanel != null)
            archerInfoPanel.SetActive(false);

        if (mageInfoPanel != null)
            mageInfoPanel.SetActive(false);

        if (fighterLevel2Image != null)
            fighterLevel2Image.SetActive(false);

        if (fighterLevel3Image != null)
            fighterLevel3Image.SetActive(false);

        if (archerLevel2Image != null)
            archerLevel2Image.SetActive(false);

        if (archerLevel3Image != null)
            archerLevel3Image.SetActive(false);

        if (mageLevel2Image != null)
            mageLevel2Image.SetActive(false);

        if (mageLevel3Image != null)
            mageLevel3Image.SetActive(false);
    }

    public void CloseInfo()
    {
        HideAllPanels();
    }
}