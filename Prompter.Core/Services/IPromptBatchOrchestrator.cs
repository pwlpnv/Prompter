namespace Prompter.Core.Services;

public interface IPromptBatchOrchestrator
{
    /// <returns>true if prompts were processed, false if idle.</returns>
    Task<bool> RunBatchAsync(CancellationToken cancellationToken);
}
