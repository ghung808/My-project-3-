using UnityEngine;

public class BuildingSpot2D : MonoBehaviour
{
    [Header("Hiệu ứng Visual")]
    public SpriteRenderer spotRenderer;
    public Color hoverColor = Color.green;

    private Color originalColor;
    private GameObject currentTower;

    public static bool canBuild = false;  // Thêm biến này

    void Start()
    {
        if (spotRenderer == null) spotRenderer = GetComponent<SpriteRenderer>();
        if (spotRenderer != null) originalColor = spotRenderer.color;
    }

    void OnMouseEnter()
    {
        // Thêm điều kiện !canBuild
        if (spotRenderer == null || currentTower != null || !canBuild) return;
        spotRenderer.color = hoverColor;
    }

    void OnMouseExit()
    {
        if (spotRenderer != null) spotRenderer.color = originalColor;
    }

    void OnMouseDown()
    {
        if (currentTower != null)
        {
            BuildManager.instance.SelectSpotToUpgrade(this);
            return;
        }

        BuildShopUI.instance.ShowShop(this);

        // Báo cho Tutorial biết người chơi đã bấm ô xây
        if (GuideManager.instance != null)
        {
            GuideManager.instance.BuildingSpotClicked();
        }
    }

    public bool BuildTower(GameObject towerPrefab)
    {
        if (towerPrefab == null)
            return false;

        SpawnTower towerScript = towerPrefab.GetComponent<SpawnTower>();

        int cost = towerScript != null ? towerScript.cost : 50;

        if (PlayerStats.Money >= cost)
        {
            PlayerStats.Money -= cost;

            currentTower = Instantiate(
                towerPrefab,
                transform.position,
                Quaternion.identity
            );

            SpawnTower installedTower = currentTower.GetComponent<SpawnTower>();

            if (installedTower != null)
            {
                installedTower.targetSpot = this;
            }

            if (spotRenderer != null)
            {
                spotRenderer.enabled = false;
            }

            Debug.Log("Xây tháp thành công!");

            return true;
        }
        else
        {
            Debug.Log("Không đủ tiền xây thành!");

            return false;
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
            spotRenderer.color = originalColor;
        }
    }

    public GameObject GetCurrentTower() => currentTower;
}