#region License
// The PostgreSQL License
//
// Copyright (C) 2015 The  EnterpriseDB.EDBClient Development Team
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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;

namespace  EnterpriseDB.EDBClient.FrontendMessages
{
    class ParseOutMessage : ChunkingFrontendMessage
    {
        /// <summary>
        /// The query string to be parsed.
        /// </summary>
        string Query { get; set; }

        /// <summary>
        /// The name of the destination prepared statement (an empty string selects the unnamed prepared statement).
        /// </summary>
        string Statement { get; set; }

        // ReSharper disable once InconsistentNaming
        internal List<uint> ParameterTypeOIDs { get; private set; }

        byte[] _statementNameBytes;
        int _queryLen;
        char[] _queryChars;
        int _charPos;
        EDBParameterCollection _parameters;
        State _state;
        string name;
        const byte Code = (byte)'O';

        internal ParseOutMessage()
        {
            ParameterTypeOIDs = new List<uint>();
        }

        internal ParseOutMessage Populate(EDBStatement statement,EDBParameterCollection _parameter, TypeHandlerRegistry typeHandlerRegistry)
        {
            _state = State.WroteNothing;
            ParameterTypeOIDs.Clear();
            Query = statement.SQL;
            _parameters = _parameter;
            Statement = statement.PreparedStatementName ?? "";
            foreach (var inputParam in statement.InputParameters) {
                inputParam.ResolveHandler(typeHandlerRegistry);
                ParameterTypeOIDs.Add(inputParam.Handler.OID);
            }
            return this;
        }

        internal override bool Write(EDBBuffer buf, ref DirectBuffer directBuf)
        {
            Contract.Requires(Statement != null && Statement.All(c => c < 128));

            switch (_state)
            {
                case State.WroteNothing:
                    _statementNameBytes = PGUtil.UTF8Encoding.GetBytes(Statement);
                    _queryLen = PGUtil.UTF8Encoding.GetByteCount(Query);
                    if (buf.WriteSpaceLeft < 1 + 4 + _statementNameBytes.Length + 1) {
                        return false;
                    }

                    var messageLength =
                        1 +                         // Message code
                        4 +                         // Length
                        _statementNameBytes.Length +
                        1 +                         // Null terminator
                        _queryLen +
                        1 +                         // Null terminator
                        2 +                         // Number of parameters
                        _parameters.Count * 4 +
                         _parameters.Count * 2;

                    buf.WriteByte(Code);
                    buf.WriteInt32(messageLength - 1);
                    buf.WriteBytesNullTerminated(_statementNameBytes);
                    goto case State.WroteHeader;

                case State.WroteHeader:
                    _state = State.WroteHeader;

                    if (_queryLen <= buf.WriteSpaceLeft) {
                        buf.WriteString(Query);
                        goto case State.WroteQuery;
                    }

                    if (_queryLen <= buf.Size) {
                        // String can fit entirely in an empty buffer. Flush and retry rather than
                        // going into the partial writing flow below (which requires ToCharArray())
                        return false;
                    }

                    _queryChars = Query.ToCharArray();
                    _charPos = 0;
                    goto case State.WritingQuery;

                case State.WritingQuery:
                    _state = State.WritingQuery;
                    int charsUsed;
                    bool completed;
                    buf.WriteStringChunked(_queryChars, _charPos, _queryChars.Length - _charPos, true,
                                           out charsUsed, out completed);
                    if (!completed)
                    {
                        _charPos += charsUsed;
                        return false;
                    }
                    goto case State.WroteQuery;

                case State.WroteQuery:
                    _state = State.WroteQuery;
                    if (buf.WriteSpaceLeft < 1 + 2 + _parameters.Count * 4 + _parameters.Count * 2)
                    {
                        return false;
                    }
                    buf.WriteByte(0); // Null terminator for the query
                    buf.WriteInt16((short)_parameters.Count);


                    //TODO ZK Check why its here
                    //foreach (var t in ParameterTypeOIDs) {
                    //    buf.WriteInt32((int)t);
                    //}

                    /*EDB should change to goto etc*/
                    for (Int32 i = 0; i < _parameters.Count; i++)
                    {
                        // PGUtil.WriteInt32(outputStream, Convert.ToInt32(EDBParameter.ParamToOid(_parameters[i].TypeInfo.Name.ToString())));

                        name = _parameters[i].EDBDbType.ToString();
                        buf.WriteInt32((Int32)EDBParameter.ParamToOid((string)_parameters[i].EDBDbType.ToString()));
                    }

                    for (Int32 i = 0; i < _parameters.Count; i++)
                    {
                       // PGUtil.WriteInt16(outputStream, Convert.ToInt16(EDBParameter.NetParamDirectionToEDBParamDirection(_parameters[i].Direction)));
                        buf.WriteInt16((Int16)EDBParameter.NetParamDirectionToEDBParamDirection(_parameters[i].Direction));
                    }
                    _state = State.WroteAll;
                    return true;





                default:
                    throw PGUtil.ThrowIfReached();
            }
        }

        public override string ToString()
        {
            return String.Format("[Parse(Statement={0},NumParams={1}]", Statement, _parameters.Count);
        }

        private enum State
        {
            WroteNothing,
            WroteHeader,
            WritingQuery,
            WroteQuery,
            WroteAll
        }
    }
}
