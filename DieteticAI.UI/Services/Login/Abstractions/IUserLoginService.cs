using DieteticAI.UI.Services.Login.Models;
using DieteticAI.UI.Services.Login.Requests;

namespace DieteticAI.UI.Services.Login.Abstractions;

public interface IUserLoginService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    void ClearSession();
}
