using UnityEngine;

public class AuthSwitchController : MonoBehaviour
{
    public GameObject loginPanel;
    public GameObject registerPanel;

    // Hàm chuyển sang màn hình Đăng ký
    public void ShowRegisterPanel()
    {
        if (loginPanel != null) loginPanel.SetActive(false);
        if (registerPanel != null) registerPanel.SetActive(true);
    }

    // Hàm chuyển sang màn hình Đăng nhập
    public void ShowLoginPanel()
    {
        if (registerPanel != null) registerPanel.SetActive(false);
        if (loginPanel != null) loginPanel.SetActive(true);
    }
}