using DieteticAI.UI.Services.Login.Abstractions;
using DieteticAI.UI.Services.Login.Requests;
using DieteticAI.UI.Tools;
using Microsoft.AspNetCore.Components;

namespace DieteticAI.UI.Pages;

public partial class LoginUser
{
    private const string LoginPageRoute = "/loadUser";
    private const string DebugUsername = "admin";
    private const string DebugPassword = "password";

    private readonly LoginRequest _loginRequest = new();
    private bool _isLoggingIn;
    private bool _isUserTestMode;
    private string ErrorMessage { get; set; } = string.Empty;

    [Inject]
    private SessionManager SessionManager { get; set; } = default!;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IUserLoginService UserLoginService { get; set; } = default!;

    [Inject]
    private ILoginCallbackService LoginCallbackService { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        _isUserTestMode = string.Equals(Configuration["debugUser"], "true", StringComparison.OrdinalIgnoreCase);

        if (LoginCallbackService.TryCompleteLogin(Navigation.ToAbsoluteUri(Navigation.Uri)))
        {
            ErrorMessage = string.Empty;
            Navigation.NavigateTo(LoginPageRoute, replace: true);
            return;
        }

        if (!SessionManager.IsUserLoaded && _isUserTestMode)
        {
            await LoginCoreAsync(CreateDebugLoginRequest(), true);
        }
    }

    private Task Login() => LoginCoreAsync(CloneLoginRequest());

    private async Task LoginCoreAsync(LoginRequest request, bool isTestMode = false)
    {
        if (!HasRequiredCredentials(request))
        {
            ErrorMessage = "Please enter both username and password";
            return;
        }

        _isLoggingIn = true;
        ErrorMessage = string.Empty;

        try
        {
            if (isTestMode)
            {
                CompleteDebugLogin(request.Username);
                return;
            }

            await UserLoginService.LoginAsync(request);
            _loginRequest.Password = string.Empty;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            _isLoggingIn = false;
        }
    }

    private Task OidcLogin()
    {
        Navigation.NavigateTo(UserLoginService.GetKeycloakLoginUrl(), forceLoad: true);
        return Task.CompletedTask;
    }

    private string GetUserEmailDisplay() =>
        string.IsNullOrWhiteSpace(SessionManager.UserEmail)
            ? "Email unavailable"
            : SessionManager.UserEmail;

    private void Logout()
    {
        UserLoginService.ClearSession();
        ResetLoginForm();
        Navigation.NavigateTo(LoginPageRoute);
    }

    private LoginRequest CloneLoginRequest() => new()
    {
        Username = _loginRequest.Username,
        Password = _loginRequest.Password
    };

    private static LoginRequest CreateDebugLoginRequest() => new()
    {
        Username = DebugUsername,
        Password = DebugPassword
    };

    private static bool HasRequiredCredentials(LoginRequest request) =>
        !string.IsNullOrWhiteSpace(request.Username)
        && !string.IsNullOrWhiteSpace(request.Password);

    private void CompleteDebugLogin(string username)
    {
        SessionManager.UserId = Guid.CreateVersion7().ToString();
        SessionManager.UserEmail = username;
    }

    private void ResetLoginForm()
    {
        _loginRequest.Username = string.Empty;
        _loginRequest.Password = string.Empty;
        ErrorMessage = string.Empty;
    }
}
