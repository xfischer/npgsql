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

using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers
{
    /// <remarks>
    /// http://www.postgresql.org/docs/current/static/datatype-oid.html
    /// </remarks>
    [TypeMapping("oid", EDBDbType.Oid)]
    [TypeMapping("xid", EDBDbType.Xid)]
    [TypeMapping("cid", EDBDbType.Cid)]
    [TypeMapping("regtype", EDBDbType.Regtype)]
    [TypeMapping("regconfig", EDBDbType.Regconfig)]
    class UInt32Handler : EDBSimpleTypeHandler<uint>
    {
        public override uint Read(EDBReadBuffer buf, int len, FieldDescription fieldDescription = null)
            => (uint)buf.ReadInt32();

        public override int ValidateAndGetLength(uint value, EDBParameter parameter) => 4;

        public override void Write(uint value, EDBWriteBuffer buf, EDBParameter parameter)
            => buf.WriteInt32((int)value);
    }
}
