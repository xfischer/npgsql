using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;

namespace EnterpriseDB.EDBClient.TypeHandlers.CompositeHandlers
{
    class CompositeTypeHandlerFactory<T> : EDBTypeHandlerFactory<T>, ICompositeTypeHandlerFactory
    {
        public IEDBNameTranslator NameTranslator { get; }

        internal CompositeTypeHandlerFactory(IEDBNameTranslator nameTranslator)
            => NameTranslator = nameTranslator;

        public override EDBTypeHandler<T> Create(PostgresType pgType, EDBConnection conn)
            => new CompositeHandler<T>((PostgresCompositeType)pgType, conn.Connector!.TypeMapper, NameTranslator);
    }
}
