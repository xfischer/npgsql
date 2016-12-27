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

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace  EnterpriseDB.EDBClient.FrontendMessages
{
    class BindOutMessage : FrontendMessage
    {
        /// <summary>
        /// The name of the destination portal (an empty string selects the unnamed portal).
        /// </summary>
        string Portal { get; set; }

        /// <summary>
        /// The name of the source prepared statement (an empty string selects the unnamed prepared statement).
        /// </summary>
        string Statement { get; set; }

        List<EDBParameter> InputParameters { get; set; }
        internal List<FormatCode> ResultFormatCodes { get; private set; }
        internal bool AllResultTypesAreUnknown { get; set; }
        internal bool[] UnknownResultTypeList { get; set; }

        State _state;
        int _paramIndex;
        int _formatCodeListLength= 0 ;
        bool _wroteParamLen;
		EDBParameterCollection _parameters;
        const byte Code = (byte)'B';

        internal BindOutMessage Populate(TypeHandlerRegistry typeHandlerRegistry, List<EDBParameter> inputParameters,EDBParameterCollection parameters,
                             string portal="", string statement="")
        {
            Contract.Requires(inputParameters != null && inputParameters.All(p => p.IsInputDirection));
            Contract.Requires(portal != null);
            Contract.Requires(statement != null);
            AllResultTypesAreUnknown = false;
            UnknownResultTypeList = null;
            Portal = portal;
            Statement = statement;
            InputParameters = inputParameters;
			_parameters = parameters;
            _state = State.Header;
            _paramIndex = 0;
            _wroteParamLen = false;
            return this;
        }

        /// <summary>
        /// Bind is a special message in that it supports the "direct buffer" optimization, which allows us to write
        /// user byte[] data directly to the stream rather than copying it into our buffer. It therefore has its own
        /// special overload of Write below.
        /// </summary>
        /// <param name="buf"></param>
        /// <returns></returns>
        internal override bool Write(WriteBuffer buf)
        {
            throw new NotSupportedException($"Internal error, call the overload of {nameof(Write)} which accepts a {nameof(DirectBuffer)}");
        }

        internal bool Write(WriteBuffer buf, ref DirectBuffer directBuf)
        {
            Contract.Requires(Statement != null && Statement.All(c => c < 128));
            Contract.Requires(Portal != null && Portal.All(c => c < 128));

            switch (_state)
            {
                case State.Header:
                    var formatCodesSum = _parameters.Select(p => p.FormatCode).Sum(c => (int)c);
                     _formatCodeListLength = _parameters.Count; // formatCodesSum == 0 ? 0 : formatCodesSum == _parameters.Count ? 1 : _parameters.Count;

                    var headerLength =
                        1 +                        // Message code
                        4 +                        // Message length
                        Portal.Length + 1 +
                        Statement.Length + 1 +
                        2 +                        // Number of parameter format codes that follow
                        2 * _formatCodeListLength + // List of format codes
                        2;                         // Number of parameters

                    if (buf.WriteSpaceLeft < headerLength)
                    {
                        Contract.Assume(buf.Size >= headerLength, "Buffer too small for Bind header");
                        return false;
                    }

                    foreach (var c in _parameters.Select(p => p.LengthCache).Where(c => c != null))
                        c.Rewind();
                    var messageLength = headerLength +
                        4 * _parameters.Count +                                     // Parameter lengths
                        InputParameters.Select(p => p.ValidateAndGetLength()).Sum() +   // Parameter values
                        2 +                                                             // Number of result format codes
                        2 * (UnknownResultTypeList == null ? 1 : UnknownResultTypeList.Length);  // Result format codes

                    buf.WriteByte(Code);
                    buf.WriteInt32(messageLength-1);
                    buf.WriteBytesNullTerminated(Encoding.ASCII.GetBytes(Portal));
                    buf.WriteBytesNullTerminated(Encoding.ASCII.GetBytes(Statement));
                    buf.WriteInt16(_formatCodeListLength);
                    _paramIndex = 0;

                    _state = State.ParameterFormatCodes;
                    goto case State.ParameterFormatCodes;

                case State.ParameterFormatCodes:
                    // 0 length implicitly means all-text, 1 means all-binary, >1 means mix-and-match
                    if (_formatCodeListLength == 1)
                    {
                        if (buf.WriteSpaceLeft < 2)
                            return false;
                        buf.WriteInt16((short)FormatCode.Binary);
                    }
                    else if (_formatCodeListLength > 1)
                   //     for (; _paramIndex < _parameters.Count; _paramIndex++)
            //            {
                            if (buf.WriteSpaceLeft < 2)
                                return false;
                            foreach (var code in _parameters.Select(p => p.FormatCode))
                                buf.WriteInt16((short)code);
                //        }
                    _state = State.ParameterCount;
                    goto case State.ParameterCount;

                case State.ParameterCount:
                    if (buf.WriteSpaceLeft < 2)
                        return false;

                    buf.WriteInt16(_parameters.Count);
                    _paramIndex = 0;

                    _state = State.ParameterValues;
                    goto case State.ParameterValues;

                case State.ParameterValues:
                    if (!WriteParameters(buf, ref directBuf))
                        return false;
                    _state = State.ResultFormatCodes;
                    goto case State.ResultFormatCodes;

                case State.ResultFormatCodes:
                    if (UnknownResultTypeList != null)
                    {
                        if (buf.WriteSpaceLeft < 2 + UnknownResultTypeList.Length * 2)
                            return false;
                        buf.WriteInt16(UnknownResultTypeList.Length);
                        foreach (var t in UnknownResultTypeList)
                            buf.WriteInt16(t ? 0 : 1);
                    }
                    else
                    {
                        if (buf.WriteSpaceLeft < 4)
                            return false;
                        buf.WriteInt16(1);
                        buf.WriteInt16(AllResultTypesAreUnknown ? 0 : 1);
                    }

                    _state = State.Done;
                    return true;

                default:
                    throw PGUtil.ThrowIfReached();
            }
        }

        bool WriteParameters(WriteBuffer buf, ref DirectBuffer directBuf)
        {
            var len =0;
            for (; _paramIndex < _parameters.Count; _paramIndex++)
            {
                var param = _parameters[_paramIndex];

                if (!_wroteParamLen)
                {
                    if (param.Value is DBNull)
                    {
                        if (buf.WriteSpaceLeft < 4) { return false; }
                        buf.WriteInt32(-1);
                        continue;
                    }

                    param.LengthCache?.Rewind();
                }

                var handler = param.Handler;

                var asChunkingWriter = handler as IChunkingTypeHandler;
                if (asChunkingWriter != null)
                {
                    if (!_wroteParamLen)
                    {
                        if (buf.WriteSpaceLeft < 4) { return false; }
                        buf.WriteInt32(param.ValidateAndGetLength());
                        asChunkingWriter.PrepareWrite(param.Value, buf, param.LengthCache, param);
                        _wroteParamLen = true;
                    }
                    if (!asChunkingWriter.Write(ref directBuf)) {
                        return false;
                    }
                    _wroteParamLen = false;
                    continue;
                }
                if (param.Direction != System.Data.ParameterDirection.Output)
                    len = param.ValidateAndGetLength();
                else
                    len = 0;

                var asSimpleWriter = (ISimpleTypeHandler)handler;
                if (buf.WriteSpaceLeft < len + 4)
                {
                    Contract.Assume(buf.Size >= len + 4);
                    return false;
                }

                if (param.Direction != System.Data.ParameterDirection.Output)
                {
                    buf.WriteInt32(len);
                    asSimpleWriter.Write(param.Value, buf, param);
                }
                else
                    buf.WriteInt32(-1);

            }
            return true;
        }

        public override string ToString()
        {
            return $"[Bind(Portal={Portal},Statement={Statement},NumParams={InputParameters.Count}]";
        }

        private enum State
        {
            Header,
            ParameterFormatCodes,
            ParameterCount,
            ParameterValues,
            ResultFormatCodes,
            Done
        }
    }
}
