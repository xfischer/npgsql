// created on 6/14/2002 at 7:56 PM

// EDB.EDBState.cs
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
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Text;
using System.Resources;	


namespace EnterpriseDB.EDBClient
{
    ///<summary> 
    ///	This class represents the base class for the state pattern design pattern
    /// implementation.
    /// </summary>
    ///

    internal abstract class EDBState
    {
        private readonly String CLASSNAME = "EDBState";
        protected static ResourceManager resman = new ResourceManager(typeof(EDBState));

        internal EDBState()
        {
        }
        public virtual void Open(EDBConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }
        public virtual void Startup(EDBConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }
        public virtual void Authenticate(EDBConnector context, string password)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }
        public virtual void Query(EDBConnector context, EDBCommand command)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }
        public virtual void Ready( EDBConnector context )
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }
        public virtual void FunctionCall(EDBConnector context, EDBCommand command)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }
		
        public virtual void Parse(EDBConnector context, EDBParse parse ,EDBCommand cmd,Boolean callable) 
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void Flush(EDBConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }
        public virtual void Sync(EDBConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }
        public virtual void Bind(EDBConnector context, EDBBind bind)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }
        public virtual void Execute(EDBConnector context, EDBExecute execute)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void Close( EDBConnector context )
        {
            if (this != EDBClosedState.Instance)
            {
                try
                {
                    context.Stream.Close();
                }
                catch {}

                context.Stream = null;
            ChangeState( context, EDBClosedState.Instance )
                ;
            }
        }
		 public virtual void Describe(EDBConnector context, EDBDescribe describe)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }
        
        public virtual void CancelRequest(EDBConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }
//
//        public virtual void Close( EDBConnector context )
//        {
//            if (this != NpgsqlClosedState.Instance)
//            {
//                try
//                {
//                    context.Stream.Close();
//                }
//                catch {}
//
//                context.Stream = null;
//            ChangeState( context, NpgsqlClosedState.Instance )
//                ;
//            }
//        }

        ///<summary>
        ///This method is used by the states to change the state of the context.
        /// </summary>
        protected virtual void ChangeState(EDBConnector context, EDBState newState)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ChangeState");
            context.CurrentState = newState;
        }

        ///<summary>
        /// This method is responsible to handle all protocol messages sent from the backend.
        /// It holds all the logic to do it.
        /// To exchange data, it uses a Mediator object from which it reads/writes information
        /// to handle backend requests.
        /// </summary>
        ///
        internal virtual void ProcessBackendResponses( EDBConnector context )
        {

            try
            {
                switch (context.BackendProtocolVersion)
                {						
                case ProtocolVersion.Version2 :
                    ProcessBackendResponses_Ver_2(context);
                    break;

                case ProtocolVersion.Version3 :
                    ProcessBackendResponses_Ver_3(context);
                    break;

                }
            }
            finally
            {
                // reset expectations right after getting new responses
                context.Mediator.ResetExpectations();
            }
        }
		  
        protected virtual void ProcessBackendResponses_Ver_2( EDBConnector context )
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ProcessBackendResponses");

            BufferedStream 	stream = new BufferedStream(context.Stream);
            EDBMediator mediator = context.Mediator;
			
            // Often used buffer
            Byte[] inputBuffer = new Byte[ 4 ];

            Boolean readyForQuery = false;

            while (!readyForQuery)
            {
                // Check the first Byte of response.
                switch ( stream.ReadByte() )
                {
                case EDBMessageTypes_Ver_2.ErrorResponse :

                    {
                        EDBError error = new EDBError(context.BackendProtocolVersion);
                        error.ReadFromStream(stream, context.Encoding);

                        mediator.Errors.Add(error);
					
                        EDBEventLog.LogMsg(resman, "Log_ErrorResponse", LogLevel.Debug, error.Message);
                    }

                    // Return imediately if it is in the startup state or connected state as
                    // there is no more messages to consume.
                    // Possible error in the EDBStartupState:
                    //		Invalid password.
                    // Possible error in the EDBConnectedState:
                    //		No pg_hba.conf configured.

                    if (! mediator.RequireReadyForQuery)
                    {
                        return;
                    }

                    break;


                case EDBMessageTypes_Ver_2.AuthenticationRequest :

                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "AuthenticationRequest");

                    {
                        Int32 authType = PGUtil.ReadInt32(stream, inputBuffer);

                        if ( authType == EDBMessageTypes_Ver_2.AuthenticationOk )
                        {
                            EDBEventLog.LogMsg(resman, "Log_AuthenticationOK", LogLevel.Debug);

                            break;
                        }

                        if ( authType == EDBMessageTypes_Ver_2.AuthenticationClearTextPassword )
                        {
                            EDBEventLog.LogMsg(resman, "Log_AuthenticationClearTextRequest", LogLevel.Debug);

                            // Send the PasswordPacket.

                            ChangeState( context, EDBStartupState.Instance );
                            context.Authenticate(context.Password);

                            break;
                        }


                        if ( authType == EDBMessageTypes_Ver_2.AuthenticationMD5Password )
                        {
                            EDBEventLog.LogMsg(resman, "Log_AuthenticationMD5Request", LogLevel.Debug);
                            // Now do the "MD5-Thing"
                            // for this the Password has to be:
                            // 1. md5-hashed with the username as salt
                            // 2. md5-hashed again with the salt we get from the backend


                            MD5 md5 = MD5.Create();


                            // 1.
                            byte[] passwd = context.Encoding.GetBytes(context.Password);
                            byte[] saltUserName = context.Encoding.GetBytes(context.UserName);

                            byte[] crypt_buf = new byte[passwd.Length + saltUserName.Length];

                            passwd.CopyTo(crypt_buf, 0);
                            saltUserName.CopyTo(crypt_buf, passwd.Length);



                            StringBuilder sb = new StringBuilder ();
                            byte[] hashResult = md5.ComputeHash(crypt_buf);
                            foreach (byte b in hashResult)
                            sb.Append (b.ToString ("x2"));


                            String prehash = sb.ToString();

                            byte[] prehashbytes = context.Encoding.GetBytes(prehash);



                            byte[] saltServer = new byte[4];
                            stream.Read(saltServer, 0, 4);
                            // Send the PasswordPacket.
                            ChangeState( context, EDBStartupState.Instance );


                            // 2.

                            crypt_buf = new byte[prehashbytes.Length + saltServer.Length];
                            prehashbytes.CopyTo(crypt_buf, 0);
                            saltServer.CopyTo(crypt_buf, prehashbytes.Length);

                            sb = new StringBuilder ("md5"); // This is needed as the backend expects md5 result starts with "md5"
                            hashResult = md5.ComputeHash(crypt_buf);
                            foreach (byte b in hashResult)
                            sb.Append (b.ToString ("x2"));

                            context.Authenticate(sb.ToString ());

                            break;
                        }

                        // Only AuthenticationClearTextPassword and AuthenticationMD5Password supported for now.

                        mediator.Errors.Add(new EDBError(context.BackendProtocolVersion, String.Format(resman.GetString("Exception_AuthenticationMethodNotSupported"), authType)));
                    }

                    return;

					case EDBMessageTypes_Ver_2.RowDescription:
						// This is the RowDescription message.
						EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "RowDescription");

					{
						EDBRowDescription rd = new EDBRowDescription(context.BackendProtocolVersion);
                        
						rd.ReadFromStream(stream, context.Encoding, context.OidToNameMapping);
						// Initialize the array list which will contain the data from this rowdescription.
						mediator.AddRowDescription(rd);
					}

						// Now wait for the AsciiRow messages.
						break;

                case EDBMessageTypes_Ver_2.AsciiRow:
                    // This is the AsciiRow message.
                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "AsciiRow");

                    {
                        EDBAsciiRow asciiRow = new EDBAsciiRow(context.Mediator.LastRowDescription, context.BackendProtocolVersion);
                        asciiRow.ReadFromStream(stream, context.Encoding);

                        // Add this row to the rows array.
                        mediator.AddAsciiRow(asciiRow);
                    }

                    // Now wait for CompletedResponse message.
                    break;

                case EDBMessageTypes_Ver_2.BinaryRow:
                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BinaryRow");

                    {
                        EDBBinaryRow binaryRow = new EDBBinaryRow(context.Mediator.LastRowDescription);
                        binaryRow.ReadFromStream(stream, context.Encoding);

                        mediator.AddBinaryRow(binaryRow);
                    }

                    break;

                case EDBMessageTypes_Ver_2.ReadyForQuery :

                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ReadyForQuery");
                    readyForQuery = true;
                    ChangeState( context, EDBReadyState.Instance );
                    break;

                case EDBMessageTypes_Ver_2.BackendKeyData :

                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BackendKeyData");
                    // BackendKeyData message.
                    EDBBackEndKeyData backend_keydata = new EDBBackEndKeyData(context.BackendProtocolVersion);
                    backend_keydata.ReadFromStream(stream);
                    mediator.SetBackendKeydata(backend_keydata);


                    // Wait for ReadForQuery message
                    break;
                    ;

                case EDBMessageTypes_Ver_2.NoticeResponse :

                    {
                        EDBError notice = new EDBError(context.BackendProtocolVersion);
                        notice.ReadFromStream(stream, context.Encoding);

                        mediator.Notices.Add(notice);

                        EDBEventLog.LogMsg(resman, "Log_NoticeResponse", LogLevel.Debug, notice.Message);
                    }

                    // Wait for ReadForQuery message
                    break;

                case EDBMessageTypes_Ver_2.CompletedResponse :
                    // This is the CompletedResponse message.
                    // Get the string returned.


                    String result = PGUtil.ReadString(stream, context.Encoding);

                    EDBEventLog.LogMsg(resman, "Log_CompletedResponse", LogLevel.Debug, result);
                    // Add result from the processing.

                    mediator.AddCompletedResponse(result);

                    // Now wait for ReadyForQuery message.
                    break;

                case EDBMessageTypes_Ver_2.CursorResponse :
                    // This is the cursor response message.
                    // It is followed by a C NULL terminated string with the name of
                    // the cursor in a FETCH case or 'blank' otherwise.
                    // In this case it should be always 'blank'.
                    // [FIXME] Get another name for this function.
                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "CursorResponse");

                    String cursorName = PGUtil.ReadString(stream, context.Encoding);
                    // Continue waiting for ReadyForQuery message.
                    break;

                case EDBMessageTypes_Ver_2.EmptyQueryResponse :
                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "EmptyQueryResponse");
                    PGUtil.ReadString(stream, context.Encoding);
                    break;

                case EDBMessageTypes_Ver_2.NotificationResponse  :

                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "NotificationResponse");

                    Int32 PID = PGUtil.ReadInt32(stream, inputBuffer);
                    String notificationResponse = PGUtil.ReadString( stream, context.Encoding );
                    mediator.AddNotification(new EDBNotificationEventArgs(PID, notificationResponse));

                    // Wait for ReadForQuery message
                    break;
				
				


                default :
                    // This could mean a number of things
                    //   We've gotten out of sync with the backend?
                    //   We need to implement this type?
                    //   Backend has gone insane?
                    // FIXME
                    // what exception should we really throw here?
                    throw new NotSupportedException("Backend sent unrecognized response type");

                }
            }
        }

        protected virtual void ProcessBackendResponses_Ver_3( EDBConnector context )
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ProcessBackendResponses");

            BufferedStream 	stream = new BufferedStream(context.Stream);
            EDBMediator mediator = context.Mediator;			
			
            // Often used buffers
            Byte[] inputBuffer = new Byte[ 4 ];
            String Str;

            Boolean readyForQuery = false;
			
			EDBRowDescription rdout = new EDBRowDescription(context.BackendProtocolVersion); //EDB Team
			EDBRowDescription erd = new EDBRowDescription(context.BackendProtocolVersion); // EDB Team ..
            while (!readyForQuery)
            {
                // Check the first Byte of response.
                Int32 message = stream.ReadByte();
                switch ( message )
                {
                case EDBMessageTypes_Ver_3.ErrorResponse :

                    {
						
                        EDBError error = new EDBError(context.BackendProtocolVersion);
                        error.ReadFromStream(stream, context.Encoding);
						mediator.Errors.Add(error);

						EDBEventLog.LogMsg(resman, "Log_ErrorResponse", LogLevel.Debug, error.Message);						
                    }

                    // Return imediately if it is in the startup state or connected state as
                    // there is no more messages to consume.
                    // Possible error in the EDBStartupState:
                    //		Invalid password.
                    // Possible error in the EDBConnectedState:
                    //		No pg_hba.conf configured.

                    if (! mediator.RequireReadyForQuery)
                    {
                        return;
                    }

                    break;


                case EDBMessageTypes_Ver_3.AuthenticationRequest :

                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "AuthenticationRequest");

                    // Eat length
                    PGUtil.ReadInt32(stream, inputBuffer);

                    {
					
                        Int32 authType = PGUtil.ReadInt32(stream, inputBuffer);

                        if ( authType == EDBMessageTypes_Ver_3.AuthenticationOk )
                        {
                            EDBEventLog.LogMsg(resman, "Log_AuthenticationOK", LogLevel.Debug);

                            break;
                        }

                        if ( authType == EDBMessageTypes_Ver_3.AuthenticationClearTextPassword )
                        {
                            EDBEventLog.LogMsg(resman, "Log_AuthenticationClearTextRequest", LogLevel.Debug);

                            // Send the PasswordPacket.

                            ChangeState( context, EDBStartupState.Instance );
                            context.Authenticate(context.Password);

                            break;
                        }
					

                        if ( authType == EDBMessageTypes_Ver_3.AuthenticationMD5Password )
                        {
                            EDBEventLog.LogMsg(resman, "Log_AuthenticationMD5Request", LogLevel.Debug);
						
                            // Now do the "MD5-Thing"
                            // for this the Password has to be:
                            // 1. md5-hashed with the username as salt
                            // 2. md5-hashed again with the salt we get from the backend


                            MD5 md5 = MD5.Create();


                            // 1.
                            byte[] passwd = context.Encoding.GetBytes(context.Password);
                            byte[] saltUserName = context.Encoding.GetBytes(context.UserName);

                            byte[] crypt_buf = new byte[passwd.Length + saltUserName.Length];

                            passwd.CopyTo(crypt_buf, 0);
                            saltUserName.CopyTo(crypt_buf, passwd.Length);



                            StringBuilder sb = new StringBuilder ();
                            byte[] hashResult = md5.ComputeHash(crypt_buf);
                            foreach (byte b in hashResult)
                            sb.Append (b.ToString ("x2"));


                            String prehash = sb.ToString();

                            byte[] prehashbytes = context.Encoding.GetBytes(prehash);



                            stream.Read(inputBuffer, 0, 4);
                            // Send the PasswordPacket.
                            ChangeState( context, EDBStartupState.Instance );


                            // 2.

                            crypt_buf = new byte[prehashbytes.Length + 4];
                            prehashbytes.CopyTo(crypt_buf, 0);
                            inputBuffer.CopyTo(crypt_buf, prehashbytes.Length);

                            sb = new StringBuilder ("md5"); // This is needed as the backend expects md5 result starts with "md5"
                            hashResult = md5.ComputeHash(crypt_buf);
                            foreach (byte b in hashResult)
                            sb.Append (b.ToString ("x2"));

                            context.Authenticate(sb.ToString ());

                            break;
                        }

                        // Only AuthenticationClearTextPassword and AuthenticationMD5Password supported for now.
                        mediator.Errors.Add(new EDBError(context.BackendProtocolVersion, String.Format(resman.GetString("Exception_AuthenticationMethodNotSupported"), authType)));
                    }

                    return;

					case EDBMessageTypes_Ver_3.RowDescription:
						// This is the RowDescription message.
						EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "RowDescription");
					{
					
						EDBRowDescription rd = new EDBRowDescription(context.BackendProtocolVersion);
						rd.ReadFromStream(stream, context.Encoding, context.OidToNameMapping);
						//System.Windows.Forms.MessageBox.Show("In Row Description: Col Count="+rd.NumFields.ToString());
						mediator.AddRowDescription(rd);
					
					}

						// Now wait for the AsciiRow messages.
						break;

                case EDBMessageTypes_Ver_3.DataRow:
                    // This is the AsciiRow message.
                     EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "DataRow");
                    {
				
                        EDBAsciiRow asciiRow = new EDBAsciiRow(context.Mediator.LastRowDescription, context.BackendProtocolVersion);
                        asciiRow.ReadFromStream(stream, context.Encoding);
                        // Add this row to the rows array.
                        mediator.AddAsciiRow(asciiRow);
						
                    }

                    // Now wait for CompletedResponse message.
                    break;

                case EDBMessageTypes_Ver_3.ReadyForQuery :

                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ReadyForQuery");
					
                    // Possible status bytes returned:
                    //   I = Idle (no transaction active).
                    //   T = In transaction, ready for more.
                    //   E = Error in transaction, queries will fail until transaction aborted.
                    // Just eat the status byte, we have no use for it at this time.
                    PGUtil.ReadInt32(stream, inputBuffer);
                    PGUtil.ReadString(stream, context.Encoding, 1);

                    readyForQuery = true;
                    ChangeState( context, EDBReadyState.Instance );
					
                    break;
				
                case EDBMessageTypes_Ver_3.BackendKeyData :

                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BackendKeyData");
                    // BackendKeyData message.
                    EDBBackEndKeyData backend_keydata = new EDBBackEndKeyData(context.BackendProtocolVersion);
                    backend_keydata.ReadFromStream(stream);
                    mediator.SetBackendKeydata(backend_keydata);

				
                    // Wait for ReadForQuery message
                    break;

                case EDBMessageTypes_Ver_3.NoticeResponse :

                    // Notices and errors are identical except that we
                    // just throw notices away completely ignored.
                    {
                        EDBError notice = new EDBError(context.BackendProtocolVersion);
                        notice.ReadFromStream(stream, context.Encoding);

						mediator.Notices.Add(notice);
                        
						EDBEventLog.LogMsg(resman, "Log_NoticeResponse", LogLevel.Debug, notice.Message);
					
                    }

                    // Wait for ReadForQuery message
                    break;

                case EDBMessageTypes_Ver_3.CompletedResponse :
                    // This is the CompletedResponse message.
                    // Get the string returned.

					PGUtil.ReadInt32(stream, inputBuffer);
                    Str = PGUtil.ReadString(stream, context.Encoding);

					EDBEventLog.LogMsg(resman, "Log_CompletedResponse", LogLevel.Debug, Str);
					
                    // Add result from the processing.
                    mediator.AddCompletedResponse(Str);
					
					

                    break;

                case EDBMessageTypes_Ver_3.ParseComplete :
                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ParseComplete");
                    // Just read up the message length.
                    PGUtil.ReadInt32(stream, inputBuffer);
                    readyForQuery = true;
					
                    break;

                case EDBMessageTypes_Ver_3.BindComplete :
                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BindComplete");
                    // Just read up the message length.
                    PGUtil.ReadInt32(stream, inputBuffer);
                    readyForQuery = true;
                    break;
					

                case EDBMessageTypes_Ver_3.EmptyQueryResponse :
                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "EmptyQueryResponse");
                    PGUtil.ReadInt32(stream, inputBuffer);
					
                    break;

                case EDBMessageTypes_Ver_3.NotificationResponse  :
                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "NotificationResponse");

                    // Eat the length
                    PGUtil.ReadInt32(stream, inputBuffer);
                    {
                        // Process ID sending notification
                        Int32 PID = PGUtil.ReadInt32(stream, inputBuffer);
                        // Notification string
                        String notificationResponse = PGUtil.ReadString( stream, context.Encoding );
                        // Additional info, currently not implemented by PG (empty string always), eat it
                        PGUtil.ReadString( stream, context.Encoding );
                        mediator.AddNotification(new EDBNotificationEventArgs(PID, notificationResponse));
					
                    }

                    // Wait for ReadForQuery message
                    break;

                case EDBMessageTypes_Ver_3.ParameterStatus :
                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ParameterStatus");
                    EDBParameterStatus parameterStatus = new EDBParameterStatus();
                    parameterStatus.ReadFromStream(stream, context.Encoding);

                    EDBEventLog.LogMsg(resman, "Log_ParameterStatus", LogLevel.Debug, parameterStatus.Parameter, parameterStatus.ParameterValue);

                    mediator.AddParameterStatus(parameterStatus.Parameter, parameterStatus);
					
                    if (parameterStatus.Parameter == "server_version")
                    {
                        // Add this one under our own name so that if the parameter name
                        // changes in a future backend version, we can handle it here in the
                        // protocol handler and leave everybody else put of it.
                        mediator.AddParameterStatus("__npgsql_server_version", parameterStatus);
                        //                        context.ServerVersionString = parameterStatus.ParameterValue;
					}

                    break;
                case EDBMessageTypes_Ver_3.NoData :
						
                    // This nodata message may be generated by prepare commands issued with queries which doesn't return rows
                    // for example insert, update or delete.
                    // Just eat the message.
                    EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ParameterStatus");
                    PGUtil.ReadInt32(stream, inputBuffer);
                    break;
	
	
						/// <summary>
						/// EDB team Describe Out messege  
						/// Recieve tha Row description data supported in EDB new protocol version ,and add those 
						/// description in Last row description.
						/// </summary>
						/// <param name="context"></param>
				
					
				case EDBMessageTypes_Ver_3.OutDescription:
						
					 EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "RowDescription");
				 {					 
					 rdout.ReadFromStreamOutDescription(stream, context.Encoding, context.OidToNameMapping);
					 if(mediator.LastRowDescription==null) // If LastRowDes is null that means D messesge returned empty row Des 
						 mediator.AddRowDescription(erd); 
					 erd = mediator.LastRowDescription;					 
					 for(int i=0;i<rdout.NumFields;i++)
						erd.AddField(rdout.GetField(i));					 
				 }					
					break;
	

						/// <summary>
						/// //EDB team Parametere out messege
						/// 
						/// Recieved the row data and adds that in asciiRow .
						/// 
						/// </summary>
						/// <param name="context"></param>
					
				case EDBMessageTypes_Ver_3.ParamData:
				
					EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "DataRow");
				{
					//EDBAsciiRow asciiRow = new EDBAsciiRow(context.Mediator.LastRowDescription, context.BackendProtocolVersion);
					EDBAsciiRow asciiRowout = new EDBAsciiRow(erd, context.BackendProtocolVersion);
					asciiRowout.ReadFromStreamParamData(stream, context.Encoding);
					EDBAsciiRow row;
					if (mediator.size() ==0)// if  Row count is zero then param data messeges return zero 
					{
						mediator.AddAsciiRow(asciiRowout);		//So asciiRowOut is only data so add that 
																//in mediator 
						row= asciiRowout;
					}
					else
					{
						row = mediator.getLastAsciiRow();	// if param data return data row then add outdata row
															//in that row									
						int count = asciiRowout.size();
						for(int i=0;i<count;i++)
						{
							
								row.AddData(asciiRowout.GetData(i));
							
						}
					}
					//Code to arrange the parameter on their indexes set by client application  
					
					
					
					    EDBRowDescriptionFieldData[] descriptions;
						if(mediator.GetParameters().GetReturnParam()!=null)
							descriptions =new EDBRowDescriptionFieldData[mediator.GetParameters().Count + (mediator.GetParameters().GetReturnParam().Value==null?0:1)];						
						else descriptions =new EDBRowDescriptionFieldData[mediator.GetParameters().Count ];						
						Object[] data = new Object[descriptions.Length];
						EDBParameterCollection parms = mediator.GetParameters();
						for(int i=0;i < erd.NumFields;i++)						
						{							
							EDBRowDescriptionFieldData efd = erd.GetField(i);

							if(efd.ReturingIndex==-1)
							{
								int ri = parms.getReturnIndex();
								if(ri>=0)
								{
									descriptions[ri] = efd;
									
									if(efd.type_oid==1790 && row.GetData(i) != DBNull.Value )//if it is a refcursor, fetch all records
									{
										EDBCommand cmd = new EDBCommand("fetch all in \""+ row.GetData(i).ToString()+"\"",context);
										EDBDataReader rst =  cmd.ExecuteReader();
										data[ri] = rst;
									}
									else
									{
										data[ri] = row.GetData(i);
									}
								}
							}
							else
							{
								descriptions[efd.ReturingIndex] = efd;								
								if(efd.type_oid==1790 && row.GetData(i) != DBNull.Value )//if it is a refcursor, fetch all records
								{
									EDBCommand cmd = new EDBCommand("fetch all in \""+ row.GetData(i).ToString()+"\"", context);
									EDBDataReader rst =  cmd.ExecuteReader();							
									data[efd.ReturingIndex] = rst;									
								}
								else
								{
									data[efd.ReturingIndex] = row.GetData(i);
								}
							}
						}						
						EDBRowDescription eds = new EDBRowDescription(context.BackendProtocolVersion);
						EDBAsciiRow erow = new EDBAsciiRow(eds, context.BackendProtocolVersion);						
						if(parms.getReturnIndex()!=-1)
							parms.Insert(parms.getReturnIndex(),parms.GetReturnParam());
						for(int i=0;i<descriptions.Length;i++)
						{							
							EDBParameter p = parms.GetParameter(i);
							if(descriptions[i] == null)
							{
								EDBRowDescriptionFieldData fd = new EDBRowDescriptionFieldData();
								fd.name = p.ParameterName;
								fd.table_oid = 0;
								fd.column_attribute_number = 0;
								fd.type_modifier = 0;
								fd.format_code = 0;
								fd.ReturingIndex = (short)i;
								descriptions[i]=fd;
								data[i]=p.Value;
							}
							eds.AddField(descriptions[i]);

							 erow.AddData(data[i]);
						}																		
						mediator.ReplaceRowDescription(eds);
						mediator.ReplaceDataRow(erow);

						mediator.UpdateCompletedResponse();
						for(int i=0;i<data.Length;i++)
						{
							parms.GetParameter(i).Value=data[i];
						}										
				}

						// Now wait for CompletedResponse message.
				break;
				default :
                    // This could mean a number of things
                    //   We've gotten out of sync with the backend?
                    //   We need to implement this type?
                    //   Backend has gone insane?
                    // FIXME
                    // what exception should we really throw here?
                    throw new NotSupportedException(String.Format("Backend sent unrecognized response type: {0}", (Char)message));

                }
            }
        }
    }
}
