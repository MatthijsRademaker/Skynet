namespace Arnold.CustomerContract;

public class Customer
{
    public Guid Id { get; set; }
    public required string Name { get; set; }

    public required string Email { get; set; }

    public Premium? Premium { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public string Address { get; set; }
}

public class CreateCustomerCommand
{
    public required string Name { get; set; }
    public required string Email { get; set; }
}

public class UpdateAddressCommand
{
    public required Guid Id { get; set; }
    public required string Address { get; set; }
}

public class Premium
{
    public decimal Amount { get; set; }
}
