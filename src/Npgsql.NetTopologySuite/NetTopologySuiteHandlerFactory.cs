using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.NetTopologySuite
{
    public class NetTopologySuiteHandlerFactory : EDBTypeHandlerFactory<Geometry>
    {
        readonly PostGisReader _reader;
        readonly PostGisWriter _writer;

        internal NetTopologySuiteHandlerFactory(PostGisReader reader, PostGisWriter writer)
        {
            _reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _writer = writer ?? throw new ArgumentNullException(nameof(writer));
        }

        public override EDBTypeHandler<Geometry> Create(PostgresType postgresType, EDBConnection conn)
            => new NetTopologySuiteHandler(postgresType, _reader, _writer);
    }
}
