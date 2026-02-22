using Prompter.Core.Entities;
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
    private static readonly TimeSpan StaleTimeout = TimeSpan.FromMinutes(5);

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
        var promptProcessor = scope.ServiceProvider.GetRequiredService<IPromptProcessor>();

        var claimedPrompts = await ClaimPromptsAsync(unitOfWork, stoppingToken);

        if (claimedPrompts.Count == 0)
        {
            await Task.Delay(IdleDelay, stoppingToken);
            return;
        }

        logger.LogInformation("Claimed {Count} prompts for processing", claimedPrompts.Count);

        foreach (var prompt in claimedPrompts)
        {
            if (stoppingToken.IsCancellationRequested) break;
            await promptProcessor.ProcessAsync(prompt, stoppingToken);
        }

        await SaveResultsAsync(unitOfWork, stoppingToken);
        await Task.Delay(BatchCooldown, stoppingToken);
    }

    private async Task<IReadOnlyList<Prompt>> ClaimPromptsAsync(
        IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var prompts = await unitOfWork.Prompts
                .ClaimPendingAsync(BatchSize, StaleTimeout, cancellationToken);

            if (prompts.Count == 0)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return prompts;
            }

            foreach (var prompt in prompts)
                prompt.Process();

            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);

            return prompts;
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    private static async Task SaveResultsAsync(IUnitOfWork unitOfWork, CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.CommitTransactionAsync(cancellationToken);
        }
        catch
        {
            await unitOfWork.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }
}
