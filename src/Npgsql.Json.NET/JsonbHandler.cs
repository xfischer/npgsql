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
using System.Threading.Tasks;
using Newtonsoft.Json;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.TypeHandling;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace EnterpriseDB.EDBClient.Json.NET
{
    public class JsonbHandlerFactory : EDBTypeHandlerFactory<string>
    {
        readonly JsonSerializerSettings _settings;

        public JsonbHandlerFactory(JsonSerializerSettings settings) => _settings = settings;

        protected override EDBTypeHandler<string> Create(EDBConnection conn)
            => new JsonbHandler(conn, _settings);
    }

    class JsonbHandler : EnterpriseDB.EDBClient.TypeHandlers.JsonbHandler
    {
        readonly JsonSerializerSettings _settings;

        public JsonbHandler(EDBConnection connection, JsonSerializerSettings settings) : base(connection) => _settings = settings;

        protected override async ValueTask<T> Read<T>(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription = null)
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

        protected override int ValidateAndGetLength<T2>(T2 value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => typeof(T2) == typeof(string)
                ? base.ValidateAndGetLength(value, ref lengthCache, parameter)
                : ValidateObjectAndGetLength(value, ref lengthCache, parameter);
        
        protected override Task WriteWithLength<T2>(T2 value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => typeof(T2) == typeof(string)
                ? base.WriteWithLength(value, buf, lengthCache, parameter, async)
                : WriteObjectWithLength(value, buf, lengthCache, parameter, async);

        protected override int ValidateObjectAndGetLength(object value, ref EDBLengthCache lengthCache, EDBParameter parameter)
        {
            var s = value as string;
            if (s == null)
            {
                s = JsonConvert.SerializeObject(value, _settings);
                if (parameter != null)
                    parameter.ConvertedValue = s;
            }
            return base.ValidateObjectAndGetLength(s, ref lengthCache, parameter);
        }

        protected override Task WriteObjectWithLength(object value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
        {
            if (value == null || value is DBNull)
                return base.WriteObjectWithLength(value, buf, lengthCache, parameter, async);

            if (parameter?.ConvertedValue != null)
                value = parameter.ConvertedValue;
            var s = value as string ?? JsonConvert.SerializeObject(value, _settings);
            return base.WriteObjectWithLength(s, buf, lengthCache, parameter, async);
        }
    }
}
