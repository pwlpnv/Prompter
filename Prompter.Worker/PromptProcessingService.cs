using Prompter.Core.Services;

namespace Prompter.Worker;

public class PromptProcessingService(IServiceScopeFactory scopeFactory, ILogger<PromptProcessingService> logger)
    : BackgroundService
{
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan BatchCooldown = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ErrorRetryDelay = TimeSpan.FromSeconds(10);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("PromptProcessingService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var orchestrator = scope.ServiceProvider.GetRequiredService<IPromptBatchOrchestrator>();

                var hadWork = await orchestrator.RunBatchAsync(stoppingToken);
                await Task.Delay(hadWork ? BatchCooldown : IdleDelay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error in processing loop, retrying in {Delay}s", ErrorRetryDelay.TotalSeconds);
                await Task.Delay(ErrorRetryDelay, stoppingToken);
            }
        }
    }
}
