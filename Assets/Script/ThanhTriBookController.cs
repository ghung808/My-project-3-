using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ThanhTriBookController : MonoBehaviour
{
    [Header("Panel Sách Thành Trì")]
    public GameObject panelSachThanhTri;
    public CanvasGroup bookCanvasGroup;
    public RectTransform bookContentTransform;

    [Header("UI Trang Trái (Ảnh & Tên Thành Trì)")]
    public Image thanhTriImage;
    public TextMeshProUGUI tenThanhTriText;

    [Header("UI Trang Phải (Thông tin & Cốt truyện Thành Trì)")]
    public TextMeshProUGUI thanhTriInfoText;

    [Header("Dữ Liệu Cho 3 Thành Trì / Khu Vực")]
    public Sprite[] thanhTriImages = new Sprite[3];
    [TextArea(3, 10)] public string[] tenThanhTris = new string[3];
    [TextArea(5, 15)] public string[] thanhTriInfos = new string[3];

    private int currentIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        if (panelSachThanhTri != null)
        {
            panelSachThanhTri.SetActive(false); // Mở game lên thì ẩn sách đi
        }
    }

    // Gắn vào nút THÀNH TRÌ ngoài sảnh
    public void OpenThanhTriBook()
    {
        if (panelSachThanhTri != null)
        {
            panelSachThanhTri.SetActive(true);
            currentIndex = 0; // Mở ra luôn hiện Thành Trì 1
            UpdateContent(currentIndex);
            if (bookCanvasGroup != null) bookCanvasGroup.alpha = 1f;
        }
    }

    // Gắn vào nút Đóng sách
    public void CloseThanhTriBook()
    {
        if (panelSachThanhTri != null)
        {
            panelSachThanhTri.SetActive(false);
        }
    }

    // Nút lật sang phải -> Sang Thành Trì tiếp theo
    public void NextPage()
    {
        if (isTransitioning) return;
        currentIndex++;
        if (currentIndex >= 3) currentIndex = 0; // Vòng lại Thành Trì 1 nếu hết
        StartCoroutine(SlidePageAnimation(1));
    }

    // Nút lật sang trái -> Về Thành Trì trước đó
    public void PrevPage()
    {
        if (isTransitioning) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = 2; // Vòng về Thành Trì 3
        StartCoroutine(SlidePageAnimation(-1));
    }

    // Hiệu ứng animation trượt trang mượt mà y hệt bên Tướng
    IEnumerator SlidePageAnimation(int direction)
    {
        isTransitioning = true;
        float duration = 0.15f;
        float elapsed = 0f;

        Vector2 startPos = Vector2.zero;
        Vector2 outPos = new Vector2(direction * 150f, 0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (bookCanvasGroup != null) bookCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            if (bookContentTransform != null) bookContentTransform.anchoredPosition = Vector2.Lerp(startPos, outPos, t);
            yield return null;
        }

        UpdateContent(currentIndex);

        Vector2 inPos = new Vector2(-direction * 150f, 0f);
        if (bookContentTransform != null) bookContentTransform.anchoredPosition = inPos;

        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (bookCanvasGroup != null) bookCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
            if (bookContentTransform != null) bookContentTransform.anchoredPosition = Vector2.Lerp(inPos, startPos, t);
            yield return null;
        }

        if (bookCanvasGroup != null) bookCanvasGroup.alpha = 1f;
        if (bookContentTransform != null) bookContentTransform.anchoredPosition = startPos;

        isTransitioning = false;
    }

    void UpdateContent(int index)
    {
        if (index < 0 || index >= 3) return;

        if (tenThanhTriText != null) tenThanhTriText.text = tenThanhTris[index];
        if (thanhTriImage != null && thanhTriImages != null && thanhTriImages[index] != null)
            thanhTriImage.sprite = thanhTriImages[index];
        if (thanhTriInfoText != null) thanhTriInfoText.text = thanhTriInfos[index];
    }
}