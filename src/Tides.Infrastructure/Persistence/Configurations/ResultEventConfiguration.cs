using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tides.Core.Domain;

namespace Tides.Infrastructure.Persistence.Configurations;

public class ResultEventConfiguration : IEntityTypeConfiguration<ResultEvent>
{
    public void Configure(EntityTypeBuilder<ResultEvent> builder)
    {
        builder.ToTable("result_events");
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.EventId).IsUnique();
        builder.HasIndex(e => e.CarnivalId);
        builder.HasIndex(e => e.HeatId);
        builder.HasIndex(e => new { e.HeatId, e.EntryId });

        builder.Property(e => e.Status).HasMaxLength(30);
        builder.Property(e => e.DeviceId).HasMaxLength(100);
        builder.Property(e => e.RecorderId).HasMaxLength(100);
    }
}
