namespace Tides.Core.Domain.ValueObjects;

public record AuditEntry(DateTime Timestamp, string UserId, string Action, string? Detail = null);
