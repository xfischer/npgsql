using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

/// <summary>
/// Hacky temporary measure used by EFCore.PG to extract user-configured enum mappings. Accessed via reflection only.
/// </summary>
[Experimental(EDBDiagnostics.ConvertersExperimental)]
public sealed class HackyEnumTypeMapping
{
    public HackyEnumTypeMapping(Type enumClrType, string pgTypeName, IEDBNameTranslator nameTranslator)
    {
        EnumClrType = enumClrType;
        PgTypeName = pgTypeName;
        NameTranslator = nameTranslator;
    }

    public string PgTypeName { get; }
    public Type EnumClrType { get; }
    public IEDBNameTranslator NameTranslator { get; }
}
