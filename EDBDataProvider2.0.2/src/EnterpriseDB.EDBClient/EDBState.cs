// created on 6/14/2002 at 7:56 PM

// Npgsql.EDBState.cs
//
// Author:
//     Dave Joyner <d4ljoyn@yahoo.com>
//
//    Copyright (C) 2002 The Npgsql Development Team
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
using System.Net.Sockets;
using System.Resources;
using System.Text;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using System.Reflection;

namespace EnterpriseDB.EDBClient
{
    ///<summary> This class represents the base class for the state pattern design pattern
    /// implementation.
    /// </summary>
    ///
    internal abstract class EDBState
    {
        protected static readonly Encoding ENCODING_UTF8 = Encoding.UTF8;
        private readonly String CLASSNAME = MethodBase.GetCurrentMethod().DeclaringType.Name;
        protected readonly static ResourceManager resman = new ResourceManager(MethodBase.GetCurrentMethod().DeclaringType);
        
        

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

        public virtual void Authenticate(EDBConnector context, byte[] password)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual IEnumerable<IServerResponseObject> QueryEnum(EDBConnector context, EDBCommand command)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public void Query(EDBConnector context, EDBCommand command)
        {
            IterateThroughAllResponses(QueryEnum(context, command));
        }

        public virtual void FunctionCall(EDBConnector context, EDBCommand command)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void Parse(EDBConnector context, EDBParse parse,EDBCommand command, Boolean isSupportCallable)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void Flush(EDBConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual IEnumerable<IServerResponseObject> SyncEnum(EDBConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public void TestNotify(EDBConnector context)
        {
            //ZA  Hnotifytest CNOTIFY Z
            //Qlisten notifytest;notify notifytest; 
            Stream stm = context.Stream;
            string uuidString = "uuid" + Guid.NewGuid().ToString("N");
            PGUtil.WriteString("Qlisten " + uuidString + ";notify " + uuidString + ";", stm);
            Queue<byte> buffer = new Queue<byte>();
            byte[] convertBuffer = new byte[36];
            for (;;)
            {
                int newByte = stm.ReadByte();
                if (newByte == -1)
                {
                    throw new EndOfStreamException();
                }
                buffer.Enqueue((byte) newByte);
                if (buffer.Count > 35)
                {
                    buffer.CopyTo(convertBuffer, 0);
                    if (ENCODING_UTF8.GetString(convertBuffer) == uuidString)
                    {
                        for (;;)
                        {
                            switch (stm.ReadByte())
                            {
                                case -1:
                                    throw new EndOfStreamException();
                                case 'Z':
                                    context.Query(new EDBCommand("UNLISTEN *", context));
                                    return;
                            }
                        }
                    }
                    else
                    {
                        buffer.Dequeue();
                    }
                }
            }
        }

        public void TestConnector(EDBConnector context)
        {
            switch (context.BackendProtocolVersion)
            {
                case ProtocolVersion.Version2:
                    TestNotify(context);
                    break;
                case ProtocolVersion.Version3:
                    EmptySync(context);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        public void EmptySync(EDBConnector context)
        {
            Stream stm = context.Stream;
            new EDBSync().WriteToStream(stm);
            stm.Flush();
            Queue<int> buffer = new Queue<int>();
            //byte[] compareBuffer = new byte[6];
            int[] messageSought = new int[] {'Z', 0, 0, 0, 5};
            int newByte;
            for (;;)
            {
                switch (newByte = stm.ReadByte())
                {
                    case -1:
                        throw new EndOfStreamException();
                    case 'E':
                    case 'I':
                    case 'T':
                        if (buffer.Count > 4)
                        {
                            bool match = true;
                            int i = 0;
                            foreach (byte cmp in buffer)
                            {
                                if (cmp != messageSought[i++])
                                {
                                    match = false;
                                    break;
                                }
                            }
                            if (match)
                            {
                                return;
                            }
                        }
                        break;
                    default:
                        buffer.Enqueue(newByte);
                        if (buffer.Count > 5)
                        {
                            buffer.Dequeue();
                        }
                        break;
                }
            }
        }

        public EDBRowDescription Sync(EDBConnector context)
        {
            EDBRowDescription lastDescription = null;
            foreach (IServerResponseObject obj in SyncEnum(context))
            {
                if (obj is EDBRowDescription)
                {
                    lastDescription = obj as EDBRowDescription;
                }
                else
                {
                    if (obj is IDisposable)
                    {
                        (obj as IDisposable).Dispose();
                    }
                }
            }
            return lastDescription;
        }

        public virtual void Bind(EDBConnector context, EDBBind bind)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void Execute(EDBConnector context, EDBExecute execute)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual IEnumerable<IServerResponseObject> ExecuteEnum(EDBConnector context, EDBExecute execute)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void Describe(EDBConnector context, EDBDescribe describe)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void CancelRequest(EDBConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        // COPY methods

        protected virtual void StartCopy(EDBConnector context, EDBCopyFormat copyFormat)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual byte[] GetCopyData(EDBConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void SendCopyData(EDBConnector context, byte[] buf, int off, int len)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void SendCopyDone(EDBConnector context)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual void SendCopyFail(EDBConnector context, String message)
        {
            throw new InvalidOperationException("Internal Error! " + this);
        }

        public virtual EDBCopyFormat CopyFormat
        {
            get { throw new InvalidOperationException("Internal Error! " + this); }
        }


        public virtual void Close(EDBConnector context)
        {
            try
            {
                context.Stream.Close();
            }
            catch
            {
            }
            context.Stream = null;
            ChangeState(context, EDBClosedState.Instance);
        }

        ///<summary>
        ///This method is used by the states to change the state of the context.
        /// </summary>
        protected static void ChangeState(EDBConnector context, EDBState newState)
        {
            context.CurrentState = newState;
        }

        ///<summary>
        /// This method is responsible to handle all protocol messages sent from the backend.
        /// It holds all the logic to do it.
        /// To exchange data, it uses a Mediator object from which it reads/writes information
        /// to handle backend requests.
        /// </summary>
        ///
        internal void ProcessBackendResponses(EDBConnector context)
        {
            IterateThroughAllResponses(ProcessBackendResponsesEnum(context));
        }

        private static void IterateThroughAllResponses(IEnumerable<IServerResponseObject> ienum)
        {
            foreach (IServerResponseObject obj in ienum) //iterate until finished.
            {
                if (obj is IDisposable)
                {
                    (obj as IDisposable).Dispose();
                }
            }
        }

        private class ContextResetter : IDisposable
        {
            private readonly EDBConnector _connector;

            public ContextResetter(EDBConnector connector)
            {
                _connector = connector;
            }

            public void Dispose()
            {
                _connector.RequireReadyForQuery = true;
            }
        }

        ///<summary>
        /// This method is responsible to handle all protocol messages sent from the backend.
        /// It holds all the logic to do it.
        /// To exchange data, it uses a Mediator object from which it reads/writes information
        /// to handle backend requests.
        /// </summary>
        ///
        internal IEnumerable<IServerResponseObject> ProcessBackendResponsesEnum(EDBConnector context)
        {
            try
            {
            // Process commandTimeout behavior.

            if ((context.Mediator.CommandTimeout > 0) &&
                (!context.Socket.Poll(1000000*context.Mediator.CommandTimeout, SelectMode.SelectRead)))
            {
                // If timeout occurs when establishing the session with server then
                // throw an exception instead of trying to cancel query. This helps to prevent loop as CancelRequest will also try to stablish a connection and sends commands.
                if (!((this is EDBStartupState || this is EDBConnectedState)))
                {
                    try
                    {
                        context.CancelRequest();
                        foreach (IServerResponseObject obj in ProcessBackendResponsesEnum(context))
                        {
                            if (obj is IDisposable)
                            {
                                (obj as IDisposable).Dispose();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                    //We should have gotten an error from CancelRequest(). Whether we did or not, what we
                    //really have is a timeout exception, and that will be less confusing to the user than
                    //"operation cancelled by user" or similar, so whatever the case, that is what we'll throw.
                    // Changed message again to report about the two possible timeouts: connection or command as the establishment timeout only was confusing users when the timeout was a command timeout.
                }
                throw new EDBException(resman.GetString("Exception_ConnectionOrCommandTimeout"));
            }
            switch (context.BackendProtocolVersion)
            {
                case ProtocolVersion.Version2:
                    return ProcessBackendResponses_Ver_2(context);
                case ProtocolVersion.Version3:
                    return ProcessBackendResponses_Ver_3(context);
                default:
                    throw new EDBException(resman.GetString("Exception_UnknownProtocol"));
            }
            
            }
            
            catch(ThreadAbortException)
            {
                try
                {
                    context.CancelRequest();
                    context.Close();
                }
                catch {}
                
                throw;
            }
                
        }

        protected IEnumerable<IServerResponseObject> ProcessBackendResponses_Ver_2(EDBConnector context)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ProcessBackendResponses");

            using (new ContextResetter(context))
            {
                Stream stream = context.Stream;
                EDBMediator mediator = context.Mediator;
                EDBRowDescription lastRowDescription = null;
                CachingRow returnRow = null;
                List<EDBError> errors = new List<EDBError>();

                for (;;)
                {
                    // Check the first Byte of response.
                    switch ((BackEndMessageCode) stream.ReadByte())
                    {
                        case BackEndMessageCode.ErrorResponse:

                            {
                                EDBError error = new EDBError(context.BackendProtocolVersion, stream);
                                error.ErrorSql = mediator.SqlSent;

                                errors.Add(error);

                                EDBEventLog.LogMsg(resman, "Log_ErrorResponse", LogLevel.Debug, error.Message);
                            }

                            // Return imediately if it is in the startup state or connected state as
                            // there is no more messages to consume.
                            // Possible error in the EDBStartupState:
                            //        Invalid password.
                            // Possible error in the NpgsqlConnectedState:
                            //        No pg_hba.conf configured.

                            if (!context.RequireReadyForQuery)
                            {
                                throw new EDBException(errors);
                            }

                            break;


                        case BackEndMessageCode.AuthenticationRequest:
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "AuthenticationRequest");
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
                                    context.Authenticate(context.Password);
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
                                    byte[] saltUserName = ENCODING_UTF8.GetBytes(context.UserName);

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

                                    byte[] prehashbytes = ENCODING_UTF8.GetBytes(prehash);


                                    byte[] saltServer = new byte[4];
                                    stream.Read(saltServer, 0, 4);
                                    // Send the PasswordPacket.
                                    ChangeState(context, EDBStartupState.Instance);


                                    // 2.

                                    crypt_buf = new byte[prehashbytes.Length + saltServer.Length];
                                    prehashbytes.CopyTo(crypt_buf, 0);
                                    saltServer.CopyTo(crypt_buf, prehashbytes.Length);

                                    sb = new StringBuilder("md5"); // This is needed as the backend expects md5 result starts with "md5"
                                    hashResult = md5.ComputeHash(crypt_buf);
                                    foreach (byte b in hashResult)
                                    {
                                        sb.Append(b.ToString("x2"));
                                    }

                                    context.Authenticate(ENCODING_UTF8.GetBytes(sb.ToString()));

                                    break;
                                default:
                                    // Only AuthenticationClearTextPassword and AuthenticationMD5Password supported for now.
                                    errors.Add(
                                        new EDBError(context.BackendProtocolVersion,
                                                        String.Format(resman.GetString("Exception_AuthenticationMethodNotSupported"), authType)));
                                    throw new EDBException(errors);
                            }
                            break;
                        case BackEndMessageCode.RowDescription:
                            yield return lastRowDescription = new EDBRowDescriptionV2(stream, context.OidToNameMapping,context.CompatVersion);
                            ;
                            break;
                        case BackEndMessageCode.DataRow:
                            yield return new ForwardsOnlyRow(new StringRowReaderV2(lastRowDescription, stream));
                            break;
                        case BackEndMessageCode.BinaryRow:
                            throw new NotSupportedException();
                        case BackEndMessageCode.ReadyForQuery:
                            ChangeState(context, EDBReadyState.Instance);
                            if (errors.Count != 0)
                            {
                                throw new EDBException(errors);
                            }
                            yield break;
                        case BackEndMessageCode.BackendKeyData:
                            context.BackEndKeyData = new EDBBackEndKeyData(context.BackendProtocolVersion, stream);
                            break;
                        case BackEndMessageCode.NoticeResponse:
                            context.FireNotice(new EDBError(context.BackendProtocolVersion, stream));
                            break;
                        case BackEndMessageCode.CompletedResponse:
                            yield return new CompletedResponse(stream);
                            break;

                        case BackEndMessageCode.CursorResponse:
                            // This is the cursor response message.
                            // It is followed by a C NULL terminated string with the name of
                            // the cursor in a FETCH case or 'blank' otherwise.
                            // In this case it should be always 'blank'.
                            // [FIXME] Get another name for this function.
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "CursorResponse");

                            String cursorName = PGUtil.ReadString(stream);
                            // Continue waiting for ReadyForQuery message.
                            break;

                        case BackEndMessageCode.EmptyQueryResponse:
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "EmptyQueryResponse");
                            PGUtil.ReadString(stream);
                            break;

                        case BackEndMessageCode.NotificationResponse:
                            context.FireNotification(new EDBNotificationEventArgs(stream, false));
                            if (context.IsNotificationThreadRunning)
                            {
                                yield break;
                            }
                            break;
                        case BackEndMessageCode.IO_ERROR:
                            // Connection broken. Mono returns -1 instead of throw an exception as ms.net does.
                            throw new IOException();

                        default:
                            // This could mean a number of things
                            //   We've gotten out of sync with the backend?
                            //   We need to implement this type?
                            //   Backend has gone insane?
                            throw new DataException("Backend sent unrecognized response type");
                    }
                }
            }
        }

        private enum BackEndMessageCode
        {
            IO_ERROR = -1, // Connection broken. Mono returns -1 instead of throwing an exception as ms.net does.

            CopyData = 'd',
            CopyDone = 'c',
            DataRow = 'D',
            BinaryRow = 'B', //Version 2 only

            BackendKeyData = 'K',
            CancelRequest = 'F',
            CompletedResponse = 'C',
            CopyDataRows = ' ',
            CopyInResponse = 'G',
            CopyOutResponse = 'H',
            CursorResponse = 'P', //Version 2 only
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

            //EDB Team extended query backend messeges
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

        protected IEnumerable<IServerResponseObject> ProcessBackendResponses_Ver_3(EDBConnector context)
        {
            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "ProcessBackendResponses");

            using (new ContextResetter(context))
            {
                Stream stream = context.Stream;
                EDBMediator mediator = context.Mediator;

                EDBRowDescription lastRowDescription = null;
                EDBRowDescription rowOutDescription = null;
                CachingRow returnRow = null;
                Object returnData = null;

                List<EDBError> errors = new List<EDBError>();

                for (;;)
                {
                    // Check the first Byte of response.
                    BackEndMessageCode message = (BackEndMessageCode) stream.ReadByte();
                    switch (message)
                    {
                        case BackEndMessageCode.ErrorResponse:

                            EDBError error = new EDBError(context.BackendProtocolVersion, stream);
                            error.ErrorSql = mediator.SqlSent;

                            errors.Add(error);

                            EDBEventLog.LogMsg(resman, "Log_ErrorResponse", LogLevel.Debug, error.Message);

                            // Return imediately if it is in the startup state or connected state as
                            // there is no more messages to consume.
                            // Possible error in the EDBStartupState:
                            //        Invalid password.
                            // Possible error in the NpgsqlConnectedState:
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
                                    context.Authenticate(context.Password);

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
                                    byte[] saltUserName = ENCODING_UTF8.GetBytes(context.UserName);

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

                                    byte[] prehashbytes = ENCODING_UTF8.GetBytes(prehash);
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

                                    context.Authenticate(ENCODING_UTF8.GetBytes(sb.ToString()));

                                    break;
#if WINDOWS && UNMANAGED

                                case AuthenticationRequestType.AuthenticationSSPI:
                                    {
                                        if (context.IntegratedSecurity)
                                        {
                                            context.SSPI = new SSPIHandler(context.Host, "POSTGRES");
                                            ChangeState(context, EDBStartupState.Instance);
                                            context.Authenticate(context.SSPI.Continue(null));
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
                                    errors.Add(
                                        new EDBError(context.BackendProtocolVersion,
                                                        String.Format(resman.GetString("Exception_AuthenticationMethodNotSupported"), authType)));
                                    throw new EDBException(errors);
                            }
                            break;
                        case BackEndMessageCode.RowDescription:
                            lastRowDescription = new EDBRowDescriptionV3(stream, context.OidToNameMapping,context.CompatVersion);
                            /*
                             * EDBTeam:
                             * If command type is not procedure then return row description
                             * FIXME: it can be handled in a better way
                             * Return param RowDescription should not be returned
                             */
                                if (context.Mediator.Type != CommandType.StoredProcedure)
                                   yield return lastRowDescription;
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
                            ForwardsOnlyRow dataRow = new ForwardsOnlyRow(new StringRowReaderV3(lastRowDescription, stream));
                            CachingRow dataRow1 = new CachingRow(dataRow);
                            
                            /*
                             * EDBTeam:
                             * if rowOutDescription is not null then the data row is always of function return value
                             * we need to save the return param value.
                             */
                            if(context.Mediator.Type == CommandType.StoredProcedure)
                            {
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
                            }
                            else
                                yield return dataRow;
                            break;

                        case BackEndMessageCode.ReadyForQuery:

                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ReadyForQuery");

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

                            yield break;

                        case BackEndMessageCode.BackendKeyData:

                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BackendKeyData");
                            // BackendKeyData message.
                            EDBBackEndKeyData backend_keydata = new EDBBackEndKeyData(context.BackendProtocolVersion, stream);
                            context.BackEndKeyData = backend_keydata;
                            // Wait for ReadForQuery message
                            break;

                        case BackEndMessageCode.NoticeResponse:
                            // Notices and errors are identical except that we
                            // just throw notices away completely ignored.
                            context.FireNotice(new EDBError(context.BackendProtocolVersion, stream));
                            break;

                        case BackEndMessageCode.CompletedResponse:
                            PGUtil.ReadInt32(stream);
                            yield return new CompletedResponse(stream);
                            break;
                        case BackEndMessageCode.ParseComplete:
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "ParseComplete");
                            // Just read up the message length.
                            PGUtil.ReadInt32(stream);
                            yield break;
                        case BackEndMessageCode.BindComplete:
                            EDBEventLog.LogMsg(resman, "Log_ProtocolMessage", LogLevel.Debug, "BindComplete");
                            // Just read up the message length.
                            PGUtil.ReadInt32(stream);
                            yield break;
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
                            ChangeState(context, EDBCopyInState.Instance);
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

                            /*
                             * EDBTeam:
                             * Out patameter Description and out param data
                             */
                        case BackEndMessageCode.OutDescription:
                            EDBEventLog.LogMsg(resman, "Log_ProtocalMessage", LogLevel.Debug, "RowDescription");          
                            rowOutDescription = new EDBRowOutDescriptionV3(stream, context.OidToNameMapping,context.CompatVersion);
                            break;
                        case BackEndMessageCode.ParamData:
                            EDBEventLog.LogMsg(resman, "Log_ProtocalMessage", LogLevel.Debug, "ParamData");          
                            ForwardsOnlyRow paramDataRow = new ForwardsOnlyRow(new StringRowReaderV3(rowOutDescription, stream));
                            CachingRow outrow = new CachingRow(paramDataRow);
                            if (context.Mediator.Parameters.ReturnIndex != -1)
                            {
                                context.Mediator.Parameters.Insert(context.Mediator.Parameters.ReturnIndex, context.Mediator.Parameters.ReturnParam);
                                context.Mediator.Parameters[context.Mediator.Parameters.ReturnIndex].Value = returnData;
                            }
                            if (rowOutDescription != null)
                            {
                                for (int i = 0; i < rowOutDescription.NumFields; i++)
                                {
                                    	mediator.Parameters[rowOutDescription[i].ReturningIndex].Value = outrow[i];                                
                                    }
                                outrow.AddData(returnData);
                                outrow.numFields = rowOutDescription.NumFields+1;
                                rowOutDescription.AddReturnData(lastRowDescription[0]);
                                if ((context.Mediator.IsReader == true) && (context.Mediator.hasRefcursorType == false))
                                {
                                    yield return rowOutDescription;
                                    yield return outrow;
                                }
                                yield break;
                            }
                            break;
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


        private static EDBCopyFormat ReadCopyHeader(Stream stream)
        {
            byte copyFormat = (byte) stream.ReadByte();
            Int16 numCopyFields = PGUtil.ReadInt16(stream);
            Int16[] copyFieldFormats = new Int16[numCopyFields];
            for (Int16 i = 0; i < numCopyFields; i++)
            {
                copyFieldFormats[i] = PGUtil.ReadInt16(stream);
            }
            return new EDBCopyFormat(copyFormat, copyFieldFormats);
        }
    }

    /// <summary>
    /// Represents a completed response message.
    /// </summary>
    internal class CompletedResponse : IServerResponseObject
    {
        private readonly int? _rowsAffected;
        private readonly long? _lastInsertedOID;

        public CompletedResponse(Stream stream)
        {
            string[] tokens = PGUtil.ReadString(stream).Split();
            if (tokens.Length > 1)
            {
                int rowsAffected;
                if (int.TryParse(tokens[tokens.Length - 1], out rowsAffected))
                    _rowsAffected = rowsAffected;
                else
                    _rowsAffected = null;
                
            }
            _lastInsertedOID = (tokens.Length > 2 && tokens[0].Trim().ToUpperInvariant() == "INSERT")
                                   ? long.Parse(tokens[1])
                                   : (long?) null;
        }

        public long? LastInsertedOID
        {
            get { return _lastInsertedOID; }
        }

        public int? RowsAffected
        {
            get { return _rowsAffected; }
        }
    }

    /// <summary>
    /// For classes representing messages sent from the client to the server.
    /// </summary>
    internal abstract class ClientMessage
    {
        protected static readonly Encoding UTF8Encoding = Encoding.UTF8;
        public abstract void WriteToStream(Stream outputStream);
    }

    /// <summary>
    /// Marker interface which identifies a class which represents part of
    /// a response from the server.
    internal interface IServerResponseObject
    {
    }

    /// <summary>
    /// Marker interface which identifies a class which may take possession of a stream for the duration of
    /// it's lifetime (possibly temporarily giving that possession to another class for part of that time.
    /// 
    /// It inherits from IDisposable, since any such class must make sure it leaves the stream in a valid state.
    /// 
    /// The most important such class is that compiler-generated from ProcessBackendResponsesEnum. Of course
    /// we can't make that inherit from this interface, alas.
    /// </summary>
    internal interface IStreamOwner : IServerResponseObject, IDisposable
    {
    }
}