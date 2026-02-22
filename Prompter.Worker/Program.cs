using Prompter.Core.Services;
using Prompter.Infrastructure;
using Prompter.Services;
using Prompter.Worker;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.ConfigureSharedServices(builder.Configuration);
builder.Services.ConfigureWorkerServices(builder.Configuration);
builder.Services.AddHostedService<PromptProcessingService>();

var host = builder.Build();
host.Run();
