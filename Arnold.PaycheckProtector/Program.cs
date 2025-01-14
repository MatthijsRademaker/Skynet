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
    async (
        IPremiumCalculator premiumCalculator,
        ICustomerRepository customerRepository,
        string customerId
    ) =>
    {
        var premium = await premiumCalculator.CalculatePremiumAsync(
            Guid.Parse(customerId),
            default
        );
        return premium;
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
    // TODO DI
    async (CustomerFactory factory, ILogger<IEventStore> logger, Guid Id) =>
    {
        logger.LogInformation("Getting events for {Id}", Id);
        var customer = await factory.RecreateCustomer(Id, default);
        // TODO return DTO with logic if premium changed
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
