using System;
using System.Data.Common;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using EDBTypes;
using EnterpriseDB.EDBClient;

namespace EnterpriseDB.EDBClient.EntityFrameworkCore.PostgreSQL.Storage.Internal.Mapping
{
    /// <summary>
    /// The base class for mapping Npgsql-specific string types. It configures parameters with the
    /// <see cref="EDBDbType"/> provider-specific type enum.
    /// </summary>
    public class NpgsqlStringTypeMapping : StringTypeMapping
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
        /// <param name="npgsqlDbType">The database type used by Npgsql.</param>
        public NpgsqlStringTypeMapping([NotNull] string storeType, EDBDbType npgsqlDbType)
            : base(storeType)
            => EDBDbType = npgsqlDbType;

        protected NpgsqlStringTypeMapping(
            RelationalTypeMappingParameters parameters,
            EDBDbType npgsqlDbType)
            : base(parameters)
            => EDBDbType = npgsqlDbType;

        protected override RelationalTypeMapping Clone(RelationalTypeMappingParameters parameters)
            => new NpgsqlStringTypeMapping(parameters, EDBDbType);

        protected override void ConfigureParameter(DbParameter parameter)
        {
            base.ConfigureParameter(parameter);

            if (parameter is EDBParameter npgsqlParameter)
                npgsqlParameter.EDBDbType = EDBDbType;
            else
                throw new InvalidOperationException($"Npgsql-specific type mapping {GetType().Name} being used with non-Npgsql parameter type {parameter.GetType().Name}");
        }
    }
}
