using System.Text.Json;
using Arnold.CustomerContract;
using Arnold.CustomerContract.Events;
using Arnold.PaycheckProtector;
using Arnold.SkyNet.Domain;
using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddPaycheckProtector();

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

app.MapGet(
    "/customer/{Id}",
    async (ICustomerRepository repository, Guid Id) =>
    {
        var customer = await repository.GetAsync(Id, default);
        return customer;
    }
);

app.MapGet(
    "/customer/recreate/{Id}",
    async (IEventStore repository, ILogger<IEventStore> logger, Guid Id) =>
    {
        // TODO create replay logic for events
        logger.LogInformation("Getting events for {Id}", Id);
        var events = await repository.GetEvents(Id, default);
        var customer = Customer.RecreateCustomer(events);
        return customer;
    }
);

app.MapGet(
    "/customer",
    async (ICustomerRepository repository, string email) =>
    {
        var customer = await repository.GetAsync(email, default);
        return customer;
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
    "/updateKnowledgeTest/{Id}",
    async (ServiceBusClient client, Guid Id, bool passed) =>
    {
        var sender = client.CreateSender("customer");
        var command = new KnowledgeTestCommand
        {
            CustomerId = Id,
            Passed = passed,
            Version = 1,
        };

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
