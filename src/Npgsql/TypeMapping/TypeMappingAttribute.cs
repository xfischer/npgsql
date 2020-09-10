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
        /// <param name="edbDbType">
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
        internal TypeMappingAttribute(string pgName, EDBDbType? edbDbType, DbType[]? dbTypes, Type[]? clrTypes, DbType? inferredDbType)
        {
            if (string.IsNullOrWhiteSpace(pgName))
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            PgName = pgName;
            EDBDbType = edbDbType;
            DbTypes = dbTypes ?? new DbType[0];
            ClrTypes = clrTypes ?? new Type[0];
            InferredDbType = inferredDbType;
        }

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, DbType[] dbTypes, Type[]? clrTypes, DbType inferredDbType)
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

        internal TypeMappingAttribute(string pgName, EDBDbType EDBDbType, DbType dbType, Type? clrType = null)
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

        //public TypeMappingAttribute(string v, EDBDbType bit)
        //{
        //    this.v = v;
        //    this.bit = bit;
        //}

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
                sb.Append(string.Join(",", DbTypes.Select(t => t.ToString())));
            }
            if (ClrTypes.Length > 0)
            {
                sb.Append(" ClrTypes=");
                sb.Append(string.Join(",", ClrTypes.Select(t => t.Name)));
            }
            sb.AppendFormat("]");
            return sb.ToString();
        }
    }
}
