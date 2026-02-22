using Prompter.Core.Entities;

namespace Prompter.Core.Services;

public interface IPromptProcessor
{
    Task ProcessAsync(Prompt prompt, CancellationToken cancellationToken = default);
}
