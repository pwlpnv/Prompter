namespace Prompter.Infrastructure.Llm;

public class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; set; } = "http://localhost:11434";
    public string Model { get; set; } = "phi3";

    //TODO: remove or raise the limit
    public int MaxTokens { get; set; } = 40;
}
