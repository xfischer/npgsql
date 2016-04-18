#region License
// The PostgreSQL License
//
// Copyright (C) 2015 The  EnterpriseDB.EDBClient Development Team
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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using  EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;
using System.Diagnostics.Contracts;

namespace  EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-numeric.html
    /// </remarks>
    [TypeMapping("int2", EDBDbType.Smallint, new[] { DbType.Int16, DbType.Byte }, new[] { typeof(short), typeof(byte) }, DbType.Int16)]
    internal class Int16Handler : TypeHandler<short>,
        ISimpleTypeReader<short>, ISimpleTypeWriter,
        ISimpleTypeReader<byte>, ISimpleTypeReader<int>, ISimpleTypeReader<long>,
        ISimpleTypeReader<float>, ISimpleTypeReader<double>, ISimpleTypeReader<decimal>,
        ISimpleTypeReader<string>
    {
        public short Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return buf.ReadInt16();
        }

        byte ISimpleTypeReader<byte>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return (byte)Read(buf, len, fieldDescription);
        }

        int ISimpleTypeReader<int>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return Read(buf, len, fieldDescription);
        }

        long ISimpleTypeReader<long>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return Read(buf, len, fieldDescription);
        }

        float ISimpleTypeReader<float>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return Read(buf, len, fieldDescription);
        }

        double ISimpleTypeReader<double>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return Read(buf, len, fieldDescription);
        }

        decimal ISimpleTypeReader<decimal>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return Read(buf, len, fieldDescription);
        }

        string ISimpleTypeReader<string>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return Read(buf, len, fieldDescription).ToString();
        }

        public int ValidateAndGetLength(object value, EDBParameter parameter)
        {
            if (!(value is short))
            {
                var converted = Convert.ToInt16(value);
                if (parameter == null)
                {
                    throw CreateConversionButNoParamException(value.GetType());
                }
                parameter.ConvertedValue = converted;
            }
            return 2;
        }

        public void Write(object value, EDBBuffer buf, EDBParameter parameter)
        {
            if (parameter != null && parameter.ConvertedValue != null) {
                value = parameter.ConvertedValue;
            }
            buf.WriteInt16((short)value);
        }
    }
}
