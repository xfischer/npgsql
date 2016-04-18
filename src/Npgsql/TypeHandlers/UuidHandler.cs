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
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using  EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;

namespace  EnterpriseDB.EDBClient.TypeHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-uuid.html
    /// </remarks>
    [TypeMapping("uuid", EDBDbType.Uuid, DbType.Guid, typeof(Guid))]
    internal class UuidHandler : TypeHandler<Guid>,
        ISimpleTypeReader<Guid>, ISimpleTypeWriter,
        ISimpleTypeReader<string>
    {
        public Guid Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            buf.Ensure(16);
            var a = buf.ReadInt32();
            var b = buf.ReadInt16();
            var c = buf.ReadInt16();
            var d = new byte[8];
            buf.ReadBytes(d, 0, 8);
            return new Guid(a, b, c, d);
        }

        string ISimpleTypeReader<string>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return Read(buf, len, fieldDescription).ToString();
        }

        #region Write

        public int ValidateAndGetLength(object value, EDBParameter parameter)
        {
            var asString = value as string;
            if (value is string)
            {
                var converted = Guid.Parse(asString);
                if (parameter == null)
                {
                    throw CreateConversionButNoParamException(value.GetType());
                }
                parameter.ConvertedValue = converted;
            }
            else if (!(value is Guid))
            {
                throw CreateConversionException(value.GetType());
            }
            return 16;
        }

        public void Write(object value, EDBBuffer buf, EDBParameter parameter)
        {
            if (parameter != null && parameter.ConvertedValue != null) {
                value = parameter.ConvertedValue;
            }

            var bytes = ((Guid)value).ToByteArray();

            buf.WriteInt32(BitConverter.ToInt32(bytes, 0));
            buf.WriteInt16(BitConverter.ToInt16(bytes, 4));
            buf.WriteInt16(BitConverter.ToInt16(bytes, 6));
            buf.WriteBytes(bytes, 8, 8);
        }

        #endregion
    }
}
