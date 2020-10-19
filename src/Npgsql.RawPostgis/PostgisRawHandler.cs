using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandlers;
using EnterpriseDB.EDBClient.TypeHandling;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.RawPostgis
{
    public class PostgisRawHandlerFactory : EDBTypeHandlerFactory<byte[]>
    {
        public override EDBTypeHandler<byte[]> Create(PostgresType postgresType, EDBConnection conn)
            => new PostgisRawHandler(postgresType);
    }

    class PostgisRawHandler : ByteaHandler
    {
        public PostgisRawHandler(PostgresType postgresType) : base(postgresType) {}
    }
}
