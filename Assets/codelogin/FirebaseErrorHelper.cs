using Firebase;
using Firebase.Auth;
using System;

public static class FirebaseErrorHelper
{
    public static string GetErrorMessage(Exception exception)
    {
        if (exception == null) return "Có lỗi không xác định xảy ra.";

        // Firebase trả lỗi dạng AggregateException, cần "bóc" ra FirebaseException
        Exception innerException = exception;
        while (innerException is AggregateException aggEx && aggEx.InnerException != null)
        {
            innerException = aggEx.InnerException;
        }

        if (innerException is FirebaseException firebaseEx)
        {
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    return "Vui lòng nhập Email.";
                case AuthError.MissingPassword:
                    return "Vui lòng nhập Mật khẩu.";
                case AuthError.WeakPassword:
                    return "Mật khẩu quá yếu (cần tối thiểu 6 ký tự).";
                case AuthError.InvalidEmail:
                    return "Email không đúng định dạng.";
                case AuthError.EmailAlreadyInUse:
                    return "Email này đã được đăng ký.";
                case AuthError.WrongPassword:
                    return "Sai mật khẩu.";
                case AuthError.UserNotFound:
                    return "Tài khoản không tồn tại.";
                case AuthError.UserDisabled:
                    return "Tài khoản đã bị vô hiệu hóa.";
                case AuthError.NetworkRequestFailed:
                    return "Lỗi kết nối mạng. Vui lòng kiểm tra Internet.";
                case AuthError.TooManyRequests:
                    return "Bạn đã thử quá nhiều lần, vui lòng thử lại sau.";
                default:
                    return $"Lỗi: {errorCode}";
            }
        }

        return exception.Message;
    }
}