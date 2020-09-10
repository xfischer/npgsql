using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.TypeHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-boolean.html
    /// </remarks>
    [TypeMapping("void")]
    class VoidHandler : EDBSimpleTypeHandler<DBNull>
    {
        public VoidHandler(PostgresType postgresType) : base(postgresType) {}

        public override DBNull Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
            => DBNull.Value;

        public override int ValidateAndGetLength(DBNull value, EDBParameter? parameter)
            => throw new NotSupportedException();

        public override void Write(DBNull value, EDBWriteBuffer buf, EDBParameter? parameter)
            => throw new NotSupportedException();
    }
}
