using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;

    [Header("UI Nâng/Xóa Tháp")]
    public GameObject towerActionPanel;
    public UnityEngine.UI.Button upgradeButton;
    public UnityEngine.UI.Button sellButton;

    [Header("Kéo 3 Prefab Tháp vào đây")]
    public GameObject archerTowerPrefab;
    public GameObject warriorTowerPrefab;
    public GameObject mageTowerPrefab;

    private GameObject towerToBuild;
    private BuildingSpot2D selectedSpot;

    public bool CanBuild => towerToBuild != null;

    void Awake()
    {
        // MAP 2: luôn lấy BuildManager của chính Map 2
        // MAP 1 Vht: giữ cách hoạt động cũ
        if (SceneManager.GetActiveScene().name == "hgt")
        {
            instance = this;
        }
        else
        {
            if (instance == null)
                instance = this;
        }

        if (towerActionPanel != null)
            towerActionPanel.SetActive(false);
    }

    void Start()
    {
        // CHỈ MAP 2 tự nối 2 nút.
        // MAP 1 không bị thay đổi.
        if (SceneManager.GetActiveScene().name == "hgt")
        {
            if (upgradeButton != null)
            {
                upgradeButton.onClick.AddListener(UpgradeSelectedTower);
            }

            if (sellButton != null)
            {
                sellButton.onClick.AddListener(SellSelectedTower);
            }
        }
    }

    public void SelectArcherTower()
    {
        towerToBuild = archerTowerPrefab;
        selectedSpot = null;
    }

    public void SelectWarriorTower()
    {
        towerToBuild = warriorTowerPrefab;
        selectedSpot = null;
    }

    public void SelectMageTower()
    {
        towerToBuild = mageTowerPrefab;
        selectedSpot = null;
    }

    public GameObject GetTowerToBuild()
    {
        return towerToBuild;
    }

    public void ResetSelection()
    {
        towerToBuild = null;
    }

    public int GetSelectedTowerCost()
    {
        if (towerToBuild == null)
            return 0;

        SpawnTower st =
            towerToBuild.GetComponent<SpawnTower>();

        return st != null ? st.cost : 0;
    }

    public void SelectSpotToUpgrade(BuildingSpot2D spot)
    {
        selectedSpot = spot;

        if (towerActionPanel != null)
        {
            towerActionPanel.SetActive(true);

            if (Camera.main != null)
            {
                Vector3 screenPos =
                    Camera.main.WorldToScreenPoint(
                        spot.transform.position
                    );

                towerActionPanel.transform.position =
                    screenPos + new Vector3(0, -100f, 0);
            }
        }
    }

    public void UpgradeSelectedTower()
    {
        if (selectedSpot == null)
            return;

        GameObject tower =
            selectedSpot.GetCurrentTower();

        if (tower == null)
            return;

        SpawnTower spawnTower =
            tower.GetComponent<SpawnTower>();

        if (spawnTower == null)
            return;

        // SpawnTower hiện tại của bạn tự kiểm tra tiền
        // và tự trừ tiền nâng cấp.
        spawnTower.UpgradeTower();

        // Chỉ Map 1 mới có tutorial Wave 4
        if (SceneManager.GetActiveScene().name == "Vht")
        {
            if (GuideManager.instance != null)
            {
                GuideManager.instance.Wave4UpgradeButtonClicked();
            }
        }

        HideTowerActionPanel();
    }

    public void SellSelectedTower()
    {
        if (selectedSpot == null)
            return;

        GameObject tower =
            selectedSpot.GetCurrentTower();

        if (tower == null)
            return;

        SpawnTower spawnTower =
            tower.GetComponent<SpawnTower>();

        if (spawnTower == null)
            return;

        spawnTower.SellTower();

        HideTowerActionPanel();
    }

    void HideTowerActionPanel()
    {
        selectedSpot = null;

        if (towerActionPanel != null)
            towerActionPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            selectedSpot = null;

            if (towerActionPanel != null)
                towerActionPanel.SetActive(false);

            return;
        }

        if (selectedSpot == null)
            return;

        GameObject tower =
            selectedSpot.GetCurrentTower();

        if (tower == null)
            return;

        SpawnTower spawnTowerScript =
            tower.GetComponent<SpawnTower>();

        if (spawnTowerScript == null)
            return;

        if (Input.GetKeyDown(KeyCode.U))
        {
            spawnTowerScript.UpgradeTower();

            // Chỉ Map 1 mới dùng tutorial Wave 4
            if (SceneManager.GetActiveScene().name == "Vht")
            {
                if (GuideManager.instance != null)
                {
                    GuideManager.instance.Wave4UpgradeButtonClicked();
                }
            }

            selectedSpot = null;

            if (towerActionPanel != null)
                towerActionPanel.SetActive(false);
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            spawnTowerScript.SellTower();

            selectedSpot = null;

            if (towerActionPanel != null)
                towerActionPanel.SetActive(false);
        }
    }
}