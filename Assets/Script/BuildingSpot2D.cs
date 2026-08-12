using UnityEngine;

public class BuildingSpot2D : MonoBehaviour
{
    [Header("Hiệu ứng Visual")]
    public SpriteRenderer spotRenderer;
    public Color hoverColor = Color.green;

    private Color originalColor;
    private GameObject currentTower;

    public static bool canBuild = false;

    void Start()
    {
        if (spotRenderer == null) spotRenderer = GetComponent<SpriteRenderer>();
        if (spotRenderer != null) originalColor = spotRenderer.color;
    }

    void OnMouseEnter()
    {
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

        if (GuideManager.instance != null &&
            !GuideManager.instance.tutorialFinished)
        {
            GuideManager.instance.BuildingSpotClicked();
        }
    }

    public bool BuildTower(GameObject towerPrefab)
    {
        if (towerPrefab == null)
            return false;

        SpawnTower towerScript = towerPrefab.GetComponent<SpawnTower>();

        if (towerScript == null)
        {
            Debug.LogError("Tower Prefab không có SpawnTower!");
            return false;
        }

        // Kiểm tra GameUI
        if (GameUI.instance == null)
        {
            Debug.LogError("Không tìm thấy GameUI.instance!");
            return false;
        }

        // KHÔNG TRỪ TIỀN Ở ĐÂY
        // BuildShopUI sẽ trừ tiền sau khi xây thành công.

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

        Debug.Log("🏗️ Xây tháp thành công!");

        return true;
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