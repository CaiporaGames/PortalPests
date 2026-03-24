public interface IAuthService
{
    void Register(string name, string email, string password, System.Action onSuccess, System.Action<string> onFailure);
    void Login(string email, string password, System.Action onSuccess, System.Action<string> onFailure);
    void Logout();
    bool IsLoggedIn { get; }
}