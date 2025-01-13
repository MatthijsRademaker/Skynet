var builder = DistributedApplication.CreateBuilder(args);

// TODO reinstate with correct connectionstring
var mssqlInstance = builder
    .AddContainer("mssql", "mcr.microsoft.com/mssql/server:2022-latest", "")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_SA_PASSWORD", "temporarily-secure-password-!123");

var serviceBusInstance = builder
    .AddContainer("servicebus", "mcr.microsoft.com/azure-messaging/servicebus-emulator")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("SQL_SERVER", mssqlInstance.Resource.Name)
    .WithEnvironment("MSSQL_SA_PASSWORD", "temporarily-secure-password-!123")
    .WithBindMount(
        "servicebus.emulator.config.json",
        "/ServiceBus_Emulator/ConfigFiles/Config.json"
    )
    .WithEndpoint(
        "servicebus",
        (endpoint) =>
        {
            endpoint.Name = "servicebus";
            endpoint.Port = 5672;
            endpoint.TargetPort = 5672;
        }
    )
    .WaitFor(mssqlInstance);

var premiumCalcProxy = builder.AddProject<Projects.Arnold_PremiumCalcProxy>("premiumcalcproxy");

var serviceBus = builder.AddConnectionString("messaging");

var username = builder.AddParameter("username", "admin", secret: false);
var password = builder.AddParameter("password", "MyVeryStrongPassword1234", secret: false);

var postgres = builder
    .AddPostgres("postgres", username, password)
    .WithLifetime(ContainerLifetime.Session)
    .WithPgAdmin(
        (builder) =>
        {
            builder.WithHostPort(5050);
        }
    );

var skynetDb = postgres.AddDatabase("skynetdb");

builder
    .AddProject<Projects.Arnold_PaycheckProtector>("paycheckprotector")
    .WithReference(premiumCalcProxy)
    .WithReference(serviceBus)
    .WaitFor(serviceBusInstance)
    .WaitFor(postgres)
    .WithReference(skynetDb);

builder
    .AddProject<Projects.Arnold_PremiumWidget>("premiumwidget")
    .WithReference(premiumCalcProxy)
    .WaitFor(serviceBusInstance)
    .WaitFor(postgres)
    .WithReference(serviceBus);

builder
    .AddProject<Projects.Arnold_SkyNet>("skynet")
    .WithReference(serviceBus)
    .WaitFor(serviceBusInstance)
    .WaitFor(postgres)
    .WithReference(skynetDb);

builder
    .AddProject<Projects.Arnold_KnowledgeTest>("knowledgetest")
    .WithReference(serviceBus)
    .WithReference(skynetDb)
    .WaitFor(serviceBusInstance)
    .WaitFor(postgres);

// TODO add KnowledgeTest project

builder.Build().Run();
