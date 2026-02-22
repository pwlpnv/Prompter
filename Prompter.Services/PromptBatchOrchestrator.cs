using Microsoft.Extensions.Logging;
using Prompter.Core.Entities;
using Prompter.Core.Services;
using Prompter.Core.UnitOfWork;

namespace Prompter.Services;

public class PromptBatchOrchestrator(
    IUnitOfWork unitOfWork,
    IPromptProcessor promptProcessor,
    ILogger<PromptBatchOrchestrator> logger) : IPromptBatchOrchestrator
{
    private const int BatchSize = 10;
    private static readonly TimeSpan StaleTimeout = TimeSpan.FromMinutes(5);

    /// <returns>true if prompts were processed, false if idle.</returns>
    public async Task<bool> RunBatchAsync(CancellationToken cancellationToken)
    {
        var claimedPrompts = await ClaimPromptsAsync(cancellationToken);

        if (claimedPrompts.Count == 0)
            return false;

        logger.LogInformation("Claimed {Count} prompts for processing", claimedPrompts.Count);

        foreach (var prompt in claimedPrompts)
        {
            if (cancellationToken.IsCancellationRequested) break;
            await promptProcessor.ProcessAsync(prompt, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<IReadOnlyList<Prompt>> ClaimPromptsAsync(CancellationToken cancellationToken)
    {
        await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var prompts = await unitOfWork.Prompts
                .ClaimPendingAsync(BatchSize, StaleTimeout, cancellationToken);

            if (prompts.Count == 0)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                return [];
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
}
