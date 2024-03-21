// EnterpriseDB.EDBClient.EDBQuery.cs
//
// Author:
//     Dave Joyner <d4ljoyn@yahoo.com>
//
//    Copyright (C) 2002 The EnterpriseDB.EDBClient Development Team
//    npgsql-general@gborg.postgresql.org
//    http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// Permission to use, copy, modify, and distribute this software and its
// documentation for any purpose, without fee, and without a written
// agreement is hereby granted, provided that the above copyright notice
// and this paragraph and the following two paragraphs appear in all copies.
//
// IN NO EVENT SHALL THE NPGSQL DEVELOPMENT TEAM BE LIABLE TO ANY PARTY
// FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES,
// INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS
// DOCUMENTATION, EVEN IF THE NPGSQL DEVELOPMENT TEAM HAS BEEN ADVISED OF
// THE POSSIBILITY OF SUCH DAMAGE.
//
// THE NPGSQL DEVELOPMENT TEAM SPECIFICALLY DISCLAIMS ANY WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY
// AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE PROVIDED HEREUNDER IS
// ON AN "AS IS" BASIS, AND THE NPGSQL DEVELOPMENT TEAM HAS NO OBLIGATIONS
// TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.

using System;
using System.IO;
using System.Text;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Summary description for EDBQuery
    /// </summary>
    internal sealed class EDBQuery : ClientMessage
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private byte[] commandBytes = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private string commandText = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private readonly byte[] pgCommandBytes;

        public static readonly EDBQuery BeginTransRepeatableRead = new EDBQuery("BEGIN; SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;");
        public static readonly EDBQuery BeginTransSerializable = new EDBQuery("BEGIN; SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;");
        public static readonly EDBQuery BeginTransReadCommitted = new EDBQuery("BEGIN; SET TRANSACTION ISOLATION LEVEL READ COMMITTED;");
        public static readonly EDBQuery CommitTransaction = new EDBQuery("COMMIT");
        public static readonly EDBQuery RollbackTransaction = new EDBQuery("ROLLBACK");
        public static readonly EDBQuery DiscardAll = new EDBQuery("DISCARD ALL");
        public static readonly EDBQuery UnlistenAll = new EDBQuery("UNLISTEN *");
        public static readonly EDBQuery SetStmtTimeout10Sec = new EDBQuery("SET statement_timeout = 10000");
        public static readonly EDBQuery SetStmtTimeout20Sec = new EDBQuery("SET statement_timeout = 20000");
        public static readonly EDBQuery SetStmtTimeout30Sec = new EDBQuery("SET statement_timeout = 30000");
        public static readonly EDBQuery SetStmtTimeout60Sec = new EDBQuery("SET statement_timeout = 60000");
        public static readonly EDBQuery SetStmtTimeout90Sec = new EDBQuery("SET statement_timeout = 90000");
        public static readonly EDBQuery SetStmtTimeout120Sec = new EDBQuery("SET statement_timeout = 120000");

        public EDBQuery(byte[] command)
        {
            // Message length: Inr32 + command length + null terminator.
            int len = 4 + command.Length + 1;

            // Length + command code ('Q').
            pgCommandBytes = new byte[1 + len];

            MemoryStream commandWriter = new MemoryStream(pgCommandBytes);
         
            commandWriter
                .WriteBytes((byte)FrontEndMessageCode.Query)
                .WriteInt32(len)
                .WriteBytesNullTerminated(command);

            commandBytes = command;
        }

        public EDBQuery(string command)
        {
            // Message length: Inr32 + command length + null terminator.
            int len = 4 + command.Length + 1;

            // Length + command code ('Q').
            pgCommandBytes = new byte[1 + len];

            MemoryStream commandWriter = new MemoryStream(pgCommandBytes);

            commandWriter
                .WriteBytes((byte)FrontEndMessageCode.Query)
                .WriteInt32(len)
                .WriteStringNullTerminated(command);

            commandText = command;
        }

        public override void WriteToStream(Stream outputStream)
        {
            if (EDBEventLog.Level >= LogLevel.Debug)
            {
                // Log the string being sent.
                // If (this) was constructed with a byte[], then commandText has to be
                // initialized before the first Log call.
                if (commandText == null)
                {
                    commandText = BackendEncoding.UTF8Encoding.GetString(commandBytes);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                    commandBytes = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                }

                PGUtil.LogStringWritten(commandText);
            }

            outputStream.WriteBytes(pgCommandBytes);
        }
    }
}
