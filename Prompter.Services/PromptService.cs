using Ardalis.GuardClauses;
using MassTransit;
using Prompter.Core.Entities;
using Prompter.Core.Messages;
using Prompter.Core.Models;
using Prompter.Core.Services;
using Prompter.Core.UnitOfWork;

namespace Prompter.Services;

public class PromptService(IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint) : IPromptService
{
    private readonly IUnitOfWork _unitOfWork = Guard.Against.Null(unitOfWork);
    private readonly IPublishEndpoint _publishEndpoint = Guard.Against.Null(publishEndpoint);

    public async Task<IEnumerable<Prompt>> CreatePromptsAsync(string[] promptTexts, CancellationToken cancellationToken = default)
    {
        Guard.Against.Null(promptTexts);
        Guard.Against.InvalidInput(promptTexts, nameof(promptTexts), arr => arr.Length > 0, "At least one prompt is required.");
        Guard.Against.InvalidInput(promptTexts, nameof(promptTexts), arr => arr.All(t => !string.IsNullOrWhiteSpace(t)), "Prompts cannot be empty or whitespace.");

        var prompts = promptTexts.Select(text => new Prompt(text)).ToList();

        await _unitOfWork.BeginTransactionAsync(cancellationToken);

        await _unitOfWork.Prompts.AddRangeAsync(prompts, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        foreach (var prompt in prompts)
            await _publishEndpoint.Publish(new ProcessPrompt(prompt.Id), cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _unitOfWork.CommitTransactionAsync(cancellationToken);

        return prompts;
    }

    public async Task<PagedResult<Prompt>> GetPromptsPagedAsync(int skip, int take, CancellationToken cancellationToken = default)
    {
        return await _unitOfWork.Prompts.GetPagedAsync(skip, take, cancellationToken);
    }
}
