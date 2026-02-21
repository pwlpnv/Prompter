namespace Prompter.Core.Services;

public interface ILlmClient
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default);
}
