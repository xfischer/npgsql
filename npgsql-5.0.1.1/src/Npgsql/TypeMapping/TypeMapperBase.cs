using System;
using System.Collections.Generic;
using System.Reflection;
using EnterpriseDB.EDBClient.TypeHandlers;
using EnterpriseDB.EDBClient.TypeHandlers.CompositeHandlers;
using EnterpriseDB.EDBClient.TypeHandling;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeMapping
{
    abstract class TypeMapperBase : IEDBTypeMapper
    {
        internal Dictionary<string, EDBTypeMapping> Mappings { get; } = new Dictionary<string, EDBTypeMapping>();

        public IEDBNameTranslator DefaultNameTranslator { get; }

        protected TypeMapperBase(IEDBNameTranslator defaultNameTranslator)
        {
            if (defaultNameTranslator == null)
                throw new ArgumentNullException(nameof(defaultNameTranslator));

            DefaultNameTranslator = defaultNameTranslator;
        }

        #region Mapping management

        public virtual IEDBTypeMapper AddMapping(EDBTypeMapping mapping)
        {
            if (Mappings.ContainsKey(mapping.PgTypeName))
                RemoveMapping(mapping.PgTypeName);
            Mappings[mapping.PgTypeName] = mapping;
            return this;
        }

        public virtual bool RemoveMapping(string pgTypeName) => Mappings.Remove(pgTypeName);

        IEnumerable<EDBTypeMapping> IEDBTypeMapper.Mappings => Mappings.Values;

        public abstract void Reset();

        #endregion Mapping management

        #region Enum mapping

        public IEDBTypeMapper MapEnum<TEnum>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
            where TEnum : struct, Enum
        {
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;
            if (pgName == null)
                pgName = GetPgName(typeof(TEnum), nameTranslator);

            return AddMapping(new EDBTypeMappingBuilder
            {
                PgTypeName = pgName,
                ClrTypes = new[] { typeof(TEnum) },
                TypeHandlerFactory = new EnumTypeHandlerFactory<TEnum>(nameTranslator)
            }.Build());
        }

        public bool UnmapEnum<TEnum>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
            where TEnum : struct, Enum
        {
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;
            if (pgName == null)
                pgName = GetPgName(typeof(TEnum), nameTranslator);

            return RemoveMapping(pgName);
        }

        #endregion Enum mapping

        #region Composite mapping

        public IEDBTypeMapper MapComposite<T>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
            => MapComposite(pgName, nameTranslator, typeof(T), t => new CompositeTypeHandlerFactory<T>(t));

        public IEDBTypeMapper MapComposite(Type clrType, string? pgName = null, IEDBNameTranslator? nameTranslator = null)
            => MapComposite(pgName, nameTranslator, clrType, t => (EDBTypeHandlerFactory)
                Activator.CreateInstance(typeof(CompositeTypeHandlerFactory<>).MakeGenericType(clrType), BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[] { t }, null)!);

        IEDBTypeMapper MapComposite(string? pgName, IEDBNameTranslator? nameTranslator, Type type, Func<IEDBNameTranslator, EDBTypeHandlerFactory> factory)
        {
            if (pgName != null && string.IsNullOrWhiteSpace(pgName))
                throw new ArgumentException("pgName can't be empty.", nameof(pgName));

            nameTranslator ??= DefaultNameTranslator;
            pgName ??= GetPgName(type, nameTranslator);

            return AddMapping(
                new EDBTypeMappingBuilder
                {
                    PgTypeName = pgName,
                    ClrTypes = new[] { type },
                    TypeHandlerFactory = factory(nameTranslator),
                }
                .Build());
        }

        public bool UnmapComposite<T>(string? pgName = null, IEDBNameTranslator? nameTranslator = null)
            => UnmapComposite(typeof(T), pgName, nameTranslator);

        public bool UnmapComposite(Type clrType, string? pgName = null, IEDBNameTranslator? nameTranslator = null)
        {
            if (pgName != null && string.IsNullOrWhiteSpace(pgName))
                throw new ArgumentException("pgName can't be empty.", nameof(pgName));

            nameTranslator ??= DefaultNameTranslator;
            pgName ??= GetPgName(clrType, nameTranslator);

            return RemoveMapping(pgName);
        }

        #endregion Composite mapping

        #region Misc

        // TODO: why does ReSharper think `GetCustomAttribute<T>` is non-nullable?
        // ReSharper disable once ConstantConditionalAccessQualifier ConstantNullCoalescingCondition
        private protected static string GetPgName(Type clrType, IEDBNameTranslator nameTranslator)
            => clrType.GetCustomAttribute<PgNameAttribute>()?.PgName
               ?? nameTranslator.TranslateTypeName(clrType.Name);

        #endregion Misc
    }
}
