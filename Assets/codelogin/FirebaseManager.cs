using System.Collections;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public bool IsFirebaseReady { get; private set; } = false;

    private void Awake()
    {
        // Singleton pattern - giữ FirebaseManager xuyên suốt các scene
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                Auth = FirebaseAuth.DefaultInstance;
                IsFirebaseReady = true;
                Debug.Log("[FirebaseManager] Firebase khởi tạo thành công.");
            }
            else
            {
                Debug.LogError($"[FirebaseManager] Lỗi dependency: {dependencyStatus}");
            }
        });
    }
}