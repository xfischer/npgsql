using EnterpriseDB.EDBClient.Internal.Postgres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient.PostgresTypes;

/// <summary>
/// Represents a nested table type (aka: PL/SQL Collections and Records, or TABLE OF type)
/// This type can hold a set of a defined type (domain, record, composite)
/// </summary>
public class EDBTableOfType : PostgresArrayType
{
    // EnterpriseDB: The sole purpose of this derived class is to avoid confusion with domain PostgresArrayType
    // This way when EDBTableOfResolver.GetTypeInfo(...) is called we can filter on this type and map the dedicated text converter
    // Other solution would be to add a flag to PostgresArrayType and set it for table of types, but this might complicate the upstream merge

    /// <summary>
    /// Constructs a representation of an EDBTableOfType data type.
    /// </summary>
    protected internal EDBTableOfType(string ns, string name, uint oid, PostgresType elementPostgresType) : base(ns, name, oid, elementPostgresType) { }

    internal EDBTableOfType(DataTypeName dataTypeName, Oid oid, PostgresType elementPostgresType) : base(dataTypeName, oid, elementPostgresType) { }
}
