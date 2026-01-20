using System.Threading;
using System.Threading.Tasks;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class CubeConverter : PgStreamingConverter<EDBCube>
{
    const uint PointBit = 0x80000000;
    const int DimMask = 0x7fffffff;

    public override EDBCube Read(PgReader reader)
        => Read(async: false, reader, CancellationToken.None).GetAwaiter().GetResult();

    public override ValueTask<EDBCube> ReadAsync(PgReader reader, CancellationToken cancellationToken = default)
        => Read(async: true, reader, cancellationToken);

    async ValueTask<EDBCube> Read(bool async, PgReader reader, CancellationToken cancellationToken)
    {
        if (reader.ShouldBuffer(sizeof(int)))
            await reader.Buffer(async, sizeof(int), cancellationToken).ConfigureAwait(false);

        var header = reader.ReadInt32();
        var dim = header & DimMask;
        var point = (header & PointBit) != 0;

        var lowerLeft = new double[dim];
        for (var i = 0; i < dim; i++)
        {
            if (reader.ShouldBuffer(sizeof(double)))
                await reader.Buffer(async, sizeof(double), cancellationToken).ConfigureAwait(false);
            lowerLeft[i] = reader.ReadDouble();
        }

        if (point)
            return new EDBCube(lowerLeft);

        var upperRight = new double[dim];
        for (var i = 0; i < dim; i++)
        {
            if (reader.ShouldBuffer(sizeof(double)))
                await reader.Buffer(async, sizeof(double), cancellationToken).ConfigureAwait(false);
            upperRight[i] = reader.ReadDouble();
        }

        return new EDBCube(lowerLeft, upperRight);
    }

    public override Size GetSize(SizeContext context, EDBCube value, ref object? writeState)
        => sizeof(int) + sizeof(double) * (value.IsPoint ? value.Dimensions : value.Dimensions * 2);

    public override void Write(PgWriter writer, EDBCube value)
        => Write(async: false, writer, value, CancellationToken.None).GetAwaiter().GetResult();

    public override ValueTask WriteAsync(PgWriter writer, EDBCube value, CancellationToken cancellationToken = default)
        => Write(async: true, writer, value, cancellationToken);

    async ValueTask Write(bool async, PgWriter writer, EDBCube value, CancellationToken cancellationToken)
    {
        if (writer.ShouldFlush(sizeof(int)))
            await writer.Flush(async, cancellationToken).ConfigureAwait(false);

        var header = value.Dimensions;
        if (value.IsPoint)
            header |= 1 << 31;

        writer.WriteInt32(header);

        for (var i = 0; i < value.Dimensions; i++)
        {
            if (writer.ShouldFlush(sizeof(double)))
                await writer.Flush(async, cancellationToken).ConfigureAwait(false);
            writer.WriteDouble(value.LowerLeft[i]);
        }

        if (value.IsPoint)
            return;

        for (var i = 0; i < value.Dimensions; i++)
        {
            if (writer.ShouldFlush(sizeof(double)))
                await writer.Flush(async, cancellationToken).ConfigureAwait(false);
            writer.WriteDouble(value.UpperRight[i]);
        }
    }
}
