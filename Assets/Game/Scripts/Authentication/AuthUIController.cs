using UnityEngine;
using TMPro;
using Cysharp.Threading.Tasks;
using UnityEngine.UI;

public class AuthUIController : BaseUIController
{
    [SerializeField] private TMP_InputField _nameInput;
    [SerializeField] private TMP_InputField _emailInput;
    [SerializeField] private TMP_InputField _passwordInput;
    [SerializeField] private TMP_InputField _confirmPasswordInput;
    [SerializeField] private Button _loginButton;
    [SerializeField] private Button _qustionButton;
    private bool alreadyHaveAccount = true;
    private IAuthService _authService;
    public override async UniTask InitializeAsync()
    {
        _qustionButton.GetComponentInChildren<TextMeshProUGUI>().text = alreadyHaveAccount ? "Don't have an account?" : "Already have an account?";
        _loginButton.onClick.AddListener(OnLoginClicked);
        _qustionButton.onClick.AddListener(IsAlreadyHaveAccount);
        _authService = ServiceLocator.Resolve<IAuthService>();
        _confirmPasswordInput.gameObject.SetActive(!alreadyHaveAccount);
        _nameInput.gameObject.SetActive(!alreadyHaveAccount);
        // Load data, e.g. player prefs, settings
        await UniTask.Delay(100);
        // setup buttons callbacks
    }

    private void OnRegisterClicked()
    {
        if (_passwordInput.text != _confirmPasswordInput.text)
        {
            Debug.LogError("Passwords do not match!");
            return;
        }
        if (string.IsNullOrEmpty(_nameInput.text) || string.IsNullOrEmpty(_emailInput.text)
            || string.IsNullOrEmpty(_passwordInput.text) || string.IsNullOrEmpty(_confirmPasswordInput.text))
        {
            Debug.LogError("All fields are required!");
            return;
        }
        _authService.Register(_nameInput.text, _emailInput.text, _passwordInput.text,
        onSuccess: async () => await ServiceLocator.Resolve<IUIService>().ShowScreenAsync<object>(ScreenTypes.LevelLoaderScreen),
        onFailure: error => Debug.LogError("Register failed: " + error));
    }

    private void OnLoginClicked()
    {
        if (string.IsNullOrEmpty(_emailInput.text) || string.IsNullOrEmpty(_passwordInput.text))
        {
            Debug.LogError("All fields are required!");
            return;
        }
        
        _authService.Login(_emailInput.text, _passwordInput.text,
        onSuccess: async () => await ServiceLocator.Resolve<IUIService>().ShowScreenAsync<object>(ScreenTypes.LevelLoaderScreen),
        onFailure: error => Debug.LogError("Login failed: " + error));
    }
    
    private void IsAlreadyHaveAccount()
    {
        alreadyHaveAccount = !alreadyHaveAccount;
        _confirmPasswordInput.gameObject.SetActive(!alreadyHaveAccount);
        _nameInput.gameObject.SetActive(!alreadyHaveAccount);
        _loginButton.onClick.RemoveAllListeners();
        _loginButton.onClick.AddListener(alreadyHaveAccount ? OnLoginClicked : OnRegisterClicked);
        _loginButton.GetComponentInChildren<TextMeshProUGUI>().text = alreadyHaveAccount ? "Login" : "Register";
        _qustionButton.GetComponentInChildren<TextMeshProUGUI>().text = alreadyHaveAccount ? "Create an account" : "Already have an account?";
    }
}
