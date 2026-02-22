using MassTransit;
using Prompter.Infrastructure;
using Prompter.Worker.Consumers;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.ConfigureSharedServices(builder.Configuration);
builder.Services.ConfigureWorkerServices(builder.Configuration, bus =>
{
    bus.AddConsumer<ProcessPromptFaultConsumer>();
    bus.AddConsumer<ProcessPromptConsumer>(cfg =>
    {
        cfg.UseDelayedRedelivery(r => r.Intervals(
            TimeSpan.FromSeconds(10),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(5)));
        cfg.UseMessageRetry(r => r.Intervals(
            TimeSpan.FromSeconds(1),
            TimeSpan.FromSeconds(5),
            TimeSpan.FromSeconds(15)));
    });
});

var host = builder.Build();
host.Run();
