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
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.TypeHandlers
{
    /// <summary>
    /// Type handler for the Postgresql "char" type, used only internally
    /// </summary>
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-character.html
    /// </remarks>
    [TypeMapping("char", EDBDbType.InternalChar)]
    class InternalCharHandler : EDBSimpleTypeHandler<char>,
        IEDBSimpleTypeHandler<byte>, IEDBSimpleTypeHandler<short>, IEDBSimpleTypeHandler<int>, IEDBSimpleTypeHandler<long>
    {
        #region Read

        public override char Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
            => (char)buf.ReadByte();

        byte IEDBSimpleTypeHandler<byte>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => buf.ReadByte();

        short IEDBSimpleTypeHandler<short>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => buf.ReadByte();

        int IEDBSimpleTypeHandler<int>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => buf.ReadByte();

        long IEDBSimpleTypeHandler<long>.Read(EDBReadBuffer buf, int len, [CanBeNull] FieldDescription fieldDescription)
            => buf.ReadByte();

        #endregion

        #region Write

        public override int ValidateAndGetLength(char value, EDBParameter parameter) => 1;
        public int ValidateAndGetLength(byte value, EDBParameter parameter)          => 1;
        public int ValidateAndGetLength(short value, EDBParameter parameter)         => 1;
        public int ValidateAndGetLength(int value, EDBParameter parameter)           => 1;
        public int ValidateAndGetLength(long value, EDBParameter parameter)          => 1;

        public override void Write(char value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteByte((byte)value);

        public void Write(byte value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteByte(value);

        public void Write(short value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteByte((byte)value);

        public void Write(int value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteByte((byte)value);

        public void Write(long value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteByte((byte)value);

        #endregion
    }
}
