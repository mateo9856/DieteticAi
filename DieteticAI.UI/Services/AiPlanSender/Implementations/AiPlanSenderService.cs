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
        SendAsync("v1/plan/send", request, cancellationToken);

    public Task<Diets> SendPlanUpdateAsync(
        SendUpdatePlanRequest update,
        CancellationToken cancellationToken = default) =>
        SendAsync("v1/plan/update", update, cancellationToken);

    public async Task<IReadOnlyList<Diets>> GetPlanHistoryAsync(CancellationToken cancellationToken = default)
    {
        using var response = await SendAuthorizedAsync(HttpMethod.Get, "v1/plan/history", null, cancellationToken);
        var diets = await response.Content.ReadFromJsonAsync<List<Diets>>(cancellationToken: cancellationToken);
        return diets ?? [];
    }

    public async Task<Diets> GetPlanHistoryDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        using var response = await SendAuthorizedAsync(HttpMethod.Get, $"v1/plan/history/{id}", null, cancellationToken);
        var diet = await response.Content.ReadFromJsonAsync<Diets>(cancellationToken: cancellationToken);
        return diet ?? throw new InvalidOperationException("Failed to deserialize diet history response");
    }

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

        using var response = await SendAuthorizedAsync(HttpMethod.Post, uri, JsonContent.Create(request), cancellationToken);

        var diet = await response.Content.ReadFromJsonAsync<Diets>(cancellationToken: cancellationToken);
        return diet ?? throw new InvalidOperationException("Failed to deserialize diet response");
    }

    private async Task<HttpResponseMessage> SendAuthorizedAsync(
        HttpMethod method,
        string uri,
        HttpContent? content,
        CancellationToken cancellationToken)
    {
        if (!_sessionManager.IsUserLoaded)
        {
            throw new InvalidOperationException("User session must be loaded before requesting a diet plan");
        }

        using var httpRequest = new HttpRequestMessage(method, uri)
        {
            Content = content
        };

        if (!string.IsNullOrWhiteSpace(_sessionManager.AccessToken))
        {
            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _sessionManager.AccessToken);
        }

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            response.Dispose();
            throw new InvalidOperationException("User session must be loaded before requesting a diet plan");
        }

        if (!response.IsSuccessStatusCode)
        {
            var details = await response.Content.ReadAsStringAsync(cancellationToken);
            response.Dispose();
            throw new InvalidOperationException($"Plan request failed: {details}");
        }

        return response;
    }
}
