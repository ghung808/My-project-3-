using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class HeroSlideBookController : MonoBehaviour
{
    [Header("Panel Cuốn Sách")]
    public GameObject panelSach;
    public CanvasGroup bookCanvasGroup;
    public RectTransform bookContentTransform;

    [Header("UI Trang Trái (Ảnh & Tên)")]
    public Image heroAvatarImage;
    public TextMeshProUGUI heroNameText;

    [Header("UI Trang Phải (Cốt truyện & Chỉ số)")]
    public TextMeshProUGUI heroInfoText;

    [Header("Dữ Liệu Cho 3 Tướng")]
    public Sprite[] heroAvatars = new Sprite[3];
    [TextArea(3, 10)] public string[] heroNames = new string[3];
    [TextArea(5, 15)] public string[] heroInfos = new string[3];

    private int currentIndex = 0;
    private bool isTransitioning = false;

    void Start()
    {
        if (panelSach != null)
        {
            panelSach.SetActive(false); // Mở game lên thì ẩn sách đi
        }
    }

    // Gắn vào nút TƯỚNG ngoài sảnh
    public void OpenBook()
    {
        if (panelSach != null)
        {
            panelSach.SetActive(true);
            currentIndex = 0; // Mở ra luôn hiện Tướng 1 (Index 0)
            UpdateContent(currentIndex);
            if (bookCanvasGroup != null) bookCanvasGroup.alpha = 1f;
        }
    }

    // Gắn vào nút Đóng sách
    public void CloseBook()
    {
        if (panelSach != null)
        {
            panelSach.SetActive(false);
        }
    }

    // Gắn vào nút lật sang phải -> Sang Tướng tiếp theo
    public void NextPage()
    {
        if (isTransitioning) return;
        currentIndex++;
        if (currentIndex >= 3) currentIndex = 0; // Vòng lại Tướng 1 nếu hết
        StartCoroutine(SlidePageAnimation(1));
    }

    // Gắn vào nút lật sang trái -> Về Tướng trước đó
    public void PrevPage()
    {
        if (isTransitioning) return;
        currentIndex--;
        if (currentIndex < 0) currentIndex = 2; // Vòng về Tướng 3 nếu ở Tướng 1
        StartCoroutine(SlidePageAnimation(-1));
    }

    // Hiệu ứng animation trượt trang mượt mà
    IEnumerator SlidePageAnimation(int direction)
    {
        isTransitioning = true;
        float duration = 0.15f;
        float elapsed = 0f;

        Vector2 startPos = Vector2.zero;
        Vector2 outPos = new Vector2(direction * 150f, 0f);

        // 1. Trượt và mờ dần trang cũ đi
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            if (bookCanvasGroup != null) bookCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            if (bookContentTransform != null) bookContentTransform.anchoredPosition = Vector2.Lerp(startPos, outPos, t);
            yield return null;
        }

        // 2. Cập nhật thông tin của tướng mới (Tướng 1, 2 hoặc 3)
        UpdateContent(currentIndex);

        Vector2 inPos = new Vector2(-direction * 150f, 0f);
        if (bookContentTransform != null) bookContentTransform.anchoredPosition = inPos;

        elapsed = 0f;
        // 3. Trượt và hiện rõ trang mới vào giữa
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

        if (heroNameText != null) heroNameText.text = heroNames[index];
        if (heroAvatarImage != null && heroAvatars != null && heroAvatars[index] != null)
            heroAvatarImage.sprite = heroAvatars[index];
        if (heroInfoText != null) heroInfoText.text = heroInfos[index];
    }
}