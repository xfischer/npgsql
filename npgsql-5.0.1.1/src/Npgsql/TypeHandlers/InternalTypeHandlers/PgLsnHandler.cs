using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.InternalTypeHandlers
{
    [TypeMapping("pg_lsn", EDBDbType.PgLsn, typeof(EDBLogSequenceNumber))]
    class PgLsnHandler : EDBSimpleTypeHandler<EDBLogSequenceNumber>
    {
        public PgLsnHandler(PostgresType postgresType) : base(postgresType) {}

        #region Read

        public override EDBLogSequenceNumber Read(EDBReadBuffer buf, int len, FieldDescription? fieldDescription = null)
        {
            Debug.Assert(len == 8);
            return new EDBLogSequenceNumber(buf.ReadUInt64());
        }

        #endregion Read

        #region Write

        public override int ValidateAndGetLength(EDBLogSequenceNumber value, EDBParameter? parameter) => 8;

        public override void Write(EDBLogSequenceNumber value, EDBWriteBuffer buf, EDBParameter? parameter)
            => buf.WriteUInt64((ulong)value);

        #endregion Write
    }
}
