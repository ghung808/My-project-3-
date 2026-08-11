using UnityEngine;
using UnityEngine.UI;

public class FireballSkillController : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject fireballPrefab; // Kéo Prefab cầu lửa vào đây
    public float cooldownTime = 3f;    // Thời gian hồi chiêu
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

            // Khi đã chọn skill và click chuột trái lên map
            if (isSkillSelected && Input.GetMouseButtonDown(0))
            {
                CastFireballAtMousePosition();
            }
        }
    }

    public void OnSkillButtonClicked()
    {
        if (Time.time >= nextReadyTime)
        {
            isSkillSelected = true;
            Debug.Log("Đã chọn chiêu! Click chuột lên map để giáng xuống.");
        }
    }

    void CastFireballAtMousePosition()
    {
        // 1. Lấy vị trí click chuột trên màn hình chuyển thành tọa độ trong game (World Point)
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f; // Cố định trục Z trong game 2D

        // 2. Tạo quả cầu lửa ngay tại vị trí click chuột
        if (fireballPrefab != null)
        {
            Instantiate(fireballPrefab, worldPos, Quaternion.identity);
        }

        // 3. Kích hoạt hồi chiêu và tắt trạng thái chọn
        nextReadyTime = Time.time + cooldownTime;
        isSkillSelected = false;
    }
}