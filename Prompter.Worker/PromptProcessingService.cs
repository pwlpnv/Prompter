using Prompter.Core.Services;
using Prompter.Core.UnitOfWork;

namespace Prompter.Worker;

public class PromptProcessingService(IServiceScopeFactory scopeFactory, ILogger<PromptProcessingService> logger)
    : BackgroundService
{
    private const int BatchSize = 10;
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
                await ProcessPendingPromptsAsync(stoppingToken);
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

    private async Task ProcessPendingPromptsAsync(CancellationToken stoppingToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var llmClient = scope.ServiceProvider.GetRequiredService<ILlmClient>();

        // Transaction wraps the entire claim + process cycle.
        // If the worker crashes, the transaction rolls back and prompts return to Pending.
        await unitOfWork.BeginTransactionAsync(stoppingToken);

        try
        {
            var claimedPrompts = await unitOfWork.Prompts.ClaimPendingAsync(BatchSize, stoppingToken);

            if (claimedPrompts.Count == 0)
            {
                await unitOfWork.RollbackTransactionAsync(stoppingToken);
                await Task.Delay(IdleDelay, stoppingToken);
                return;
            }

            logger.LogInformation("Claimed {Count} prompts for processing", claimedPrompts.Count);

            foreach (var prompt in claimedPrompts)
            {
                if (stoppingToken.IsCancellationRequested) break;

                prompt.Process();

                try
                {
                    logger.LogInformation("Processing prompt {Id}: {Text}", prompt.Id, prompt.Text);

                    var response = await llmClient.GenerateAsync(prompt.Text, stoppingToken);

                    prompt.Complete(response);

                    logger.LogInformation("Prompt {Id} completed", prompt.Id);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    logger.LogError(ex, "Failed to process prompt {Id}", prompt.Id);
                    prompt.Fail();
                }
            }

            await unitOfWork.SaveChangesAsync(stoppingToken);
            await unitOfWork.CommitTransactionAsync(stoppingToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }

        await Task.Delay(BatchCooldown, stoppingToken);
    }
}
