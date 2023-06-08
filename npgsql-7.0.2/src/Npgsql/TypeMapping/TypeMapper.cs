using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using EnterpriseDB.EDBClient.Internal;
using EnterpriseDB.EDBClient.Internal.TypeHandlers;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.Internal.TypeMapping;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.Properties;
using EnterpriseDB.EDBClient.Util;
using EDBTypes;

namespace EnterpriseDB.EDBClient.TypeMapping;

sealed class TypeMapper
{
    internal EDBConnector Connector { get; }
    readonly object _writeLock = new();

    EDBDatabaseInfo? _databaseInfo;

    internal EDBDatabaseInfo DatabaseInfo
        => _databaseInfo ?? throw new InvalidOperationException("Internal error: this type mapper hasn't yet been bound to a database info object");

    volatile TypeHandlerResolver[] _resolvers;
    internal EDBTypeHandler UnrecognizedTypeHandler { get; }

    readonly ConcurrentDictionary<uint, EDBTypeHandler> _handlersByOID = new();
    readonly ConcurrentDictionary<EDBDbType, EDBTypeHandler> _handlersByEDBDbType = new();
    readonly ConcurrentDictionary<Type, EDBTypeHandler> _handlersByClrType = new();
    readonly ConcurrentDictionary<string, EDBTypeHandler> _handlersByDataTypeName = new();

    readonly Dictionary<uint, TypeMappingInfo> _userTypeMappings = new();
    readonly IEDBNameTranslator _defaultNameTranslator;

    readonly ILogger _commandLogger;

    #region Construction

    internal TypeMapper(EDBConnector connector, IEDBNameTranslator defaultNameTranslator)
    {
        Connector = connector;
        _defaultNameTranslator = defaultNameTranslator;
        UnrecognizedTypeHandler = new UnknownTypeHandler(Connector.TextEncoding);
        _resolvers = Array.Empty<TypeHandlerResolver>();
        _commandLogger = connector.LoggingConfiguration.CommandLogger;
    }

    #endregion Constructors

    internal void Initialize(
        EDBDatabaseInfo databaseInfo,
        List<TypeHandlerResolverFactory> resolverFactories,
        Dictionary<string, IUserTypeMapping> userTypeMappings)
    {
        _databaseInfo = databaseInfo;

        var resolvers = new TypeHandlerResolver[resolverFactories.Count];
        for (var i = 0; i < resolverFactories.Count; i++)
            resolvers[i] = resolverFactories[i].Create(Connector);
        _resolvers = resolvers;

        foreach (var userTypeMapping in userTypeMappings.Values)
        {
            if (DatabaseInfo.TryGetPostgresTypeByName(userTypeMapping.PgTypeName, out var pgType))
            {
                _handlersByOID[pgType.OID] =
                    _handlersByDataTypeName[pgType.FullName] =
                        _handlersByDataTypeName[pgType.Name] =
                            _handlersByClrType[userTypeMapping.ClrType] = userTypeMapping.CreateHandler(pgType, Connector);

                _userTypeMappings[pgType.OID] = new(npgsqlDbType: null, pgType.Name, userTypeMapping.ClrType);
            }
        }
    }

    #region Type handler lookup

    /// <summary>
    /// Looks up a type handler by its PostgreSQL type's OID.
    /// </summary>
    /// <param name="oid">A PostgreSQL type OID</param>
    /// <returns>A type handler that can be used to encode and decode values.</returns>
    internal EDBTypeHandler ResolveByOID(uint oid)
        => TryResolveByOID(oid, out var result) ? result : UnrecognizedTypeHandler;

    internal bool TryResolveByOID(uint oid, [NotNullWhen(true)] out EDBTypeHandler? handler)
    {
        if (_handlersByOID.TryGetValue(oid, out handler))
            return true;

        if (!DatabaseInfo.ByOID.TryGetValue(oid, out var pgType))
            return false;

        lock (_writeLock)
        {
            if ((handler = ResolveByDataTypeNameCore(pgType.FullName)) is not null)
            {
                _handlersByOID[oid] = handler;
                return true;
            }
            
            if ((handler = ResolveByDataTypeNameCore(pgType.Name)) is not null)
            {
                _handlersByOID[oid] = handler;
                return true;
            }
            
            if ((handler = ResolveComplexTypeByDataTypeName(pgType.FullName, throwOnError: false)) is not null)
            {
                _handlersByOID[oid] = handler;
                return true;
            }

            handler = null;
            return false;
        }
    }

    internal EDBTypeHandler ResolveByEDBDbType(EDBDbType npgsqlDbType)
    {
        if (_handlersByEDBDbType.TryGetValue(npgsqlDbType, out var handler))
            return handler;

        lock (_writeLock)
        {
            // First, try to resolve as a base type; translate the EDBDbType to a PG data type name and look that up.
            if (GlobalTypeMapper.EDBDbTypeToDataTypeName(npgsqlDbType) is { } dataTypeName)
            {
                foreach (var resolver in _resolvers)
                {
                    try
                    {
                        if ((handler = resolver.ResolveByDataTypeName(dataTypeName)) is not null)
                            return _handlersByEDBDbType[npgsqlDbType] = handler;
                    }
                    catch (Exception e)
                    {
                        _commandLogger.LogError(e,
                            $"Type resolver {resolver.GetType().Name} threw exception while resolving EDBDbType {npgsqlDbType}");
                    }
                }
            }

            if (npgsqlDbType.HasFlag(EDBDbType.Array))
            {
                var elementHandler = ResolveByEDBDbType(npgsqlDbType & ~EDBDbType.Array);

                if (elementHandler.PostgresType.Array is not { } pgArrayType)
                    throw new ArgumentException(
                        $"No array type could be found in the database for element {elementHandler.PostgresType}");

                return _handlersByEDBDbType[npgsqlDbType] =
                    elementHandler.CreateArrayHandler(pgArrayType, Connector.Settings.ArrayNullabilityMode);
            }

            if (npgsqlDbType.HasFlag(EDBDbType.Range))
            {
                var subtypeHandler = ResolveByEDBDbType(npgsqlDbType & ~EDBDbType.Range);

                if (subtypeHandler.PostgresType.Range is not { } pgRangeType)
                    throw new ArgumentException(
                        $"No range type could be found in the database for subtype {subtypeHandler.PostgresType}");

                return _handlersByEDBDbType[npgsqlDbType] = subtypeHandler.CreateRangeHandler(pgRangeType);
            }

            if (npgsqlDbType.HasFlag(EDBDbType.Multirange))
            {
                var subtypeHandler = ResolveByEDBDbType(npgsqlDbType & ~EDBDbType.Multirange);

                if (subtypeHandler.PostgresType.Range?.Multirange is not { } pgMultirangeType)
                    throw new ArgumentException(string.Format(EDBStrings.NoMultirangeTypeFound, subtypeHandler.PostgresType));

                return _handlersByEDBDbType[npgsqlDbType] = subtypeHandler.CreateMultirangeHandler(pgMultirangeType);
            }

            throw new EDBException($"The EDBDbType '{npgsqlDbType}' isn't present in your database. " +
                                      "You may need to install an extension or upgrade to a newer version.");
        }
    }

    internal EDBTypeHandler ResolveByDataTypeName(string typeName)
        => ResolveByDataTypeNameCore(typeName) ?? ResolveComplexTypeByDataTypeName(typeName, throwOnError: true)!;

    EDBTypeHandler? ResolveByDataTypeNameCore(string typeName)
    {
        if (_handlersByDataTypeName.TryGetValue(typeName, out var handler))
            return handler;

        lock (_writeLock)
        {
            foreach (var resolver in _resolvers)
            {
                try
                {
                    if ((handler = resolver.ResolveByDataTypeName(typeName)) is not null)
                        return _handlersByDataTypeName[typeName] = handler;
                }
                catch (Exception e)
                {
                    _commandLogger.LogError(e, $"Type resolver {resolver.GetType().Name} threw exception while resolving data type name {typeName}");
                }
            }

            return null;
        }
    }

    EDBTypeHandler? ResolveComplexTypeByDataTypeName(string typeName, bool throwOnError)
    {
        lock (_writeLock)
        {
            if (DatabaseInfo.GetPostgresTypeByName(typeName) is not { } pgType)
                throw new NotSupportedException("Could not find PostgreSQL type " + typeName);

            switch (pgType)
            {
            case PostgresArrayType pgArrayType:
            {
                var elementHandler = ResolveByOID(pgArrayType.Element.OID);
                return _handlersByDataTypeName[typeName] =
                    elementHandler.CreateArrayHandler(pgArrayType, Connector.Settings.ArrayNullabilityMode);
            }

            case PostgresRangeType pgRangeType:
            {
                var subtypeHandler = ResolveByOID(pgRangeType.Subtype.OID);
                return _handlersByDataTypeName[typeName] = subtypeHandler.CreateRangeHandler(pgRangeType);
            }

            case PostgresMultirangeType pgMultirangeType:
            {
                var subtypeHandler = ResolveByOID(pgMultirangeType.Subrange.Subtype.OID);
                return _handlersByDataTypeName[typeName] = subtypeHandler.CreateMultirangeHandler(pgMultirangeType);
            }

            case PostgresEnumType pgEnumType:
            {
                // A mapped enum would have been registered in _extraHandlersByDataTypeName and bound above - this is unmapped.
                return _handlersByDataTypeName[typeName] =
                    new UnmappedEnumHandler(pgEnumType, _defaultNameTranslator, Connector.TextEncoding);
            }

            case PostgresDomainType pgDomainType:
                return _handlersByDataTypeName[typeName] = ResolveByOID(pgDomainType.BaseType.OID);

            case PostgresBaseType pgBaseType:
                return throwOnError
                    ? throw new NotSupportedException($"PostgreSQL type '{pgBaseType}' isn't supported by EDB")
                    : null;

            case PostgresCompositeType pgCompositeType:
                // We don't support writing unmapped composite types, but we do support reading unmapped composite types.
                // So when we're invoked from ResolveOID (which is the read path), we don't want to raise an exception.
                return throwOnError
                    ? throw new NotSupportedException(
                        $"Composite type '{pgCompositeType}' must be mapped with EDB before being used, see the docs.")
                    : null;

            default:
                throw new ArgumentOutOfRangeException($"Unhandled PostgreSQL type type: {pgType.GetType()}");
            }
        }
    }

    internal EDBTypeHandler ResolveByValue<T>(T value)
    {
        if (value is null)
            return ResolveByClrType(typeof(T));

        if (typeof(T).IsValueType)
        {
            // Attempt to resolve value types generically via the resolver. This is the efficient fast-path, where we don't even need to
            // do a dictionary lookup (the JIT elides type checks in generic methods for value types)
            EDBTypeHandler? handler;

            foreach (var resolver in _resolvers)
            {
                try
                {
                    if ((handler = resolver.ResolveValueTypeGenerically(value)) is not null)
                        return handler;
                }
                catch (Exception e)
                {
                    _commandLogger.LogError(e, $"Type resolver {resolver.GetType().Name} threw exception while resolving value with type {typeof(T)}");
                }
            }

            // There may still be some value types not resolved by the above, e.g. EDBRange
        }

        // Value types would have been resolved above, so this is a reference type - no JIT optimizations.
        // We go through the regular logic (and there's no boxing).
        return ResolveByValue((object)value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal EDBTypeHandler ResolveByValue(object value)
    {
        // We resolve as follows:
        // 1. Cached by-type lookup (fast path). This will work for almost all types after the very first resolution.
        // 2. Value-dependent type lookup (e.g. DateTime by Kind) via the resolvers. This includes complex types (e.g. array/range
        //    over DateTime), and the results cannot be cached.
        // 3. Uncached by-type lookup (for the very first resolution of a given type)

        var type = value.GetType();
        if (_handlersByClrType.TryGetValue(type, out var handler))
            return handler;

        foreach (var resolver in _resolvers)
        {
            try
            {
                if ((handler = resolver.ResolveValueDependentValue(value)) is not null)
                    return handler;
            }
            catch (Exception e)
            {
                _commandLogger.LogError(e, $"Type resolver {resolver.GetType().Name} threw exception while resolving value with type {type}");
            }
        }

        // ResolveByClrType either throws, or resolves a handler and caches it in _handlersByClrType (where it would be found above the
        // next time we resolve this type)
        return ResolveByClrType(type);
    }

    // TODO: This is needed as a separate method only because of binary COPY, see #3957
    internal EDBTypeHandler ResolveByClrType(Type type)
    {
        if (_handlersByClrType.TryGetValue(type, out var handler))
            return handler;

        lock (_writeLock)
        {
            foreach (var resolver in _resolvers)
            {
                try
                {
                    if ((handler = resolver.ResolveByClrType(type)) is not null)
                        return _handlersByClrType[type] = handler;
                }
                catch (Exception e)
                {
                    _commandLogger.LogError(e, $"Type resolver {resolver.GetType().Name} threw exception while resolving value with type {type}");
                }
            }

            // Try to see if it is an array type
            var arrayElementType = GetArrayListElementType(type);
            if (arrayElementType is not null)
            {
                // With PG14, we map arrays over range types to PG multiranges by default, not to regular arrays over ranges.
                if (arrayElementType.IsGenericType &&
                    arrayElementType.GetGenericTypeDefinition() == typeof(EDBRange<>) &&
                    DatabaseInfo.Version.IsGreaterOrEqual(14))
                {
                    var subtypeType = arrayElementType.GetGenericArguments()[0];

                    return ResolveByClrType(subtypeType) is
                        { PostgresType : { Range : { Multirange: { } pgMultirangeType } } } subtypeHandler
                        ? _handlersByClrType[type] = subtypeHandler.CreateMultirangeHandler(pgMultirangeType)
                        : throw new NotSupportedException($"The CLR range type {type} isn't supported by EDB or your PostgreSQL.");
                }

                if (ResolveByClrType(arrayElementType) is not { } elementHandler)
                    throw new ArgumentException($"Array type over CLR type {arrayElementType.Name} isn't supported by EDB");

                if (elementHandler.PostgresType.Array is not { } pgArrayType)
                    throw new ArgumentException(
                        $"No array type could be found in the database for element {elementHandler.PostgresType}");

                return _handlersByClrType[type] =
                    elementHandler.CreateArrayHandler(pgArrayType, Connector.Settings.ArrayNullabilityMode);
            }

            if (Nullable.GetUnderlyingType(type) is { } underlyingType && ResolveByClrType(underlyingType) is { } underlyingHandler)
                return _handlersByClrType[type] = underlyingHandler;

            if (type.IsEnum)
            {
                return DatabaseInfo.GetPostgresTypeByName(GetPgName(type, _defaultNameTranslator)) is PostgresEnumType pgEnumType
                    ? _handlersByClrType[type] = new UnmappedEnumHandler(pgEnumType, _defaultNameTranslator, Connector.TextEncoding)
                    : throw new NotSupportedException(
                        $"Could not find a PostgreSQL enum type corresponding to {type.Name}. " +
                        "Consider mapping the enum before usage, refer to the documentation for more details.");
            }

            // TODO: We can make the following compatible with reflection-free mode by having EDBRange implement some interface, and
            // check for that.
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(EDBRange<>))
            {
                var subtypeType = type.GetGenericArguments()[0];

                return ResolveByClrType(subtypeType) is { PostgresType : { Range : { } pgRangeType } } subtypeHandler
                    ? _handlersByClrType[type] = subtypeHandler.CreateRangeHandler(pgRangeType)
                    : throw new NotSupportedException($"The CLR range type {type} isn't supported by EDB or your PostgreSQL.");
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
                throw new NotSupportedException("IEnumerable parameters are not supported, pass an array or List instead");

            throw new NotSupportedException($"The CLR type {type} isn't natively supported by EDB or your PostgreSQL. " +
                                            $"To use it with a PostgreSQL composite you need to specify {nameof(EDBParameter.DataTypeName)} or to map it, please refer to the documentation.");
        }

        static Type? GetArrayListElementType(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsArray)
                return GetUnderlyingType(type.GetElementType()!); // The use of bang operator is justified here as Type.GetElementType() only returns null for the Array base class which can't be mapped in a useful way.

            var ilist = typeInfo.ImplementedInterfaces.FirstOrDefault(x => x.GetTypeInfo().IsGenericType && x.GetGenericTypeDefinition() == typeof(IList<>));
            if (ilist != null)
                return GetUnderlyingType(ilist.GetGenericArguments()[0]);

            if (typeof(IList).IsAssignableFrom(type))
                throw new NotSupportedException("Non-generic IList is a supported parameter, but the EDBDbType parameter must be set on the parameter");

            return null;

            Type GetUnderlyingType(Type t)
                => Nullable.GetUnderlyingType(t) ?? t;
        }
    }

    internal bool TryGetMapping(PostgresType pgType, [NotNullWhen(true)] out TypeMappingInfo? mapping)
    {
        foreach (var resolver in _resolvers)
            if ((mapping = resolver.GetMappingByDataTypeName(pgType.FullName)) is not null)
                return true;
        
        foreach (var resolver in _resolvers)
            if ((mapping = resolver.GetMappingByDataTypeName(pgType.Name)) is not null)
                return true;

        switch (pgType)
        {
        case PostgresArrayType pgArrayType:
            if (TryGetMapping(pgArrayType.Element, out var elementMapping))
            {
                mapping = new(elementMapping.EDBDbType | EDBDbType.Array, pgType.DisplayName);
                return true;
            }

            break;

        case PostgresRangeType pgRangeType:
        {
            if (TryGetMapping(pgRangeType.Subtype, out var subtypeMapping))
            {
                mapping = new(subtypeMapping.EDBDbType | EDBDbType.Range, pgType.DisplayName);
                return true;
            }

            break;
        }

        case PostgresMultirangeType pgMultirangeType:
        {
            if (TryGetMapping(pgMultirangeType.Subrange.Subtype, out var subtypeMapping))
            {
                mapping = new(subtypeMapping.EDBDbType | EDBDbType.Multirange, pgType.DisplayName);
                return true;
            }

            break;
        }

        case PostgresDomainType pgDomainType:
            if (TryGetMapping(pgDomainType.BaseType, out var baseMapping))
            {
                mapping = new(baseMapping.EDBDbType, pgType.DisplayName, baseMapping.ClrTypes);
                return true;
            }

            break;

        case PostgresEnumType or PostgresCompositeType:
            return _userTypeMappings.TryGetValue(pgType.OID, out mapping);
        }

        mapping = null;
        return false;
    }

    #endregion Type handler lookup

    internal (EDBDbType? npgsqlDbType, PostgresType postgresType) GetTypeInfoByOid(uint oid)
    {
        if (!DatabaseInfo.ByOID.TryGetValue(oid, out var pgType))
            throw new InvalidOperationException($"Couldn't find PostgreSQL type with OID {oid}");

        foreach (var resolver in _resolvers)
            if (resolver.GetMappingByDataTypeName(pgType.FullName) is { } mapping)
                return (mapping.EDBDbType, pgType);
        
        foreach (var resolver in _resolvers)
            if (resolver.GetMappingByDataTypeName(pgType.Name) is { } mapping)
                return (mapping.EDBDbType, pgType);

        switch (pgType)
        {
        case PostgresArrayType pgArrayType:
            var (elementEDBDbType, _) = GetTypeInfoByOid(pgArrayType.Element.OID);
            if (elementEDBDbType.HasValue)
                return new(elementEDBDbType | EDBDbType.Array, pgType);
            break;

        case PostgresDomainType pgDomainType:
            var (baseEDBDbType, _) = GetTypeInfoByOid(pgDomainType.BaseType.OID);
            return new(baseEDBDbType, pgType);
        }

        return (null, pgType);
    }

    static string GetPgName(Type clrType, IEDBNameTranslator nameTranslator)
        => clrType.GetCustomAttribute<PgNameAttribute>()?.PgName
           ?? nameTranslator.TranslateTypeName(clrType.Name);
}
