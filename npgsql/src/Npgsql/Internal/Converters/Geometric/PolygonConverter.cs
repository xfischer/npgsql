using System.Threading;
using System.Threading.Tasks;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class PolygonConverter : PgStreamingConverter<EDBPolygon>
{
    public override EDBPolygon Read(PgReader reader)
        => Read(async: false, reader, CancellationToken.None).GetAwaiter().GetResult();

    public override ValueTask<EDBPolygon> ReadAsync(PgReader reader, CancellationToken cancellationToken = default)
        => Read(async: true, reader, cancellationToken);

    async ValueTask<EDBPolygon> Read(bool async, PgReader reader, CancellationToken cancellationToken)
    {
        if (reader.ShouldBuffer(sizeof(int)))
            await reader.Buffer(async, sizeof(int), cancellationToken).ConfigureAwait(false);
        var numPoints = reader.ReadInt32();
        var result = new EDBPolygon(numPoints);
        for (var i = 0; i < numPoints; i++)
        {
            if (reader.ShouldBuffer(sizeof(double) * 2))
                await reader.Buffer(async, sizeof(double) * 2, cancellationToken).ConfigureAwait(false);
            result.Add(new EDBPoint(reader.ReadDouble(), reader.ReadDouble()));
        }

        return result;
    }

    public override Size GetSize(SizeContext context, EDBPolygon value, ref object? writeState)
        => 4 + value.Count * sizeof(double) * 2;

    public override void Write(PgWriter writer, EDBPolygon value)
        => Write(async: false, writer, value, CancellationToken.None).GetAwaiter().GetResult();

    public override ValueTask WriteAsync(PgWriter writer, EDBPolygon value, CancellationToken cancellationToken = default)
        => Write(async: true, writer, value, cancellationToken);

    async ValueTask Write(bool async, PgWriter writer, EDBPolygon value, CancellationToken cancellationToken)
    {
        if (writer.ShouldFlush(sizeof(int)))
            await writer.Flush(async, cancellationToken).ConfigureAwait(false);
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
