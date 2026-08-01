using UnityEngine;

public class BuildingSpot2D : MonoBehaviour
{
    private GameObject currentTower;
    private SpriteRenderer spotRenderer;

    void Start()
    {
        spotRenderer = GetComponent<SpriteRenderer>();
    }

    void OnMouseDown()
    {
        if (currentTower != null)
        {
            SpawnTower towerScript = currentTower.GetComponent<SpawnTower>();
            if (towerScript != null && BuildManager.instance != null)
            {
                BuildManager.instance.SelectSpotToUpgrade(this);
            }
            return;
        }

        if (BuildShopUI.instance != null)
        {
            BuildShopUI.instance.ShowShop(this);
        }
    }

    public void BuildTower(GameObject towerPrefab)
    {
        if (towerPrefab == null) return;

        SpawnTower towerScript = towerPrefab.GetComponent<SpawnTower>();
        int cost = towerScript != null ? towerScript.cost : 50;

        if (PlayerStats.Money >= cost)
        {
            PlayerStats.Money -= cost;

            // Khởi tạo tháp tại vị trí ô đất
            currentTower = Instantiate(towerPrefab, transform.position, Quaternion.identity);

            // Đảm bảo tháp mới sinh ra luôn hiện hình ảnh (Sprite) và nằm đè lên trên nền cỏ
            SpriteRenderer towerSprite = currentTower.GetComponent<SpriteRenderer>();
            if (towerSprite != null)
            {
                towerSprite.enabled = true;
                towerSprite.sortingOrder = 5; // Số lớn hơn nền cỏ để hiển thị rõ
            }

            // Liên kết ô đất với tháp
            SpawnTower installedTower = currentTower.GetComponent<SpawnTower>();
            if (installedTower != null)
            {
                installedTower.SetAssignedSpot(this);
            }

            // Tắt hiệu ứng màu xanh của ô đất đi
            if (spotRenderer != null) spotRenderer.enabled = false;
        }
        else
        {
            Debug.Log("Không đủ tiền xây thành!");
        }
    }

    public void ClearSpot()
    {
        if (currentTower != null)
        {
            currentTower = null;
        }

        if (spotRenderer != null)
        {
            spotRenderer.enabled = true;
        }
    }

    public GameObject GetCurrentTower()
    {
        return currentTower;
    }
}