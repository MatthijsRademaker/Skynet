var builder = DistributedApplication.CreateBuilder(args);

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
