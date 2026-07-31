using UnityEngine;

public class BuildShopUI : MonoBehaviour
{
    public static BuildShopUI instance;

    [Header("Tham chiếu UI")]
    public GameObject shopPanel;
    private BuildingSpot2D selectedSpot;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        HideShop();
    }

    public void ShowShop(BuildingSpot2D spot)
    {
        selectedSpot = spot;
        Vector3 screenPos = Camera.main.WorldToScreenPoint(spot.transform.position);
        shopPanel.transform.position = screenPos + new Vector3(0, 40f, 0);
        shopPanel.SetActive(true);
    }

    public void HideShop()
    {
        selectedSpot = null;
        if (shopPanel != null) shopPanel.SetActive(false);
    }

    public void SelectWarrior()
    {
        if (selectedSpot != null && BuildManager.instance != null)
        {
            selectedSpot.BuildTower(BuildManager.instance.warriorTowerPrefab);
        }
        HideShop();
    }

    public void SelectArcher()
    {
        if (selectedSpot != null && BuildManager.instance != null)
        {
            selectedSpot.BuildTower(BuildManager.instance.archerTowerPrefab);
        }
        HideShop();
    }

    public void SelectMage()
    {
        if (selectedSpot != null && BuildManager.instance != null)
        {
            selectedSpot.BuildTower(BuildManager.instance.mageTowerPrefab);
        }
        HideShop();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            HideShop();
        }
    }
}