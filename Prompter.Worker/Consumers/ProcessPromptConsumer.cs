using MassTransit;
using Microsoft.Extensions.Logging;
using Prompter.Core.Messages;
using Prompter.Core.Services;
using Prompter.Core.UnitOfWork;

namespace Prompter.Worker.Consumers;

public class ProcessPromptConsumer(
    IUnitOfWork unitOfWork,
    ILlmClient llmClient,
    ILogger<ProcessPromptConsumer> logger) : IConsumer<ProcessPrompt>
{
    public async Task Consume(ConsumeContext<ProcessPrompt> context)
    {
        var promptId = context.Message.PromptId;
        logger.LogInformation("Received message to process prompt {Id}", promptId);

        var prompt = await unitOfWork.Prompts.GetByIdAsync(promptId, context.CancellationToken);

        if (prompt is null)
            throw new InvalidOperationException($"Prompt {promptId} not found");

        prompt.Process();
        
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        var response = await llmClient.GenerateAsync(prompt.Text, context.CancellationToken);
        prompt.Complete(response);

        await unitOfWork.SaveChangesAsync(context.CancellationToken);
        logger.LogInformation("Prompt {Id} completed", promptId);
    }
}
