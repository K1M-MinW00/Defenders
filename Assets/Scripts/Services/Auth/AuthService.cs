using Firebase;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using UnityEngine;

public sealed class AuthService : MonoBehaviour
{
    public static AuthService Instance { get; private set; }

    public bool IsInitialized { get; private set; }
    public bool IsBusy { get; private set; }

    private FirebaseAuth auth;
    private FirebaseUser CurrentUser => auth?.CurrentUser;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }


    public async Task<AuthLoginResult> SignInAsync()
    {
        if (IsBusy)
            return AuthLoginResult.Fail("Authentication is already in progress.");

        IsBusy = true;

        try
        {
            bool initOk = await InitializeFirebaseAsync();

            if (!initOk)
                return Fail("Firebase initialization failed.");

            if (CurrentUser != null)
            {
                bool isSessionValid = await ValidateCurrentUserSessionAsync();

                if (isSessionValid)
                {
                    Debug.Log($"[AuthManager] Auto login Success. UID : {CurrentUser.UserId}");
                    return AuthLoginResult.Success(CurrentUser.UserId);
                }

                Debug.LogWarning("[AuthManager] Cached User session is invalid. Signing out");
                SignOut();
            }

            AuthResult authResult = await auth.SignInAnonymouslyAsync();

            if (authResult?.User == null)
                return Fail("Anonymous login returned null user.");

            Debug.Log($"[AuthManager] Anonymous login success. UID : {authResult.User.UserId}");
            return AuthLoginResult.Success(authResult.User.UserId);
        }
        catch (Exception e)
        {
            return Fail($"Login exception: {e}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<bool> InitializeFirebaseAsync()
    {
        if (IsInitialized)
            return true;

        try
        {
            DependencyStatus dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

            if (dependencyStatus != DependencyStatus.Available)
            {
                Debug.LogError($"[AuthManager] Firebase Dependency error : {dependencyStatus}");
                return false;
            }

            auth = FirebaseAuth.DefaultInstance;
            IsInitialized = auth != null;

            Debug.Log("[AuthManager] Firebase Auth Initialized");
            return IsInitialized;
        }
        catch (Exception e)
        {
            Debug.LogError($"[AuthManager] Firebase initialize exception : {e}");
            return false;
        }
    }

    private async Task<bool> ValidateCurrentUserSessionAsync()
    {
        if (CurrentUser == null)
            return false;

        try
        {
            await CurrentUser.TokenAsync(true);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[AuthManager] Session validation failed : {e}");
            return false;
        }
    }

    public void SignOut()
    {
        if (!IsInitialized || auth == null)
            return;

        auth.SignOut();
        Debug.Log("[AuthManager] Signed out");
    }

    private static AuthLoginResult Fail(string message)
    {
        Debug.LogError($"[AuthManager] {message}");
        return AuthLoginResult.Fail(message);
    }
}
