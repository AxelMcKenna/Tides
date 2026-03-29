using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tides.Core.Domain;

namespace Tides.Infrastructure.Persistence.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.ToTable("members");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.LastName).HasMaxLength(100).IsRequired();
        builder.Property(m => m.DateOfBirth).IsRequired();
        builder.Property(m => m.Gender).HasConversion<string>().HasMaxLength(20);
        builder.Property(m => m.SurfguardId).HasMaxLength(50);

        builder.HasIndex(m => m.ClubId);
        builder.HasIndex(m => m.SurfguardId).IsUnique().HasFilter("\"SurfguardId\" IS NOT NULL");
    }
}
