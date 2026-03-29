using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tides.Core.Domain;

namespace Tides.Infrastructure.Persistence.Configurations;

public class CarnivalConfiguration : IEntityTypeConfiguration<Carnival>
{
    public void Configure(EntityTypeBuilder<Carnival> builder)
    {
        builder.ToTable("carnivals");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).HasMaxLength(200).IsRequired();
        builder.Property(c => c.Sanction).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.StartDate).IsRequired();
        builder.Property(c => c.EndDate).IsRequired();

        builder.HasMany(c => c.Events)
            .WithOne()
            .HasForeignKey(e => e.CarnivalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.PointsTable)
            .WithOne()
            .HasForeignKey<PointsTable>("CarnivalId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Protests)
            .WithOne()
            .HasForeignKey(p => p.CarnivalId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.HostingClubId);

        // ClubPoints and DomainEvents are not persisted — derived/transient
        builder.Ignore(c => c.ClubPoints);
        builder.Ignore(c => c.DomainEvents);
    }
}
