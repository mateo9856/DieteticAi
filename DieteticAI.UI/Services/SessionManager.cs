namespace DieteticAI.UI.Services;

public class SessionManager
{
    private string? _userId;

    public string? UserId
    {
        get => _userId;
        set => _userId = value;
    }

    public bool IsUserLoaded => !string.IsNullOrEmpty(_userId);

    public void SetUserId(string userId)
    {
        _userId = userId;
    }

    public void ClearUserId()
    {
        _userId = null;
    }
}

