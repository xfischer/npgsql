// created on 6/14/2002 at 7:56 PM

// EnterpriseDB.EDBClient.EDBState.cs
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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Resources;
using System.Text;
using System.Threading;

namespace EnterpriseDB.EDBClient
{
    ///<summary> This class represents the base class for the state pattern design pattern
    /// implementation.
    /// </summary>
    ///
    internal abstract partial class EDBState
    {
        private enum BackEndMessageCode
        {
            IO_ERROR = -1, // Connection broken. Mono returns -1 instead of throwing an exception as ms.net does.

            CopyData = 'd',
            CopyDone = 'c',
            DataRow = 'D',

            BackendKeyData = 'K',
            CancelRequest = 'F',
            CompletedResponse = 'C',
            CopyDataRows = ' ',
            CopyInResponse = 'G',
            CopyOutResponse = 'H',
            EmptyQueryResponse = 'I',
            ErrorResponse = 'E',
            FunctionCall = 'F',
            FunctionCallResponse = 'V',

            AuthenticationRequest = 'R',

            NoticeResponse = 'N',
            NotificationResponse = 'A',
            ParameterStatus = 'S',
            PasswordPacket = ' ',
            ReadyForQuery = 'Z',
            RowDescription = 'T',
            SSLRequest = ' ',

            // extended query backend messages
            ParseComplete = '1',
            BindComplete = '2',
            PortalSuspended = 's',
            ParameterDescription = 't',
            NoData = 'n',
            CloseComplete = '3',

            /* EnterpriseDB team */
           OutDescription = 'u', //Describe Out from server
		    ParamData = 'v'		//Parameter Out data

        }

        private enum AuthenticationRequestType
        {
            AuthenticationOk = 0,
            AuthenticationKerberosV4 = 1,
            AuthenticationKerberosV5 = 2,
            AuthenticationClearTextPassword = 3,
            AuthenticationCryptPassword = 4,
            AuthenticationMD5Password = 5,
            AuthenticationSCMCredential = 6,
            AuthenticationGSS = 7,
            AuthenticationGSSContinue = 8,
            AuthenticationSSPI = 9
        }

        static byte[] NullTerminateArray(byte[] input)
        {
            byte[] output = new byte[input.Length + 1];
            input.CopyTo(output, 0);

            return output;
        }

        protected IEnumerable<IServerResponseObject> ProcessBackendResponses(EDBConnector context)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ProcessBackendResponses");

            using (new ContextResetter(context))
            {
                Stream stream = context.Stream;
                EDBMediator mediator = context.Mediator;

                /* EnterpriseDB Team */

               // EDBRowDescription lastRowDescription = null;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                EDBRowDescription lastRowDescription = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                EDBRowDescription rowOutDescription = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                CachingRow returnRow = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                Object returnData = null;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
               int lastParam = 0;

                List<EDBError> errors = new List<EDBError>();

                for (;;)
                {
                    // Check the first Byte of response.
                    BackEndMessageCode message = (BackEndMessageCode) stream.ReadByte();
                      switch (message)
                    {
                        case BackEndMessageCode.ErrorResponse:

                            EDBError error = new EDBError(stream);
                            error.ErrorSql = mediator.GetSqlSent();

                            errors.Add(error);

                            EDBEventLog.LogMsg(resman, "Log_ErrorResponse", LogLevel.Debug, error.Message);

                            // Return imediately if it is in the startup state or connected state as
                            // there is no more messages to consume.
                            // Possible error in the EDBStartupState:
                            //        Invalid password.
                            // Possible error in the EDBConnectedState:
                            //        No pg_hba.conf configured.

                            if (!context.RequireReadyForQuery)
                            {
                                throw new EDBException(errors);
                            }

                            break;
                        case BackEndMessageCode.AuthenticationRequest:

                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "AuthenticationRequest");

                            // Get the length in case we're getting AuthenticationGSSContinue
                            int authDataLength = PGUtil.ReadInt32(stream) - 8;

                            AuthenticationRequestType authType = (AuthenticationRequestType) PGUtil.ReadInt32(stream);
                            switch (authType)
                            {
                                case AuthenticationRequestType.AuthenticationOk:
                                    EDBEventLog.LogMsg(resman, "Log_AuthenticationOK", LogLevel.Debug);
                                    break;
                                case AuthenticationRequestType.AuthenticationClearTextPassword:
                                    EDBEventLog.LogMsg(resman, "Log_AuthenticationClearTextRequest", LogLevel.Debug);

                                    // Send the PasswordPacket.

                                    ChangeState(context, EDBStartupState.Instance);
                                    context.Authenticate(NullTerminateArray(context.Password));

                                    break;
                                case AuthenticationRequestType.AuthenticationMD5Password:
                                    EDBEventLog.LogMsg(resman, "Log_AuthenticationMD5Request", LogLevel.Debug);
                                    // Now do the "MD5-Thing"
                                    // for this the Password has to be:
                                    // 1. md5-hashed with the username as salt
                                    // 2. md5-hashed again with the salt we get from the backend

                                    MD5 md5 = MD5.Create();

                                    // 1.
                                    byte[] passwd = context.Password;
                                    byte[] saltUserName = BackendEncoding.UTF8Encoding.GetBytes(context.UserName);

                                    byte[] crypt_buf = new byte[passwd.Length + saltUserName.Length];

                                    passwd.CopyTo(crypt_buf, 0);
                                    saltUserName.CopyTo(crypt_buf, passwd.Length);

                                    StringBuilder sb = new StringBuilder();
                                    byte[] hashResult = md5.ComputeHash(crypt_buf);
                                    foreach (byte b in hashResult)
                                    {
                                        sb.Append(b.ToString("x2"));
                                    }

                                    String prehash = sb.ToString();

                                    byte[] prehashbytes = BackendEncoding.UTF8Encoding.GetBytes(prehash);
                                    crypt_buf = new byte[prehashbytes.Length + 4];

                                    stream.Read(crypt_buf, prehashbytes.Length, 4);
                                    // Send the PasswordPacket.
                                    ChangeState(context, EDBStartupState.Instance);

                                    // 2.
                                    prehashbytes.CopyTo(crypt_buf, 0);

                                    sb = new StringBuilder("md5"); // This is needed as the backend expects md5 result starts with "md5"
                                    hashResult = md5.ComputeHash(crypt_buf);
                                    foreach (byte b in hashResult)
                                    {
                                        sb.Append(b.ToString("x2"));
                                    }

                                    context.Authenticate(NullTerminateArray(BackendEncoding.UTF8Encoding.GetBytes(sb.ToString())));

                                    break;
#if WINDOWS && UNMANAGED

                                case AuthenticationRequestType.AuthenticationGSS:
                                    {
                                        if (context.IntegratedSecurity)
                                        {
                                            // For GSSAPI we have to use the supplied hostname
                                            context.SSPI = new SSPIHandler(context.Host, "POSTGRES", true);
                                            ChangeState(context, EDBStartupState.Instance);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                                            context.Authenticate(context.SSPI.Continue(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                            break;
                                        }
                                        else
                                        {
                                            // TODO: correct exception
                                            throw new Exception();
                                        }
                                    }

                                case AuthenticationRequestType.AuthenticationSSPI:
                                    {
                                        if (context.IntegratedSecurity)
                                        {
                                            // For SSPI we have to get the IP-Address (hostname doesn't work)
                                            string ipAddressString = ((IPEndPoint)context.Socket.RemoteEndPoint).Address.ToString();
                                            context.SSPI = new SSPIHandler(ipAddressString, "POSTGRES", false);
                                            ChangeState(context, EDBStartupState.Instance);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                                            context.Authenticate(context.SSPI.Continue(null));
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                                            break;
                                        }
                                        else
                                        {
                                            // TODO: correct exception
                                            throw new Exception();
                                        }
                                    }

                                case AuthenticationRequestType.AuthenticationGSSContinue:
                                    {
                                        byte[] authData = new byte[authDataLength];
                                        PGUtil.CheckedStreamRead(stream, authData, 0, authDataLength);
                                        byte[] passwd_read = context.SSPI.Continue(authData);
                                        if (passwd_read.Length != 0)
                                        {
                                            context.Authenticate(passwd_read);
                                        }
                                        break;
                                    }

#endif

                                default:
                                    // Only AuthenticationClearTextPassword and AuthenticationMD5Password supported for now.
                                    errors.Add(new EDBError(String.Format(resman.GetString("Exception_AuthenticationMethodNotSupported"), authType)));

                                    throw new EDBException(errors);
                            }
                            break;
                        case BackEndMessageCode.RowDescription:

                            lastRowDescription = new EDBRowDescription(stream, context.OidToNameMapping, context.CompatVersion,false);


                      /*      if (context.Mediator.Type != CommandType.StoredProcedure)   //TODO ZK 
                            {

                                lastRowDescription = new EDBRowDescription(stream, context.OidToNameMapping, context.CompatVersion);
                                yield return lastRowDescription;
                            }
                          /*  else
                            {
                                lastRowOutDescription = new EDBRowOutDescription(stream, context.OidToNameMapping, context.CompatVersion,true);
                                break;
                            }*/
                            /*
                             * EDBTeam:
                             * If command type is not procedure then return row description
                             * FIXME: it can be handled in a better way
                             * Return param RowDescription should not be returned
                             */
                               if (context.Mediator.Type != CommandType.StoredProcedure) //TODO ZK 
                                yield return lastRowDescription;
                             break;

                         //   yield return new EDBRowDescription(stream, context.OidToNameMapping, context.CompatVersion);
                       

                        /* EnterpriseDB Team */
                        case BackEndMessageCode.OutDescription:
                           EDBEventLog.LogMsg(resman, "Log_ProtocalMessage", LogLevel.Debug, "RowDescription");
                            rowOutDescription = new EDBRowDescription(stream, context.OidToNameMapping, context.CompatVersion,true);
                            //yield break;
                            break;
                        case BackEndMessageCode.ParameterDescription:

                            // Do nothing,for instance,  just read...
                            int lenght = PGUtil.ReadInt32(stream);
                            int nb_param = PGUtil.ReadInt16(stream);
                            for (int i = 0; i < nb_param; i++)
                            {
                                int typeoid = PGUtil.ReadInt32(stream);
                            }

                            break;

                        case BackEndMessageCode.DataRow:
                            if (context.Mediator.Type == CommandType.StoredProcedure)
                            {
#pragma warning disable CS8604 // Possible null reference argument.
                                ForwardsOnlyRow dataRow = new ForwardsOnlyRow(new StringRowReaderV3(lastRowDescription, stream));
#pragma warning restore CS8604 // Possible null reference argument.
                                CachingRow dataRow1 = new CachingRow(dataRow);

                                if (lastRowDescription[0].TypeOID == 1790)
                                {
                                    returnData = dataRow[0];
                                }
                                else
                                {
                                    returnData = dataRow1[0];

                                    if (context.Mediator.Parameters.ReturnIndex != -1)
                                    {
                                        context.Mediator.Parameters.Insert(context.Mediator.Parameters.ReturnIndex, context.Mediator.Parameters.ReturnParam);
                                        context.Mediator.Parameters[context.Mediator.Parameters.ReturnIndex].Value = returnData;
                                    }

                                    returnRow = dataRow1;
                                }
                                break;
                            }
                            yield return new StringRowReader(stream);
                            break;
                                                     
                        /* EnterpriseDB Team */

                       case BackEndMessageCode.ParamData:
                            EDBEventLog.LogMsg(resman, "Log_ProtocalMessage", LogLevel.Debug, "ParamData");
                         //   ForwardsOnlyRow paramDataRow = new ForwardsOnlyRow(new StringRowReaderV3(rowOutDescription, stream));

#pragma warning disable CS8604 // Possible null reference argument.
                            ForwardsOnlyRow paramDataRow = new ForwardsOnlyRow(new StringRowReaderV3(rowOutDescription,stream));
#pragma warning restore CS8604 // Possible null reference argument.
                           
                            CachingRow outrow = new CachingRow(paramDataRow);
                            if (context.Mediator.Parameters.ReturnIndex != -1)
                            {
                                context.Mediator.Parameters.Insert(context.Mediator.Parameters.ReturnIndex, context.Mediator.Parameters.ReturnParam);
#pragma warning disable CS8601 // Possible null reference assignment.
                                context.Mediator.Parameters[context.Mediator.Parameters.ReturnIndex].Value = returnData;
#pragma warning restore CS8601 // Possible null reference assignment.
                            }
                            if (rowOutDescription != null)
                            {
                                for (int i = 0; i < rowOutDescription.NumFields; i++)
                                {
                                    mediator.Parameters[rowOutDescription[i].ReturningIndex].Value = outrow[i];
                                }
#pragma warning disable CS8604 // Possible null reference argument.
                                outrow.AddData(returnData);
#pragma warning restore CS8604 // Possible null reference argument.
                                outrow.numFields = rowOutDescription.NumFields + 1;
                                if (lastRowDescription != null)
                                    rowOutDescription.AddReturnData(lastRowDescription[0]);

                                if ((context.Mediator.IsReader == true) && (context.Mediator.hasRefcursorType == false))
                                {
                                    yield return rowOutDescription;
                                    yield return outrow;
                                   // yield return new CompletedResponse(stream);
                                }

                            }
                            lastParam = 1;
                            break;
                            
                        case BackEndMessageCode.ReadyForQuery:

//                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ReadyForQuery");

                            // Possible status bytes returned:
                            //   I = Idle (no transaction active).
                            //   T = In transaction, ready for more.
                            //   E = Error in transaction, queries will fail until transaction aborted.
                            // Just eat the status byte, we have no use for it at this time.
                             PGUtil.ReadInt32(stream);
                            stream.ReadByte();

                            ChangeState(context, EDBReadyState.Instance);

                            if (errors.Count != 0)
                            {
                                throw new EDBException(errors);
                            }
                          if (context.Mediator.Type == CommandType.StoredProcedure &&  lastParam == 1) // || context.Mediator.ExecutingRefCursor == true) 
                            {
                               lastParam++;
                                break;
                            }
                            else

                                yield break;

                        case BackEndMessageCode.BackendKeyData:

                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BackendKeyData");
                            // BackendKeyData message.
                            EDBBackEndKeyData backend_keydata = new EDBBackEndKeyData(stream);
                            context.BackEndKeyData = backend_keydata;

                            // Wait for ReadForQuery message
                            break;

                        case BackEndMessageCode.NoticeResponse:
                            // Notices and errors are identical except that we
                            // just throw notices away completely ignored.
                            context.FireNotice(new EDBError(stream));
                            break;

                        case BackEndMessageCode.CompletedResponse:
                            PGUtil.ReadInt32(stream);
                            yield return new CompletedResponse(stream);
                            break;
                        case BackEndMessageCode.ParseComplete:
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ParseComplete");
                            // Just read up the message length.
                            PGUtil.ReadInt32(stream);
                            break;
                        case BackEndMessageCode.BindComplete:
//                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BindComplete");
                            // Just read up the message length.
                            PGUtil.ReadInt32(stream);
                            break;
                        case BackEndMessageCode.EmptyQueryResponse:
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "EmptyQueryResponse");
                            PGUtil.ReadInt32(stream);
                            break;
                        case BackEndMessageCode.NotificationResponse:
                            // Eat the length
                            PGUtil.ReadInt32(stream);
                            context.FireNotification(new EDBNotificationEventArgs(stream, true));
                            if (context.IsNotificationThreadRunning)
                            {
                                yield break;
                            }
                            break;
                        case BackEndMessageCode.ParameterStatus:
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ParameterStatus");
                            EDBParameterStatus parameterStatus = new EDBParameterStatus(stream);

                            EDBEventLog.LogMsg(resman, "Log_ParameterStatus", LogLevel.Debug, parameterStatus.Parameter,
                                                  parameterStatus.ParameterValue);

                            context.AddParameterStatus(parameterStatus);

                            if (parameterStatus.Parameter == "server_version")
                            {
                                // Deal with this here so that if there are
                                // changes in a future backend version, we can handle it here in the
                                // protocol handler and leave everybody else put of it.
                                string versionString = parameterStatus.ParameterValue.Trim();
                                for (int idx = 0; idx != versionString.Length; ++idx)
                                {
                                    char c = parameterStatus.ParameterValue[idx];
                                    if (!char.IsDigit(c) && c != '.')
                                    {
                                        versionString = versionString.Substring(0, idx);
                                        break;
                                    }
                                }
                                context.ServerVersion = new Version(versionString);
                            }
                            break;
                        case BackEndMessageCode.NoData:
                            // This nodata message may be generated by prepare commands issued with queries which doesn't return rows
                            // for example insert, update or delete.
                            // Just eat the message.
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ParameterStatus");
                            PGUtil.ReadInt32(stream);
                            break;

                        case BackEndMessageCode.CopyInResponse:
                            // Enter COPY sub protocol and start pushing data to server
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "CopyInResponse");
                            ChangeState(context, new EDBCopyInState());
                            PGUtil.ReadInt32(stream); // length redundant
                            context.CurrentState.StartCopy(context, ReadCopyHeader(stream));
                            yield break;
                                // Either StartCopy called us again to finish the operation or control should be passed for user to feed copy data

                        case BackEndMessageCode.CopyOutResponse:
                            // Enter COPY sub protocol and start pulling data from server
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "CopyOutResponse");
                            ChangeState(context, EDBCopyOutState.Instance);
                            PGUtil.ReadInt32(stream); // length redundant
                            context.CurrentState.StartCopy(context, ReadCopyHeader(stream));
                            yield break;
                                // Either StartCopy called us again to finish the operation or control should be passed for user to feed copy data

                        case BackEndMessageCode.CopyData:
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "CopyData");
                            Int32 len = PGUtil.ReadInt32(stream) - 4;
                            byte[] buf = new byte[len];
                            PGUtil.ReadBytes(stream, buf, 0, len);
                            context.Mediator.ReceivedCopyData = buf;
                            yield break; // read data from server one chunk at a time while staying in copy operation mode

                        case BackEndMessageCode.CopyDone:
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "CopyDone");
                            PGUtil.ReadInt32(stream); // CopyDone can not have content so this is always 4
                            // This will be followed by normal CommandComplete + ReadyForQuery so no op needed
                            break;

                        case BackEndMessageCode.IO_ERROR:
                            // Connection broken. Mono returns -1 instead of throwing an exception as ms.net does.
                            throw new IOException();

                        default:
                            // This could mean a number of things
                            //   We've gotten out of sync with the backend?
                            //   We need to implement this type?
                            //   Backend has gone insane?
                            // FIXME
                            // what exception should we really throw here?
                            throw new NotSupportedException(String.Format("Backend sent unrecognized response type: {0}", (Char) message));
                    }
                }
            }
        }
    }
}
