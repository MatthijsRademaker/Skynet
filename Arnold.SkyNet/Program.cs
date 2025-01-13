using Arnold.CustomerContract;
using Arnold.SkyNet;

var builder = Host.CreateApplicationBuilder(args);
builder.AddSkyNet();

var host = builder.Build();

host.Run();
