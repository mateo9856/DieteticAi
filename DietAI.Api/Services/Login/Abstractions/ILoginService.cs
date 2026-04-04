using DietAI.Api.Services.Login.Models;
using DietAI.Api.Services.Login.Requests;

namespace DietAI.Api.Services.Login.Abstractions;

public interface ILoginService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
