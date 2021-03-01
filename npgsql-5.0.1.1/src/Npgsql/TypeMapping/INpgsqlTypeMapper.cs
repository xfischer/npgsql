using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.NameTranslation;
using EDBTypes;

// ReSharper disable UnusedMember.Global
namespace EnterpriseDB.EDBClient.TypeMapping
{
    /// <summary>
    /// A type mapper, managing how to read and write CLR values to PostgreSQL data types.
    /// A type mapper exists for each connection, as well as a single global type mapper
    /// (accessible via <see cref="P:EDBConnection.GlobalTypeMapper"/>).
    /// </summary>
    /// <remarks>
    /// </remarks>
    public interface IEDBTypeMapper
    {
        /// <summary>
        /// The default name translator to convert CLR type names and member names.
        /// </summary>
        [NotNull]
        IEDBNameTranslator DefaultNameTranslator { get; }

        /// <summary>
        /// Enumerates all mappings currently set up on this type mapper.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IEnumerable<EDBTypeMapping> Mappings { get; }

        /// <summary>
        /// Adds a new type mapping to this mapper, overwriting any existing mapping in the process.
        /// </summary>
        [NotNull]
        IEDBTypeMapper AddMapping([NotNull] EDBTypeMapping mapping);

        /// <summary>
        /// Removes an existing mapping from this mapper. Attempts to read or write this type
        /// after removal will result in an exception.
        /// </summary>
        /// <param name="pgTypeName">A PostgreSQL type name for the type in the database.</param>
        bool RemoveMapping([NotNull] string pgTypeName);

        /// <summary>
        /// Maps a CLR enum to a PostgreSQL enum type.
        /// </summary>
        /// <remarks>
        /// CLR enum labels are mapped by name to PostgreSQL enum labels.
        /// The translation strategy can be controlled by the <paramref name="nameTranslator"/> parameter,
        /// which defaults to <see cref="EDBSnakeCaseNameTranslator"/>.
        /// You can also use the <see cref="PgNameAttribute"/> on your enum fields to manually specify a PostgreSQL enum label.
        /// If there is a discrepancy between the .NET and database labels while an enum is read or written,
        /// an exception will be raised.
        /// </remarks>
        /// <param name="pgName">
        /// A PostgreSQL type name for the corresponding enum type in the database.
        /// If null, the name translator given in <paramref name="nameTranslator"/> will be used.
        /// </param>
        /// <param name="nameTranslator">
        /// A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class).
        /// Defaults to <see cref="EDBSnakeCaseNameTranslator"/>
        /// </param>
        /// <typeparam name="TEnum">The .NET enum type to be mapped</typeparam>
        [NotNull]
        IEDBTypeMapper MapEnum<TEnum>(
            string? pgName = null,
            IEDBNameTranslator? nameTranslator = null)
            where TEnum : struct, Enum;

        /// <summary>
        /// Removes an existing enum mapping.
        /// </summary>
        /// <param name="pgName">
        /// A PostgreSQL type name for the corresponding enum type in the database.
        /// If null, the name translator given in <paramref name="nameTranslator"/> will be used.
        /// </param>
        /// <param name="nameTranslator">
        /// A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class).
        /// Defaults to <see cref="EDBSnakeCaseNameTranslator"/>
        /// </param>
        bool UnmapEnum<TEnum>(
            string? pgName = null,
            IEDBNameTranslator? nameTranslator = null)
            where TEnum : struct, Enum;

        /// <summary>
        /// Maps a CLR type to a PostgreSQL composite type.
        /// </summary>
        /// <remarks>
        /// CLR fields and properties by string to PostgreSQL names.
        /// The translation strategy can be controlled by the <paramref name="nameTranslator"/> parameter,
        /// which defaults to <see cref="EDBSnakeCaseNameTranslator"/>.
        /// You can also use the <see cref="PgNameAttribute"/> on your members to manually specify a PostgreSQL name.
        /// If there is a discrepancy between the .NET type and database type while a composite is read or written,
        /// an exception will be raised.
        /// </remarks>
        /// <param name="pgName">
        /// A PostgreSQL type name for the corresponding composite type in the database.
        /// If null, the name translator given in <paramref name="nameTranslator"/> will be used.
        /// </param>
        /// <param name="nameTranslator">
        /// A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class).
        /// Defaults to <see cref="EDBSnakeCaseNameTranslator"/>
        /// </param>
        /// <typeparam name="T">The .NET type to be mapped</typeparam>
        [NotNull]
        IEDBTypeMapper MapComposite<T>(
            string? pgName = null,
            IEDBNameTranslator? nameTranslator = null);

        /// <summary>
        /// Removes an existing composite mapping.
        /// </summary>
        /// <param name="pgName">
        /// A PostgreSQL type name for the corresponding composite type in the database.
        /// If null, the name translator given in <paramref name="nameTranslator"/> will be used.
        /// </param>
        /// <param name="nameTranslator">
        /// A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class).
        /// Defaults to <see cref="EDBSnakeCaseNameTranslator"/>
        /// </param>
        bool UnmapComposite<T>(
            string? pgName = null,
            IEDBNameTranslator? nameTranslator = null);

        /// <summary>
        /// Maps a CLR type to a composite type.
        /// </summary>
        /// <remarks>
        /// Maps CLR fields and properties by string to PostgreSQL names.
        /// The translation strategy can be controlled by the <paramref name="nameTranslator"/> parameter,
        /// which defaults to <see cref="EDBSnakeCaseNameTranslator"/>.
        /// If there is a discrepancy between the .NET type and database type while a composite is read or written,
        /// an exception will be raised.
        /// </remarks>
        /// <param name="clrType">The .NET type to be mapped.</param>
        /// <param name="pgName">
        /// A PostgreSQL type name for the corresponding composite type in the database.
        /// If null, the name translator given in <paramref name="nameTranslator"/> will be used.
        /// </param>
        /// <param name="nameTranslator">
        /// A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class).
        /// Defaults to <see cref="EDBSnakeCaseNameTranslator"/>
        /// </param>
        [NotNull]
        IEDBTypeMapper MapComposite(
            Type clrType,
            string? pgName = null,
            IEDBNameTranslator? nameTranslator = null);

        /// <summary>
        /// Removes an existing composite mapping.
        /// </summary>
        /// <param name="clrType">The .NET type to be unmapped.</param>
        /// <param name="pgName">
        /// A PostgreSQL type name for the corresponding composite type in the database.
        /// If null, the name translator given in <paramref name="nameTranslator"/> will be used.
        /// </param>
        /// <param name="nameTranslator">
        /// A component which will be used to translate CLR names (e.g. SomeClass) into database names (e.g. some_class).
        /// Defaults to <see cref="EDBSnakeCaseNameTranslator"/>
        /// </param>
        bool UnmapComposite(
            Type clrType,
            string? pgName = null,
            IEDBNameTranslator? nameTranslator = null);

        /// <summary>
        /// Resets all mapping changes performed on this type mapper and reverts it to its original, starting state.
        /// </summary>
        void Reset();
    }
}
