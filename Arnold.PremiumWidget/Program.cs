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

app.MapPost(
    "/callMeBack",
    async (ServiceBusClient client) =>
    {
        var sender = client.CreateSender("customer");

        var customer = new CreateCustomerCommand
        {
            Name = "Arnold",
            Email = "arnold@terminator.com",
        };

        var json = JsonSerializer.Serialize(customer);

        var message = new ServiceBusMessage(json);

        await sender.SendMessageAsync(message);
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
