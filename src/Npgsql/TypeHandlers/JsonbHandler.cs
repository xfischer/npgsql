#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The  EnterpriseDB.EDBClient DEVELOPMENT Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

namespace  EnterpriseDB.EDBClient.TypeHandlers
{
    /// <summary>
    /// JSONB binary encoding is a simple UTF8 string, but prepended with a version number.
    /// </summary>
    [TypeMapping("jsonb", EDBDbType.Jsonb)]
    class JsonbHandler : TextHandler
    {
        /// <summary>
        /// Prepended to the string in the wire encoding
        /// </summary>
        const byte JsonbProtocolVersion = 1;

        internal override bool PreferTextWrite => false;

        internal JsonbHandler(PostgresType postgresType, TypeHandlerRegistry registry) : base(postgresType, registry) {}

        #region Write

        public override int ValidateAndGetLength(object value, ref LengthCache lengthCache, EDBParameter parameter=null)
        {
            if (lengthCache == null)
                lengthCache = new LengthCache(1);
            if (lengthCache.IsPopulated)
                return lengthCache.Get() + 1;

            // Add one byte for the prepended version number
            return base.ValidateAndGetLength(value, ref lengthCache, parameter) + 1;
        }

        protected override async Task Write(object value, WriteBuffer buf, LengthCache lengthCache, EDBParameter parameter,
            bool async, CancellationToken cancellationToken)
        {
            if (buf.WriteSpaceLeft < 1)
                await buf.Flush(async, cancellationToken);
            buf.WriteByte(JsonbProtocolVersion);
            await base.Write(value, buf, lengthCache, parameter, async, cancellationToken);
        }

        #endregion

        #region Read

        public override async ValueTask<string> Read(ReadBuffer buf, int len, bool async, FieldDescription fieldDescription = null)
        {
            await buf.Ensure(1, async);
            var version = buf.ReadByte();
            if (version != JsonbProtocolVersion)
                throw new NotSupportedException($"Don't know how to decode JSONB with wire format {version}, your connection is now broken");

            return await base.Read(buf, len - 1, async, fieldDescription);
        }

        #endregion

        public override TextReader GetTextReader(Stream stream)
        {
            var version = stream.ReadByte();
            if (version != JsonbProtocolVersion)
                throw new EDBException($"Don't know how to decode jsonb with wire format {version}, your connection is now broken");

            return base.GetTextReader(stream);
        }
    }
}
