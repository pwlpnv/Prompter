using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prompter.Core.Repositories;
using Prompter.Core.Services;
using Prompter.Data;
using Prompter.Data.Repositories;
using Prompter.Infrastructure.Llm;
using Prompter.Services;

namespace Prompter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PrompterDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IPromptRepository, PromptRepository>();

        return services;
    }

    public static IServiceCollection ConfigureWebServices(this IServiceCollection services)
    {
        services.AddScoped<IPromptService, PromptService>();

        return services;
    }

    public static IServiceCollection ConfigureWorkerServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.AddSingleton<ILlmClient, OllamaLlmClient>();

        return services;
    }
}
