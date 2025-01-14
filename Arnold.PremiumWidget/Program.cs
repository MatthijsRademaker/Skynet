using System.Text.Json;
using Arnold.CustomerContract;
using Arnold.SkyNet.Domain;
using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddAzureServiceBusClient("messaging");

var app = builder.Build();

app.MapGet(
    "/getPremium",
    async (IHttpClientFactory clientFactory) =>
    {
        var client = clientFactory.CreateClient();
        var response = await client.GetAsync("https+http://premiumcalcproxy/getPremium");
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        return content;
    }
);

app.MapPost(
    "/callMeBack",
    async (ServiceBusClient client, string name, decimal premium) =>
    {
        var sender = client.CreateSender("customer");

        // TODO verify if this is actually a new customer
        var command = new CreateCustomerCommand
        {
            Name = name,
            Email = $"{name}@terminator.com",
            Version = 1,
        };

        await sender.SendMessageAsync(command.ToServiceBusMessage());

        // mock get premium
        await sender.SendMessageAsync(
            new PremiumCalculatedCommand()
            {
                Id = command.CustomerId,
                Premium = new Premium(premium),
                Version = 1,
            }.ToServiceBusMessage()
        );

        return command.CustomerId;
    }
);

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapDefaultEndpoints();

app.Run();
