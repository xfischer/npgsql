using System;
using System.Data;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandlers.NumericHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;
using NUnit.Framework;
using static EnterpriseDB.EDBClient.Tests.TestUtil;

namespace EnterpriseDB.EDBClient.Tests
{
    [NonParallelizable]
    public class TypeMapperTests : TestBase
    {
        [Test]
        public void Global_mapping()
        {
            var myFactory = new MyInt32TypeHandlerResolverFactory();
            EDBConnection.GlobalTypeMapper.AddTypeResolverFactory(myFactory);

            using var pool = CreateTempPool(ConnectionString, out var connectionString);
            using var conn = OpenConnection(connectionString);
            using var cmd = new EDBCommand("SELECT @p", conn);
            var range = new EDBRange<int>(8, true, false, 0, false, true);
            var parameters = new[]
            {
                // Base
                new EDBParameter("p", EDBDbType.Integer) { Value = 8 },
                new EDBParameter("p", DbType.Int32) { Value = 8 },
                new EDBParameter { ParameterName = "p", Value = 8 },
                // Array
                new EDBParameter { ParameterName = "p", Value = new[] { 8 } },
                new EDBParameter("p", EDBDbType.Array | EDBDbType.Integer) { Value = new[] { 8 } },
                // Range
                new EDBParameter { ParameterName = "p", Value = range },
                new EDBParameter("p", EDBDbType.Range | EDBDbType.Integer) { Value = range },
            };

            for (var i = 0; i < parameters.Length; i++)
            {
                cmd.Parameters.Add(parameters[i]);
                cmd.ExecuteScalar();
                Assert.That(myFactory.Reads, Is.EqualTo(i+1));
                Assert.That(myFactory.Writes, Is.EqualTo(i+1));
                cmd.Parameters.Clear();
            }
        }

        [Test]
        public void Local_mapping()
        {
            var myFactory = new MyInt32TypeHandlerResolverFactory();
            using var _ = CreateTempPool(ConnectionString, out var connectionString);

            using (var conn = OpenConnection(connectionString))
            using (var cmd = new EDBCommand("SELECT @p", conn))
            {
                conn.TypeMapper.AddTypeResolverFactory(myFactory);
                cmd.Parameters.AddWithValue("p", 8);
                cmd.ExecuteScalar();
                Assert.That(myFactory.Reads, Is.EqualTo(1));
                Assert.That(myFactory.Writes, Is.EqualTo(1));
            }

            // Make sure reopening (same physical connection) reverts the mapping
            using (var conn = OpenConnection(connectionString))
            using (var cmd = new EDBCommand("SELECT @p", conn))
            {
                cmd.Parameters.AddWithValue("p", 8);
                cmd.ExecuteScalar();
                Assert.That(myFactory.Reads, Is.EqualTo(1));
                Assert.That(myFactory.Writes, Is.EqualTo(1));
            }
        }

        [Test]
        public void Global_reset()
        {
            var myFactory = new MyInt32TypeHandlerResolverFactory();
            EDBConnection.GlobalTypeMapper.AddTypeResolverFactory(myFactory);
            using var _ = CreateTempPool(ConnectionString, out var connectionString);

            using (OpenConnection(connectionString))
            {
            }
            // We now have a connector in the pool with our custom mapping

            EDBConnection.GlobalTypeMapper.Reset();
            using (var conn = OpenConnection(connectionString))
            {
                // Should be the pooled connector from before, but it should have picked up the reset
                conn.ExecuteScalar("SELECT 1");
                Assert.That(myFactory.Reads, Is.Zero);

                // Now create a second *physical* connection to make sure it picks up the new mapping as well
                using (var conn2 = OpenConnection(connectionString))
                {
                    conn2.ExecuteScalar("SELECT 1");
                    Assert.That(myFactory.Reads, Is.Zero);
                }

                EDBConnection.ClearPool(conn);
            }
        }

        [Test]
        public async Task String_to_citext()
        {
            using (CreateTempPool(ConnectionString, out var connectionString))
            using (var conn = OpenConnection(connectionString))
            {
                await EnsureExtensionAsync(conn, "citext");

                conn.TypeMapper.AddTypeResolverFactory(new CitextToStringTypeHandlerResolverFactory());

                using (var cmd = new EDBCommand("SELECT @p = 'hello'::citext", conn))
                {
                    cmd.Parameters.AddWithValue("p", "HeLLo");
                    Assert.That(cmd.ExecuteScalar(), Is.True);
                }
            }
        }

        #region Support

        class MyInt32TypeHandlerResolverFactory : TypeHandlerResolverFactory
        {
            internal int Reads, Writes;

            public override TypeHandlerResolver Create(EDBConnector connector)
                => new MyInt32TypeHandlerResolver(connector, this);

            public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName) => throw new NotSupportedException();
            public override string? GetDataTypeNameByClrType(Type clrType) => throw new NotSupportedException();
            public override string? GetDataTypeNameByValueDependentValue(object value) => throw new NotSupportedException();
        }

        class MyInt32TypeHandlerResolver : TypeHandlerResolver
        {
            readonly EDBTypeHandler _handler;

            public MyInt32TypeHandlerResolver(EDBConnector connector, MyInt32TypeHandlerResolverFactory factory)
                => _handler = new MyInt32Handler(connector.DatabaseInfo.GetPostgresTypeByName("integer"), factory);

            public override EDBTypeHandler? ResolveByClrType(Type type)
                => type == typeof(int) ? _handler : null;
            public override EDBTypeHandler? ResolveByDataTypeName(string typeName)
                => typeName == "integer" ? _handler : null;

            public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName) => throw new NotSupportedException();

        }

        class MyInt32Handler : Int32Handler
        {
            readonly MyInt32TypeHandlerResolverFactory _factory;

            public MyInt32Handler(PostgresType postgresType, MyInt32TypeHandlerResolverFactory factory)
                : base(postgresType)
                => _factory = factory;

            public override int Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            {
                _factory.Reads++;
                return base.Read(buf, len, fieldDescription);
            }

            public override void Write(int value, EDBWriteBuffer buf, EDBParameter? parameter)
            {
                _factory.Writes++;
                base.Write(value, buf, parameter);
            }
        }

        class CitextToStringTypeHandlerResolverFactory : TypeHandlerResolverFactory
        {
            public override TypeHandlerResolver Create(EDBConnector connector)
                => new CitextToStringTypeHandlerResolver(connector);

            public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName) => throw new NotSupportedException();
            public override string? GetDataTypeNameByClrType(Type clrType) => throw new NotSupportedException();
            public override string? GetDataTypeNameByValueDependentValue(object value) => throw new NotSupportedException();

            class CitextToStringTypeHandlerResolver : TypeHandlerResolver
            {
                readonly EDBConnector _connector;
                readonly PostgresType _pgCitextType;

                public CitextToStringTypeHandlerResolver(EDBConnector connector)
                {
                    _connector = connector;
                    _pgCitextType = connector.DatabaseInfo.GetPostgresTypeByName("citext");
                }

                public override EDBTypeHandler? ResolveByClrType(Type type)
                    => type == typeof(string) ? new TextHandler(_pgCitextType, _connector.TextEncoding) : null;
                public override EDBTypeHandler? ResolveByDataTypeName(string typeName) => null;

                public override TypeMappingInfo? GetMappingByDataTypeName(string dataTypeName) => throw new NotSupportedException();
            }
        }

        #endregion Support

        [TearDown]
        public void TearDown() => EDBConnection.GlobalTypeMapper.Reset();
    }
}
