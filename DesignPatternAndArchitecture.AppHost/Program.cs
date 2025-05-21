using CommunityToolkit.Aspire.Hosting.Dapr;
using NapalmCodes.Aspire.Hosting.Krakend;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

var rabbitmq = builder.AddRabbitMQ("messaging").WithManagementPlugin();

var krakend = builder.AddKrakend("gateway", "../krakend.json", port: 8080)
    .WithExternalHttpEndpoints();

var kafka = builder.AddKafka("kafka")
                   .WithKafkaUI(kafkaUI => kafkaUI.WithHostPort(9100));

var statestore = builder.AddDaprStateStore(
    "statestore",
    new DaprComponentOptions
    {
        LocalPath = "../../components/statestore.yaml"
    }).WaitFor(redis);

IResourceBuilder<IDaprComponentResource> pubsub = builder.AddDaprPubSub(
    "pubsub",
    new DaprComponentOptions
    {
        LocalPath = "../../components/pubsub.yaml"
    });

IResourceBuilder<IDaprComponentResource> secretstore = builder.AddDaprComponent(
    "secretstore",
    "secretstores.local.file",
    new DaprComponentOptions
    {
        LocalPath = "../../components/secretstore.yaml"
    });

IResourceBuilder<IDaprComponentResource> configstore = builder.AddDaprComponent(
    "configstore",
    "configuration.redis",
    new DaprComponentOptions
    {
        LocalPath = "../../components/configstore.yaml"
    }).WaitFor(redis);

IResourceBuilder<IDaprComponentResource> cron = builder.AddDaprComponent(
    "scheduler",
    "bindings.cron",
    new DaprComponentOptions
    {
        LocalPath = "../../components/cron.yaml"
    });

var server = builder
    .AddPostgres("postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin(pgAdmin => pgAdmin.WithHostPort(5050));

//the add database in aspire just add aa connection string to it, not real physical db. so need to init db in each project
var db = server
    .AddDatabase("cleanpatternwithcloudnativedb");

var dbVerticalSlice = server
    .AddDatabase("verticalslicedb");

var migration = builder
    .AddProject<Projects.CleanPatternWithCloudNative_MigrationService>("migrations")
    .WithReference(db).WaitFor(db);

var api = builder
    .AddProject<Projects.CleanPatternWithCloudNative_Api>("cleanpatternwithcloudnative-api")
    .WithReference(db).WaitFor(db)
    .WithReference(redis).WaitFor(redis)
    .WithReference(statestore)
    .WithReference(pubsub)
    .WithReference(secretstore)
    .WithReference(configstore)
    .WithReference(cron)
    .WithDaprSidecar();

migration.WithParentRelationship(api);

builder.AddProject<Projects.KafkaProducer>("kafkaproducer")
    .WithReference(kafka).WaitFor(kafka);

builder.AddProject<Projects.KafkaConsumer>("kafkaconsumer")
    .WithReference(kafka).WaitFor(kafka);

builder.AddProject<Projects.VerticalSliceApi>("verticalsliceapi")
    .WithReference(dbVerticalSlice).WaitFor(dbVerticalSlice);

builder.AddProject<Projects.RabbitMqPublisher>("rabbitmqpublisher")
    .WithReference(rabbitmq).WaitFor(rabbitmq);

builder.AddProject<Projects.RabbitMqSubsciber>("rabbitmqsubsciber")
        .WithReference(rabbitmq).WaitFor(rabbitmq);

await builder.Build().RunAsync().ConfigureAwait(false);