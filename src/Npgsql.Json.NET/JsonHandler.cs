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
            var s = await base.Read<string>(buf, len, async, fieldDescription);
            if (typeof(T) == typeof(string))
                return (T)(object)s;
            try
            {
                return JsonConvert.DeserializeObject<T>(s, _settings);
            }
            catch (Exception e)
            {
                throw new EDBSafeReadException(e);
            }
        }

        protected override int ValidateAndGetLength<T2>(T2 value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => typeof(T2) == typeof(string)
                ? base.ValidateAndGetLength(value, ref lengthCache, parameter)
                : ValidateObjectAndGetLength(value!, ref lengthCache, parameter);

        protected override Task WriteWithLength<T2>(T2 value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async)
            => typeof(T2) == typeof(string)
                ? base.WriteWithLength(value, buf, lengthCache, parameter, async)
                : WriteObjectWithLength(value!, buf, lengthCache, parameter, async);

        protected override int ValidateObjectAndGetLength(object value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
        {
            var s = value as string;
            if (s == null)
            {
                s = JsonConvert.SerializeObject(value, _settings);
                if (parameter != null)
                    parameter.ConvertedValue = s;
            }
            return base.ValidateAndGetLength(s, ref lengthCache, parameter);
        }

        protected override Task WriteObjectWithLength(object value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async)
        {
            if (value is DBNull)
                return base.WriteObjectWithLength(DBNull.Value, buf, lengthCache, parameter, async);

            if (parameter?.ConvertedValue != null)
                value = parameter.ConvertedValue;
            var s = value as string ?? JsonConvert.SerializeObject(value, _settings);
            return base.WriteObjectWithLength(s, buf, lengthCache, parameter, async);
        }
    }
}
