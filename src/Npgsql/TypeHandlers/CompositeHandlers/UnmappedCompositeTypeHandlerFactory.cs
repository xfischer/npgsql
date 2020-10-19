using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;

#nullable disable // About to be removed

namespace EnterpriseDB.EDBClient.TypeHandlers.CompositeHandlers
{
    class UnmappedCompositeTypeHandlerFactory : EDBTypeHandlerFactory<object>
    {
        readonly IEDBNameTranslator _nameTranslator;

        internal UnmappedCompositeTypeHandlerFactory(IEDBNameTranslator nameTranslator)
        {
            _nameTranslator = nameTranslator;
        }

        public override EDBTypeHandler<object> Create(PostgresType postgresType, EDBConnection conn)
            => new UnmappedCompositeHandler(postgresType, _nameTranslator, conn.Connector.TypeMapper);
    }
}
