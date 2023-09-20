#if NETSTANDARD2_0 || NETFRAMEWORK
using Microsoft.Extensions.Logging;
using System.Buffers;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System.IO
{
    // Helpers to read/write Span/Memory<byte> to Stream before netstandard 2.1
    static class StreamExtensions
    {
        public static int Read(this Stream stream, Span<byte> buffer)
        {
            var sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                var numRead = stream.Read(sharedBuffer, 0, buffer.Length);
                new Span<byte>(sharedBuffer, 0, numRead).CopyTo(buffer);
                return numRead;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }

        // EnterpriseDB Team
        // In .Net framework, stream.ReadAsync hangs when there is no data, even if cancellationToken is canceled
        // This shim wraps the call with the token and a 10s timeout and raises the same exception as .NET core if such cancellation happens
        public static async ValueTask<int> ReadAsync(this Stream stream, Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                // Old implementation
                //var result = await stream.ReadAsync(sharedBuffer, 0, buffer.Length, cancellationToken);
                //new Span<byte>(sharedBuffer, 0, result).CopyTo(buffer.Span);
                //return result;

                var readTask = stream.ReadAsync(sharedBuffer, 0, buffer.Length, cancellationToken);

                Task.WaitAny(new Task[] { readTask }, 10000, cancellationToken);

                if (readTask.IsCompleted)
                {
                    var result = readTask.Result;
                    new Span<byte>(sharedBuffer, 0, result).CopyTo(buffer.Span);
                    return result;
                }
                else
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }
            catch (Exception e)
            {
                //EnterpriseDB.EDBClient.EDBLoggingConfiguration.GlobalLoggerFactory.CreateLogger("StreamExtensions").LogError($"{e.GetType().Name}: {e.Message}");
                cancellationToken.ThrowIfCancellationRequested();
                return 0;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }

        public static void Write(this Stream stream, ReadOnlySpan<byte> buffer)
        {
            var sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            try
            {
                buffer.CopyTo(sharedBuffer);
                stream.Write(sharedBuffer, 0, buffer.Length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }

        public static async ValueTask WriteAsync(this Stream stream, ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            var sharedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
            buffer.Span.CopyTo(sharedBuffer);
            try
            {
                await stream.WriteAsync(sharedBuffer, 0, buffer.Length, cancellationToken);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(sharedBuffer);
            }
        }
    }
}
#endif
