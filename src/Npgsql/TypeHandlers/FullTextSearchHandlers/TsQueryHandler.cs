#region License
// The PostgreSQL License
//
// Copyright (C) 2016 The  EnterpriseDB.EDBClient Development Team
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE  EnterpriseDB.EDBClient DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
#endregion

using  EnterpriseDB.EDBClient.BackendMessages;
using EDBTypes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace  EnterpriseDB.EDBClient.TypeHandlers.FullTextSearchHandlers
{
    /// <summary>
    /// http://www.postgresql.org/docs/current/static/datatype-textsearch.html
    /// </summary>
    [TypeMapping("tsquery", EDBDbType.TsQuery, new[] {
        typeof(EDBTsQuery), typeof(EDBTsQueryAnd), typeof(EDBTsQueryEmpty),
        typeof(EDBTsQueryLexeme), typeof(EDBTsQueryNot), typeof(EDBTsQueryOr), typeof(EDBTsQueryBinOp) })
    ]
    internal class TsQueryHandler : ChunkingTypeHandler<EDBTsQuery>
    {
        // 1 (type) + 1 (weight) + 1 (is prefix search) + 2046 (max str len) + 1 (null terminator)
        const int MaxSingleTokenBytes = 2050;

        ReadBuffer  _readBuf;
        WriteBuffer _writeBuf;
        Stack<Tuple<EDBTsQuery, int>> _nodes;
        int _numTokens;
        int _tokenPos;
        int _bytesLeft;
        EDBTsQuery _value;

        Stack<EDBTsQuery> _stack;

        internal TsQueryHandler(IBackendType backendType) : base(backendType) { }

        public override void PrepareRead(ReadBuffer buf, int len, FieldDescription fieldDescription)
        {
            _readBuf = buf;
            _nodes = new Stack<Tuple<EDBTsQuery, int>>();
            _tokenPos = -1;
            _bytesLeft = len;
        }

        public override bool Read([CanBeNull] out EDBTsQuery result)
        {
            result = null;

            if (_tokenPos == -1)
            {
                if (_readBuf.ReadBytesLeft < 4)
                    return false;
                _numTokens = _readBuf.ReadInt32();
                _bytesLeft -= 4;
                _tokenPos = 0;
            }

            if (_numTokens == 0)
            {
                result = new EDBTsQueryEmpty();
                _readBuf = null;
                _nodes = null;
                return true;
            }

            for (; _tokenPos < _numTokens; _tokenPos++)
            {
                if (_readBuf.ReadBytesLeft < Math.Min(_bytesLeft, MaxSingleTokenBytes))
                    return false;

                int readPos = _readBuf.ReadPosition;

                bool isOper = _readBuf.ReadByte() == 2;
                if (isOper)
                {
                    EDBTsQuery.NodeKind operKind = (EDBTsQuery.NodeKind)_readBuf.ReadByte();
                    if (operKind == EDBTsQuery.NodeKind.Not)
                    {
                        var node = new EDBTsQueryNot(null);
                        InsertInTree(node);
                        _nodes.Push(new Tuple<EDBTsQuery, int>(node, 0));
                    }
                    else
                    {
                        EDBTsQuery node = null;

                        if (operKind == EDBTsQuery.NodeKind.And)
                            node = new EDBTsQueryAnd(null, null);
                        else if (operKind == EDBTsQuery.NodeKind.Or)
                            node = new EDBTsQueryOr(null, null);
                        else
                            PGUtil.ThrowIfReached();

                        InsertInTree(node);

                        _nodes.Push(new Tuple<EDBTsQuery, int>(node, 2));
                        _nodes.Push(new Tuple<EDBTsQuery, int>(node, 1));
                    }
                }
                else
                {
                    EDBTsQueryLexeme.Weight weight = (EDBTsQueryLexeme.Weight)_readBuf.ReadByte();
                    bool prefix = _readBuf.ReadByte() != 0;
                    string str = _readBuf.ReadNullTerminatedString();
                    InsertInTree(new EDBTsQueryLexeme(str, weight, prefix));
                }

                _bytesLeft -= _readBuf.ReadPosition - readPos;
            }

            if (_nodes.Count != 0)
                PGUtil.ThrowIfReached();

            result = _value;
            _readBuf = null;
            _nodes = null;
            _value = null;
            return true;
        }

        void InsertInTree(EDBTsQuery node)
        {
            if (_nodes.Count == 0)
            {
                _value = node;
            }
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

        public override int ValidateAndGetLength(object value, ref LengthCache lengthCache, EDBParameter parameter=null)
        {
            var vec = value as EDBTsQuery;
            if (vec == null) {
                throw CreateConversionException(value.GetType());
            }

            if (vec.Kind == EDBTsQuery.NodeKind.Empty)
                return 4;

            return 4 + GetNodeLength(vec);
        }

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
                case EDBTsQuery.NodeKind.Not:
                    return 2 + GetNodeLength(((EDBTsQueryNot)node).Child);
                case EDBTsQuery.NodeKind.Empty:
                    throw new InvalidOperationException("Empty tsquery nodes must be top-level");
                default:
                    throw new InvalidOperationException("Illegal node kind: " + node.Kind);
            }
        }

        public override void PrepareWrite(object value, WriteBuffer buf, LengthCache lengthCache, EDBParameter parameter=null)
        {
            _writeBuf = buf;
            _value = (EDBTsQuery)value;
            _numTokens = GetTokenCount(_value);
        }

        int GetTokenCount(EDBTsQuery node)
        {
            switch (node.Kind)
            {
                case EDBTsQuery.NodeKind.Lexeme:
                    return 1;
                case EDBTsQuery.NodeKind.And:
                case EDBTsQuery.NodeKind.Or:
                    return 1 + GetTokenCount(((EDBTsQueryBinOp)node).Left) + GetTokenCount(((EDBTsQueryBinOp)node).Right);
                case EDBTsQuery.NodeKind.Not:
                    return 1 + GetTokenCount(((EDBTsQueryNot)node).Child);
                case EDBTsQuery.NodeKind.Empty:
                    return 0;
            }
            return -1;
        }

        public override bool Write(ref DirectBuffer directBuf)
        {
            if (_stack == null)
            {
                if (_writeBuf.WriteSpaceLeft < 4)
                    return false;
                _writeBuf.WriteInt32(_numTokens);

                if (_numTokens == 0)
                {
                    _writeBuf = null;
                    _value = null;
                    return true;
                }
                _stack = new Stack<EDBTsQuery>();
                _stack.Push(_value);
            }

            while (_stack.Count > 0)
            {
                if (_writeBuf.WriteSpaceLeft < 2)
                    return false;

                if (_stack.Peek().Kind == EDBTsQuery.NodeKind.Lexeme && _writeBuf.WriteSpaceLeft < MaxSingleTokenBytes)
                    return false;

                var node = _stack.Pop();
                _writeBuf.WriteByte(node.Kind == EDBTsQuery.NodeKind.Lexeme ? (byte)1 : (byte)2);
                if (node.Kind != EDBTsQuery.NodeKind.Lexeme)
                {
                    _writeBuf.WriteByte((byte)node.Kind);
                    if (node.Kind == EDBTsQuery.NodeKind.Not)
                        _stack.Push(((EDBTsQueryNot)node).Child);
                    else
                    {
                        _stack.Push(((EDBTsQueryBinOp)node).Right);
                        _stack.Push(((EDBTsQueryBinOp)node).Left);
                    }
                }
                else
                {
                    var lexemeNode = (EDBTsQueryLexeme)node;
                    _writeBuf.WriteByte((byte)lexemeNode.Weights);
                    _writeBuf.WriteByte(lexemeNode.IsPrefixSearch ? (byte)1 : (byte)0);
                    _writeBuf.WriteString(lexemeNode.Text);
                    _writeBuf.WriteByte(0);
                }
            }

            _writeBuf = null;
            _value = null;
            _stack = null;
            return true;
        }
    }
}
