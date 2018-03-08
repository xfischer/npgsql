#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The EnterpriseDB.EDBClient Development Team
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

namespace EnterpriseDB.EDBClient.PostgresTypes
{
    /// <summary>
    /// Represents a PostgreSQL range data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/rangetypes.html.
    /// </remarks>
    public class PostgresRangeType : PostgresType
    {
        /// <summary>
        /// The PostgreSQL data type of the subtype of this range.
        /// </summary>
        [PublicAPI]
        public PostgresType Subtype { get; }

        /// <summary>
        /// Constructs a representation of a PostgreSQL range data type.
        /// </summary>
        protected internal PostgresRangeType(string ns, string name, uint oid, PostgresType subtypePostgresType)
            : base(ns, name, oid)
        {
            Subtype = subtypePostgresType;
            if (subtypePostgresType.EDBDbType.HasValue)
                EDBDbType = EDBTypes.EDBDbType.Range | subtypePostgresType.EDBDbType;
            Subtype.Range = this;
        }

        internal override TypeHandler Activate(TypeHandlerRegistry registry)
        {
            if (!registry.TryGetByOID(Subtype.OID, out var subtypeHandler))
            {
                // Subtype hasn't been set up yet, do it now
                subtypeHandler = Subtype.Activate(registry);
            }

            var handler = subtypeHandler.CreateRangeHandler(this);
            registry.ByOID[OID] = handler;
            if (EDBDbType.HasValue)
                registry.ByEDBDbType.Add(EDBDbType.Value, handler);
            registry.ByType[handler.GetFieldType()] = handler;
            return handler;
        }
    }
}
