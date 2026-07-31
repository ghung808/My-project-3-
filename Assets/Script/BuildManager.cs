using UnityEngine;

public class BuildManager : MonoBehaviour
{
    public static BuildManager instance;
    void Awake() { if (instance == null) instance = this; }

    [Header("Kéo 3 Prefab Tháp vào đây")]
    public GameObject archerTowerPrefab;
    public GameObject warriorTowerPrefab;
    public GameObject mageTowerPrefab;

    private GameObject towerToBuild;
    private BuildingSpot2D selectedSpot;

    public bool CanBuild => towerToBuild != null;

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
        Debug.Log("Đã chọn ô thành! Nhấn 'U' để Nâng cấp ($60), Nhấn 'S' để Gỡ bỏ thành.");
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
            selectedSpot = null;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            spawnTowerScript.SellTower();
            selectedSpot = null;
        }
    }
}