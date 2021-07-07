using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using EDBTypes;
using EnterpriseDB.EDBClient;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The base class for mapping Npgsql-specific types. It configures parameters with the
    /// <see cref="EDBDbType"/> provider-specific type enum.
    /// </summary>
    public abstract class NpgsqlTypeMapping : RelationalTypeMapping
    {
        /// <summary>
        /// The database type used by Npgsql.
        /// </summary>
        public virtual EDBDbType EDBDbType { get; }

        // ReSharper disable once PublicConstructorInAbstractClass
        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlTypeMapping"/> class.
        /// </summary>
        /// <param name="storeType">The database type to map.</param>
        /// <param name="clrType">The CLR type to map.</param>
        /// <param name="npgsqlDbType">The database type used by Npgsql.</param>
        public NpgsqlTypeMapping([NotNull] string storeType, [NotNull] Type clrType, EDBDbType npgsqlDbType)
            : base(storeType, clrType)
            => EDBDbType = npgsqlDbType;

        /// <summary>
        /// Constructs an instance of the <see cref="NpgsqlTypeMapping"/> class.
        /// </summary>
        /// <param name="parameters">The parameters for this mapping.</param>
        /// <param name="npgsqlDbType">The database type of the range subtype.</param>
        protected NpgsqlTypeMapping(RelationalTypeMappingParameters parameters, EDBDbType npgsqlDbType)
            : base(parameters)
            => EDBDbType = npgsqlDbType;

        protected override void ConfigureParameter(DbParameter parameter)
        {
            var npgsqlParameter = parameter as EDBParameter;
            if (npgsqlParameter == null)
                throw new ArgumentException($"Npgsql-specific type mapping {GetType().Name} being used with non-Npgsql parameter type {parameter.GetType().Name}");

            base.ConfigureParameter(parameter);

            npgsqlParameter.EDBDbType = EDBDbType;
        }
    }
}
