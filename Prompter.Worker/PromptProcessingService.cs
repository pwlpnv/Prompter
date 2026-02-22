using Prompter.Core.Enums;
using Prompter.Core.Repositories;
using Prompter.Core.Services;

namespace Prompter.Worker;

public class PromptProcessingService : BackgroundService
{
    private const int BatchSize = 10;
    private static readonly TimeSpan IdleDelay = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan BatchCooldown = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan ErrorRetryDelay = TimeSpan.FromSeconds(10);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PromptProcessingService> _logger;

    public PromptProcessingService(IServiceScopeFactory scopeFactory, ILogger<PromptProcessingService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PromptProcessingService started");

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
                _logger.LogError(ex, "Unexpected error in processing loop, retrying in {Delay}s", ErrorRetryDelay.TotalSeconds);
                await Task.Delay(ErrorRetryDelay, stoppingToken);
            }
        }
    }

    private async Task ProcessPendingPromptsAsync(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPromptRepository>();
        var llmClient = scope.ServiceProvider.GetRequiredService<ILlmClient>();

        var pendingPrompts = await repository.GetByStatusAsync(PromptStatus.Pending, take: BatchSize);

        if (pendingPrompts.Count == 0)
        {
            await Task.Delay(IdleDelay, stoppingToken);
            return;
        }

        _logger.LogInformation("Found {Count} pending prompts", pendingPrompts.Count);

        foreach (var prompt in pendingPrompts)
        {
            if (stoppingToken.IsCancellationRequested) break;

            prompt.Process();
            await repository.UpdateAsync(prompt);

            try
            {
                _logger.LogInformation("Processing prompt {Id}: {Text}", prompt.Id, prompt.Text);

                var response = await llmClient.GenerateAsync(prompt.Text, stoppingToken);

                prompt.Complete(response);

                _logger.LogInformation("Prompt {Id} completed", prompt.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process prompt {Id}", prompt.Id);

                prompt.Fail();
            }

            await repository.UpdateAsync(prompt);
        }

        await Task.Delay(BatchCooldown, stoppingToken);
    }
}
