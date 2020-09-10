using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;

namespace EnterpriseDB.EDBClient.TypeHandlers.CompositeHandlers
{
    class MappedCompositeTypeHandlerFactory<T> : EDBTypeHandlerFactory<T>, IMappedCompositeTypeHandlerFactory
    {
        public IEDBNameTranslator NameTranslator { get; }

        internal MappedCompositeTypeHandlerFactory(IEDBNameTranslator nameTranslator)
            => NameTranslator = nameTranslator;

        public override EDBTypeHandler<T> Create(PostgresType pgType, EDBConnection conn)
            => new MappedCompositeHandler<T>((PostgresCompositeType)pgType, conn.Connector!.TypeMapper, NameTranslator);
    }
}
