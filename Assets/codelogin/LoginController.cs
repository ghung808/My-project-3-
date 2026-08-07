using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement; // Dùng để chuyển Scene sau khi đăng nhập thành công

public class LoginController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text messageText;

    [Header("Scene Settings")]
    public string mainGameSceneName = "GameScene"; // Tên scene màn hình chính sau khi đăng nhập

    public void OnLoginButtonClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;

        // Kiểm tra dữ liệu đầu vào cơ bản
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Vui lòng nhập đầy đủ Email và Mật khẩu.");
            return;
        }

        if (!FirebaseManager.Instance.IsFirebaseReady)
        {
            ShowMessage("Firebase chưa sẵn sàng, vui lòng thử lại.");
            return;
        }

        Login(email, password);
    }

    private void Login(string email, string password)
    {
        ShowMessage("Đang đăng nhập...");

        FirebaseAuth auth = FirebaseManager.Instance.Auth;
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                // Xử lý bắt lỗi thân thiện khi đăng nhập sai
                string errorMessage = "Đăng nhập thất bại. Sai email hoặc mật khẩu.";
                if (task.Exception != null)
                {
                    errorMessage = task.Exception.GetBaseException().Message;
                }
                ShowMessage(errorMessage);
                return;
            }

            AuthResult result = task.Result;
            FirebaseUser user = result.User;
            Debug.Log($"[Login] Đăng nhập thành công: {user.Email}");

            ShowMessage("Đăng nhập thành công! Đang vào game...");

            // Gọi hàm chuyển sang màn hình chính sau 1.5 giây
            Invoke(nameof(GoToMainGame), 1.5f);
        });
    }

    private void GoToMainGame()
    {
        // Chuyển sang Scene chính (Nhớ thêm Scene này vào Build Settings trước nhé)
        SceneManager.LoadScene(mainGameSceneName);
    }

    private void ShowMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
        Debug.Log($"[Login] {msg}");
    }
}