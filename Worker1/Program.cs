using Worker1.Jobs;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton<JobProcessor>();

builder.Services.AddHostedService<JobWorker>();

var host = builder.Build();

host.Run();
