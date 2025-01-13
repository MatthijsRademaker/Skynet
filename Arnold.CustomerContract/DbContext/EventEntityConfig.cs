using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arnold.CustomerContract;

public class StoredEvent
{
    public Guid Id { get; set; }
    public string Type { get; set; }
    public string Data { get; set; }
    public Guid AggregateId { get; set; }
    public DateTime Timestamp { get; set; }
    public int Version { get; set; }
}

public class StoredEventTypeConfiguration : IEntityTypeConfiguration<StoredEvent>
{
    public void Configure(EntityTypeBuilder<StoredEvent> builder)
    {
        builder.ToTable("Events", "core");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id).ValueGeneratedNever();
        builder.Property(e => e.Type).HasMaxLength(256).IsRequired();
        builder.Property(e => e.Data).IsRequired();
        builder.Property(e => e.AggregateId).IsRequired();
        builder.Property(e => e.Timestamp).IsRequired();
        builder.Property(e => e.Version).IsRequired();
    }
}
