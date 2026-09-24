using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Worker1.Jobs;
using Worker1.Recovery;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Connection");

builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<JobClaimer>();
builder.Services.AddScoped<JobProcessor>();

builder.Services.AddHostedService<JobWorker>();
builder.Services.AddHostedService<JobRecoveryWorker>();

var host = builder.Build();

host.Run();