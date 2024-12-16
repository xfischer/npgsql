using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EDBTypes;
using static EDBTypes.EDBTsQuery.NodeKind;

// ReSharper disable once CheckNamespace
namespace EnterpriseDB.EDBClient.Internal.Converters;

sealed class TsQueryConverter<T> : PgStreamingConverter<T>
    where T : EDBTsQuery
{
    readonly Encoding _encoding;

    public TsQueryConverter(Encoding encoding)
        => _encoding = encoding;

    public override T Read(PgReader reader)
        => (T)Read(async: false, reader, CancellationToken.None).GetAwaiter().GetResult();

    public override async ValueTask<T> ReadAsync(PgReader reader, CancellationToken cancellationToken = default)
        => (T)await Read(async: true, reader, cancellationToken).ConfigureAwait(false);

    async ValueTask<EDBTsQuery> Read(bool async, PgReader reader, CancellationToken cancellationToken)
    {
        if (reader.ShouldBuffer(sizeof(int)))
            await reader.Buffer(async, sizeof(int), cancellationToken).ConfigureAwait(false);
        var numTokens = reader.ReadInt32();
        if (numTokens == 0)
            return new EDBTsQueryEmpty();

        EDBTsQuery? value = null;
        var nodes = new Stack<(EDBTsQuery Node, int Location)>();

        for (var i = 0; i < numTokens; i++)
        {
            if (reader.ShouldBuffer(sizeof(byte)))
                await reader.Buffer(async, sizeof(byte), cancellationToken).ConfigureAwait(false);

            switch (reader.ReadByte())
            {
            case 1: // lexeme
                if (reader.ShouldBuffer(sizeof(byte) + sizeof(byte)))
                    await reader.Buffer(async, sizeof(byte) + sizeof(byte), cancellationToken).ConfigureAwait(false);
                var weight = (EDBTsQueryLexeme.Weight)reader.ReadByte();
                var prefix = reader.ReadByte() != 0;

                var str = async
                    ? await reader.ReadNullTerminatedStringAsync(_encoding, cancellationToken).ConfigureAwait(false)
                    : reader.ReadNullTerminatedString(_encoding);
                InsertInTree(new EDBTsQueryLexeme(str, weight, prefix), nodes, ref value);
                continue;

            case 2: // operation
                if (reader.ShouldBuffer(sizeof(byte)))
                    await reader.Buffer(async, sizeof(byte), cancellationToken).ConfigureAwait(false);
                var kind = (EDBTsQuery.NodeKind)reader.ReadByte();

                EDBTsQuery node;
                switch (kind)
                {
                case Not:
                    node = new EDBTsQueryNot(null!);
                    InsertInTree(node, nodes, ref value);
                    nodes.Push((node, 0));
                    continue;

                case And:
                    node = new EDBTsQueryAnd(null!, null!);
                    break;
                case Or:
                    node = new EDBTsQueryOr(null!, null!);
                    break;
                case Phrase:
                    if (reader.ShouldBuffer(sizeof(short)))
                        await reader.Buffer(async, sizeof(short), cancellationToken).ConfigureAwait(false);
                    node = new EDBTsQueryFollowedBy(null!, reader.ReadInt16(), null!);
                    break;
                default:
                    throw new UnreachableException(
                        $"Internal EDB bug: unexpected value {kind} of enum {nameof(EDBTsQuery.NodeKind)}. Please file a bug.");
                }

                InsertInTree(node, nodes, ref value);

                nodes.Push((node, 1));
                nodes.Push((node, 2));
                continue;

            case var tokenType:
                throw new UnreachableException(
                    $"Internal EDB bug: unexpected token type {tokenType} when reading tsquery. Please file a bug.");
            }
        }

        if (nodes.Count != 0)
            throw new UnreachableException("Internal EDB bug, please report.");

        return value!;

        static void InsertInTree(EDBTsQuery node, Stack<(EDBTsQuery Node, int Location)> nodes, ref EDBTsQuery? value)
        {
            if (nodes.Count == 0)
                value = node;
            else
            {
                var parent = nodes.Pop();
                switch (parent.Location)
                {
                case 0:
                    ((EDBTsQueryNot)parent.Node).Child = node;
                    break;
                case 1:
                    ((EDBTsQueryBinOp)parent.Node).Left = node;
                    break;
                case 2:
                    ((EDBTsQueryBinOp)parent.Node).Right = node;
                    break;
                default:
                    throw new UnreachableException("Internal EDB bug, please report.");
                }
            }
        }
    }

    public override Size GetSize(SizeContext context, T value, ref object? writeState)
        => value.Kind is Empty
            ? 4
            : 4 + GetNodeLength(value);

    int GetNodeLength(EDBTsQuery node)
        => node.Kind switch
        {
            Lexeme when _encoding.GetByteCount(((EDBTsQueryLexeme)node).Text) is var strLen
                => strLen > 2046
                    ? throw new InvalidCastException("Lexeme text too long. Must be at most 2046 encoded bytes.")
                    : 4 + strLen,
            And or Or => 2 + GetNodeLength(((EDBTsQueryBinOp)node).Left) + GetNodeLength(((EDBTsQueryBinOp)node).Right),
            Not => 2 + GetNodeLength(((EDBTsQueryNot)node).Child),
            Empty => throw new InvalidOperationException("Empty tsquery nodes must be top-level"),

            // 2 additional bytes for uint16 phrase operator "distance" field.
            Phrase => 4 + GetNodeLength(((EDBTsQueryBinOp)node).Left) + GetNodeLength(((EDBTsQueryBinOp)node).Right),

            _ => throw new UnreachableException(
                $"Internal EDB bug: unexpected value {node.Kind} of enum {nameof(EDBTsQuery.NodeKind)}. Please file a bug.")
        };

    public override void Write(PgWriter writer, T value)
        => Write(async: false, writer, value, CancellationToken.None).GetAwaiter().GetResult();

    public override ValueTask WriteAsync(PgWriter writer, T value, CancellationToken cancellationToken = default)
        => Write(async: true, writer, value, cancellationToken);

    async ValueTask Write(bool async, PgWriter writer, EDBTsQuery value, CancellationToken cancellationToken)
    {
        var numTokens = GetTokenCount(value);

        if (writer.ShouldFlush(sizeof(int)))
            await writer.Flush(async, cancellationToken).ConfigureAwait(false);
        writer.WriteInt32(numTokens);

        if (numTokens is 0)
            return;

        await WriteCore(value).ConfigureAwait(false);

        async Task WriteCore(EDBTsQuery node)
        {
            if (writer.ShouldFlush(sizeof(byte)))
                await writer.Flush(async, cancellationToken).ConfigureAwait(false);
            writer.WriteByte(node.Kind is Lexeme ? (byte)1 : (byte)2);

            if (node.Kind is Lexeme)
            {
                var lexemeNode = (EDBTsQueryLexeme)node;

                if (writer.ShouldFlush(sizeof(byte) + sizeof(byte)))
                    await writer.Flush(async, cancellationToken).ConfigureAwait(false);

                writer.WriteByte((byte)lexemeNode.Weights);
                writer.WriteByte(lexemeNode.IsPrefixSearch ? (byte)1 : (byte)0);

                if (async)
                    await writer.WriteCharsAsync(lexemeNode.Text.AsMemory(), _encoding, cancellationToken).ConfigureAwait(false);
                else
                    writer.WriteChars(lexemeNode.Text.AsMemory().Span, _encoding);

                if (writer.ShouldFlush(sizeof(byte)))
                    await writer.Flush(async, cancellationToken).ConfigureAwait(false);

                writer.WriteByte(0);
                return;
            }

            writer.WriteByte((byte)node.Kind);

            switch (node.Kind)
            {
            case Not:
                await WriteCore(((EDBTsQueryNot)node).Child).ConfigureAwait(false);
                return;
            case Phrase:
                writer.WriteInt16(((EDBTsQueryFollowedBy)node).Distance);
                break;
            }

            await WriteCore(((EDBTsQueryBinOp)node).Right).ConfigureAwait(false);
            await WriteCore(((EDBTsQueryBinOp)node).Left).ConfigureAwait(false);
        }
    }

    int GetTokenCount(EDBTsQuery node)
        => node.Kind switch
        {
            Lexeme => 1,
            And or Or or Phrase => 1 + GetTokenCount(((EDBTsQueryBinOp)node).Left) + GetTokenCount(((EDBTsQueryBinOp)node).Right),
            Not => 1 + GetTokenCount(((EDBTsQueryNot)node).Child),
            Empty => 0,

            _ => throw new UnreachableException(
                $"Internal EDB bug: unexpected value {node.Kind} of enum {nameof(EDBTsQuery.NodeKind)}. Please file a bug.")
        };
}
