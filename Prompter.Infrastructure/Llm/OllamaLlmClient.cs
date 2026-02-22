using System.ClientModel;
using Microsoft.Extensions.Options;
using OpenAI.Chat;
using Prompter.Core.Services;

namespace Prompter.Infrastructure.Llm;

public class OllamaLlmClient : ILlmClient
{
    private readonly ChatClient _chatClient;
    private readonly OllamaOptions _options;

    public OllamaLlmClient(IOptions<OllamaOptions> options)
    {
        _options = options.Value;

        var clientOptions = new OpenAI.OpenAIClientOptions
        {
            Endpoint = new Uri(_options.BaseUrl + "/v1")
        };

        _chatClient = new ChatClient(_options.Model, new ApiKeyCredential("ollama"), clientOptions);
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var chatOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _options.MaxTokens
        };

        var completion = await _chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            chatOptions,
            cancellationToken);

        var content = completion.Value.Content;
        if (content is null || content.Count == 0 || string.IsNullOrEmpty(content[0].Text))
            throw new InvalidOperationException("LLM returned empty or null response.");

        return content[0].Text;
    }
}
