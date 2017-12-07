#region License
// The PostgreSQL License
//
// Copyright (C) 2017 The  EnterpriseDB.EDBClient DEVELOPMENT Team
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace  EnterpriseDB.EDBClient.FrontendMessages
{
    class ParseOutMessage : FrontendMessage
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

        readonly Encoding _encoding;
        string name;

        EDBParameterCollection _parameters;
        const byte Code = (byte)'O';

        internal ParseOutMessage(Encoding encoding)
        {
            _encoding = encoding;
            ParameterTypeOIDs = new List<uint>();
        }

        internal ParseOutMessage Populate(string sql, string statementName, EDBParameterCollection _parameter, List<EDBParameter> inputParameters, TypeHandlerRegistry typeHandlerRegistry)
        {
            ParameterTypeOIDs.Clear();
            Query = sql;
            Statement = statementName;
            _parameters = _parameter;
            foreach (var inputParam in inputParameters) {
                inputParam.ResolveHandler(typeHandlerRegistry);
                ParameterTypeOIDs.Add(inputParam.Handler.PostgresType.OID);
            }
            return this;
        }

        internal override async Task Write(WriteBuffer buf, bool async, CancellationToken cancellationToken)
        {
            Debug.Assert(Statement != null && Statement.All(c => c < 128));

            var queryByteLen = _encoding.GetByteCount(Query);
            if (buf.WriteSpaceLeft < 1 + 4 + Statement.Length + 1)
                await buf.Flush(async, cancellationToken);

            var messageLength =
                1 +                         // Message code
                4 +                         // Length
                Statement.Length +
                1 +                         // Null terminator
                queryByteLen +
                1 +                         // Null terminator
                2 +                         // Number of parameters
                _parameters.Count * 4 +
                _parameters.Count * 2;

            buf.WriteByte(Code);
            buf.WriteInt32(messageLength - 1);
            buf.WriteNullTerminatedString(Statement);

            await buf.WriteString(Query, queryByteLen, async, cancellationToken);

            if (buf.WriteSpaceLeft < 1 + 2 + _parameters.Count * 4 + _parameters.Count * 2)
                await buf.Flush(async, cancellationToken);
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
        }

        public override string ToString()
            => $"[Parse(Statement={Statement},NumParams={ParameterTypeOIDs.Count}]";
    }
}
