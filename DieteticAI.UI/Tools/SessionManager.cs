namespace DieteticAI.UI.Tools;

public class SessionManager
{
    public string? UserId { get; set; }

    public bool? IsActiveRabbitConnection { get; set; }
    
    public bool IsUserLoaded => !string.IsNullOrEmpty(UserId);
}