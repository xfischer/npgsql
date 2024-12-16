using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class PathConverter : PgStreamingConverter<EDBPath>
{
    public override EDBPath Read(PgReader reader)
        => Read(async: false, reader, CancellationToken.None).GetAwaiter().GetResult();

    public override ValueTask<EDBPath> ReadAsync(PgReader reader, CancellationToken cancellationToken = default)
        => Read(async: true, reader, cancellationToken);

    async ValueTask<EDBPath> Read(bool async, PgReader reader, CancellationToken cancellationToken)
    {
        if (reader.ShouldBuffer(sizeof(byte) + sizeof(int)))
            await reader.Buffer(async, sizeof(byte) + sizeof(int), cancellationToken).ConfigureAwait(false);

        var open = reader.ReadByte() switch
        {
            1 => false,
            0 => true,
            _ => throw new UnreachableException("Error decoding binary geometric path: bad open byte")
        };

        var numPoints = reader.ReadInt32();
        var result = new EDBPath(numPoints, open);

        for (var i = 0; i < numPoints; i++)
        {
            if (reader.ShouldBuffer(sizeof(double) * 2))
                await reader.Buffer(async, sizeof(double) * 2, cancellationToken).ConfigureAwait(false);

            result.Add(new EDBPoint(reader.ReadDouble(), reader.ReadDouble()));
        }

        return result;
    }

    public override Size GetSize(SizeContext context, EDBPath value, ref object? writeState)
        => 5 + value.Count * sizeof(double) * 2;

    public override void Write(PgWriter writer, EDBPath value)
        => Write(async: false, writer, value, CancellationToken.None).GetAwaiter().GetResult();

    public override ValueTask WriteAsync(PgWriter writer, EDBPath value, CancellationToken cancellationToken = default)
        => Write(async: true, writer, value, cancellationToken);

    async ValueTask Write(bool async, PgWriter writer, EDBPath value, CancellationToken cancellationToken)
    {
        if (writer.ShouldFlush(sizeof(byte) + sizeof(int)))
            await writer.Flush(async, cancellationToken).ConfigureAwait(false);

        writer.WriteByte((byte)(value.Open ? 0 : 1));
        writer.WriteInt32(value.Count);

        foreach (var p in value)
        {
            if (writer.ShouldFlush(sizeof(double) * 2))
                await writer.Flush(async, cancellationToken).ConfigureAwait(false);
            writer.WriteDouble(p.X);
            writer.WriteDouble(p.Y);
        }
    }
}
