using System.ComponentModel.Design;
using Microsoft.Extensions.DependencyInjection;

namespace Prompter.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureIoC(this IServiceCollection services)
    {
        return services;
    }
}