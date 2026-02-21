using Prompter.Core.Enums;
using Prompter.Core.Repositories;
using Prompter.Core.Services;

namespace Prompter.Worker;

public class PromptProcessingService : BackgroundService
{
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
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IPromptRepository>();
            var llmClient = scope.ServiceProvider.GetRequiredService<ILlmClient>();

            var pendingPrompts = await repository.GetByStatusAsync(PromptStatus.Pending);
            var promptList = pendingPrompts.ToList();

            if (promptList.Count == 0)
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                continue;
            }

            _logger.LogInformation("Found {Count} pending prompts", promptList.Count);

            foreach (var prompt in promptList)
            {
                if (stoppingToken.IsCancellationRequested) break;

                prompt.Status = PromptStatus.Processing;
                await repository.UpdateAsync(prompt);

                try
                {
                    _logger.LogInformation("Processing prompt {Id}: {Text}", prompt.Id, prompt.Text);

                    var response = await llmClient.GenerateAsync(prompt.Text, stoppingToken);

                    prompt.Response = response;
                    prompt.Status = PromptStatus.Completed;
                    prompt.CompletedAt = DateTime.UtcNow;

                    _logger.LogInformation("Prompt {Id} completed", prompt.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process prompt {Id}", prompt.Id);

                    prompt.Status = PromptStatus.Failed;
                    prompt.CompletedAt = DateTime.UtcNow;
                }

                await repository.UpdateAsync(prompt);
            }
        }
    }
}
