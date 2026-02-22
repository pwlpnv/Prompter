using MassTransit;
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

    public static IServiceCollection ConfigureWebServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IPromptService, PromptService>();

        services.AddMassTransit(x =>
        {
            x.AddEntityFrameworkOutbox<PrompterDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost");
            });
        });

        return services;
    }

    public static IServiceCollection ConfigureWorkerServices(this IServiceCollection services, IConfiguration configuration,
        Action<IBusRegistrationConfigurator> configureBus)
    {
        services.Configure<OllamaOptions>(configuration.GetSection(OllamaOptions.SectionName));
        services.AddScoped<ILlmClient, OllamaLlmClient>();

        services.AddMassTransit(x =>
        {
            configureBus(x);

            x.AddEntityFrameworkOutbox<PrompterDbContext>(o =>
            {
                o.UsePostgres();
                o.UseBusOutbox();
            });

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration.GetValue<string>("RabbitMQ:Host") ?? "localhost");
                cfg.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}
