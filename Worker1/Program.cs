using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Worker1.Jobs;

var builder = Host.CreateApplicationBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Connection");

builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddScoped<JobClaimer>();
builder.Services.AddScoped<JobProcessor>();

builder.Services.AddHostedService<JobWorker>();

var host = builder.Build();

host.Run();