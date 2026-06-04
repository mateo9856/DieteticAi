using DieteticAI.UI.Services.AiPlanSender.Abstractions;
using DieteticAI.UI.Services.AiPlanSender.Models;
using DieteticAI.UI.Services.AiPlanSender.Requests;
using DieteticAI.UI.Services.DietPlan.Abstractions;
using DieteticAI.UI.Tools;
using Microsoft.AspNetCore.Components;

namespace DieteticAI.UI.Pages;

public partial class DietPlan
{
    private const string UnexpectedHistoryErrorMessage =
        "Unexpected error was occurred, please contact with administrator";

    private SendPlanRequest planRequest = new();
    private Diets? generatedDiet;
    private List<Diets> planHistory = [];
    private string allergiesText = string.Empty;
    private string excludedIngredientsText = string.Empty;
    private string? errorMessage;
    private bool isGenerating;
    private bool showResults;
    private bool isLoadingHistory;
    private bool IsLoggedIn => SessionManager.IsUserLoaded;

    [Inject]
    private IAiPlanSender AiPlanSenderService { get; set; } = default!;

    [Inject]
    private IDietPlanRequestBuilder DietPlanRequestBuilder { get; set; } = default!;

    [Inject]
    private SessionManager SessionManager { get; set; } = default!;

    protected override async Task OnInitializedAsync()
    {
        if (IsLoggedIn)
        {
            await LoadHistory();
        }
    }

    private async Task GenerateDietPlan()
    {
        isGenerating = true;
        showResults = false;
        errorMessage = null;

        try
        {
            var request = DietPlanRequestBuilder.Build(planRequest, allergiesText, excludedIngredientsText);
            var generatedPlan = await AiPlanSenderService.SendPlanRequestAsync(request);

            if (generatedPlan is not null)
            {
                generatedDiet = generatedPlan;
            }

            showResults = true;
            await ReloadHistoryAfterGeneration();
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
        finally
        {
            isGenerating = false;
        }
    }

    private async Task LoadHistory()
    {
        if (!IsLoggedIn)
        {
            planHistory.Clear();
            return;
        }

        isLoadingHistory = true;
        errorMessage = null;

        try
        {
            planHistory = (await AiPlanSenderService.GetPlanHistoryAsync()).ToList();
        }
        catch (Exception)
        {
            errorMessage = UnexpectedHistoryErrorMessage;
        }
        finally
        {
            isLoadingHistory = false;
        }
    }

    private async Task SelectHistoryPlan(int id)
    {
        errorMessage = null;

        try
        {
            generatedDiet = await AiPlanSenderService.GetPlanHistoryDetailAsync(id);
            showResults = true;
        }
        catch (Exception ex)
        {
            errorMessage = ex.Message;
        }
    }

    private async Task ReloadHistoryAfterGeneration()
    {
        if (IsLoggedIn)
        {
            await LoadHistory();
        }
    }

    private static string FormatCreatedAt(DateTime createdAtUtc)
    {
        return createdAtUtc == default
            ? string.Empty
            : createdAtUtc.ToLocalTime().ToString("g");
    }
}
