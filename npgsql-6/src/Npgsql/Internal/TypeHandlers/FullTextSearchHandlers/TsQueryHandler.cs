using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.BackendMessages;
using EnterpriseDB.EDBClient.Internal.TypeHandling;
using EnterpriseDB.EDBClient.PostgresTypes;
using EDBTypes;

// TODO: Need to work on the nullability here
#nullable disable
#pragma warning disable CS8632

namespace EnterpriseDB.EDBClient.Internal.TypeHandlers.FullTextSearchHandlers
{
    /// <summary>
    /// A type handler for the PostgreSQL tsquery data type.
    /// </summary>
    /// <remarks>
    /// See https://www.postgresql.org/docs/current/static/datatype-textsearch.html.
    ///
    /// The type handler API allows customizing EDB's behavior in powerful ways. However, although it is public, it
    /// should be considered somewhat unstable, and may change in breaking ways, including in non-major releases.
    /// Use it at your own risk.
    /// </remarks>
    public partial class TsQueryHandler : EDBTypeHandler<EDBTsQuery>,
        IEDBTypeHandler<EDBTsQueryEmpty>, IEDBTypeHandler<EDBTsQueryLexeme>,
        IEDBTypeHandler<EDBTsQueryNot>, IEDBTypeHandler<EDBTsQueryAnd>,
        IEDBTypeHandler<EDBTsQueryOr>, IEDBTypeHandler<EDBTsQueryFollowedBy>
    {
        // 1 (type) + 1 (weight) + 1 (is prefix search) + 2046 (max str len) + 1 (null terminator)
        const int MaxSingleTokenBytes = 2050;

        readonly Stack<EDBTsQuery> _stack = new();

        public TsQueryHandler(PostgresType pgType) : base(pgType) {}

        #region Read

        /// <inheritdoc />
        public override async ValueTask<EDBTsQuery> Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription = null)
        {
            await buf.Ensure(4, async);
            var numTokens = buf.ReadInt32();
            if (numTokens == 0)
                return new EDBTsQueryEmpty();

            EDBTsQuery? value = null;
            var nodes = new Stack<Tuple<EDBTsQuery, int>>();
            len -= 4;

            for (var tokenPos = 0; tokenPos < numTokens; tokenPos++)
            {
                await buf.Ensure(Math.Min(len, MaxSingleTokenBytes), async);
                var readPos = buf.ReadPosition;

                var isOper = buf.ReadByte() == 2;
                if (isOper)
                {
                    var operKind = (EDBTsQuery.NodeKind)buf.ReadByte();
                    if (operKind == EDBTsQuery.NodeKind.Not)
                    {
                        var node = new EDBTsQueryNot(null);
                        InsertInTree(node, nodes, ref value);
                        nodes.Push(new Tuple<EDBTsQuery, int>(node, 0));
                    }
                    else
                    {
                        var node = operKind switch
                        {
                            EDBTsQuery.NodeKind.And    => (EDBTsQuery)new EDBTsQueryAnd(null, null),
                            EDBTsQuery.NodeKind.Or     => new EDBTsQueryOr(null, null),
                            EDBTsQuery.NodeKind.Phrase => new EDBTsQueryFollowedBy(null, buf.ReadInt16(), null),
                            _ => throw new InvalidOperationException($"Internal EDB bug: unexpected value {operKind} of enum {nameof(EDBTsQuery.NodeKind)}. Please file a bug.")
                        };

                        InsertInTree(node, nodes, ref value);

                        nodes.Push(new Tuple<EDBTsQuery, int>(node, 1));
                        nodes.Push(new Tuple<EDBTsQuery, int>(node, 2));
                    }
                }
                else
                {
                    var weight = (EDBTsQueryLexeme.Weight)buf.ReadByte();
                    var prefix = buf.ReadByte() != 0;
                    var str = buf.ReadNullTerminatedString();
                    InsertInTree(new EDBTsQueryLexeme(str, weight, prefix), nodes, ref value);
                }

                len -= buf.ReadPosition - readPos;
            }

            if (nodes.Count != 0)
                throw new InvalidOperationException("Internal EDB bug, please report.");

            return value!;

            static void InsertInTree(EDBTsQuery node, Stack<Tuple<EDBTsQuery, int>> nodes, ref EDBTsQuery? value)
            {
                if (nodes.Count == 0)
                    value = node;
                else
                {
                    var parent = nodes.Pop();
                    if (parent.Item2 == 0)
                        ((EDBTsQueryNot)parent.Item1).Child = node;
                    else if (parent.Item2 == 1)
                        ((EDBTsQueryBinOp)parent.Item1).Left = node;
                    else
                        ((EDBTsQueryBinOp)parent.Item1).Right = node;
                }
            }
        }

        async ValueTask<EDBTsQueryEmpty> IEDBTypeHandler<EDBTsQueryEmpty>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => (EDBTsQueryEmpty)await Read(buf, len, async, fieldDescription);

        async ValueTask<EDBTsQueryLexeme> IEDBTypeHandler<EDBTsQueryLexeme>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => (EDBTsQueryLexeme)await Read(buf, len, async, fieldDescription);

        async ValueTask<EDBTsQueryNot> IEDBTypeHandler<EDBTsQueryNot>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => (EDBTsQueryNot)await Read(buf, len, async, fieldDescription);

        async ValueTask<EDBTsQueryAnd> IEDBTypeHandler<EDBTsQueryAnd>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => (EDBTsQueryAnd)await Read(buf, len, async, fieldDescription);

        async ValueTask<EDBTsQueryOr> IEDBTypeHandler<EDBTsQueryOr>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => (EDBTsQueryOr)await Read(buf, len, async, fieldDescription);

        async ValueTask<EDBTsQueryFollowedBy> IEDBTypeHandler<EDBTsQueryFollowedBy>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription? fieldDescription)
            => (EDBTsQueryFollowedBy)await Read(buf, len, async, fieldDescription);

        #endregion Read

        #region Write

        /// <inheritdoc />
        public override int ValidateAndGetLength(EDBTsQuery value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => value.Kind == EDBTsQuery.NodeKind.Empty
                ? 4
                : 4 + GetNodeLength(value);

        int GetNodeLength(EDBTsQuery node)
        {
            // TODO: Figure out the nullability strategy here
            switch (node.Kind)
            {
            case EDBTsQuery.NodeKind.Lexeme:
                var strLen = Encoding.UTF8.GetByteCount(((EDBTsQueryLexeme)node).Text);
                if (strLen > 2046)
                    throw new InvalidCastException("Lexeme text too long. Must be at most 2046 bytes in UTF8.");
                return 4 + strLen;
            case EDBTsQuery.NodeKind.And:
            case EDBTsQuery.NodeKind.Or:
                return 2 + GetNodeLength(((EDBTsQueryBinOp)node).Left) + GetNodeLength(((EDBTsQueryBinOp)node).Right);
            case EDBTsQuery.NodeKind.Phrase:
                // 2 additional bytes for uint16 phrase operator "distance" field.
                return 4 + GetNodeLength(((EDBTsQueryBinOp)node).Left) + GetNodeLength(((EDBTsQueryBinOp)node).Right);
            case EDBTsQuery.NodeKind.Not:
                return 2 + GetNodeLength(((EDBTsQueryNot)node).Child);
            case EDBTsQuery.NodeKind.Empty:
                throw new InvalidOperationException("Empty tsquery nodes must be top-level");
            default:
                throw new InvalidOperationException("Illegal node kind: " + node.Kind);
            }
        }

        /// <inheritdoc />
        public override async Task Write(EDBTsQuery query, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
        {
            var numTokens = GetTokenCount(query);

            if (buf.WriteSpaceLeft < 4)
                await buf.Flush(async, cancellationToken);
            buf.WriteInt32(numTokens);

            if (numTokens == 0)
                return;

            _stack.Push(query);

            while (_stack.Count > 0)
            {
                if (buf.WriteSpaceLeft < 2)
                    await buf.Flush(async, cancellationToken);

                if (_stack.Peek().Kind == EDBTsQuery.NodeKind.Lexeme && buf.WriteSpaceLeft < MaxSingleTokenBytes)
                    await buf.Flush(async, cancellationToken);

                var node = _stack.Pop();
                buf.WriteByte(node.Kind == EDBTsQuery.NodeKind.Lexeme ? (byte)1 : (byte)2);
                if (node.Kind != EDBTsQuery.NodeKind.Lexeme)
                {
                    buf.WriteByte((byte)node.Kind);
                    if (node.Kind == EDBTsQuery.NodeKind.Not)
                        _stack.Push(((EDBTsQueryNot)node).Child);
                    else
                    {
                        if (node.Kind == EDBTsQuery.NodeKind.Phrase)
                            buf.WriteInt16(((EDBTsQueryFollowedBy)node).Distance);

                        _stack.Push(((EDBTsQueryBinOp)node).Left);
                        _stack.Push(((EDBTsQueryBinOp)node).Right);
                    }
                }
                else
                {
                    var lexemeNode = (EDBTsQueryLexeme)node;
                    buf.WriteByte((byte)lexemeNode.Weights);
                    buf.WriteByte(lexemeNode.IsPrefixSearch ? (byte)1 : (byte)0);
                    buf.WriteString(lexemeNode.Text);
                    buf.WriteByte(0);
                }
            }

            _stack.Clear();
        }

        int GetTokenCount(EDBTsQuery node)
        {
            switch (node.Kind)
            {
            case EDBTsQuery.NodeKind.Lexeme:
                return 1;
            case EDBTsQuery.NodeKind.And:
            case EDBTsQuery.NodeKind.Or:
            case EDBTsQuery.NodeKind.Phrase:
                return 1 + GetTokenCount(((EDBTsQueryBinOp)node).Left) + GetTokenCount(((EDBTsQueryBinOp)node).Right);
            case EDBTsQuery.NodeKind.Not:
                return 1 + GetTokenCount(((EDBTsQueryNot)node).Child);
            case EDBTsQuery.NodeKind.Empty:
                return 0;
            }
            return -1;
        }

        /// <inheritdoc />
        public int ValidateAndGetLength(EDBTsQueryOr value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength((EDBTsQuery)value, ref lengthCache, parameter);

        /// <inheritdoc />
        public int ValidateAndGetLength(EDBTsQueryAnd value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength((EDBTsQuery)value, ref lengthCache, parameter);

        /// <inheritdoc />
        public int ValidateAndGetLength(EDBTsQueryNot value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength((EDBTsQuery)value, ref lengthCache, parameter);

        /// <inheritdoc />
        public int ValidateAndGetLength(EDBTsQueryLexeme value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength((EDBTsQuery)value, ref lengthCache, parameter);

        /// <inheritdoc />
        public int ValidateAndGetLength(EDBTsQueryEmpty value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength((EDBTsQuery)value, ref lengthCache, parameter);

        /// <inheritdoc />
        public int ValidateAndGetLength(EDBTsQueryFollowedBy value, ref EDBLengthCache? lengthCache, EDBParameter? parameter)
            => ValidateAndGetLength((EDBTsQuery)value, ref lengthCache, parameter);

        /// <inheritdoc />
        public Task Write(EDBTsQueryOr value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => Write((EDBTsQuery)value, buf, lengthCache, parameter, async, cancellationToken);

        /// <inheritdoc />
        public Task Write(EDBTsQueryAnd value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => Write((EDBTsQuery)value, buf, lengthCache, parameter, async, cancellationToken);

        /// <inheritdoc />
        public Task Write(EDBTsQueryNot value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => Write((EDBTsQuery)value, buf, lengthCache, parameter, async, cancellationToken);

        /// <inheritdoc />
        public Task Write(EDBTsQueryLexeme value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => Write((EDBTsQuery)value, buf, lengthCache, parameter, async, cancellationToken);

        /// <inheritdoc />
        public Task Write(EDBTsQueryEmpty value, EDBWriteBuffer buf, EDBLengthCache? lengthCache, EDBParameter? parameter, bool async, CancellationToken cancellationToken = default)
            => Write((EDBTsQuery)value, buf, lengthCache, parameter, async, cancellationToken);

        /// <inheritdoc />
        public Task Write(
            EDBTsQueryFollowedBy value,
            EDBWriteBuffer buf,
            EDBLengthCache? lengthCache,
            EDBParameter? parameter,
            bool async,
            CancellationToken cancellationToken = default)
            => Write((EDBTsQuery)value, buf, lengthCache, parameter, async, cancellationToken);

        #endregion Write
    }
}
