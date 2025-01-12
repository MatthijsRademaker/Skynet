var builder = DistributedApplication.CreateBuilder(args);

// TODO
var sqlEdge = builder
    .AddContainer("sqledge", "mcr.microsoft.com/azure-sql-edge")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_SA_PASSWORD", "sa");

var serviceBusInstance = builder
    .AddContainer("servicebus", "mcr.microsoft.com/azure-messaging/servicebus-emulator")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("SQL_SERVER", sqlEdge.Resource.Name)
    .WithEnvironment("MSSQL_SA_PASSWORD", "sa")
    .WithBindMount(
        "servicebus.emulator.config.json",
        "/ServiceBus_Emulator/ConfigFiles/Config.json"
    );

var premiumCalcProxy = builder
    .AddProject<Projects.Arnold_PremiumCalcProxy>("premiumcalcproxy")
    .WithExternalHttpEndpoints();

var serviceBus = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureServiceBus("messaging")
    : builder.AddConnectionString("messaging");

var postgres = builder.AddPostgres("postgres").WithLifetime(ContainerLifetime.Session);

var skynetDb = postgres.AddDatabase("skynetDb");

builder
    .AddProject<Projects.Arnold_PaycheckProtector>("paycheckprotector")
    .WithReference(premiumCalcProxy)
    .WithReference(serviceBus)
    .WithReference(skynetDb);

builder
    .AddProject<Projects.Arnold_PremiumWidget>("premiumwidget")
    .WithReference(premiumCalcProxy)
    .WithReference(serviceBus);

builder
    .AddProject<Projects.Arnold_SkyNet>("skynet")
    .WithReference(serviceBus)
    .WithReference(skynetDb);

// TODO add KnowledgeTest project

builder.Build().Run();
