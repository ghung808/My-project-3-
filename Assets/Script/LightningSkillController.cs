using UnityEngine;
using UnityEngine.UI;

public class LightningSkillController : MonoBehaviour
{
    [Header("Skill Settings")]
    public GameObject lightningPrefab; // Kéo Prefab sét vào đây
    public float cooldownTime = 3f;    // Thời gian hồi chiêu (giây)
    private float nextReadyTime = 0f;

    [Header("UI References")]
    public Button skillButton;         // Kéo nút Skill vào đây
    public Image cooldownOverlay;      // Kéo ảnh mờ hồi chiêu vào đây

    [Header("State")]
    private bool isSkillSelected = false; // Trạng thái đã bấm chọn skill chưa

    void Start()
    {
        if (cooldownOverlay != null)
            cooldownOverlay.fillAmount = 0f;

        if (skillButton != null)
            skillButton.onClick.AddListener(OnSkillButtonClicked);
    }

    void Update()
    {
        // 1. Kiểm tra thời gian hồi chiêu
        if (Time.time < nextReadyTime)
        {
            float timeLeft = nextReadyTime - Time.time;
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = timeLeft / cooldownTime;
            }
        }
        else
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.fillAmount = 0f;
            }

            // 2. Nếu đã bấm chọn skill và đang trong thời gian sẵn sàng, chờ click chuột trái lên map
            if (isSkillSelected && Input.GetMouseButtonDown(0))
            {
                CastLightning();
            }
        }
    }

    // Hàm được gọi khi bấm vào nút ở góc dưới bên trái
    public void OnSkillButtonClicked()
    {
        if (Time.time >= nextReadyTime)
        {
            isSkillSelected = true;
            Debug.Log("Đã chọn kỹ năng sét! Hãy click chuột lên bản đồ.");
        }
        else
        {
            Debug.Log("Kỹ năng đang hồi chiêu!");
        }
    }

    // Hàm thực hiện triệu hồi sét
    void CastLightning()
    {
        // Lấy tọa độ click chuột trên màn hình chuyển thành tọa độ trong thế giới game 2D
        Vector3 mousePos = Input.mousePosition;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        worldPos.z = 0f; // Đảm bảo đưa về mặt phẳng 2D Z = 0

        // Sinh ra sét tại vị trí click chuột
        if (lightningPrefab != null)
        {
            Instantiate(lightningPrefab, worldPos, Quaternion.identity);
        }

        // Bắt đầu tính thời gian hồi chiêu
        nextReadyTime = Time.time + cooldownTime;
        isSkillSelected = false; // Hủy trạng thái chọn để chuẩn bị cho lần sau
    }
}