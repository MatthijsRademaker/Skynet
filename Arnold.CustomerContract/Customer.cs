﻿using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;

namespace Arnold.CustomerContract;

public static class CommandsExtenstions
{
    public static ServiceBusMessage ToServiceBusMessage<T>(this T command)
        where T : BaseCommand
    {
        var json = JsonSerializer.Serialize(command);
        var message = new ServiceBusMessage(json) { Subject = command.GetType().Name };
        return message;
    }

    public static string GetCommandName(this ServiceBusReceivedMessage message)
    {
        return message.Subject;
    }

    public static T ToCommand<T>(this ServiceBusReceivedMessage message)
    {
        var json = message.Body.ToString();
        return JsonSerializer.Deserialize<T>(json);
    }
}

public abstract class BaseCommand
{
    public required int Version { get; set; }
};

public class CreateCustomerCommand : BaseCommand
{
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class UpdateAddressCommand : BaseCommand
{
    public required Guid Id { get; set; }
    public required string Address { get; set; }
}
