#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EDB Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EDB DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EDB DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EDB DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EDB DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using System.IO;
using EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.TypeHandlers
{
    [TypeMapping("text", EDBDbType.Text,
        new[] { DbType.String, DbType.StringFixedLength, DbType.AnsiString, DbType.AnsiStringFixedLength },
        new[] { typeof(string), typeof(char[]), typeof(char), typeof(ArraySegment<char>) },
        DbType.String
    )]
    [TypeMapping("xml", EDBDbType.Xml, dbType: DbType.Xml)]

    [TypeMapping("character varying", EDBDbType.Varchar, inferredDbType: DbType.String)]
    [TypeMapping("character", EDBDbType.Char, inferredDbType: DbType.String)]
    [TypeMapping("name", EDBDbType.Name, inferredDbType: DbType.String)]
    [TypeMapping("json", EDBDbType.Json, inferredDbType: DbType.String)]
    [TypeMapping("refcursor", EDBDbType.Refcursor, inferredDbType: DbType.String)]
    [TypeMapping("citext", EDBDbType.Citext, inferredDbType: DbType.String)]
    [TypeMapping("unknown")]
    public class TextHandlerFactory : EDBTypeHandlerFactory<string>
    {
        protected override EDBTypeHandler<string> Create(EDBConnection conn)
            => new TextHandler(conn);
    }

    public class TextHandler : EDBTypeHandler<string>, IEDBTypeHandler<char[]>, IEDBTypeHandler<ArraySegment<char>>,
        IEDBTypeHandler<char>, IEDBTypeHandler<byte[]>, ITextReaderHandler
    {
        // Text types are handled a bit more efficiently when sent as text than as binary
        // see https://github.com/EDB/EDB/issues/1210#issuecomment-235641670
        internal override bool PreferTextWrite => true;

        readonly Encoding _encoding;

        #region State

        readonly char[] _singleCharArray = new char[1];

        #endregion

        protected internal TextHandler(EDBConnection connection)
            => _encoding = connection.Connector.TextEncoding;

        #region Read

        public override ValueTask<string> Read(EDBReadBuffer buf, int byteLen, bool async, FieldDescription fieldDescription = null)
        {
            return buf.ReadBytesLeft >= byteLen
                ? new ValueTask<string>(buf.ReadString(byteLen))
                : ReadLong();

            async ValueTask<string> ReadLong()
            {
                if (byteLen <= buf.Size)
                {
                    // The string's byte representation can fit in our read buffer, read it.
                    while (buf.ReadBytesLeft < byteLen)
                        await buf.ReadMore(async);
                    return buf.ReadString(byteLen);
                }

                // Bad case: the string's byte representation doesn't fit in our buffer.
                // This is rare - will only happen in CommandBehavior.Sequential mode (otherwise the
                // entire row is in memory). Tweaking the buffer length via the connection string can
                // help avoid this.

                // Allocate a temporary byte buffer to hold the entire string and read it in chunks.
                var tempBuf = new byte[byteLen];
                var pos = 0;
                while (true)
                {
                    var len = Math.Min(buf.ReadBytesLeft, byteLen - pos);
                    buf.ReadBytes(tempBuf, pos, len);
                    pos += len;
                    if (pos < byteLen)
                    {
                        await buf.ReadMore(async);
                        continue;
                    }
                    break;
                }
                return buf.TextEncoding.GetString(tempBuf);
            }
        }

        async ValueTask<char[]> IEDBTypeHandler<char[]>.Read(EDBReadBuffer buf, int byteLen, bool async, FieldDescription fieldDescription)
        {
            if (byteLen <= buf.Size)
            {
                // The string's byte representation can fit in our read buffer, read it.
                while (buf.ReadBytesLeft < byteLen)
                    await buf.ReadMore(async);
                return buf.ReadChars(byteLen);
            }

            // TODO: The following can be optimized with Decoder - no need to allocate a byte[]
            var tempBuf = new byte[byteLen];
            var pos = 0;
            while (true)
            {
                var len = Math.Min(buf.ReadBytesLeft, byteLen - pos);
                buf.ReadBytes(tempBuf, pos, len);
                pos += len;
                if (pos < byteLen)
                {
                    await buf.ReadMore(async);
                    continue;
                }
                break;
            }
            return buf.TextEncoding.GetChars(tempBuf);
        }

        async ValueTask<char> IEDBTypeHandler<char>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
        {
            // Make sure we have enough bytes in the buffer for a single character
            var maxBytes = Math.Min(buf.TextEncoding.GetMaxByteCount(1), len);
            while (buf.ReadBytesLeft < maxBytes)
                await buf.ReadMore(async);

            var decoder = buf.TextEncoding.GetDecoder();
            decoder.Convert(buf.Buffer, buf.ReadPosition, maxBytes, _singleCharArray, 0, 1, true, out var bytesUsed, out var charsUsed, out var completed);
            buf.Skip(len - bytesUsed);

            if (charsUsed < 1)
                throw new EDBSafeReadException(new EDBException("Could not read char - string was empty"));

            return _singleCharArray[0];
        }

        ValueTask<ArraySegment<char>> IEDBTypeHandler<ArraySegment<char>>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
        {
            buf.Skip(len);
            throw new EDBSafeReadException(new NotSupportedException("Only writing ArraySegment<char> to PostgreSQL text is supported, no reading."));
        }

        ValueTask<byte[]> IEDBTypeHandler<byte[]>.Read(EDBReadBuffer buf, int byteLen, bool async, FieldDescription fieldDescription)
        {
            var bytes = new byte[byteLen];
            if (buf.ReadBytesLeft >= byteLen)
            {
                buf.ReadBytes(bytes, 0, byteLen);
                return new ValueTask<byte[]>(bytes);
            }
            return ReadLong();

            async ValueTask<byte[]> ReadLong()
            {
                if (byteLen <= buf.Size)
                {
                    // The bytes can fit in our read buffer, read it.
                    while (buf.ReadBytesLeft < byteLen)
                        await buf.ReadMore(async);
                    buf.ReadBytes(bytes, 0, byteLen);
                    return bytes;
                }

                // Bad case: the bytes don't fit in our buffer.
                // This is rare - will only happen in CommandBehavior.Sequential mode (otherwise the
                // entire row is in memory). Tweaking the buffer length via the connection string can
                // help avoid this.

                var pos = 0;
                while (true)
                {
                    var len = Math.Min(buf.ReadBytesLeft, byteLen - pos);
                    buf.ReadBytes(bytes, pos, len);
                    pos += len;
                    if (pos < byteLen)
                    {
                        await buf.ReadMore(async);
                        continue;
                    }
                    break;
                }
                return bytes;
            }
        }

        #endregion

        #region Write

        public override unsafe int ValidateAndGetLength(string value, ref EDBLengthCache lengthCache, EDBParameter parameter)
        {
            if (lengthCache == null)
                lengthCache = new EDBLengthCache(1);
            if (lengthCache.IsPopulated)
                return lengthCache.Get();

            if (parameter == null || parameter.Size <= 0 || parameter.Size >= value.Length)
                return lengthCache.Set(_encoding.GetByteCount(value));
            fixed (char* p = value)
                return lengthCache.Set(_encoding.GetByteCount(p, parameter.Size));
        }

        public virtual int ValidateAndGetLength(char[] value, ref EDBLengthCache lengthCache, EDBParameter parameter)
        {
            if (lengthCache == null)
                lengthCache = new EDBLengthCache(1);
            if (lengthCache.IsPopulated)
                return lengthCache.Get();

            return lengthCache.Set(
                parameter == null || parameter.Size <= 0 || parameter.Size >= value.Length
                    ? _encoding.GetByteCount(value)
                    : _encoding.GetByteCount(value, 0, parameter.Size)
            );
        }

        public virtual int ValidateAndGetLength(ArraySegment<char> value, ref EDBLengthCache lengthCache, EDBParameter parameter)
        {
            if (lengthCache == null)
                lengthCache = new EDBLengthCache(1);
            if (lengthCache.IsPopulated)
                return lengthCache.Get();

            if (parameter?.Size > 0)
                throw new ArgumentException($"Parameter {parameter.ParameterName} is of type ArraySegment<char> and should not have its Size set", parameter.ParameterName);

            return lengthCache.Set(_encoding.GetByteCount(value.Array, value.Offset, value.Count));
        }

        public int ValidateAndGetLength(char value, ref EDBLengthCache lengthCache, EDBParameter parameter)
        {
            _singleCharArray[0] = value;
            return _encoding.GetByteCount(_singleCharArray);
        }

        public int ValidateAndGetLength(byte[] value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => value.Length;

        public override Task Write(string value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => WriteString(value, buf, lengthCache, parameter, async);

        public virtual Task Write(char[] value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
        {
            var charLen = parameter == null || parameter.Size <= 0 || parameter.Size >= value.Length
                ? value.Length
                : parameter.Size;
            return buf.WriteChars(value, 0, charLen, lengthCache.GetLast(), async);
        }

        public virtual Task Write(ArraySegment<char> value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async) => 
            buf.WriteChars(value.Array, value.Offset, value.Count, lengthCache.GetLast(), async);

        Task WriteString(string str, EDBWriteBuffer buf, EDBLengthCache lengthCache, [CanBeNull] EDBParameter parameter, bool async)
        {
            var charLen = parameter == null || parameter.Size <= 0 || parameter.Size >= str.Length
                ? str.Length
                : parameter.Size;
            return buf.WriteString(str, charLen, lengthCache.GetLast(), async);
        }

        public Task Write(char value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
        {
            _singleCharArray[0] = value;
            var len = _encoding.GetByteCount(_singleCharArray);
            return buf.WriteChars(_singleCharArray, 0, 1, len, async);
        }

        public Task Write(byte[] value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => buf.WriteBytesRaw(value, async);

        #endregion

#pragma warning disable CA2119 // Seal methods that satisfy private interfaces
        public virtual TextReader GetTextReader(Stream stream) => new StreamReader(stream);
#pragma warning restore CA2119 // Seal methods that satisfy private interfaces
    }
}
