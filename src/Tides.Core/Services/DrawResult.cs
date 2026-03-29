using Tides.Core.Domain;
using Tides.Core.Domain.ValueObjects;

namespace Tides.Core.Services;

public record DrawResult(List<Heat> Heats, List<AuditEntry> AuditTrail);
