using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tides.Core.Domain;

namespace Tides.Infrastructure.Persistence.Configurations;

public class EntryConfiguration : IEntityTypeConfiguration<Entry>
{
    public void Configure(EntityTypeBuilder<Entry> builder)
    {
        builder.ToTable("entries");
        builder.HasKey(e => e.Id);

        // MemberIds stored as JSON array — supports individual and relay entries
        builder.Property(e => e.MemberIds)
            .HasColumnType("jsonb");

        // HeatId is nullable — entries exist before draw assignment
        builder.HasIndex(e => e.HeatId);
        builder.HasIndex(e => e.ClubId);
        builder.HasIndex(e => e.EventDefinitionId);
    }
}
