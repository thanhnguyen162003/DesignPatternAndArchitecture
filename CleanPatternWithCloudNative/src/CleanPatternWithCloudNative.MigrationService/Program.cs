using CleanPatternWithCloudNative.Infrastructure;
using CleanPatternWithCloudNative.MigrationService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<Worker>();

builder.AddServiceDefaults();

builder
    .AddNpgsqlDbContext<ApplicationDbContext>("cleanpatternwithcloudnativedb");

var host = builder.Build();

await host.RunAsync();