using System.IO;
using EnterpriseDB.EDBClient.Internal;

namespace EnterpriseDB.EDBClient.Tests.Support;

class PgCancellationRequest(EDBReadBuffer readBuffer, EDBWriteBuffer writeBuffer, Stream stream, int processId, int secret)
{
    public int ProcessId { get; } = processId;
    public int Secret { get; } = secret;

    bool completed;

    public void Complete()
    {
        if (completed)
            return;

        readBuffer.Dispose();
        writeBuffer.Dispose();
        stream.Dispose();

        completed = true;
    }
}