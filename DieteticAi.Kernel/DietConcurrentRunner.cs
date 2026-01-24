using DietAI.RabbitServer.Abstractions;
using Microsoft.Extensions.Logging;

namespace DietAI.AiKernel;

public class DietConcurrentRunner(
    ILogger<DietConcurrentRunner> logger,
    IReceiveService receiveService,
    ISenderService senderService)
{
    private readonly IReceiveService _receiveService = receiveService;
    private readonly ISenderService _senderService = senderService;

    public Task Run(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting DietConcurrentRunner");
        //TODO: read consuming data from RabbitMq and logic to write prompt
        return Task.CompletedTask;
    }
}