using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;

public class PlayFabAuthService : IAuthService
{
    public bool IsLoggedIn => PlayFabClientAPI.IsClientLoggedIn();

    public void Register(string name, string email, string password, System.Action onSuccess, System.Action<string> onFailure)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Email = email,
            Password = password,
            Username = name, // Optional
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request,
            result => {
                Debug.Log("PlayFab registration successful.");
                onSuccess?.Invoke();
            },
            error => {
                Debug.LogError("PlayFab registration failed: " + error.GenerateErrorReport());
                onFailure?.Invoke(error.ErrorMessage);
            });
    }

    public void Login(string email, string password, System.Action onSuccess, System.Action<string> onFailure)
    {
        var request = new LoginWithEmailAddressRequest
        {
            Email = email,
            Password = password
        };

        PlayFabClientAPI.LoginWithEmailAddress(request,
            result => {
                Debug.Log("PlayFab login successful.");
                onSuccess?.Invoke();
            },
            error => {
                Debug.LogError("PlayFab login failed: " + error.GenerateErrorReport());
                onFailure?.Invoke(error.ErrorMessage);
            });
    }

    public void Logout()
    {
        PlayFabClientAPI.ForgetAllCredentials();
        Debug.Log("PlayFab user logged out.");
    }
}
