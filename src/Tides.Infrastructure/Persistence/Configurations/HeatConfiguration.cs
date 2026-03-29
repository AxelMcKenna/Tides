using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tides.Core.Domain;

namespace Tides.Infrastructure.Persistence.Configurations;

public class HeatConfiguration : IEntityTypeConfiguration<Heat>
{
    public void Configure(EntityTypeBuilder<Heat> builder)
    {
        builder.ToTable("heats");
        builder.HasKey(h => h.Id);

        builder.HasMany(h => h.Entries)
            .WithOne()
            .HasForeignKey(e => e.HeatId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(h => h.Results)
            .WithOne()
            .HasForeignKey(r => r.HeatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(h => h.RoundId);
    }
}
