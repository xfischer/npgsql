#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using System;
using EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;
using System.Diagnostics;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-numeric.html
    /// </remarks>
    [TypeMapping("integer", EDBDbType.Integer, DbType.Int32, typeof(int))]
    class Int32Handler : EDBSimpleTypeHandler<int>,
        IEDBSimpleTypeHandler<byte>, IEDBSimpleTypeHandler<short>, IEDBSimpleTypeHandler<long>,
        IEDBSimpleTypeHandler<float>, IEDBSimpleTypeHandler<double>, IEDBSimpleTypeHandler<decimal>
    {
        #region Read

        public override int Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
            => buf.ReadInt32();

        byte IEDBSimpleTypeHandler<byte>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => checked((byte)Read(buf, len, fieldDescription));

        short IEDBSimpleTypeHandler<short>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => checked((short)Read(buf, len, fieldDescription));

        long IEDBSimpleTypeHandler<long>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => Read(buf, len, fieldDescription);

        float IEDBSimpleTypeHandler<float>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => Read(buf, len, fieldDescription);

        double IEDBSimpleTypeHandler<double>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => Read(buf, len, fieldDescription);

        decimal IEDBSimpleTypeHandler<decimal>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => Read(buf, len, fieldDescription);

        #endregion Read

        #region Write

        public override int ValidateAndGetLength(int value, EDBParameter parameter) => 4;
        public int ValidateAndGetLength(short value, EDBParameter parameter)        => 4;
        public int ValidateAndGetLength(long value, EDBParameter parameter)         => 4;
        public int ValidateAndGetLength(float value, EDBParameter parameter)        => 4;
        public int ValidateAndGetLength(double value, EDBParameter parameter)       => 4;
        public int ValidateAndGetLength(decimal value, EDBParameter parameter)      => 4;
        public int ValidateAndGetLength(byte value, EDBParameter parameter)         => 4;

        public override void Write(int value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteInt32(value);
        public void Write(short value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteInt32(value);
        public void Write(long value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteInt32(checked((int)value));
        public void Write(byte value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteInt32(value);
        public void Write(float value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteInt32(checked((int)value));
        public void Write(double value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteInt32(checked((int)value));
        public void Write(decimal value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteInt32((int)value);

        #endregion Write
    }
}
