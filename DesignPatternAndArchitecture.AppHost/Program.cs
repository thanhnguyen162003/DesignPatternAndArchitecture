using Aspire.Hosting;
using CommunityToolkit.Aspire.Hosting.Dapr;
using NapalmCodes.Aspire.Hosting.Krakend;

IDistributedApplicationBuilder builder = DistributedApplication.CreateBuilder(args);

var redis = builder.AddRedis("redis");

// hide resources new in 9.3.0
var apiKey = builder.AddParameter("api-key", secret: true);

var rabbitmq = builder.AddRabbitMQ("messaging").WithManagementPlugin();

var krakend = builder.AddKrakend("kradend-gateway", "../krakend.json", port: 8080)
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



// new in 9.3.0
//var yarp = builder.AddYarp("yarp-gateway")
//    .WithConfigFile("yarp.json")
//    .WithExternalHttpEndpoints();

/* .NET Aspire 9.3 has preview support for the following environment resources:

    AddDockerComposeEnvironment(...)
    AddKubernetesEnvironment(...)
    AddAzureContainerAppEnvironment(...)
    AddAzureAppServiceEnvironment(...) — see new App Service support →

These represent deployment targets that can transform and emit infrastructure-specific artifacts from your app model. */

/*---------- new in 9.3.0 environment variable for each services --------------------*/
//var k8s = builder.AddKubernetesEnvironment("k8s-env");
//var compose = builder.AddDockerComposeEnvironment("docker-env");

//builder.AddProject<Projects.Api>("api")
//       .WithComputeEnvironment(compose);

//builder.AddProject<Projects.Frontend>("frontend")
//       .WithComputeEnvironment(k8s);

/*------------------------ Deploy to Azure App Service ------------------------------*/
//var env = builder.AddAzureAppServiceEnvironment("env");

//builder.AddProject<Projects.Api>("api")
//       .WithExternalHttpEndpoints()
//       .PublishAsAzureAppServiceWebsite((infra, site) =>
//       {
//           site.SiteConfig.IsWebSocketsEnabled = true;
//       });
