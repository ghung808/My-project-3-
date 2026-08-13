using UnityEngine;

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
        if (instance == null)
            instance = this;

        if (towerActionPanel != null)
            towerActionPanel.SetActive(false);
    }

    public void SelectArcherTower() { towerToBuild = archerTowerPrefab; selectedSpot = null; }
    public void SelectWarriorTower() { towerToBuild = warriorTowerPrefab; selectedSpot = null; }
    public void SelectMageTower() { towerToBuild = mageTowerPrefab; selectedSpot = null; }

    public GameObject GetTowerToBuild() => towerToBuild;
    public void ResetSelection() => towerToBuild = null;

    public int GetSelectedTowerCost()
    {
        if (towerToBuild == null) return 0;
        SpawnTower st = towerToBuild.GetComponent<SpawnTower>();
        return st != null ? st.cost : 0;
    }

    public void SelectSpotToUpgrade(BuildingSpot2D spot)
    {
        selectedSpot = spot;

        if (towerActionPanel != null)
        {
            towerActionPanel.SetActive(true);

            Vector3 screenPos = Camera.main.WorldToScreenPoint(
                spot.transform.position
            );

            towerActionPanel.transform.position =
                screenPos + new Vector3(0, -100f, 0);
        }
    }

    public void UpgradeSelectedTower()
    {
        if (selectedSpot == null)
            return;

        GameObject tower = selectedSpot.GetCurrentTower();

        if (tower == null)
            return;

        SpawnTower spawnTower = tower.GetComponent<SpawnTower>();

        if (spawnTower == null)
            return;

        spawnTower.UpgradeTower();

        // Giữ nguyên tutorial Wave 4
        if (GuideManager.instance != null)
        {
            GuideManager.instance.Wave4UpgradeButtonClicked();
        }

        HideTowerActionPanel();
    }

    public void SellSelectedTower()
    {
        if (selectedSpot == null)
            return;

        GameObject tower = selectedSpot.GetCurrentTower();

        if (tower == null)
            return;

        SpawnTower spawnTower = tower.GetComponent<SpawnTower>();

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
        if (Input.GetMouseButtonDown(1)) selectedSpot = null;

        if (selectedSpot == null) return;

        GameObject tower = selectedSpot.GetCurrentTower();
        if (tower == null) return;

        SpawnTower spawnTowerScript = tower.GetComponent<SpawnTower>();

        if (Input.GetKeyDown(KeyCode.U))
        {
            spawnTowerScript.UpgradeTower();

            if (GuideManager.instance != null)
            {
                GuideManager.instance.Wave4UpgradeButtonClicked();
            }

            selectedSpot = null;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            spawnTowerScript.SellTower();
            selectedSpot = null;
        }
    }
}