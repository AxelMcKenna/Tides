using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tides.Core.Domain;

namespace Tides.Infrastructure.Persistence.Configurations;

public class EventDefinitionConfiguration : IEntityTypeConfiguration<EventDefinition>
{
    public void Configure(EntityTypeBuilder<EventDefinition> builder)
    {
        builder.ToTable("event_definitions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.AgeGroup).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Gender).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.AdvancementRule).HasConversion<string>().HasMaxLength(30);

        builder.HasMany(e => e.Rounds)
            .WithOne()
            .HasForeignKey(r => r.EventDefinitionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => e.CarnivalId);
        builder.HasIndex(e => new { e.CarnivalId, e.AgeGroup, e.Category, e.Gender });
    }
}
