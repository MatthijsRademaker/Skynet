using Arnold.CustomerContract;
using Arnold.SkyNet.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Arnold.CustomerContractDbContext;

class CustomerTypeConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");

        builder.HasKey(ci => ci.Id);

        builder.Property(ci => ci.Name).HasMaxLength(50);

        builder.HasIndex(ci => ci.Email).IsUnique();
        // Configure Premium as owned entity
        builder.OwnsMany(
            c => c.PremiumAmounts,
            premium =>
            {
                premium.Property(p => p.Amount).HasColumnName("PremiumAmount");
            }
        );
    }
}
