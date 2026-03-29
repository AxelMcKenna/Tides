using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tides.Core.Domain;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Infrastructure.Persistence.Configurations;

public class ResultConfiguration : IEntityTypeConfiguration<Result>
{
    public void Configure(EntityTypeBuilder<Result> builder)
    {
        builder.ToTable("results");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.JudgeScore).HasPrecision(8, 2);

        // Placing as complex property — stored as an int column
        builder.ComplexProperty(r => r.Placing, p =>
        {
            p.Property(x => x.Position).HasColumnName("placing");
            p.IsRequired(false);
        });

        // TimeResult as complex property — stored as interval
        builder.ComplexProperty(r => r.Time, t =>
        {
            t.Property(x => x.Time).HasColumnName("time");
            t.IsRequired(false);
        });

        // AuditTrail stored as JSON
        builder.Property(r => r.AuditTrail)
            .HasColumnType("jsonb")
            .HasColumnName("audit_trail");

        builder.HasIndex(r => r.HeatId);
        builder.HasIndex(r => r.EntryId);
    }
}
