using System.Diagnostics.CodeAnalysis;
namespace EnterpriseDB.EDBClient.Internal.Postgres;

/// Base field type shared between tables and composites.
[Experimental(EDBDiagnostics.ConvertersExperimental)]
public readonly struct Field(string name, PgTypeId pgTypeId, int typeModifier)
{
    public string Name { get; init; } = name;
    public PgTypeId PgTypeId { get; init; } = pgTypeId;
    public int TypeModifier { get; init; } = typeModifier;
}
