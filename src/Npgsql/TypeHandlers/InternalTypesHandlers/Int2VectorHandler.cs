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

using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandlers.NumericHandlers;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;
using System;

namespace EnterpriseDB.EDBClient.TypeHandlers.InternalTypesHandlers
{
    [TypeMapping("int2vector", EDBDbType.Int2Vector)]
    class Int2VectorHandlerFactory : EDBTypeHandlerFactory
    {
        internal override EDBTypeHandler Create(PostgresType pgType, EDBConnection conn)
            => new Int2VectorHandler(conn.Connector.TypeMapper.DatabaseInfo.ByName["smallint"])
            {
                PostgresType = pgType
            };

        internal override Type DefaultValueType => null;
    }

    /// <summary>
    /// An int2vector is simply a regular array of shorts, with the sole exception that its lower bound must
    /// be 0 (we send 1 for regular arrays).
    /// </summary>
    class Int2VectorHandler : ArrayHandler<short>
    {
        public Int2VectorHandler(PostgresType postgresShortType)
            : base(new Int16Handler { PostgresType = postgresShortType }, 0) { }

        public override ArrayHandler CreateArrayHandler(PostgresType arrayBackendType)
            => new ArrayHandler<ArrayHandler<short>>(this) { PostgresType = arrayBackendType };
    }
}
