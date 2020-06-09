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
using JetBrains.Annotations;

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
        [CanBeNull]
        internal bool[] UnknownResultTypeList { get; set; }
        EDBParameterCollection _parameters;

        const byte Code = (byte)'B';

        internal BindOutMessage Populate(List<EDBParameter> inputParameters, EDBParameterCollection parameters, string portal = "", string statement = "")
        {
            Debug.Assert(inputParameters != null && inputParameters.All(p => p.IsInputDirection));
            Debug.Assert(portal != null);
            Debug.Assert(statement != null);

            AllResultTypesAreUnknown = false;
            UnknownResultTypeList = null;
            Portal = portal;
            Statement = statement;
            InputParameters = inputParameters;
            _parameters = parameters;
            return this;
        }

        internal override async Task Write(EDBWriteBuffer buf, bool async)
        {
            Debug.Assert(Statement != null && Statement.All(c => c < 128));
            Debug.Assert(Portal != null && Portal.All(c => c < 128));
            var formatCodesSum = 0;
            var paramsLength = 0;
            foreach (EDBParameter p in InputParameters)
            {
                formatCodesSum += (int)p.FormatCode;
                p.LengthCache?.Rewind();
                paramsLength += p.ValidateAndGetLength();
            }

            var formatCodeListLength =  _parameters.Count;

            var headerLength =
                1 +                        // Message code
                4 +                        // Message length
                1 +                        // Portal is always empty (only a null terminator)
                Statement.Length + 1 + 2 +                        // Number of parameter format codes that follow
                2 * formatCodeListLength + // List of format codes
                2;     // Number of parameter format codes that follow

            if (buf.WriteSpaceLeft < headerLength)
            {
                Debug.Assert(buf.Size >= headerLength, "Buffer too small for Bind header");
                await buf.Flush(async);
            }


            
            var messageLength = headerLength +
                4 * _parameters.Count +                                    // Parameter lengths
                paramsLength +                                                  // Parameter values
                2 +                                                             // Number of result format codes
                2 * (UnknownResultTypeList == null ? 1 : UnknownResultTypeList.Length);  // Result format codes
            buf.WriteByte(Code);
            buf.WriteInt32(messageLength - 1);
            Debug.Assert(Portal == string.Empty);
            buf.WriteByte(0);  // Portal is always empty
            
            buf.WriteNullTerminatedString(Statement);
            buf.WriteInt16(formatCodeListLength);

            // 0 length implicitly means all-text, 1 means all-binary, >1 means mix-and-match
            if (formatCodeListLength == 1)
            {
                if (buf.WriteSpaceLeft < 2)
                    await buf.Flush(async);
                buf.WriteInt16((short)FormatCode.Binary);
            }
            else if (formatCodeListLength > 1)
            {
                foreach (EDBParameter p in _parameters)
                {
                    buf.WriteInt16((short)p.FormatCode);
                }
            }

            if (buf.WriteSpaceLeft < 2)
                await buf.Flush(async);

            buf.WriteInt16(_parameters.Count);

            foreach (EDBParameter param in _parameters)
            {
                try
                {
                    //Console.WriteLine(buf.WriteSpaceLeft);
                    if (buf.WriteSpaceLeft < 80)
                        await buf.Flush(async);

                    param.LengthCache?.Rewind();
                    if (param.Direction == System.Data.ParameterDirection.Output)
                    {
                        buf.WriteInt32(-1);
                        continue;
                    }
                    await param.WriteWithLength(buf, async);
                } catch (Exception e)
                {
                    e.ToString();
                }
               
            }

            if (UnknownResultTypeList != null)
            {
                if (buf.WriteSpaceLeft < 2 + UnknownResultTypeList.Length * 2)
                    await buf.Flush(async);
                buf.WriteInt16(UnknownResultTypeList.Length);
                foreach (var t in UnknownResultTypeList)
                    buf.WriteInt16(t ? 0 : 1);
            }
            else
            {
                if (buf.WriteSpaceLeft < 4)
                    await buf.Flush(async);
                buf.WriteInt16(1);
                buf.WriteInt16(AllResultTypesAreUnknown ? 0 : 1);
            }
        }

        public override string ToString()
            => $"[Bind(Portal={Portal},Statement={Statement},NumParams={InputParameters.Count}]";
    }
}
