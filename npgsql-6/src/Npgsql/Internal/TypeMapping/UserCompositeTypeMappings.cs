using System;
using EnterpriseDB.EDBClient.Internal.TypeHandlers.CompositeHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.Internal.TypeMapping
{
    public interface IUserCompositeTypeMapping : IUserTypeMapping
    {
        IEDBNameTranslator NameTranslator { get; }
    }

    class UserCompositeTypeMapping<T> : IUserCompositeTypeMapping
    {
        public string PgTypeName { get; }
        public Type ClrType => typeof(T);
        public IEDBNameTranslator NameTranslator { get; }

        public UserCompositeTypeMapping(string pgTypeName, IEDBNameTranslator nameTranslator)
            => (PgTypeName, NameTranslator) = (pgTypeName, nameTranslator);

        public EDBTypeHandler CreateHandler(PostgresType pgType, EDBConnector connector)
            => new CompositeHandler<T>((PostgresCompositeType)pgType, connector.TypeMapper, NameTranslator);
    }
}
