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
using System.Linq;
using System.Text;
using  EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System.Data;

namespace  EnterpriseDB.EDBClient.TypeHandlers
{
    /// <summary>
    /// Type handler for the Postgresql "char" type, used only internally
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-character.html
    /// </remarks>
    [TypeMapping("char", EDBDbType.InternalChar)]
    internal class InternalCharHandler : TypeHandler<char>,
        ISimpleTypeReader<char>, ISimpleTypeWriter,
        ISimpleTypeReader<byte>, ISimpleTypeReader<short>, ISimpleTypeReader<int>, ISimpleTypeReader<long>
    {
        #region Read

        public char Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return (char)buf.ReadByte();
        }

        byte ISimpleTypeReader<byte>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return buf.ReadByte();
        }

        short ISimpleTypeReader<short>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return buf.ReadByte();
        }

        int ISimpleTypeReader<int>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return buf.ReadByte();
        }

        long ISimpleTypeReader<long>.Read(EDBBuffer buf, int len, FieldDescription fieldDescription)
        {
            return buf.ReadByte();
        }

        #endregion

        #region Write

        public int ValidateAndGetLength(object value, EDBParameter parameter)
        {
            if (!(value is byte))
            {
                // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
                Convert.ToByte(value);
            }
            return 1;
        }

        public void Write(object value, EDBBuffer buf, EDBParameter parameter)
        {
            buf.WriteByte(value as byte? ?? Convert.ToByte(value));
        }

        #endregion
    }
}
