using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChallengeMenuController : MonoBehaviour
{
    [Header("Panel chứa 3 Map")]
    public GameObject panelChonMap;

    [Header("Nút các Map")]
    public Button btnMap2;
    public Button btnMap3;

    private void Start()
    {
        UpdateMapButtonsUI();
    }

    public void OpenChallengeMenu()
    {
        if (panelChonMap != null) panelChonMap.SetActive(true);
        UpdateMapButtonsUI(); // Cập nhật lại trạng thái nút mỗi khi mở menu
    }

    public void CloseChallengeMenu()
    {
        if (panelChonMap != null) panelChonMap.SetActive(false);
    }

    // Cập nhật trạng thái bật/tắt của nút bấm
    private void UpdateMapButtonsUI()
    {
        bool isMap2Unlocked = PlayerPrefs.GetInt("Map2_Unlocked", 0) == 1;
        bool isMap3Unlocked = PlayerPrefs.GetInt("Map3_Unlocked", 0) == 1;

        if (btnMap2 != null) btnMap2.interactable = isMap2Unlocked;
        if (btnMap3 != null) btnMap3.interactable = isMap3Unlocked;
    }

    // --- MAP 1 ---
    public void OnClickMap1()
    {
        SceneManager.LoadScene("Vht");
    }

    // --- MAP 2 ---
    public void OnClickMap2()
    {
        if (PlayerPrefs.GetInt("Map2_Unlocked", 0) == 1)
        {
            SceneManager.LoadScene("hgt");
        }
        else
        {
            Debug.Log("Map 2 đang bị khóa!");
        }
    }

    // --- MAP 3 ---
    public void OnClickMap3()
    {
        if (PlayerPrefs.GetInt("Map3_Unlocked", 0) == 1)
        {
            SceneManager.LoadScene("dh");
        }
        else
        {
            Debug.Log("Map 3 đang bị khóa!");
        }
    }

    public void OnClickBackToLobby()
    {
        SceneManager.LoadScene("Sanh");
    }

    // (Tùy chọn) Hàm xóa dữ liệu để test lại từ đầu
    [ContextMenu("Reset All Map Progress")]
    public void ResetProgress()
    {
        PlayerPrefs.DeleteKey("Map2_Unlocked");
        PlayerPrefs.DeleteKey("Map3_Unlocked");
        PlayerPrefs.Save();
        UpdateMapButtonsUI();
    }
}