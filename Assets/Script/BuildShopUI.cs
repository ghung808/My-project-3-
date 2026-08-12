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
        Debug.Log("ShowShop"); // Added debug log to check if method is called

        selectedSpot = spot;

        if (shopPanel != null && Camera.main != null)
        {
            // Lấy Canvas chứa shopPanel để kiểm tra RenderMode
            Canvas canvas = shopPanel.GetComponentInParent<Canvas>();
            Vector3 screenPos = Camera.main.WorldToScreenPoint(spot.transform.position);

            if (canvas != null && canvas.renderMode == RenderMode.ScreenSpaceCamera)
            {
                // Nếu Canvas dùng chế độ Screen Space - Camera, cần dùng RectTransformUtility để chuyển đổi chuẩn xác
                Vector2 localPoint;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    screenPos,
                    canvas.worldCamera,
                    out localPoint
                );

                // Đặt vị trí cục bộ của shopPanel theo điểm đã tính + đẩy lên một chút (ví dụ 50 đơn vị)
                shopPanel.transform.localPosition = (Vector3)localPoint + new Vector3(0, 50f, 0);
            }
            else
            {
                // Nếu Canvas dùng chế độ Screen Space - Overlay thông thường
                shopPanel.transform.position = screenPos + new Vector3(0, 50f, 0);
            }

            shopPanel.SetActive(true);
        }
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