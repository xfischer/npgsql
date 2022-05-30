using System;
using System.Data;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeHandling
{
    public class TypeMappingInfo
    {
        public TypeMappingInfo(EDBDbType? npgsqlDbType, string? dataTypeName, Type clrType)
            => (EDBDbType, DataTypeName, ClrTypes) = (npgsqlDbType, dataTypeName, new[] { clrType });

        public TypeMappingInfo(EDBDbType? npgsqlDbType, string? dataTypeName, params Type[] clrTypes)
            => (EDBDbType, DataTypeName, ClrTypes) = (npgsqlDbType, dataTypeName, clrTypes);

        public EDBDbType? EDBDbType { get; }
        DbType? dbType;
        public DbType DbType
            => dbType ??= EDBDbType is null ? DbType.Object : GlobalTypeMapper.EDBDbTypeToDbType(EDBDbType.Value);
        public string? DataTypeName { get; }
        public Type[] ClrTypes { get; }

        internal void Reset() => dbType = null;
    }
}
