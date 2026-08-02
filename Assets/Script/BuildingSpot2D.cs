using UnityEngine;

public class BuildingSpot2D : MonoBehaviour
{
    [Header("Hiệu ứng Visual")]
    public SpriteRenderer spotRenderer;
    public Color hoverColor = Color.green;

    private Color originalColor;
    private GameObject currentTower;

    void Start()
    {
        if (spotRenderer == null) spotRenderer = GetComponent<SpriteRenderer>();
        if (spotRenderer != null) originalColor = spotRenderer.color;
    }

    void OnMouseEnter()
    {
        if (spotRenderer == null || currentTower != null) return;
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
    }

    public void BuildTower(GameObject towerPrefab)
    {
        if (towerPrefab == null) return;

        SpawnTower towerScript = towerPrefab.GetComponent<SpawnTower>();
        int cost = towerScript != null ? towerScript.cost : 50;

        if (PlayerStats.Money >= cost)
        {
            PlayerStats.Money -= cost;

            currentTower = Instantiate(towerPrefab, transform.position, Quaternion.identity);

            SpawnTower installedTower = currentTower.GetComponent<SpawnTower>();
            if (installedTower != null)
            {
                installedTower.targetSpot = this;
            }

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
            spotRenderer.color = originalColor;
        }
    }

    public GameObject GetCurrentTower() => currentTower;
}