using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tides.Core.Domain;

namespace Tides.Infrastructure.Persistence.Configurations;

public class ProtestConfiguration : IEntityTypeConfiguration<Protest>
{
    public void Configure(EntityTypeBuilder<Protest> builder)
    {
        builder.ToTable("protests");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Reason).HasMaxLength(1000).IsRequired();
        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.AdjudicationReason).HasMaxLength(1000);

        builder.HasIndex(p => p.CarnivalId);
        builder.HasIndex(p => p.EventId);
    }
}
