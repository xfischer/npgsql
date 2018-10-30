#region License
// The PostgreSQL License
//
// Copyright (C) 2018 The EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using EnterpriseDB.EDBClient.TypeHandling;
using EnterpriseDB.EDBClient.TypeMapping;

namespace EnterpriseDB.EDBClient.TypeHandlers.FullTextSearchHandlers
{
    /// <summary>
    /// http://www.postgresql.org/docs/current/static/datatype-textsearch.html
    /// </summary>
    [TypeMapping("tsquery", EDBDbType.TsQuery, new[] {
        typeof(EDBTsQuery), typeof(EDBTsQueryAnd), typeof(EDBTsQueryEmpty), typeof(EDBTsQueryFollowedBy),
        typeof(EDBTsQueryLexeme), typeof(EDBTsQueryNot), typeof(EDBTsQueryOr), typeof(EDBTsQueryBinOp) })
    ]
    class TsQueryHandler : EDBTypeHandler<EDBTsQuery>,
        IEDBTypeHandler<EDBTsQueryEmpty>, IEDBTypeHandler<EDBTsQueryLexeme>,
        IEDBTypeHandler<EDBTsQueryNot>, IEDBTypeHandler<EDBTsQueryAnd>,
        IEDBTypeHandler<EDBTsQueryOr>, IEDBTypeHandler<EDBTsQueryFollowedBy>
    {
        // 1 (type) + 1 (weight) + 1 (is prefix search) + 2046 (max str len) + 1 (null terminator)
        const int MaxSingleTokenBytes = 2050;

        Stack<Tuple<EDBTsQuery, int>> _nodes;
        EDBTsQuery _value;

        readonly Stack<EDBTsQuery> _stack = new Stack<EDBTsQuery>();

        #region Read

        public override async ValueTask<EDBTsQuery> Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription = null)
        {
            await buf.Ensure(4, async);
            var numTokens = buf.ReadInt32();
            if (numTokens == 0)
                return new EDBTsQueryEmpty();

            _nodes = new Stack<Tuple<EDBTsQuery, int>>();
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
                        InsertInTree(node);
                        _nodes.Push(new Tuple<EDBTsQuery, int>(node, 0));
                    }
                    else
                    {
                        EDBTsQuery node;
                        switch (operKind)
                        {
                        case EDBTsQuery.NodeKind.And:
                            node = new EDBTsQueryAnd(null, null);
                            break;
                        case EDBTsQuery.NodeKind.Or:
                            node = new EDBTsQueryOr(null, null);
                            break;
                        case EDBTsQuery.NodeKind.Phrase:
                            var distance = buf.ReadInt16();
                            node = new EDBTsQueryFollowedBy(null, distance, null);
                            break;
                        default:
                            throw new InvalidOperationException($"Internal EnterpriseDB.EDBClient bug: unexpected value {operKind} of enum {nameof(EDBTsQuery.NodeKind)}. Please file a bug.");
                        }

                        InsertInTree(node);

                        _nodes.Push(new Tuple<EDBTsQuery, int>(node, 2));
                        _nodes.Push(new Tuple<EDBTsQuery, int>(node, 1));
                    }
                }
                else
                {
                    var weight = (EDBTsQueryLexeme.Weight)buf.ReadByte();
                    var prefix = buf.ReadByte() != 0;
                    var str = buf.ReadNullTerminatedString();
                    InsertInTree(new EDBTsQueryLexeme(str, weight, prefix));
                }

                len -= buf.ReadPosition - readPos;
            }

            if (_nodes.Count != 0)
                throw new InvalidOperationException("Internal EnterpriseDB.EDBClient bug, please report.");

            return _value;
        }

        async ValueTask<EDBTsQueryEmpty> IEDBTypeHandler<EDBTsQueryEmpty>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => (EDBTsQueryEmpty)await Read(buf, len, async, fieldDescription);

        async ValueTask<EDBTsQueryLexeme> IEDBTypeHandler<EDBTsQueryLexeme>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => (EDBTsQueryLexeme)await Read(buf, len, async, fieldDescription);

        async ValueTask<EDBTsQueryNot> IEDBTypeHandler<EDBTsQueryNot>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => (EDBTsQueryNot)await Read(buf, len, async, fieldDescription);

        async ValueTask<EDBTsQueryAnd> IEDBTypeHandler<EDBTsQueryAnd>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => (EDBTsQueryAnd)await Read(buf, len, async, fieldDescription);

        async ValueTask<EDBTsQueryOr> IEDBTypeHandler<EDBTsQueryOr>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => (EDBTsQueryOr)await Read(buf, len, async, fieldDescription);

        async ValueTask<EDBTsQueryFollowedBy> IEDBTypeHandler<EDBTsQueryFollowedBy>.Read(EDBReadBuffer buf, int len, bool async, FieldDescription fieldDescription)
            => (EDBTsQueryFollowedBy)await Read(buf, len, async, fieldDescription);

        #endregion Read

        #region Write

        void InsertInTree([CanBeNull] EDBTsQuery node)
        {
            if (_nodes.Count == 0)
                _value = node;
            else
            {
                var parent = _nodes.Pop();
                if (parent.Item2 == 0)
                    ((EDBTsQueryNot)parent.Item1).Child = node;
                else if (parent.Item2 == 1)
                    ((EDBTsQueryBinOp)parent.Item1).Left = node;
                else
                    ((EDBTsQueryBinOp)parent.Item1).Right = node;
            }
        }

        public override int ValidateAndGetLength(EDBTsQuery value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => value.Kind == EDBTsQuery.NodeKind.Empty
                ? 4
                : 4 + GetNodeLength(value);

        int GetNodeLength(EDBTsQuery node)
        {
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

        public override async Task Write(EDBTsQuery query, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
        {
            var numTokens = GetTokenCount(query);

            if (buf.WriteSpaceLeft < 4)
                await buf.Flush(async);
            buf.WriteInt32(numTokens);

            if (numTokens == 0)
                return;

            _stack.Push(query);

            while (_stack.Count > 0)
            {
                if (buf.WriteSpaceLeft < 2)
                    await buf.Flush(async);

                if (_stack.Peek().Kind == EDBTsQuery.NodeKind.Lexeme && buf.WriteSpaceLeft < MaxSingleTokenBytes)
                    await buf.Flush(async);

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

                        _stack.Push(((EDBTsQueryBinOp)node).Right);
                        _stack.Push(((EDBTsQueryBinOp)node).Left);
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

        public int ValidateAndGetLength(EDBTsQueryOr value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength((EDBTsQuery)value, ref lengthCache, parameter);

        public int ValidateAndGetLength(EDBTsQueryAnd value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength((EDBTsQuery)value, ref lengthCache, parameter);

        public int ValidateAndGetLength(EDBTsQueryNot value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength((EDBTsQuery)value, ref lengthCache, parameter);

        public int ValidateAndGetLength(EDBTsQueryLexeme value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength((EDBTsQuery)value, ref lengthCache, parameter);

        public int ValidateAndGetLength(EDBTsQueryEmpty value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength((EDBTsQuery)value, ref lengthCache, parameter);

        public int ValidateAndGetLength(EDBTsQueryFollowedBy value, ref EDBLengthCache lengthCache, EDBParameter parameter)
            => ValidateAndGetLength((EDBTsQuery)value, ref lengthCache, parameter);

        public Task Write(EDBTsQueryOr value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => Write((EDBTsQuery)value, buf, lengthCache, parameter, async);

        public Task Write(EDBTsQueryAnd value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => Write((EDBTsQuery)value, buf, lengthCache, parameter, async);

        public Task Write(EDBTsQueryNot value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => Write((EDBTsQuery)value, buf, lengthCache, parameter, async);

        public Task Write(EDBTsQueryLexeme value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => Write((EDBTsQuery)value, buf, lengthCache, parameter, async);

        public Task Write(EDBTsQueryEmpty value, EDBWriteBuffer buf, EDBLengthCache lengthCache, EDBParameter parameter, bool async)
            => Write((EDBTsQuery)value, buf, lengthCache, parameter, async);

        public Task Write(
            EDBTsQueryFollowedBy value,
            EDBWriteBuffer buf,
            EDBLengthCache lengthCache,
            EDBParameter parameter,
            bool async)
            => Write((EDBTsQuery)value, buf, lengthCache, parameter, async);

        #endregion Write
    }
}
