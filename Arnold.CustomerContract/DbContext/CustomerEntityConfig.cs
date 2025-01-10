using Arnold.CustomerContract;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arnold.CustomerContractDbContext;

class CustomerTypeConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers", "core");

        builder.Property(ci => ci.Name).HasMaxLength(50);

        builder.HasIndex(ci => ci.Email).IsUnique();
        // Configure Premium as owned entity
        builder.OwnsOne(
            c => c.Premium,
            premium =>
            {
                premium.Property(p => p.Amount).HasColumnName("PremiumAmount");
            }
        );
    }
}
