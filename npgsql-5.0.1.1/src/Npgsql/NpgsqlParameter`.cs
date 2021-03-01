using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.TypeMapping;
using EDBTypes;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// A generic version of <see cref="EDBParameter"/> which provides more type safety and
    /// avoids boxing of value types. Use <see cref="TypedValue"/> instead of <see cref="EDBParameter.Value"/>.
    /// </summary>
    /// <typeparam name="T">The type of the value that will be stored in the parameter.</typeparam>
    public sealed class EDBParameter<T> : EDBParameter
    {
        /// <summary>
        /// Gets or sets the strongly-typed value of the parameter.
        /// </summary>
        [MaybeNull, AllowNull]
        public T TypedValue { get; set; } = default!;

        /// <summary>
        /// Gets or sets the value of the parameter. This delegates to <see cref="TypedValue"/>.
        /// </summary>
        public override object? Value
        {
            get => TypedValue;
            set => TypedValue = (T)value!;
        }

        #region Constructors

        /// <summary>
        /// Initializes a new instance of <see cref="EDBParameter{T}" />.
        /// </summary>
        public EDBParameter() {}

        /// <summary>
        /// Initializes a new instance of <see cref="EDBParameter{T}" /> with a parameter name and value.
        /// </summary>
        public EDBParameter(string parameterName, T value)
        {
            ParameterName = parameterName;
            TypedValue = value;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EDBParameter{T}" /> with a parameter name and type.
        /// </summary>
        public EDBParameter(string parameterName, EDBDbType eDBDbType)
        {
            ParameterName = parameterName;
            EDBDbType = eDBDbType;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="EDBParameter{T}" /> with a parameter name and type.
        /// </summary>
        public EDBParameter(string parameterName, DbType dbType)
        {
            ParameterName = parameterName;
            DbType = dbType;
        }

        #endregion Constructors

        internal override void ResolveHandler(ConnectorTypeMapper typeMapper)
        {
            if (Handler != null)
                return;

            // TODO: Better exceptions in case of cast failure etc.
            if (_EDBDbType.HasValue)
                Handler = typeMapper.GetByEDBDbType(_EDBDbType.Value);
            else if (_dataTypeName != null)
                Handler = typeMapper.GetByDataTypeName(_dataTypeName);
            else
                Handler = typeMapper.GetByClrType(typeof(T));
        }

        internal override int ValidateAndGetLength()
        {
            if (TypedValue == null)
                return 0;

            // TODO: Why do it like this rather than a handler?
            if (typeof(T) == typeof(DBNull))
                return 0;

            var lengthCache = LengthCache;
            var len = Handler!.ValidateAndGetLength(TypedValue, ref lengthCache, this);
            LengthCache = lengthCache;
            return len;
        }

        internal override Task WriteWithLength(EDBWriteBuffer buf, bool async, CancellationToken cancellationToken = default)
            => Handler!.WriteWithLengthInternal(TypedValue, buf, LengthCache, this, async, cancellationToken);
    }
}
