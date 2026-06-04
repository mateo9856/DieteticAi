using DieteticAI.UI.Services.AiPlanSender.Requests;

namespace DieteticAI.UI.Services.DietPlan.Abstractions;

public interface IDietPlanRequestBuilder
{
    SendPlanRequest Build(SendPlanRequest formRequest, string allergiesText, string excludedIngredientsText);
}
