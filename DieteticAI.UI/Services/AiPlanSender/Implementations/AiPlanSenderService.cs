using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using DieteticAI.UI.Services.AiPlanSender.Abstractions;
using DieteticAI.UI.Services.AiPlanSender.Models;
using DieteticAI.UI.Services.AiPlanSender.Requests;
using DieteticAI.UI.Tools;

namespace DieteticAI.UI.Services.AiPlanSender.Implementations;

public class AiPlanSenderService : IAiPlanSender
{
    private readonly HttpClient _httpClient;
    private readonly SessionManager _sessionManager;

    public AiPlanSenderService(HttpClient httpClient, SessionManager sessionManager)
    {
        _httpClient = httpClient;
        _sessionManager = sessionManager;
    }

    public Task<Diets> SendPlanRequestAsync(
        SendPlanRequest request,
        CancellationToken cancellationToken = default) =>
        SendAsync("api/plan/send", request, cancellationToken);

    public Task<Diets> SendPlanUpdateAsync(
        SendUpdatePlanRequest update,
        CancellationToken cancellationToken = default) =>
        SendAsync("api/plan/update", update, cancellationToken);

    private async Task<Diets> SendAsync<TRequest>(
        string uri,
        TRequest request,
        CancellationToken cancellationToken)
        where TRequest : SendPlanRequest
    {
        if (!_sessionManager.IsUserLoaded)
        {
            throw new InvalidOperationException("User session must be loaded before requesting a diet plan");
        }

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Add("X-User-Id", _sessionManager.UserId);

        if (!string.IsNullOrWhiteSpace(_sessionManager.AccessToken))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sessionManager.AccessToken);
        }

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            throw new InvalidOperationException("User session must be loaded before requesting a diet plan");
        }

        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Plan request failed: {details}");
        }

        var diet = await response.Content.ReadFromJsonAsync<Diets>(cancellationToken: cancellationToken);
        return diet ?? throw new InvalidOperationException("Failed to deserialize diet response");
    }
}
