using System.Text.Json;
using Arnold.CustomerContract;
using Arnold.SkyNet.CommandHandlers;
using Azure.Messaging.ServiceBus;
using MediatR;

namespace Arnold.SkyNet;

public class Worker : BackgroundService
{
    private readonly ServiceBusProcessor serviceBusProcessor;
    private readonly ILogger<Worker> logger;
    private readonly ServiceBusClient serviceBusClient;

    public Worker(
        ILogger<Worker> logger,
        ServiceBusClient serviceBusClient,
        IServiceScopeFactory serviceScopeFactory
    )
    {
        this.logger = logger;
        this.serviceBusClient = serviceBusClient;
        serviceBusProcessor = serviceBusClient.CreateProcessor("customer");

        serviceBusProcessor.ProcessMessageAsync += async args =>
        {
            using var scope = serviceScopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            switch (args.Message.GetCommandName())
            {
                // TODO Versioning
                case nameof(CreateCustomerCommand):
                    await mediator.Publish(
                        new CreateCustomerCommandRequest(
                            args.Message.ToCommand<CreateCustomerCommand>()
                        ),
                        args.CancellationToken
                    );
                    break;

                case nameof(UpdateAddressCommand):
                    await mediator.Publish(
                        // TODO wrap in a request
                        args.Message.ToCommand<UpdateAddressCommand>(),
                        args.CancellationToken
                    );
                    break;

                default:
                    logger.LogWarning(
                        "Unknown message type: {MessageType}",
                        args.Message.GetCommandName()
                    );
                    break;
            }
        };

        serviceBusProcessor.ProcessErrorAsync += args =>
        {
            logger.LogError(
                args.Exception,
                "Error processing message: {ExceptionMessage}",
                args.Exception.Message
            );

            return Task.CompletedTask;
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await serviceBusProcessor.StartProcessingAsync(stoppingToken);
    }
}
