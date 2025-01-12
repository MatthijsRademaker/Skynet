using System.Text.Json;
using Arnold.CustomerContract;
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

// TODO create full blown service for this
app.MapPost(
    "/apply",
    async (ServiceBusClient client) =>
    {
        var sender = client.CreateSender("customer");
        var command = new CreateCustomerCommand
        {
            Name = "Arnold",
            Email = "arnold@terminator.com",
            Version = 1,
        };

        await sender.SendMessageAsync(command.ToServiceBusMessage());
    }
);

app.MapPatch(
    "/updateAddress/{Id}",
    async (ServiceBusClient client, Guid Id) =>
    {
        var sender = client.CreateSender("customer");
        var command = new UpdateAddressCommand()
        {
            Id = Id,
            Address = "1234 Elm St",
            Version = 1,
        }; // TODO add more properties

        await sender.SendMessageAsync(command.ToServiceBusMessage());
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
