namespace DieteticAI.UI.Services.Login.Abstractions;

public interface ILoginCallbackService
{
    bool TryCompleteLogin(Uri currentUri);
}
