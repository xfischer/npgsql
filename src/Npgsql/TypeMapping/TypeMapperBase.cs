#region License

// The PostgreSQL License
//
// Copyright (C) 2018 The EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.TypeHandlers;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeMapping
{
    abstract class TypeMapperBase : IEDBTypeMapper
    {
        internal Dictionary<string, EDBTypeMapping> Mappings { get; set; }

        public IEDBNameTranslator DefaultNameTranslator { get; }

        protected TypeMapperBase([NotNull] IEDBNameTranslator defaultNameTranslator)
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

        public IEDBTypeMapper MapEnum<TEnum>(string pgName = null, IEDBNameTranslator nameTranslator = null)
            where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
                throw new ArgumentException("An enum type must be provided");
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;
            if (pgName == null)
                pgName = GetPgName<TEnum>(nameTranslator);

            return AddMapping(new EDBTypeMappingBuilder
            {
                PgTypeName = pgName,
                ClrTypes = new[] { typeof(TEnum) },
                TypeHandlerFactory = new EnumTypeHandlerFactory<TEnum>(nameTranslator)
            }.Build());
        }

        public bool UnmapEnum<TEnum>(string pgName = null, IEDBNameTranslator nameTranslator = null) where TEnum : struct
        {
            if (!typeof(TEnum).GetTypeInfo().IsEnum)
                throw new ArgumentException("An enum type must be provided");
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;
            if (pgName == null)
                pgName = GetPgName<TEnum>(nameTranslator);

            return RemoveMapping(pgName);
        }

        #endregion Enum mapping

        #region Composite mapping

        public IEDBTypeMapper MapComposite<T>(string pgName = null, IEDBNameTranslator nameTranslator = null)
            where T : new()
        {
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;
            if (pgName == null)
                pgName = GetPgName<T>(nameTranslator);

            return AddMapping(new EDBTypeMappingBuilder
            {
                PgTypeName = pgName,
                ClrTypes = new[] { typeof(T) },
                TypeHandlerFactory = new MappedCompositeTypeHandlerFactory<T>(nameTranslator)
            }.Build());
        }

        public bool UnmapComposite<T>(string pgName = null, IEDBNameTranslator nameTranslator = null)
            where T : new()
        {
            if (pgName != null && pgName.Trim() == "")
                throw new ArgumentException("pgName can't be empty", nameof(pgName));

            if (nameTranslator == null)
                nameTranslator = DefaultNameTranslator;
            if (pgName == null)
                pgName = GetPgName<T>(nameTranslator);

            return RemoveMapping(pgName);
        }

        #endregion Composite mapping

        #region Misc

        // TODO: why does ReSharper think `GetCustomAttribute<T>` is non-nullable?
        // ReSharper disable once ConstantConditionalAccessQualifier ConstantNullCoalescingCondition
        static string GetPgName<T>(IEDBNameTranslator nameTranslator)
            => typeof(T).GetCustomAttribute<PgNameAttribute>()?.PgName
               ?? nameTranslator.TranslateTypeName(typeof(T).Name);

        #endregion Misc
    }
}
