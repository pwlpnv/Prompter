using System.ClientModel;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using Prompter.Core.Services;

namespace Prompter.Infrastructure.Llm;

public class OllamaLlmClient : ILlmClient
{
    private readonly ChatClient _chatClient;
    private readonly int _maxTokens;

    public OllamaLlmClient(IConfiguration configuration)
    {
        var baseUrl = configuration["Ollama:BaseUrl"] ?? "http://localhost:11434";
        var model = configuration["Ollama:Model"] ?? "phi3";
        _maxTokens = int.TryParse(configuration["Ollama:MaxTokens"], out var mt) ? mt : 40;

        var options = new OpenAI.OpenAIClientOptions
        {
            Endpoint = new Uri(baseUrl + "/v1")
        };

        _chatClient = new ChatClient(model, new ApiKeyCredential("ollama"), options);
    }

    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var chatOptions = new ChatCompletionOptions
        {
            MaxOutputTokenCount = _maxTokens
        };

        var completion = await _chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            chatOptions,
            cancellationToken);

        return completion.Value.Content[0].Text;
    }
}
