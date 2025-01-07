using EnterpriseDB.EDBClient.BackendMessages;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using EnterpriseDB.EDBClient.Internal;

namespace EnterpriseDB.EDBClient;

static class ThrowHelper
{
    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRangeException()
        => throw new ArgumentOutOfRangeException();

    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRangeException(string paramName, string message)
        => throw new ArgumentOutOfRangeException(paramName, message);

    [DoesNotReturn]
    internal static void ThrowArgumentOutOfRangeException(string paramName, string message, object argument)
        => throw new ArgumentOutOfRangeException(paramName, string.Format(message, argument));

    [DoesNotReturn]
    internal static void ThrowUnreachableException(string message, object argument)
        => throw new UnreachableException(string.Format(message, argument));

    [DoesNotReturn]
    internal static void ThrowInvalidOperationException()
        => throw new InvalidOperationException();

    [DoesNotReturn]
    internal static void ThrowInvalidOperationException(string message)
        => throw new InvalidOperationException(message);

    [DoesNotReturn]
    internal static void ThrowInvalidOperationException(string message, object argument)
        => throw new InvalidOperationException(string.Format(message, argument));

    [DoesNotReturn]
    internal static void ThrowObjectDisposedException(string? objectName)
        => throw new ObjectDisposedException(objectName);

    [DoesNotReturn]
    internal static void ThrowObjectDisposedException(string objectName, string message)
        => throw new ObjectDisposedException(objectName, message);

    [DoesNotReturn]
    internal static void ThrowObjectDisposedException(string objectName, Exception? innerException)
        => throw new ObjectDisposedException(objectName, innerException);

    [DoesNotReturn]
    internal static void ThrowInvalidCastException(string message, object argument)
        => throw new InvalidCastException(string.Format(message, argument));

    [DoesNotReturn]
    internal static void ThrowInvalidCastException_NoValue(FieldDescription field) =>
        throw new InvalidCastException($"Column '{field.Name}' is null.");

    [DoesNotReturn]
    internal static void ThrowInvalidCastException(string message) =>
        throw new InvalidCastException(message);

    [DoesNotReturn]
    internal static void ThrowInvalidCastException_NoValue() =>
        throw new InvalidCastException("Field is null.");

    [DoesNotReturn]
    internal static void ThrowEDBException(string message)
        => throw new EDBException(message);

    [DoesNotReturn]
    internal static void ThrowEDBException(string message, Exception? innerException)
        => throw new EDBException(message, innerException);

    [DoesNotReturn]
    internal static void ThrowEDBOperationInProgressException(EDBCommand command)
        => throw new EDBOperationInProgressException(command);

    [DoesNotReturn]
    internal static void ThrowEDBOperationInProgressException(ConnectorState state)
        => throw new EDBOperationInProgressException(state);

    [DoesNotReturn]
    internal static void ThrowArgumentException(string message)
        => throw new ArgumentException(message);

    [DoesNotReturn]
    internal static void ThrowArgumentException(string message, string paramName)
        => throw new ArgumentException(message, paramName);

    [DoesNotReturn]
    internal static void ThrowArgumentNullException(string paramName)
        => throw new ArgumentNullException(paramName);

    [DoesNotReturn]
    internal static void ThrowArgumentNullException(string message, string paramName)
        => throw new ArgumentNullException(paramName, message);

    [DoesNotReturn]
    internal static void ThrowIndexOutOfRangeException(string message)
        => throw new IndexOutOfRangeException(message);

    [DoesNotReturn]
    internal static void ThrowIndexOutOfRangeException(string message, object argument)
        => throw new IndexOutOfRangeException(string.Format(message, argument));

    [DoesNotReturn]
    internal static void ThrowNotSupportedException(string? message = null)
        => throw new NotSupportedException(message);

    [DoesNotReturn]
    internal static void ThrowEDBExceptionWithInnerTimeoutException(string message)
        => throw new EDBException(message, new TimeoutException());
}