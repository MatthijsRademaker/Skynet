using System.Text.Json;
using Arnold.CustomerContract;
using Arnold.CustomerContract.Events;
using Arnold.KnowledgeTest;
using Arnold.SkyNet.Domain;
using Azure.Messaging.ServiceBus;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();
builder.AddKnowledgeTest();

var app = builder.Build();

app.MapGet(
    "/customer/recreate/{Id}",
    async (CustomerFactory factory, ILogger<IEventStore> logger, Guid Id) =>
    {
        // TODO create replay logic for events
        logger.LogInformation("Getting events for {Id}", Id);
        var customer = await factory.RecreateCustomer(Id, default);
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

app.MapPost(
    "/apply",
    // TODO add query params for failed test and customer id
    // Then run test with recreating customer from PaycheckProtector and vice versa
    async (ServiceBusClient client, KnowledgeTestResult result) =>
    {
        // TODO verify if new customer is already in the system
        var sender = client.CreateSender("customer");
        var command = new CreateCustomerCommand
        {
            Name = result.Name,
            Email = $"{result.Name}@terminator.com",
            Version = 1,
        };

        await sender.SendMessageAsync(command.ToServiceBusMessage());

        var knowledgeTestCommand = new KnowledgeTestCommand
        {
            CustomerId = command.CustomerId,
            Passed = result.Passed,
            Version = 1,
        };

        await sender.SendMessageAsync(knowledgeTestCommand.ToServiceBusMessage());

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
