using UnityEngine;
using UnityEngine.UI;

public class IceSkillController : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject icePrefab;     // Kéo Prefab Băng vào đây
    public float cooldownTime = 4f;  // Thời gian hồi chiêu
    private float nextReadyTime = 0f;

    [Header("UI References")]
    public Button skillButton;
    public Image cooldownOverlay;

    private bool isSkillSelected = false;

    void Start()
    {
        if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;
        if (skillButton != null) skillButton.onClick.AddListener(OnSkillButtonClicked);
    }

    void Update()
    {
        if (Time.time < nextReadyTime)
        {
            float timeLeft = nextReadyTime - Time.time;
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = timeLeft / cooldownTime;
        }
        else
        {
            if (cooldownOverlay != null) cooldownOverlay.fillAmount = 0f;

            if (isSkillSelected && Input.GetMouseButtonDown(0))
            {
                CastIceSkill();
            }
        }
    }

    public void OnSkillButtonClicked()
    {
        if (Time.time >= nextReadyTime)
        {
            isSkillSelected = true;
            Debug.Log("Đã chọn kỹ năng Băng! Click chuột lên bản đồ.");
        }
    }

    void CastIceSkill()
    {
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f;

        if (icePrefab != null)
        {
            Instantiate(icePrefab, worldPos, Quaternion.identity);
        }

        nextReadyTime = Time.time + cooldownTime;
        isSkillSelected = false;
    }
}