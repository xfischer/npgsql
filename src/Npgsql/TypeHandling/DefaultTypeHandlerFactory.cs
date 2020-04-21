using System;
using System.Reflection;
using EnterpriseDB.EDBClient.PostgresTypes;

namespace EnterpriseDB.EDBClient.TypeHandling
{
    /// <summary>
    /// A type handler factory used to instantiate EDB's built-in type handlers.
    /// </summary>
    class DefaultTypeHandlerFactory : EDBTypeHandlerFactory
    {
        readonly Type _handlerType;

        internal DefaultTypeHandlerFactory(Type handlerType)
        {
            // Recursively look for the TypeHandler<T> superclass to extract its T as the
            // DefaultValueType
            Type? baseClass = handlerType;
            while (!baseClass.GetTypeInfo().IsGenericType || baseClass.GetGenericTypeDefinition() != typeof(EDBTypeHandler<>))
            {
                baseClass = baseClass.GetTypeInfo().BaseType;
                if (baseClass == null)
                    throw new Exception($"EDB type handler {handlerType} doesn't inherit from TypeHandler<>?");
            }

            DefaultValueType = baseClass.GetGenericArguments()[0];
            _handlerType = handlerType;
        }

        public override EDBTypeHandler CreateNonGeneric(PostgresType pgType, EDBConnection conn)
            => (EDBTypeHandler)Activator.CreateInstance(_handlerType, pgType)!;

        public override Type DefaultValueType { get; }
    }
}
