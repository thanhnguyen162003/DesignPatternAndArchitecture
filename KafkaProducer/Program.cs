using KafkaProducer.Common;
using KafkaProducer.Common.Models;
using KafkaProducer.Intefaces;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<IKafkaProducerDefault, KafkaProducerDefault>();

builder.Services.AddOpenApi();

var app = builder.Build();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();


app.MapPost("/data", async (IKafkaProducerDefault producer) =>
{
    DataModelProduce data = new DataModelProduce()
    {
        Id = Guid.NewGuid(),
        Name = "Test",
        Payload = "Test"
    };
    await producer.ProduceObjectWithKeyAsync("test-topic", "key", data);
    return new ResponseModel(System.Net.HttpStatusCode.OK, "Produced to Kafka");
}).WithName("data");

app.MapGet("/data", () =>
{
    return new ResponseModel(System.Net.HttpStatusCode.OK, "OK"); ;
}).WithName("data");
app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
