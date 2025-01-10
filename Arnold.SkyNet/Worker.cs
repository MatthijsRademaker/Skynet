using System.Text.Json;
using Arnold.CustomerContract;
using Azure.Messaging.ServiceBus;

namespace Arnold.SkyNet;

public class Worker(ILogger<Worker> logger, ServiceBusClient serviceBusClient) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var receiver = serviceBusClient.CreateProcessor("customer");
        receiver.ProcessMessageAsync += async args =>
        {
            // TODO Mediatr to publish internal events based on message type
            var customer = JsonSerializer.Deserialize<Customer>(args.Message.Body.ToString());
            logger.LogInformation($"Received customer: {customer.Name}");
        };
    }
}
