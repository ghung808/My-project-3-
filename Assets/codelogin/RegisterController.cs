using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class RegisterController : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_InputField confirmPasswordInput;
    public TMP_Text messageText;

    public void OnRegisterButtonClicked()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        string confirmPassword = confirmPasswordInput.text;

        // Kiểm tra dữ liệu đầu vào cơ bản
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            ShowMessage("Vui lòng nhập đầy đủ Email và Mật khẩu.");
            return;
        }

        if (password != confirmPassword)
        {
            ShowMessage("Mật khẩu xác nhận không khớp.");
            return;
        }

        if (password.Length < 6)
        {
            ShowMessage("Mật khẩu phải có ít nhất 6 ký tự.");
            return;
        }

        if (!FirebaseManager.Instance.IsFirebaseReady)
        {
            ShowMessage("Firebase chưa sẵn sàng, vui lòng thử lại.");
            return;
        }

        Register(email, password);
    }

    private void Register(string email, string password)
    {
        ShowMessage("Đang tạo tài khoản...");

        FirebaseAuth auth = FirebaseManager.Instance.Auth;
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                // Xử lý lấy thông báo lỗi thân thiện
                string errorMessage = "Đăng ký thất bại. Vui lòng kiểm tra lại thông tin.";
                if (task.Exception != null)
                {
                    errorMessage = task.Exception.GetBaseException().Message;
                }
                ShowMessage(errorMessage);
                return;
            }

            AuthResult result = task.Result;
            FirebaseUser newUser = result.User;
            Debug.Log($"[Register] Tạo tài khoản thành công: {newUser.Email}");

            ShowMessage("Đăng ký thành công! Đang chuyển sang màn hình đăng nhập...");
        });
    }

    private void ShowMessage(string msg)
    {
        if (messageText != null) messageText.text = msg;
        Debug.Log($"[Register] {msg}");
    }
}