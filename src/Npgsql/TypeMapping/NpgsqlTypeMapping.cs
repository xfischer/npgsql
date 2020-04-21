using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using EnterpriseDB.EDBClient.TypeHandling;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeMapping
{
    /// <summary>
    /// Builds instances of <see cref="EDBTypeMapping"/> for addition into <see cref="IEDBTypeMapper"/>.
    /// </summary>
    public class EDBTypeMappingBuilder
    {
        /// <summary>
        /// The name of the PostgreSQL type name, as it appears in the pg_type catalog.
        /// </summary>
        /// <remarks>
        /// This can a a partial name (without the schema), or a fully-qualified name
        /// (schema.typename) - the latter can be used if you have two types with the same
        /// name in different schemas.
        /// </remarks>
        [DisallowNull]
        public string? PgTypeName { get; set; }

        /// <summary>
        /// The <see cref="EDBDbType"/> that corresponds to this type. Setting an
        /// <see cref="EDBParameter"/>'s <see cref="EDBParameter.EDBDbType"/> property
        /// to this value will make EDB write its value to PostgreSQL with this mapping.
        /// </summary>
        public EDBDbType? EDBDbType { get; set; }

        /// <summary>
        /// A set of <see cref="DbType"/>s that correspond to this type. Setting an
        /// <see cref="EDBParameter"/>'s <see cref="EDBParameter.DbType"/> property
        /// to one of these values will make EDB write its value to PostgreSQL with this mapping.
        /// </summary>
        public DbType[]? DbTypes { get; set; }

        /// <summary>
        /// A set of CLR types that correspond to this type. Setting an
        /// <see cref="EDBParameter"/>'s <see cref="EDBParameter.Value"/> property
        /// to one of these types will make EDB write its value to PostgreSQL with this mapping.
        /// </summary>
        public Type[]? ClrTypes { get; set; }

        /// <summary>
        /// Determines what is returned from <see cref="EDBParameter.DbType"/> when this mapping
        /// is used.
        /// </summary>
        public DbType? InferredDbType { get; set; }

        /// <summary>
        /// A factory for a type handler that will be used to read and write values for PostgreSQL type.
        /// </summary>
        [DisallowNull]
        public EDBTypeHandlerFactory? TypeHandlerFactory { get; set; }

        /// <summary>
        /// Builds an <see cref="EDBTypeMapping"/> that can be added to an <see cref="IEDBTypeMapper"/>.
        /// </summary>
        /// <returns></returns>
        public EDBTypeMapping Build()
        {
            if (string.IsNullOrWhiteSpace(PgTypeName))
                throw new ArgumentException($"{nameof(PgTypeName)} must contain the name of a PostgreSQL data type", nameof(PgTypeName));

            if (TypeHandlerFactory is null)
                throw new ArgumentException($"{nameof(TypeHandlerFactory)} must refer to a type handler factory");

            return new EDBTypeMapping(PgTypeName!, EDBDbType, DbTypes, ClrTypes, InferredDbType, TypeHandlerFactory);
        }
    }

    /// <summary>
    /// Represents a type mapping for a PostgreSQL data type, which can be added to a type mapper,
    /// managing when that data type will be read and written and how.
    /// </summary>
    /// <seealso cref="EDBConnection.GlobalTypeMapper"/>
    /// <seealso cref="EDBConnection.TypeMapper"/>
    public sealed class EDBTypeMapping
    {
        internal EDBTypeMapping(
            string pgTypeName,
            EDBDbType? edbDbType, DbType[]? dbTypes, Type[]? clrTypes, DbType? inferredDbType,
            EDBTypeHandlerFactory typeHandlerFactory)
        {
            PgTypeName = pgTypeName;
            EDBDbType = edbDbType;
            DbTypes = dbTypes ?? EmptyDbTypes;
            ClrTypes = clrTypes ?? EmptyClrTypes;
            InferredDbType = inferredDbType;
            TypeHandlerFactory = typeHandlerFactory;
        }

        /// <summary>
        /// The name of the PostgreSQL type name, as it appears in the pg_type catalog.
        /// </summary>
        /// <remarks>
        /// This can a a partial name (without the schema), or a fully-qualified name
        /// (schema.typename) - the latter can be used if you have two types with the same
        /// name in different schemas.
        /// </remarks>
        public string PgTypeName { get; }

        /// <summary>
        /// The <see cref="EDBDbType"/> that corresponds to this type. Setting an
        /// <see cref="EDBParameter"/>'s <see cref="EDBParameter.EDBDbType"/> property
        /// to this value will make EDB write its value to PostgreSQL with this mapping.
        /// </summary>
        public EDBDbType? EDBDbType { get; }

        /// <summary>
        /// A set of <see cref="DbType"/>s that correspond to this type. Setting an
        /// <see cref="EDBParameter"/>'s <see cref="EDBParameter.DbType"/> property
        /// to one of these values will make EDB write its value to PostgreSQL with this mapping.
        /// </summary>
        public DbType[] DbTypes { get; }

        /// <summary>
        /// A set of CLR types that correspond to this type. Setting an
        /// <see cref="EDBParameter"/>'s <see cref="EDBParameter.Value"/> property
        /// to one of these types will make EDB write its value to PostgreSQL with this mapping.
        /// </summary>
        public Type[] ClrTypes { get; }

        /// <summary>
        /// Determines what is returned from <see cref="EDBParameter.DbType"/> when this mapping
        /// is used.
        /// </summary>
        public DbType? InferredDbType { get; }

        /// <summary>
        /// A factory for a type handler that will be used to read and write values for PostgreSQL type.
        /// </summary>
        public EDBTypeHandlerFactory TypeHandlerFactory { get; }

        /// <summary>
        /// The default CLR type that handlers produced by this factory will read and write.
        /// Used by the EF Core provider (and possibly others in the future).
        /// </summary>
        internal Type DefaultClrType => TypeHandlerFactory.DefaultValueType;

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        public override string ToString() => $"{PgTypeName} => {TypeHandlerFactory.GetType().Name}";

        static readonly DbType[] EmptyDbTypes = new DbType[0];
        static readonly Type[] EmptyClrTypes = new Type[0];
    }
}
