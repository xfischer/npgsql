// Npgsql.EDBReadyState.cs
//
// Author:
// 	Dave Joyner <d4ljoyn@yahoo.com>
//
//	Copyright (C) 2002 The Npgsql Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
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
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace EnterpriseDB.EDBClient
{
	internal sealed class EDBReadyState : EDBState
	{
		public static readonly EDBReadyState Instance = new EDBReadyState();

        private bool isEDBProtocol = false; //EDB Team for command type ..Set in Parse
        private bool isSupportCallable = false;

		// Flush and Sync messages. It doesn't need to be created every time it is called.
		private static readonly EDBFlush _flushMessage = new EDBFlush();

		private static readonly EDBSync _syncMessage = new EDBSync();

        private readonly String CLASSNAME = MethodBase.GetCurrentMethod().DeclaringType.Name;

		private EDBReadyState()
			: base()
		{
		}

		public override IEnumerable<IServerResponseObject> QueryEnum(EDBConnector context, EDBCommand command)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "QueryEnum");

            
			//String commandText = command.GetCommandText();
			//EDBEventLog.LogMsg(resman, "Log_QuerySent", LogLevel.Debug, commandText);

			// Send the query request to backend.

			EDBQuery query = new EDBQuery(command, context.BackendProtocolVersion);

			query.WriteToStream(context.Stream);
			context.Stream.Flush();
			return ProcessBackendResponsesEnum(context);
		}

		public override void Parse(EDBConnector context, EDBParse parse,EDBCommand command, Boolean callable)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Parse");

			Stream stream = context.Stream;
            if (callable)
            {	 //31 OCT 05 .Patch for server version 
                if ((command.CommandType.ToString() == "Text") || (command.CommandType.ToString() == "TableDirect"))
                {
                    parse.WriteToStream(stream);
                    isEDBProtocol = true;
                }
                else
                {
                    /*
                     * EDBTeam:
                     * Send custom message in case of procedure type command 
                     */
                    parse.WriteToStreamParseOut(stream);
                    isSupportCallable = true;
                    isEDBProtocol = false;
                }
            }
            else
                parse.WriteToStream(stream);    

			//stream.Flush();
		}


		public override IEnumerable<IServerResponseObject> SyncEnum(EDBConnector context)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Sync");
			_syncMessage.WriteToStream(context.Stream);
			context.Stream.Flush();
			return ProcessBackendResponsesEnum(context);
		}

		public override void Flush(EDBConnector context)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Flush");
			_flushMessage.WriteToStream(context.Stream);
			context.Stream.Flush();
			ProcessBackendResponses(context);
		}

		public override void Bind(EDBConnector context, EDBBind bind)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Bind");

			Stream stream = context.Stream;

			bind.WriteToStream(stream);
			//stream.Flush();
		}

		public override void Describe(EDBConnector context, EDBDescribe describe)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Describe");
			describe.WriteToStream(context.Stream);
			//context.Stream.Flush();
		}

		public override void Execute(EDBConnector context, EDBExecute execute)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Execute");
			EDBDescribe describe = new EDBDescribe('P', execute.PortalName);
			Stream stream = context.Stream;
			describe.WriteToStream(stream);
			execute.WriteToStream(stream);
			//stream.Flush();
			Sync(context);
		}

		public override IEnumerable<IServerResponseObject> ExecuteEnum(EDBConnector context, EDBExecute execute)
		{
            
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Execute");
			EDBDescribe describe = new EDBDescribe('P', execute.PortalName);
			Stream stream = context.Stream;
            /*
             * EDBTeam:
             * Handling of parse of describe messages
             */
            if (isSupportCallable)	//EDB Team 
            {
                if (isEDBProtocol) //in case 'P' messege 
                {
                    describe.WriteToStream(stream);
                    execute.WriteToStream(stream);
                }
                else //in case 'O' messege in Parse 
                {
                    describe.WriteToStream(stream);
                    describe.WriteToStreamDescribeOut(stream);
                    execute.WriteToStream(stream);
                    execute.WriteToStreamExecuteOut(stream);
                }

            }
            else
            {
                describe.WriteToStream(stream);
                execute.WriteToStream(stream);
            }
            return SyncEnum(context);
		}

		public override void Close(EDBConnector context)
		{
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Close");
			Stream stream = context.Stream;
			stream.WriteByte((byte) FrontEndMessageCode.Termination);
			if (context.BackendProtocolVersion >= ProtocolVersion.Version3)
			{
				PGUtil.WriteInt32(stream, 4);
			}
			stream.Flush();

			try
			{
				stream.Close();
			}
			catch
			{
			}

			context.Stream = null;
			ChangeState(context, EDBClosedState.Instance);
		}
	}
}