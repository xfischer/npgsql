// created on 30/7/2002 at 00:31

// EnterpriseDB.EDBClient.EDBMediator.cs
//
// Author:
//    Francisco Jr. (fxjrlists@yahoo.com.br)
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
using System.Data;

namespace EnterpriseDB.EDBClient
{
    ///<summary>
    /// This class is responsible for serving as bridge between the backend
    /// protocol handling and the core classes. It is used as the mediator for
    /// exchanging data generated/sent from/to backend.
    /// </summary>
    ///
    internal sealed class EDBMediator
    {
        public enum SQLSentType
        {
            None,
            Simple,
            Parse,
            Execute
        }

        // Stream for user to exchange COPY data
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private Stream _copyStream;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        // Size of data chunks read from user stream and written to server in COPY IN
        private int _copyBufferSize = 8192;
        // Very temporary holder of data received during COPY OUT
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private byte[] _receivedCopyData;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        // Last command sent.  This is saved for possible later use by EDBException if an error occurs.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        private byte[] _sqlSent = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        private SQLSentType _sqlSentType = SQLSentType.None;
        internal bool hasRefcursorType = false;
        internal bool _executingRefCursor = false;
   
        /*EnterpriseDB Team */
        private CommandType _type;
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public EDBParameterCollection parameters;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private bool _isReader = true;
    
        // The current command timeout on the backend.  This is set via "SET statement_timeout = <milliseconds>".
        private Int32 _backendCommandTimeout = -1; // -1 means unknown - we have no way to know it until we set it.

        public String GetSqlSent()
        {
            if (_sqlSent == null)
            {
                return "";
            }
            switch (_sqlSentType)
            {
                case SQLSentType.None :
                    return "";

                case SQLSentType.Parse:
                    return string.Format("{{PARSE}} {0}", BackendEncoding.UTF8Encoding.GetString(_sqlSent));

                case SQLSentType.Execute :
                    return string.Format("{{EXECUTE}} {0}", BackendEncoding.UTF8Encoding.GetString(_sqlSent));

                default :
                    return BackendEncoding.UTF8Encoding.GetString(_sqlSent);

            } 
        }

        public void SetSqlSent(byte[] sqlSent, SQLSentType sqlSentType)
        {//We only use this if there is an error, so let's only get the string when that happens.
            _sqlSent = sqlSent;
            _sqlSentType = sqlSentType;
        }

        /// <summary>
        /// The current command timeout on the backend.  This is set via "SET statement_timeout = (milliseconds)".
        /// A value of -1 means the backend's timeout value is unknown because it has not yet been set.
        /// </summary>
        public Int32 BackendCommandTimeout
        {
            get { return _backendCommandTimeout; }
            set { _backendCommandTimeout = value; }
        }

        public Stream CopyStream
        {
            get { return _copyStream; }
            set { _copyStream = value; }
        }

        public int CopyBufferSize
        {
            get { return _copyBufferSize; }
            set { _copyBufferSize = value; }
        }
        /* EnterpriseDB Team */
        public EDBParameterCollection Parameters
        {
            set { parameters = value; }
            get { return parameters; }
        }

        public bool IsReader
        {
            set { _isReader = value; }

            get { return _isReader; }
        }

        public CommandType Type
        {
            set { _type = value; }
            get { return _type; }
        }
        public bool ExecutingRefCursor {

            set { _executingRefCursor = value; }
            get { return _executingRefCursor; }

        }

        public byte[] ReceivedCopyData
        {
            get
            {
                byte[] result = _receivedCopyData;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                _receivedCopyData = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
                return result;
            }
            set { _receivedCopyData = value; }
        }
    }
}
