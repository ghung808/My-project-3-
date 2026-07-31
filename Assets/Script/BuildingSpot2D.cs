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
        // Nếu ô đã có tháp -> Click vào để mở bảng nâng cấp/gỡ bỏ
        if (currentTower != null)
        {
            SpawnTower towerScript = currentTower.GetComponent<SpawnTower>();
            if (towerScript != null && BuildManager.instance != null)
            {
                BuildManager.instance.SelectSpotToUpgrade(this);
            }
            return;
        }

        // Nếu ô chưa có tháp -> Mở shop xây dựng
        if (BuildShopUI.instance != null)
        {
            BuildShopUI.instance.ShowShop(this);
        }
    }

    // Hàm xây tháp được gọi từ BuildShopUI
    public void BuildTower(GameObject towerPrefab)
    {
        if (towerPrefab == null) return;

        // Lấy thông tin giá từ script SpawnTower của tháp
        SpawnTower towerScript = towerPrefab.GetComponent<SpawnTower>();
        int cost = towerScript != null ? towerScript.cost : 50;

        // Kiểm tra tiền của người chơi
        if (PlayerStats.Money >= cost)
        {
            PlayerStats.Money -= cost;

            // Khởi tạo (sinh ra) tháp chính xác tại vị trí của ô đất này
            currentTower = Instantiate(towerPrefab, transform.position, Quaternion.identity);

            // Liên kết tháp vừa tạo với ô đất hiện tại
            SpawnTower installedTower = currentTower.GetComponent<SpawnTower>();
            if (installedTower != null)
            {
                // Nếu SpawnTower của bạn cần lưu ô đất, ta có thể dùng hàm hoặc biến tương thích
                // Hoặc đơn giản là không cần gọi targetSpot nếu SpawnTower tự tìm waypoint.
            }

            // Tắt hiệu ứng màu xanh của ô đất đi khi đã có tháp đứng lên trên
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
            Destroy(currentTower);
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