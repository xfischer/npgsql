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
    [TypeMapping("int4", EDBDbType.Integer, DbType.Int32, typeof(int))]
    internal class Int32Handler : TypeHandler<int>,
        ISimpleTypeReader<int>, ISimpleTypeWriter,
        ISimpleTypeReader<byte>, ISimpleTypeReader<short>, ISimpleTypeReader<long>,
        ISimpleTypeReader<float>, ISimpleTypeReader<double>, ISimpleTypeReader<decimal>,
        ISimpleTypeReader<string>
    {
        public int Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return buf.ReadInt32();
        }

        byte ISimpleTypeReader<byte>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return (byte)Read(buf, len, fieldDescription);
        }

        short ISimpleTypeReader<short>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return (short)Read(buf, len, fieldDescription);
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
            if (!(value is int))
            {
                var converted = Convert.ToInt32(value);
                if (parameter == null) {
                    throw CreateConversionButNoParamException(value.GetType());
                }
                parameter.ConvertedValue = converted;
            }
            return 4;
        }

        public void Write(object value, EDBBuffer buf, EDBParameter parameter)
        {
            if (parameter != null && parameter.ConvertedValue != null) {
                value = parameter.ConvertedValue;
            }
            buf.WriteInt32((int)value);
        }
    }
}
