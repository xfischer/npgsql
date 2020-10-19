using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.PostgresTypes;
using EnterpriseDB.EDBClient.TypeHandling;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.Json.NET
{
    public class JsonHandlerFactory : EDBTypeHandlerFactory<string>
    {
        readonly JsonSerializerSettings _settings;

        public JsonHandlerFactory(JsonSerializerSettings? settings = null)
            => _settings = settings ?? new JsonSerializerSettings();

        public override EDBTypeHandler<string> Create(PostgresType postgresType, EDBConnection conn)
            => new JsonHandler(postgresType, conn, _settings);
    }

    class JsonHandler : TypeHandlers.TextHandler
    {
        readonly JsonSerializerSettings _settings;

        public JsonHandler(PostgresType postgresType, EDBConnection connection, JsonSerializerSettings settings)
            : base(postgresType, connection) => _settings = settings;

        protected override async ValueTask<T> Read<T>(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        {
            if (typeof(T) == typeof(string) ||
                typeof(T) == typeof(char[]) ||
                typeof(T) == typeof(ArraySegment<char>) ||
                typeof(T) == typeof(char) ||
                typeof(T) == typeof(byte[]))
            {
                return await base.Read<T>(buf, len, async, fieldDescription);
            }

            try
            {
                return JsonConvert.DeserializeObject<T>(await base.Read<string>(buf, len, async, fieldDescription), _settings);
            }
            catch (Exception e)
            {
                throw new EDBSafeReadException(e);
            }
        }

        protected override int ValidateAndGetLength<T2>(T2 value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        {
            if (typeof(T2) == typeof(string) ||
                typeof(T2) == typeof(char[]) ||
                typeof(T2) == typeof(ArraySegment<char>) ||
                typeof(T2) == typeof(char) ||
                typeof(T2) == typeof(byte[]))
            {
                return base.ValidateAndGetLength(value, ref lengthCache, parameter);
            }

            var serialized = JsonConvert.SerializeObject(value, _settings);
            if (parameter != null)
                parameter.ConvertedValue = serialized;
            return base.ValidateAndGetLength(serialized, ref lengthCache, parameter);
        }

        protected override Task WriteWithLength<T2>(T2 value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async)
        {
            if (typeof(T2) == typeof(string) ||
                typeof(T2) == typeof(char[]) ||
                typeof(T2) == typeof(ArraySegment<char>) ||
                typeof(T2) == typeof(char) ||
                typeof(T2) == typeof(byte[]))
            {
                return base.WriteWithLength(value, buf, lengthCache, parameter, async);
            }

            // User POCO, read serialized representation from the validation phase
            var serialized = parameter?.ConvertedValue != null
                ? (string)parameter.ConvertedValue
                : JsonConvert.SerializeObject(value, _settings);
            return base.WriteWithLength(serialized, buf, lengthCache, parameter, async);
        }

        protected override int ValidateObjectAndGetLength(object value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        {
            if (value is null ||
                value is DBNull ||
                value is string ||
                value is char[] ||
                value is ArraySegment<char> ||
                value is char ||
                value is byte[])
            {
                return base.ValidateObjectAndGetLength(value!, ref lengthCache, parameter);
            }

            return ValidateAndGetLength(value, ref lengthCache, parameter);
        }

        protected override Task WriteObjectWithLength(object value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async)
        {
            if (value is null ||
                value is DBNull ||
                value is string ||
                value is char[] ||
                value is ArraySegment<char> ||
                value is char ||
                value is byte[])
            {
                return base.WriteObjectWithLength(value!, buf, lengthCache, parameter, async);
            }

            return WriteWithLength(value, buf, lengthCache, parameter, async);
        }
    }
}
