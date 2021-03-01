using System;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.TypeHandling
{
    /// <summary>
    /// Base class for all type handler factories, which construct type handlers that know how
    /// to read and write CLR types from/to PostgreSQL types.
    /// </summary>
    /// <remarks>
    /// In general, do not inherit from this class, inherit from <see cref="EDBTypeHandlerFactory{T}"/> instead.
    /// </remarks>
    public abstract class EDBTypeHandlerFactory
    {
        /// <summary>
        /// Creates a type handler.
        /// </summary>
        public abstract EDBTypeHandler CreateNonGeneric(PostgresType pgType, EDBConnection conn);

        /// <summary>
        /// The default CLR type that handlers produced by this factory will read and write.
        /// </summary>
        public abstract Type DefaultValueType { get; }
    }

    /// <summary>
    /// Base class for all type handler factories, which construct type handlers that know how
    /// to read and write CLR types from/to PostgreSQL types. Type handler factories are set up
    /// via <see cref="EDBTypeMapping"/> in either the global or connection-specific type mapper.
    /// </summary>
    /// <seealso cref="EDBTypeMapping"/>
    /// <seealso cref="EDBConnection.GlobalTypeMapper"/>
    /// <seealso cref="EDBConnection.TypeMapper"/>
    /// <typeparam name="TDefault">The default CLR type that handlers produced by this factory will read and write.</typeparam>
    public abstract class EDBTypeHandlerFactory<TDefault> : EDBTypeHandlerFactory
    {
        /// <summary>
        /// Creates a type handler.
        /// </summary>
        public abstract EDBTypeHandler<TDefault> Create(PostgresType pgType, EDBConnection conn);

        /// <inheritdoc />
        public override EDBTypeHandler CreateNonGeneric(PostgresType pgType, EDBConnection conn)
            => Create(pgType, conn);

        /// <inheritdoc />
        public override Type DefaultValueType => typeof(TDefault);
    }
}
