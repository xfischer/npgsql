// EDB.EDBReadyState.cs
//
// Author:
// 	Dave Joyner <d4ljoyn@yahoo.com>
//
//	Copyright (C) 2002 The EDB Development Team
//	npgsql-general@gborg.postgresql.org
//	http://gborg.postgresql.org/project/npgsql/projdisplay.php
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA


using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Resources;
using System.Windows.Forms;

namespace EnterpriseDB.EDBClient
{


    internal sealed class EDBReadyState : EDBState
    {
        private static EDBReadyState _instance = null;
		
		
		public bool EDBProtocol = false; //EDB Team for command type ..Set in Parse
		public bool _SupportCallable  = false ;  //EDB Team  31 Oct 05
        // Flush and Sync messages. It doesn't need to be created every time it is called.
        private static readonly EDBFlush _flushMessage = new EDBFlush();

        private static readonly EDBSync _syncMessage = new EDBSync();

        private readonly String CLASSNAME = "EDBReadyState";

        private EDBReadyState() : base()
        { }

        public static EDBReadyState Instance
        {
            get
            {
                if ( _instance == null )
                {
                    _instance = new EDBReadyState();
                }
                return _instance;
            }
        }



        public override void Query( EDBConnector context, EDBCommand command )
        {

            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Query");



            //String commandText = command.GetCommandText();
            //EDBEventLog.LogMsg(resman, "Log_QuerySent", LogLevel.Debug, commandText);
			
            // Send the query request to backend.

            EDBQuery query = new EDBQuery(command, context.BackendProtocolVersion);
			
            BufferedStream stream = new BufferedStream(context.Stream);
            query.WriteToStream(stream, context.Encoding);
            stream.Flush();

            ProcessBackendResponses(context);

        }


		/// <summary>
		/// Messege 'O' :FUNCTION WriteToStreamParseOut() which supports EDB new protocol . AND 'P' WriteToStream supports old versions
		/// </summary>
		/// <param name="context"></param>


        public override void Parse(EDBConnector context, EDBParse parse ,EDBCommand cmd ,Boolean SupportCallable)
        {
        	
			
			EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Parse");
            BufferedStream stream = new BufferedStream(context.Stream);

			
			//if (String.Equals(context.ServerVersion.ToString(),ServerVersion.EDB_ServerVerion)) //EDB Team Protocol change 
			if(SupportCallable)	 //31 OCT 05 .Patch for server version 
				if((cmd.CommandType.ToString() ==  "Text") ||  (cmd.CommandType.ToString() == "TableDirect") )
				{
					
					parse.WriteToStream(stream, context.Encoding);
					EDBProtocol = true;
					
				}
				else
				{
					parse.WriteToStreamParseOut(stream, context.Encoding);    
					_SupportCallable = true; //EDB Team :Zahid K: 31 Oct 05
				}

			else														//END 
				parse.WriteToStream(stream, context.Encoding);

		
             stream.Flush();
        }
		

        public override void Sync(EDBConnector context)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Sync");
            _syncMessage.WriteToStream(context.Stream, context.Encoding);
            context.Stream.Flush();
            ProcessBackendResponses(context);
        }

        public override void Flush(EDBConnector context)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Flush");
            _flushMessage.WriteToStream(context.Stream, context.Encoding);
            ProcessBackendResponses(context);
        }

        public override void Bind(EDBConnector context, EDBBind bind)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Bind");
            BufferedStream stream = new BufferedStream(context.Stream);
            bind.WriteToStream(stream, context.Encoding);
            stream.Flush();
        }

		/// <summary>
		///  Execute IF Block supports EDB new protocol version ,and Else part supports old version 
		/// </summary>
		/// <param name="context"></param>

        public override void Execute(EDBConnector context, EDBExecute execute)
        {	

            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Execute");
            EDBDescribe describe = new EDBDescribe('P', execute.PortalName);
            BufferedStream stream = new BufferedStream(context.Stream);
	      
 


			//if(String.Equals(context.ServerVersion.ToString(),ServerVersion.EDB_ServerVerion)) //EDB Team protocol change
			if(_SupportCallable)	//EDB Team 31 OCT 05
			{
				if (EDBProtocol) //in case 'P' messege 
				{
					describe.WriteToStream(stream, context.Encoding);
					execute.WriteToStream(stream, context.Encoding);
					EDBProtocol = false;
				}
				else //in case 'O' messege in Parse 
				{
					describe.WriteToStream(stream, context.Encoding);
					describe.WriteToStreamDescribeOut(stream,context.Encoding);
					execute.WriteToStream(stream, context.Encoding);
					execute.WriteToStreamExecuteOut(stream , context.Encoding);
					
				}
			}
			else{														//  EDB Team END
			
				describe.WriteToStream(stream, context.Encoding);	
				execute.WriteToStream(stream, context.Encoding);
			
			}
		

			//    if(String.Equals(context.ServerVersion.ToString(),"8.0.3.9"))
			//		
			//	 else
				

            stream.Flush();
            Sync(context);
        }

        public override void Close( EDBConnector context )
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Close");
            Stream stream = context.Stream;
            stream.WriteByte((Byte)'X');
            if (context.BackendProtocolVersion >= ProtocolVersion.Version3)
                PGUtil.WriteInt32(stream, 4);
            stream.Flush();

            try
            {
                stream.Close();
            }
            catch {}

            context.Stream = null;
        ChangeState( context, EDBClosedState.Instance )
            ;
        }
    }
}
