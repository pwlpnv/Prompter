using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Prompter.Core.Services;
using Prompter.Core.UnitOfWork;
using Prompter.Data;
using Prompter.Data.UnitOfWork;
using Prompter.Infrastructure.Llm;
using Prompter.Services;

namespace Prompter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureSharedServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PrompterDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork, UnitOfWork>();

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
        
        // Let's use scoped, since there are no info about thread-safety of this class
        // also t seems we can't use HttpClientFactory with this client
        services.AddScoped<ILlmClient, OllamaLlmClient>();
        services.AddScoped<IPromptProcessor, PromptProcessor>();
        services.AddScoped<IPromptBatchOrchestrator, PromptBatchOrchestrator>();

        return services;
    }
}
