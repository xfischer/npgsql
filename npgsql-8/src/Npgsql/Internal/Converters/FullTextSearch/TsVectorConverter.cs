using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDBTypes;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class TsVectorConverter : PgStreamingConverter<EDBTsVector>
{
    readonly Encoding _encoding;

    public TsVectorConverter(Encoding encoding)
        => _encoding = encoding;

    public override EDBTsVector Read(PgReader reader)
        => Read(async: false, reader, CancellationToken.None).GetAwaiter().GetResult();

    public override ValueTask<EDBTsVector> ReadAsync(PgReader reader, CancellationToken cancellationToken = default)
        => Read(async: true, reader, cancellationToken);

    async ValueTask<EDBTsVector> Read(bool async, PgReader reader, CancellationToken cancellationToken)
    {
        if (reader.ShouldBuffer(sizeof(int)))
            await reader.Buffer(async, sizeof(int), cancellationToken).ConfigureAwait(false);

        var numLexemes = reader.ReadInt32();
        var lexemes = new List<EDBTsVector.Lexeme>(numLexemes);

        for (var i = 0; i < numLexemes; i++)
        {
            var lexemeString = async
                ? await reader.ReadNullTerminatedStringAsync(_encoding, cancellationToken).ConfigureAwait(false)
                : reader.ReadNullTerminatedString(_encoding);

            if (reader.ShouldBuffer(sizeof(short)))
                await reader.Buffer(async, sizeof(short), cancellationToken).ConfigureAwait(false);
            var numPositions = reader.ReadInt16();

            if (numPositions == 0)
            {
                lexemes.Add(new EDBTsVector.Lexeme(lexemeString, wordEntryPositions: null, noCopy: true));
                continue;
            }

            // There can only be a maximum of 256 positions, so we just before them all (256 * sizeof(short) = 512)
            if (numPositions > 256)
                throw new EDBException($"Got {numPositions} lexeme positions when reading tsvector");

            if (reader.ShouldBuffer(numPositions * sizeof(short)))
                await reader.Buffer(async, numPositions * sizeof(short), cancellationToken).ConfigureAwait(false);

            var positions = new List<EDBTsVector.Lexeme.WordEntryPos>(numPositions);

            for (var j = 0; j < numPositions; j++)
            {
                var wordEntryPos = reader.ReadInt16();
                positions.Add(new EDBTsVector.Lexeme.WordEntryPos(wordEntryPos));
            }

            lexemes.Add(new EDBTsVector.Lexeme(lexemeString, positions, noCopy: true));
        }

        return new EDBTsVector(lexemes, noCheck: true);
    }

    public override Size GetSize(SizeContext context, EDBTsVector value, ref object? writeState)
    {
        var size = 4;
        foreach (var l in value)
            size += _encoding.GetByteCount(l.Text) + 1 + 2 + l.Count * 2;

        return size;
    }

    public override void Write(PgWriter writer, EDBTsVector value)
        => Write(async: false, writer, value, CancellationToken.None).GetAwaiter().GetResult();

    public override ValueTask WriteAsync(PgWriter writer, EDBTsVector value, CancellationToken cancellationToken = default)
        => Write(async: true, writer, value, cancellationToken);

    async ValueTask Write(bool async, PgWriter writer, EDBTsVector value, CancellationToken cancellationToken)
    {
        if (writer.ShouldFlush(sizeof(int)))
            await writer.FlushAsync(cancellationToken).ConfigureAwait(false);
        writer.WriteInt32(value.Count);

        foreach (var lexeme in value)
        {
            if (async)
                await writer.WriteCharsAsync(lexeme.Text.AsMemory(), _encoding, cancellationToken).ConfigureAwait(false);
            else
                writer.WriteChars(lexeme.Text.AsMemory().Span, _encoding);

            if (writer.ShouldFlush(sizeof(byte) + sizeof(short)))
                await writer.Flush(async, cancellationToken).ConfigureAwait(false);

            writer.WriteByte(0);
            writer.WriteInt16((short)lexeme.Count);

            for (var i = 0; i < lexeme.Count; i++)
            {
                if (writer.ShouldFlush(sizeof(short)))
                    await writer.Flush(async, cancellationToken).ConfigureAwait(false);

                writer.WriteInt16(lexeme[i].Value);
            }
        }
    }
}
