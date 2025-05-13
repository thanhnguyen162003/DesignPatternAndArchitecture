using System.Diagnostics.Metrics;
using AspNetCore.Swagger.Themes;
using CleanPatternWithCloudNative.Application;
using CleanPatternWithCloudNative.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();
builder.Services.AddDaprClient();
builder.Services.AddControllers().AddDapr();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder
    .Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration);

using var meter = new Meter("CleanPatternWithCloudNative.Counter");
var counter = meter.CreateCounter<int>("cron_counter");
builder.Services.AddSingleton(meter);
builder.Services.AddSingleton(counter);

WebApplication app = builder.Build();

app.MapDefaultEndpoints();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(ModernStyle.Futuristic);
}

app.UseAuthorization();
app.UseCloudEvents();
app.MapControllers();
app.MapSubscribeHandler();
await app.RunAsync();

public partial class Program
{
    protected Program()
    {
    }
}