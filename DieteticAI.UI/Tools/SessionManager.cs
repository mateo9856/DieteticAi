namespace DieteticAI.UI.Tools;

public class SessionManager
{
    public string? UserId { get; set; }

    public string? AccessToken { get; set; }

    public DateTime? TokenExpiresAt { get; set; }

    public bool? IsActiveRabbitConnection { get; set; }

    public bool IsUserLoaded => !string.IsNullOrEmpty(UserId);

    public void Clear()
    {
        UserId = null;
        AccessToken = null;
        TokenExpiresAt = null;
        IsActiveRabbitConnection = null;
    }
}