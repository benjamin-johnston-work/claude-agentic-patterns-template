namespace Archie.Domain.ValueObjects;

public record PatternMetadata
{
    public Dictionary<string, string> ParticipantRoles { get; init; } = new(); // EntityId -> Role
    public List<string> PatternCharacteristics { get; init; } = new();
    public string? Description { get; init; }
    public List<PatternViolation> Violations { get; init; } = new();

    public static PatternMetadata Create(
        Dictionary<string, string>? participantRoles = null,
        List<string>? patternCharacteristics = null,
        string? description = null,
        List<PatternViolation>? violations = null)
    {
        return new PatternMetadata
        {
            ParticipantRoles = participantRoles ?? new Dictionary<string, string>(),
            PatternCharacteristics = patternCharacteristics ?? new List<string>(),
            Description = description,
            Violations = violations ?? new List<PatternViolation>()
        };
    }

    public static PatternMetadata Empty => new();

    public bool HasRole(string entityId)
    {
        return !string.IsNullOrWhiteSpace(entityId) && ParticipantRoles.ContainsKey(entityId);
    }

    public string? GetRole(string entityId)
    {
        return !string.IsNullOrWhiteSpace(entityId) && ParticipantRoles.TryGetValue(entityId, out var role) ? role : null;
    }

    public List<string> GetEntitiesWithRole(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return new List<string>();

        return ParticipantRoles
            .Where(kv => kv.Value.Equals(role, StringComparison.OrdinalIgnoreCase))
            .Select(kv => kv.Key)
            .ToList();
    }

    public bool HasCharacteristic(string characteristic)
    {
        return !string.IsNullOrWhiteSpace(characteristic) &&
               PatternCharacteristics.Any(c => c.Contains(characteristic, StringComparison.OrdinalIgnoreCase));
    }

    public bool HasViolations => Violations.Count > 0;
    public bool HasCriticalViolations => Violations.Any(v => v.IsCritical);
    public bool HasErrors => Violations.Any(v => v.IsError);
    public bool HasWarnings => Violations.Any(v => v.IsWarning);

    public int ViolationCount => Violations.Count;
    public int CriticalViolationCount => Violations.Count(v => v.IsCritical);
    public int ErrorCount => Violations.Count(v => v.IsError);
    public int WarningCount => Violations.Count(v => v.IsWarning);

    public PatternMetadata AddParticipant(string entityId, string role)
    {
        if (string.IsNullOrWhiteSpace(entityId) || string.IsNullOrWhiteSpace(role))
            return this;

        var newParticipantRoles = new Dictionary<string, string>(ParticipantRoles)
        {
            [entityId] = role
        };

        return this with { ParticipantRoles = newParticipantRoles };
    }

    public PatternMetadata AddCharacteristic(string characteristic)
    {
        if (string.IsNullOrWhiteSpace(characteristic) || HasCharacteristic(characteristic))
            return this;

        var newCharacteristics = new List<string>(PatternCharacteristics) { characteristic };
        return this with { PatternCharacteristics = newCharacteristics };
    }

    public PatternMetadata AddViolation(PatternViolation violation)
    {
        if (violation == null)
            return this;

        var newViolations = new List<PatternViolation>(Violations) { violation };
        return this with { Violations = newViolations };
    }

    public PatternMetadata WithDescription(string description)
    {
        return this with { Description = description };
    }
}