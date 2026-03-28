using DietAI.AiKernel.Models.DTOs;

namespace DieteticAi.Models;

public class PlanTopicRequest<T> where T : HumanDataDto
{
    public T Request { get; set; } = default!;
    public string UserId { get; set; } = default!;
    public string RequestId { get; set; } = default!;
    public DateTime Timestamp { get; set; }
}