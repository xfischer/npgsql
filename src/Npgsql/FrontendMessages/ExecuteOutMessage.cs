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
using System.Linq;
using System.Text;

namespace  EnterpriseDB.EDBClient.FrontendMessages
{
    class ExecuteOutMessage : SimpleFrontendMessage
    {
        internal string Portal { get; private set; }
        internal int MaxRows { get; private set; }

        const byte Code = (byte)'v';

        internal ExecuteOutMessage Populate(string portal = "", int maxRows = 0)
        {
            Portal = portal;
      //      MaxRows = maxRows;
            return this;
        }

        internal ExecuteOutMessage Populate(int maxRows) => Populate("", maxRows);

        internal override int Length => 1 + 4 + (Portal.Length + 1) ;

        internal override void WriteFully(WriteBuffer buf)
        {
            Contract.Requires(Portal != null && Portal.All(c => c < 128));

            var portalNameBytes = Portal == "" ? PGUtil.EmptyBuffer : Encoding.ASCII.GetBytes(Portal);
            buf.WriteByte(Code);
            buf.WriteInt32(Length - 1);
            buf.WriteBytesNullTerminated(portalNameBytes);
        //    buf.WriteInt32(MaxRows);
        }

        public override string ToString()
        {
            return $"[Execute(Portal={Portal},MaxRows={MaxRows}]";
        }
    }
}
