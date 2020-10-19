using System;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.TypeHandlers
{
    [TypeMapping("record")]
    class RecordHandlerFactory : EDBTypeHandlerFactory<object[]>
    {
        public override EDBTypeHandler<object[]> Create(PostgresType pgType, EDBConnection conn)
            => new RecordHandler(pgType, conn.Connector!.TypeMapper);
    }

    /// <summary>
    /// Type handler for PostgreSQL record types.
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-pseudo.html
    ///
    /// Encoding (identical to composite):
    /// A 32-bit integer with the number of columns, then for each column:
    /// * An OID indicating the type of the column
    /// * The length of the column(32-bit integer), or -1 if null
    /// * The column data encoded as binary
    /// </remarks>
    class RecordHandler : EDBTypeHandler<object[]>
    {
        readonly ConnectorTypeMapper _typeMapper;

        public RecordHandler(PostgresType postgresType, ConnectorTypeMapper typeMapper)
            : base(postgresType)
        {
            _typeMapper = typeMapper;
        }

        #region Read

        public override async ValueTask<object[]> Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        {
            await buf.Ensure(4, async);
            var fieldCount = buf.ReadInt32();
            var result = new object[fieldCount];

            for (var i = 0; i < fieldCount; i++)
            {
                await buf.Ensure(8, async);
                var typeOID = buf.ReadUInt32();
                var fieldLen = buf.ReadInt32();
                if (fieldLen == -1)  // Null field, simply skip it and leave at default
                    continue;
                result[i] = await _typeMapper.GetByOID(typeOID).ReadAsObject(buf, fieldLen, async);
            }

            return result;
        }

        #endregion

        #region Write (unsupported)

        public override int ValidateAndGetLength(object[] value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => throw new NotSupportedException("Can't write record types");

        public override Task Write(object[] value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async)
            => throw new NotSupportedException("Can't write record types");

        #endregion
    }
}
