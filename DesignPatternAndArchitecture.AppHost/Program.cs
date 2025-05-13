using Aspire.Hosting;
using CommunityToolkit.Aspire.Hosting.Dapr;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

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
    .AddPostgres("cleanpatternwithcloudnative-db")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithPgAdmin();

var db = server
    .AddDatabase("cleanpatternwithcloudnativedb");

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

await builder.Build().RunAsync().ConfigureAwait(false);