using MassTransit;
using Microsoft.Extensions.Logging;
using Prompter.Core.Enums;
using Prompter.Core.Messages;
using Prompter.Core.UnitOfWork;

namespace Prompter.Worker.Consumers;

public class ProcessPromptFaultConsumer(
    IUnitOfWork unitOfWork,
    ILogger<ProcessPromptFaultConsumer> logger) : IConsumer<Fault<ProcessPrompt>>
{
    public async Task Consume(ConsumeContext<Fault<ProcessPrompt>> context)
    {
        var promptId = context.Message.Message.PromptId;
        logger.LogError("All retries exhausted for prompt {Id}, marking as Failed", promptId);

        var prompt = await unitOfWork.Prompts.GetByIdAsync(promptId, context.CancellationToken);

        if (prompt is null)
            return;

        if (prompt.Status == PromptStatus.Processing)
        {
            prompt.Fail();
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
    }
}
