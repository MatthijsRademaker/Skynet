using Arnold.SkyNet;

var builder = Host.CreateApplicationBuilder(args);
builder.AddSkyNet();
builder.AddAzureServiceBusClient("messaging");
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
