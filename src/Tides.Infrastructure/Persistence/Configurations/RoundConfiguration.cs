using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tides.Core.Domain;

namespace Tides.Infrastructure.Persistence.Configurations;

public class RoundConfiguration : IEntityTypeConfiguration<Round>
{
    public void Configure(EntityTypeBuilder<Round> builder)
    {
        builder.ToTable("rounds");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(20);

        builder.HasMany(r => r.Heats)
            .WithOne()
            .HasForeignKey(h => h.RoundId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.EventDefinitionId);
    }
}
