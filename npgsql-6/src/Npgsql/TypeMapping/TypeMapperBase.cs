using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeMapping
{
    abstract class TypeMapperBase : IEDBTypeMapper
    {
        public IEDBNameTranslator DefaultNameTranslator { get; }

        protected TypeMapperBase(IEDBNameTranslator defaultNameTranslator)
        {
            if (defaultNameTranslator == null)
                throw new ArgumentNullException(nameof(defaultNameTranslator));

            DefaultNameTranslator = defaultNameTranslator;
        }

        #region Mapping management

        /// <inheritdoc />
        public abstract IEDBTypeMapper MapEnum<TEnum>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
            where TEnum : struct, Enum;

        /// <inheritdoc />
        public abstract bool UnmapEnum<TEnum>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
            where TEnum : struct, Enum;

        /// <inheritdoc />
        [RequiresUnreferencedCode("Composite type mapping currently isn't trimming-safe.")]
        public abstract IEDBTypeMapper MapComposite<T>(string? pgName = null, IEDBNameTranslator? nameTranslator = null);

        /// <inheritdoc />
        [RequiresUnreferencedCode("Composite type mapping currently isn't trimming-safe.")]
        public abstract IEDBTypeMapper MapComposite(Type clrType, string? pgName = null, IEDBNameTranslator? nameTranslator = null);

        /// <inheritdoc />
        public abstract bool UnmapComposite<T>(string? pgName = null, IEDBNameTranslator? nameTranslator = null);

        /// <inheritdoc />
        public abstract bool UnmapComposite(Type clrType, string? pgName = null, IEDBNameTranslator? nameTranslator = null);

        /// <inheritdoc />
        public abstract void AddTypeResolverFactory(TypeHandlerResolverFactory resolverFactory);

        public abstract void Reset();

        #endregion Composite mapping

        #region Misc

        private protected static string GetPgName(Type clrType, IEDBNameTranslator nameTranslator)
            => clrType.GetCustomAttribute<PgNameAttribute>()?.PgName
               ?? nameTranslator.TranslateTypeName(clrType.Name);

        #endregion Misc
    }
}
