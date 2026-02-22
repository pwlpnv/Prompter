using Microsoft.Extensions.Logging;
using Prompter.Core.Entities;
using Prompter.Core.Services;

namespace Prompter.Infrastructure.Llm;

public class PromptProcessor(ILlmClient llmClient, ILogger<PromptProcessor> logger) : IPromptProcessor
{
    public async Task ProcessAsync(Prompt prompt, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Processing prompt {Id}: {Text}", prompt.Id, prompt.Text);

            var response = await llmClient.GenerateAsync(prompt.Text, cancellationToken);
            prompt.Complete(response);

            logger.LogInformation("Prompt {Id} completed", prompt.Id);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Failed to process prompt {Id}", prompt.Id);
            prompt.Fail();
        }
    }
}
