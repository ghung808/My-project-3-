using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Cấu hình Kéo Chuột")]
    // 0 = Chuột Trái, 1 = Chuột Phải, 2 = Chuột Giữa
    public int mouseButton = 0;

    private Vector3 dragOrigin;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        HandleMouseDrag();
    }

    void HandleMouseDrag()
    {
        // Khi bắt đầu nhấn giữ chuột: Lưu vị trí điểm bấm trong World Space
        if (Input.GetMouseButtonDown(mouseButton))
        {
            dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        // Trong lúc đang giữ chuột và kéo: Cập nhật vị trí Camera
        if (Input.GetMouseButton(mouseButton))
        {
            Vector3 difference = dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
            transform.position += difference;
        }
    }
}