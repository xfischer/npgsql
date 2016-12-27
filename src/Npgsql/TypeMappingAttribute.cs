using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using EDBTypes;

namespace  EnterpriseDB.EDBClient
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    [MeansImplicitUse]
    internal class TypeMappingAttribute : Attribute
    {
        /// <summary>
        /// Maps an  EnterpriseDB.EDBClient type handler to a PostgreSQL type.
        /// </summary>
        /// <param name="pgName">A PostgreSQL type name as it appears in the pg_type table.</param>
        /// <param name="npgsqlDbType">
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
        internal TypeMappingAttribute(string pgName, EDBDbType? npgsqlDbType, [CanBeNull] DbType[] dbTypes, [CanBeNull] Type[] clrTypes, DbType? inferredDbType)
        {
            if (String.IsNullOrWhiteSpace(pgName))
                throw new ArgumentException("pgName can't be empty", nameof(pgName));
            Contract.EndContractBlock();

            PgName = pgName;
            EDBDbType = npgsqlDbType;
            DbTypes = dbTypes ?? new DbType[0];
            ClrTypes = clrTypes ?? new Type[0];
            InferredDbType = inferredDbType;
        }

        internal TypeMappingAttribute(string pgName, EDBDbType npgsqlDbType, DbType[] dbTypes, [CanBeNull] Type[] clrTypes, DbType inferredDbType)
            : this(pgName, (EDBDbType?)npgsqlDbType, dbTypes, clrTypes, inferredDbType)
        { }

        //internal TypeMappingAttribute(string pgName, EDBDbType npgsqlDbType, DbType[] dbTypes=null, Type type=null)
        //    : this(pgName, npgsqlDbType, dbTypes, type == null ? null : new[] { type }) {}

        internal TypeMappingAttribute(string pgName, EDBDbType npgsqlDbType)
            : this(pgName, npgsqlDbType, new DbType[0], new Type[0], null)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType npgsqlDbType, DbType inferredDbType)
            : this(pgName, npgsqlDbType, new DbType[0], new Type[0], inferredDbType)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType npgsqlDbType, DbType[] dbTypes, Type clrType, DbType inferredDbType)
            : this(pgName, npgsqlDbType, dbTypes, new[] { clrType }, inferredDbType)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType npgsqlDbType, DbType[] dbTypes)
            : this(pgName, npgsqlDbType, dbTypes, new Type[0], null)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType npgsqlDbType, DbType dbType, Type[] clrTypes)
            : this(pgName, npgsqlDbType, new[] { dbType }, clrTypes, dbType)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType npgsqlDbType, DbType dbType, Type clrType = null)
            : this(pgName, npgsqlDbType, new[] { dbType }, clrType == null ? null : new[] { clrType }, dbType)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType npgsqlDbType, Type[] clrTypes, DbType inferredDbType)
            : this(pgName, npgsqlDbType, new DbType[0], clrTypes, inferredDbType)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType npgsqlDbType, Type[] clrTypes)
            : this(pgName, npgsqlDbType, new DbType[0], clrTypes, null)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType npgsqlDbType, Type clrType, DbType inferredDbType)
            : this(pgName, npgsqlDbType, new DbType[0], new[] { clrType }, inferredDbType)
        { }

        internal TypeMappingAttribute(string pgName, EDBDbType npgsqlDbType, Type clrType)
            : this(pgName, npgsqlDbType, new DbType[0], new[] { clrType }, null)
        { }

        /// <summary>
        /// Read-only parameter
        /// </summary>
        internal TypeMappingAttribute(string pgName)
            : this(pgName, null, null, null, null)
        { }

        internal string PgName { get; private set; }
        internal EDBDbType? EDBDbType { get; private set; }
        internal DbType[] DbTypes { get; private set; }
        internal Type[] ClrTypes { get; private set; }
        internal DbType? InferredDbType { get; private set; }

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

        [ContractInvariantMethod]
        void ObjectInvariants()
        {
            Contract.Invariant(!String.IsNullOrWhiteSpace(PgName));
            Contract.Invariant(ClrTypes != null);
            Contract.Invariant(DbTypes != null);
        }
    }
}
