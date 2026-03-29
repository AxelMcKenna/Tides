using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tides.Core.Domain;

namespace Tides.Infrastructure.Persistence.Configurations;

public class PointsTableConfiguration : IEntityTypeConfiguration<PointsTable>
{
    public void Configure(EntityTypeBuilder<PointsTable> builder)
    {
        builder.ToTable("points_tables");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();

        // Entries stored as JSON array
        builder.Property(p => p.Entries)
            .HasColumnType("jsonb")
            .HasColumnName("entries");
    }
}
