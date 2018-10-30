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

using JetBrains.Annotations;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;
using System;
using System.Data;
using System.Runtime.CompilerServices;

namespace EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-numeric.html
    /// </remarks>
    [TypeMapping("numeric", EDBDbType.Numeric, new[] { DbType.Decimal, DbType.VarNumeric }, typeof(decimal), DbType.Decimal)]
    class NumericHandler : EDBSimpleTypeHandler<decimal>,
        IEDBSimpleTypeHandler<byte>, IEDBSimpleTypeHandler<short>, IEDBSimpleTypeHandler<int>, IEDBSimpleTypeHandler<long>,
        IEDBSimpleTypeHandler<float>, IEDBSimpleTypeHandler<double>
    {
        const int MaxDecimalScale = 28;

        const int SignPositive = 0x0000;
        const int SignNegative = 0x4000;
        const int SignNan = 0xC000;

        const int MaxGroupCount = 8;
        const int MaxGroupScale = 4;

        static readonly uint MaxGroupSize = DecimalRaw.Powers10[MaxGroupScale];

        #region Read

        public override decimal Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
        {
            var result = new DecimalRaw();
            var groups = buf.ReadInt16();
            var weight = buf.ReadInt16() - groups + 1;
            var sign = buf.ReadUInt16();

            if (sign == SignNan)
                throw new EDBSafeReadException(new InvalidCastException("Numeric NaN not supported by System.Decimal"));
            else if (sign == SignNegative)
                DecimalRaw.Negate(ref result);

            var scale = buf.ReadInt16();
            if (scale > MaxDecimalScale)
                throw new EDBSafeReadException(new OverflowException("Numeric value does not fit in a System.Decimal"));

            result.Scale = scale;

            try
            {
                var scaleDifference = scale + weight * MaxGroupScale;
                if (groups == MaxGroupCount)
                {
                    while (groups-- > 1)
                    {
                        DecimalRaw.Multiply(ref result, MaxGroupSize);
                        DecimalRaw.Add(ref result, buf.ReadUInt16());
                    }

                    var group = buf.ReadUInt16();
                    var groupSize = DecimalRaw.Powers10[-scaleDifference];
                    if (group % groupSize != 0)
                        throw new EDBSafeReadException(new OverflowException("Numeric value does not fit in a System.Decimal"));

                    DecimalRaw.Multiply(ref result, MaxGroupSize / groupSize);
                    DecimalRaw.Add(ref result, group / groupSize);
                }
                else
                {
                    while (groups-- > 0)
                    {
                        DecimalRaw.Multiply(ref result, MaxGroupSize);
                        DecimalRaw.Add(ref result, buf.ReadUInt16());
                    }

                    if (scaleDifference < 0)
                        DecimalRaw.Divide(ref result, DecimalRaw.Powers10[-scaleDifference]);
                    else
                        while (scaleDifference > 0)
                        {
                            var scaleChunk = Math.Min(DecimalRaw.MaxUInt32Scale, scaleDifference);
                            DecimalRaw.Multiply(ref result, DecimalRaw.Powers10[scaleChunk]);
                            scaleDifference -= scaleChunk;
                        }
                }
            }
            catch (OverflowException e)
            {
                throw new EDBSafeReadException(e);
            }

            return Unsafe.As<DecimalRaw, decimal>(ref result);
        }

        byte IEDBSimpleTypeHandler<byte>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => (byte)Read(buf, len, fieldDescription);

        short IEDBSimpleTypeHandler<short>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => (short)Read(buf, len, fieldDescription);

        int IEDBSimpleTypeHandler<int>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => (int)Read(buf, len, fieldDescription);

        long IEDBSimpleTypeHandler<long>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => (long)Read(buf, len, fieldDescription);

        float IEDBSimpleTypeHandler<float>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => (float)Read(buf, len, fieldDescription);

        double IEDBSimpleTypeHandler<double>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => (double)Read(buf, len, fieldDescription);

        #endregion Read

        #region Write

        public override int ValidateAndGetLength(decimal value, EDBParameter parameter)
        {
            var groupCount = 0;
            var raw = Unsafe.As<decimal, DecimalRaw>(ref value);
            if (raw.Low != 0 || raw.Mid != 0 || raw.High != 0)
            {
                uint remainder = default;
                var scaleChunk = raw.Scale % MaxGroupScale;
                if (scaleChunk > 0)
                {
                    var divisor = DecimalRaw.Powers10[scaleChunk];
                    var multiplier = DecimalRaw.Powers10[MaxGroupScale - scaleChunk];
                    remainder = DecimalRaw.Divide(ref raw, divisor) * multiplier;
                }

                while (remainder == 0)
                    remainder = DecimalRaw.Divide(ref raw, MaxGroupSize);

                groupCount++;

                while (raw.Low != 0 || raw.Mid != 0 || raw.High != 0)
                {
                    DecimalRaw.Divide(ref raw, MaxGroupSize);
                    groupCount++;
                }
            }

            return 4 * sizeof(short) + groupCount * sizeof(short);
        }

        public int ValidateAndGetLength(short value, EDBParameter parameter)
            => ValidateAndGetLength((decimal)value, parameter);

        public int ValidateAndGetLength(int value, EDBParameter parameter)
            => ValidateAndGetLength((decimal)value, parameter);

        public int ValidateAndGetLength(long value, EDBParameter parameter)
            => ValidateAndGetLength((decimal)value, parameter);

        public int ValidateAndGetLength(float value, EDBParameter parameter)
            => ValidateAndGetLength((decimal)value, parameter);

        public int ValidateAndGetLength(double value, EDBParameter parameter)
            => ValidateAndGetLength((decimal)value, parameter);

        public int ValidateAndGetLength(byte value, EDBParameter parameter)
            => ValidateAndGetLength((decimal)value, parameter);

        public override unsafe void Write(decimal value, EDBWriteBuffer buf, EDBParameter parameter)
        {
            var groupCount = 0;
            var groups = stackalloc short[MaxGroupCount];
            var weight = 0;

            var raw = Unsafe.As<decimal, DecimalRaw>(ref value);
            if (raw.Low != 0 || raw.Mid != 0 || raw.High != 0)
            {
                var scale = raw.Scale;
                weight = -scale / MaxGroupScale - 1;

                uint remainder = default;
                var scaleChunk = scale % MaxGroupScale;
                if (scaleChunk > 0)
                {
                    var divisor = DecimalRaw.Powers10[scaleChunk];
                    var multiplier = DecimalRaw.Powers10[MaxGroupScale - scaleChunk];
                    remainder = DecimalRaw.Divide(ref raw, divisor) * multiplier;

                    if (remainder != 0)
                    {
                        weight--;
                        goto WriteGroups;
                    }
                }

                while ((remainder = DecimalRaw.Divide(ref raw, MaxGroupSize)) == 0)
                    weight++;

                WriteGroups:
                groups[groupCount++] = (short)remainder;

                while (raw.Low != 0 || raw.Mid != 0 || raw.High != 0)
                    groups[groupCount++] = (short)DecimalRaw.Divide(ref raw, MaxGroupSize);
            }

            buf.WriteInt16(groupCount);
            buf.WriteInt16(groupCount + weight);
            buf.WriteInt16(raw.Negative ? SignNegative : SignPositive);
            buf.WriteInt16(raw.Scale);

            while (groupCount > 0)
                buf.WriteInt16(groups[--groupCount]);
        }

        public void Write(short value, EDBWriteBuffer buf, EDBParameter parameter)
            => Write((decimal)value, buf, parameter);

        public void Write(int value, EDBWriteBuffer buf, EDBParameter parameter)
            => Write((decimal)value, buf, parameter);

        public void Write(long value, EDBWriteBuffer buf, EDBParameter parameter)
            => Write((decimal)value, buf, parameter);

        public void Write(byte value, EDBWriteBuffer buf, EDBParameter parameter)
            => Write((decimal)value, buf, parameter);

        public void Write(float value, EDBWriteBuffer buf, EDBParameter parameter)
            => Write((decimal)value, buf, parameter);

        public void Write(double value, EDBWriteBuffer buf, EDBParameter parameter)
            => Write((decimal)value, buf, parameter);

        #endregion Write
    }
}
