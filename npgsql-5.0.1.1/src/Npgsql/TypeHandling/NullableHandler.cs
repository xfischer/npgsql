using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;

// ReSharper disable StaticMemberInGenericType
namespace EnterpriseDB.EDBClient.TypeHandling
{
    delegate T ReadDelegate<T>(EDBTypeHandler handler, EDBReadBuffer buffer, int columnLength, FieldDescription? fieldDescription = null);
    delegate ValueTask<T> ReadAsyncDelegate<T>(EDBTypeHandler handler, EDBReadBuffer buffer, int columnLen, bool async, FieldDescription? fieldDescription = null);

    delegate int ValidateAndGetLengthDelegate<T>(EDBTypeHandler handler, T value, ref EDBLengthCache? lengthCache, EDBParameter? parameter);
    delegate Task WriteAsyncDelegate<T>(EDBTypeHandler handler, T value, EDBWriteBuffer buffer, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default);

    static class NullableHandler<T>
    {
        public static readonly Type? UnderlyingType;
        [NotNull] public static readonly ReadDelegate<T>? Read;
        [NotNull] public static readonly ReadAsyncDelegate<T>? ReadAsync;
        [NotNull] public static readonly ValidateAndGetLengthDelegate<T>? ValidateAndGetLength;
        [NotNull] public static readonly WriteAsyncDelegate<T>? WriteAsync;

        public static bool Exists => UnderlyingType != null;

        static NullableHandler()
        {
            UnderlyingType = Nullable.GetUnderlyingType(typeof(T));

            if (UnderlyingType == null)
                return;

            Read = NullableHandler.CreateDelegate<ReadDelegate<T>>(UnderlyingType, NullableHandler.ReadMethod);
            ReadAsync = NullableHandler.CreateDelegate<ReadAsyncDelegate<T>>(UnderlyingType, NullableHandler.ReadAsyncMethod);
            ValidateAndGetLength = NullableHandler.CreateDelegate<ValidateAndGetLengthDelegate<T>>(UnderlyingType, NullableHandler.ValidateMethod);
            WriteAsync = NullableHandler.CreateDelegate<WriteAsyncDelegate<T>>(UnderlyingType, NullableHandler.WriteAsyncMethod);
        }
    }

    static class NullableHandler
    {
        internal static readonly MethodInfo ReadMethod = new ReadDelegate<int?>(Read<int>).Method.GetGenericMethodDefinition();
        internal static readonly MethodInfo ReadAsyncMethod = new ReadAsyncDelegate<int?>(ReadAsync<int>).Method.GetGenericMethodDefinition();
        internal static readonly MethodInfo ValidateMethod = new ValidateAndGetLengthDelegate<int?>(ValidateAndGetLength).Method.GetGenericMethodDefinition();
        internal static readonly MethodInfo WriteAsyncMethod = new WriteAsyncDelegate<int?>(WriteAsync).Method.GetGenericMethodDefinition();

        static T? Read<T>(EDBTypeHandler handler, EDBReadBuffer buffer, int columnLength, FieldDescription? fieldDescription)
            where T : struct
            => handler.Read<T>(buffer, columnLength, fieldDescription);

        static async ValueTask<T?> ReadAsync<T>(EDBTypeHandler handler, EDBReadBuffer buffer, int columnLength, bool async, FieldDescription? fieldDescription)
            where T : struct
            => await handler.Read<T>(buffer, columnLength, async, fieldDescription);

        static int ValidateAndGetLength<T>(EDBTypeHandler handler, T? value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            where T : struct
            => value.HasValue ? handler.ValidateAndGetLength(value.Value, ref lengthCache, parameter) : 0;

        static Task WriteAsync<T>(EDBTypeHandler handler, T? value, EDBWriteBuffer buffer, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            where T : struct
            => value.HasValue
                ? handler.WriteWithLengthInternal(value.Value, buffer, lengthCache, parameter, async, cancellationToken)
                : handler.WriteWithLengthInternal(DBNull.Value, buffer, lengthCache, parameter, async, cancellationToken);

        internal static TDelegate CreateDelegate<TDelegate>(Type underlyingType, MethodInfo method)
            where TDelegate : Delegate
            => (TDelegate)method.MakeGenericMethod(underlyingType).CreateDelegate(typeof(TDelegate));
    }
}
