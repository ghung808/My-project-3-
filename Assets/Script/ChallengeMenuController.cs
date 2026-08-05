using UnityEngine;
using UnityEngine.SceneManagement;

public class ChallengeMenuController : MonoBehaviour
{
    [Header("Panel chứa 3 Map")]
    public GameObject panelChonMap;

    public void OpenChallengeMenu()
    {
        if (panelChonMap != null) panelChonMap.SetActive(true);
    }

    public void CloseChallengeMenu()
    {
        if (panelChonMap != null) panelChonMap.SetActive(false);
    }

    // --- MAP 1 (Thay "Map1Scene" bằng tên Scene tương ứng của bạn, ví dụ "Vht") ---
    public void OnClickMap1()
    {
        SceneManager.LoadScene("Vht"); // Hoặc tên Scene Map 1 của bạn
    }

    // --- MAP 2 (Thay "Map2Scene" bằng tên Scene tương ứng, ví dụ "hgt") ---
    public void OnClickMap2()
    {
        if (PlayerPrefs.GetInt("Map2_Unlocked", 0) == 1)
        {
            SceneManager.LoadScene("hgt"); // Hoặc tên Scene Map 2 của bạn
        }
        else
        {
            Debug.Log("Map 2 đang bị khóa!");
        }
    }

    // --- MAP 3 (Thay "Map3Scene" bằng tên Scene tương ứng, ví dụ "dh") ---
    public void OnClickMap3()
    {
        if (PlayerPrefs.GetInt("Map3_Unlocked", 0) == 1)
        {
            SceneManager.LoadScene("dh"); // Hoặc tên Scene Map 3 của bạn
        }
        else
        {
            Debug.Log("Map 3 đang bị khóa!");
        }
    }

    // Nút quay lại sảnh
    public void OnClickBackToLobby()
    {
        SceneManager.LoadScene("Sanh");
    }
}