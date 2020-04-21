using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers
{
    /// <summary>
    /// A factory for type handlers for the PostgreSQL hstore extension data type, which stores sets of key/value pairs
    /// within a single PostgreSQL value.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/hstore.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    [TypeMapping("hstore", EDBDbType.Hstore, new[] { typeof(Dictionary<string, string?>), typeof(IDictionary<string, string?>) })]
    public class HstoreHandlerFactory : EDBTypeHandlerFactory<Dictionary<string, string?>>
    {
        /// <inheritdoc />
        public override EDBTypeHandler<Dictionary<string, string?>> Create(PostgresType postgresType, EDBConnection conn)
            => new HstoreHandler(postgresType, conn);
    }

    /// <summary>
    /// A type handler for the PostgreSQL hstore extension data type, which stores sets of key/value pairs within a
    /// single PostgreSQL value.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/hstore.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and  may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
#pragma warning disable CA1061 // Do not hide base class methods
    public class HstoreHandler : EDBTypeHandler<Dictionary<string, string?>>, IEDBTypeHandler<IDictionary<string, string?>>
    {
        /// <summary>
        /// The text handler to which we delegate encoding/decoding of the actual strings
        /// </summary>
        readonly TextHandler _textHandler;

        internal HstoreHandler(PostgresType postgresType, EDBConnection connection)
            : base(postgresType) => _textHandler = new TextHandler(postgresType, connection);

        #region Write

        /// <inheritdoc />
        public int ValidateAndGetLength(IDictionary<string, string?> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        {
            if (lengthCache == null)
                lengthCache = new EDBLengthCache(1);
            if (lengthCache.IsPopulated)
                return lengthCache.Get();

            // Leave empty slot for the entire hstore length, and go ahead an populate the individual string slots
            var pos = lengthCache.Position;
            lengthCache.Set(0);

            var totalLen = 4;  // Number of key-value pairs
            foreach (var kv in value)
            {
                totalLen += 8;   // Key length + value length
                if (kv.Key == null)
                    throw new FormatException("HSTORE doesn't support null keys");
                totalLen += _textHandler.ValidateAndGetLength(kv.Key, ref lengthCache, null);
                if (kv.Value != null)
                    totalLen += _textHandler.ValidateAndGetLength(kv.Value!, ref lengthCache, null);
            }

            return lengthCache.Lengths[pos] = totalLen;
        }

        /// <inheritdoc />
        public override int ValidateAndGetLength(Dictionary<string, string?> value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength(value, ref lengthCache, parameter);

        /// <inheritdoc />
        public async Task Write(IDictionary<string, string?> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async)
        {
            if (buf.WriteSpaceLeft < 4)
                await buf.Flush(async);
            buf.WriteInt32(value.Count);
            if (value.Count == 0)
                return;

            foreach (var kv in value)
            {
                await _textHandler.WriteWithLengthInternal(kv.Key, buf, lengthCache, parameter, async);
                await _textHandler.WriteWithLengthInternal(kv.Value, buf, lengthCache, parameter, async);
            }
        }

        /// <inheritdoc />
        public override Task Write(Dictionary<string, string?> value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async)
            => Write(value, buf, lengthCache, parameter, async);

        #endregion

        #region Read

        /// <inheritdoc />
        public override async ValueTask<Dictionary<string, string?>> Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        {
            await buf.Ensure(4, async);
            var numElements = buf.ReadInt32();
            var hstore = new Dictionary<string, string?>(numElements);

            for (var i = 0; i < numElements; i++)
            {
                await buf.Ensure(4, async);
                var keyLen = buf.ReadInt32();
                Debug.Assert(keyLen != -1);
                var key = await _textHandler.Read(buf, keyLen, async);

                await buf.Ensure(4, async);
                var valueLen = buf.ReadInt32();

                hstore[key] = valueLen == -1
                    ? null
                    : await _textHandler.Read(buf, valueLen, async);
            }
            return hstore;
        }

        ValueTask<IDictionary<string, string?>> IEDBTypeHandler<IDictionary<string, string?>>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => new ValueTask<IDictionary<string, string?>>(Read(buf, len, async, fieldDescription).Result);

        #endregion
    }
#pragma warning restore CA1061 // Do not hide base class methods
}
