using System;
using System.Data;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers
{
    #pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

    [TypeMapping("ltree", EDBDbType.LTree)]
    class LTreeHandlerFactory : EDBTypeHandlerFactory<string>
    {
        public override EDBTypeHandler<string> Create(PostgresType postgresType, EDBConnection conn)
            => new LTreeHandler(postgresType, conn);
    }

    /// <summary>
    /// Ltree binary encoding is a simple UTF8 string, but prepended with a version number.
    /// </summary>
    public class LTreeHandler : TextHandler
    {
        /// <summary>
        /// Prepended to the string in the wire encoding
        /// </summary>
        const byte LtreeProtocolVersion = 1;

        internal override bool PreferTextWrite => false;

        protected internal LTreeHandler(PostgresType postgresType, EDBConnection connection)
            : base(postgresType, connection) {}

        #region Write

        public override int ValidateAndGetLength(string value, ref EDBLengthCache? lengthCache, EDBParameter? parameter) =>
            base.ValidateAndGetLength(value, ref lengthCache, parameter) + 1;


        public override int ValidateAndGetLength(char[] value, ref EDBLengthCache? lengthCache, EDBParameter? parameter) =>
            base.ValidateAndGetLength(value, ref lengthCache, parameter) + 1;


        public override int ValidateAndGetLength(ArraySegment<char> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter) =>
            base.ValidateAndGetLength(value, ref lengthCache, parameter) + 1;


        public override async Task Write(string value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        {
            if (buf.WriteSpaceLeft < 1)
                await buf.Flush(async, cancellationToken);

            buf.WriteByte(LtreeProtocolVersion);
            await base.Write(value, buf, lengthCache, parameter, async, cancellationToken);
        }

        public override async Task Write(char[] value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        {
            if (buf.WriteSpaceLeft < 1)
                await buf.Flush(async, cancellationToken);

            buf.WriteByte(LtreeProtocolVersion);
            await base.Write(value, buf, lengthCache, parameter, async, cancellationToken);
        }

        public override async Task Write(ArraySegment<char> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        {
            if (buf.WriteSpaceLeft < 1)
                await buf.Flush(async, cancellationToken);

            buf.WriteByte(LtreeProtocolVersion);
            await base.Write(value, buf, lengthCache, parameter, async, cancellationToken);
        }

        #endregion

        #region Read

        public override async ValueTask<string> Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        {
            await buf.Ensure(1, async);

            var version = buf.ReadByte();
            if (version != LtreeProtocolVersion)
                throw new NotSupportedException($"Don't know how to decode ltree with wire format {version}, your connection is now broken");

            return await base.Read(buf, len - 1, async, fieldDescription);
        }

        #endregion

        public override TextReader GetTextReader(Stream stream)
        {
            var version = stream.ReadByte();
            if (version != LtreeProtocolVersion)
                throw new EDBException($"Don't know how to decode ltree with wire format {version}, your connection is now broken");

            return base.GetTextReader(stream);
        }
    }
}
