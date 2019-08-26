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

using System;
using System.Data;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeMapping
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [MeansImplicitUse]
    class TypeMappingAttribute : Attribute
    {
        /// <summary>
        /// Maps an EDB type handler to a PostgreSQL type.
        /// </summary>
        /// <param name="pgName">A PostgreSQL type name as it appears in the pg_type table.</param>
        /// <param name="eDBDbType">
        /// A member of <see cref="EDBDbType"/> which represents this PostgreSQL type.
        /// An <see cref="EDBParameter"/> with <see cref="EDBParameter.EDBDbType"/> set to
        /// this value will be sent with the type handler mapped by this attribute.
        /// </param>
        /// <param name="dbTypes">
        /// All members of <see cref="DbType"/> which represent this PostgreSQL type.
        /// An <see cref="EDBParameter"/> with <see cref="EDBParameter.DbType"/> set to
        /// one of these values will be sent with the type handler mapped by this attribute.
        /// </param>
        /// <param name="clrTypes">
        /// Any .NET type which corresponds to this PostgreSQL type.
        /// An <see cref="EDBParameter"/> with <see cref="EDBParameter.Value"/> set to
        /// one of these values will be sent with the type handler mapped by this attribute.
        /// </param>
        /// <param name="inferredDbType">
        /// The "primary" <see cref="DbType"/> which best corresponds to this PostgreSQL type.
        /// When <see cref="EDBParameter.EDBDbType"/> or <see cref="EDBParameter.Value"/>
        /// set, <see cref="EDBParameter.DbType"/> will be set to this value.
        /// </param>
        internal TypeMappingAttribute(string pgName, EDBDbType? eDBDbType, [CanBeNull] DbType[] dbTypes, [CanBeNull] Type[] clrTypes, DbType? inferredDbType)
        {
            if (String.IsNullOrWhiteSpace(pgName))
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            PgName = pgName;
            EDBDbType = eDBDbType;
            DbTypes = dbTypes ?? new DbType[0];
            ClrTypes = clrTypes ?? new Type[0];
            InferredDbType = inferredDbType;
        }

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, DbType[] dbTypes, [CanBeNull] Type[] clrTypes, DbType inferredDbType)
            : this(pgName, (EDBDbType?)EDBDbType, dbTypes, clrTypes, inferredDbType)
        { }

        //internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, DbType[] dbTypes=null, Type type=null)
        //    : this(pgName, EDBDbType, dbTypes, type == null ? null : new[] { type }) {}

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType)
            : this(pgName, EDBDbType, new DbType[0], new Type[0], null)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, DbType inferredDbType)
            : this(pgName, EDBDbType, new DbType[0], new Type[0], inferredDbType)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, DbType[] dbTypes, Type clrType, DbType inferredDbType)
            : this(pgName, EDBDbType, dbTypes, new[] { clrType }, inferredDbType)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, DbType[] dbTypes)
            : this(pgName, EDBDbType, dbTypes, new Type[0], null)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, DbType dbType, Type[] clrTypes)
            : this(pgName, EDBDbType, new[] { dbType }, clrTypes, dbType)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, DbType dbType, Type clrType = null)
            : this(pgName, EDBDbType, new[] { dbType }, clrType == null ? null : new[] { clrType }, dbType)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, Type[] clrTypes, DbType inferredDbType)
            : this(pgName, EDBDbType, new DbType[0], clrTypes, inferredDbType)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, Type[] clrTypes)
            : this(pgName, EDBDbType, new DbType[0], clrTypes, null)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, Type clrType, DbType inferredDbType)
            : this(pgName, EDBDbType, new DbType[0], new[] { clrType }, inferredDbType)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, Type clrType)
            : this(pgName, EDBDbType, new DbType[0], new[] { clrType }, null)
        { }

        /// <summary>
        /// Read-only parameter
        /// </summary>
        internal TypeMappingAttribute(string pgName)
            : this(pgName, null, null, null, null)
        { }

        internal string PgName { get; }
        internal EDBDbType? EDBDbType { get; }
        internal DbType[] DbTypes { get; }
        internal Type[] ClrTypes { get; }
        internal DbType? InferredDbType { get; }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("[{0} EDBDbType={1}", PgName, EDBDbType);
            if (DbTypes.Length > 0)
            {
                sb.Append(" DbTypes=");
                sb.Append(String.Join(",", DbTypes.Select(t => t.ToString())));
            }
            if (ClrTypes.Length > 0)
            {
                sb.Append(" ClrTypes=");
                sb.Append(String.Join(",", ClrTypes.Select(t => t.Name)));
            }
            sb.AppendFormat("]");
            return sb.ToString();
        }
    }
}
