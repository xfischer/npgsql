// created on 17/11/2002 at 19:04

// EDB.EDBTransaction.cs
//
// Author:
//	Francisco Jr. (fxjrlists@yahoo.com.br)
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
using System.Text;
using System.Resources;
using System.Data;


namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Represents a transaction to be made in a PostgreSQL database. This class cannot be inherited.
    /// </summary>
    public sealed class EDBTransaction : MarshalByRefObject, IDbTransaction
    {
        private static readonly String CLASSNAME = "EDBTransaction";
        private static ResourceManager resman = new ResourceManager(typeof(EDBTransaction));

        private EDBConnection    _conn = null;
        private IsolationLevel      _isolation = IsolationLevel.ReadCommitted;
        private bool                _disposed = false;

        internal EDBTransaction(EDBConnection conn) : this(conn, IsolationLevel.ReadCommitted)
        {}

        internal EDBTransaction(EDBConnection conn, IsolationLevel isolation)
        {
            resman = new System.Resources.ResourceManager(this.GetType());

            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, CLASSNAME);
            if ((isolation != IsolationLevel.ReadCommitted) &&
                    (isolation != IsolationLevel.Serializable))
                throw new ArgumentOutOfRangeException(resman.GetString("Exception_UnsopportedIsolationLevel"), "isolation");

            _conn = conn;
            _isolation = isolation;

            StringBuilder commandText = new StringBuilder("SET TRANSACTION ISOLATION LEVEL ");

            if (isolation == IsolationLevel.ReadCommitted)
                commandText.Append("READ COMMITTED");
            else
                commandText.Append("SERIALIZABLE");

            commandText.Append("; BEGIN");

            EDBCommand command = new EDBCommand(commandText.ToString(), conn.Connector);
            command.ExecuteNonQuery();
            _conn.Connector.Transaction = this;
        }

        /// <summary>
        /// Gets the <see cref="EDB.EDBConnection">EDBConnection</see>
        /// object associated with the transaction, or a null reference if the
        /// transaction is no longer valid.
        /// </summary>
        /// <value>The <see cref="EDB.EDBConnection">EDBConnection</see>
        /// object associated with the transaction.</value>
        public EDBConnection Connection
        {
            get
            {
                return _conn;
            }
        }


        IDbConnection IDbTransaction.Connection
        {
            get
            {
                return Connection;
            }
        }

        /// <summary>
        /// Specifies the <see cref="System.Data.IsolationLevel">IsolationLevel</see> for this transaction.
        /// </summary>
        /// <value>The <see cref="System.Data.IsolationLevel">IsolationLevel</see> for this transaction.
        /// The default is <b>ReadCommitted</b>.</value>
        public IsolationLevel IsolationLevel
        {
            get
            {
                if (_conn == null)
                {
                    throw new InvalidOperationException(resman.GetString("Exception_NoTransaction"));
                }

                return _isolation;
            }
        }

        /// <summary>
        /// Releases the unmanaged resources used by the
        /// <see cref="EDB.EDBTransaction">EDBTransaction</see>
        /// and optionally releases the managed resources.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            this.Dispose(true);
        }

        private void Dispose(Boolean disposing)
        {
            if(disposing && this._conn != null)
            {
                if (_conn.Connector.Transaction != null)
                    this.Rollback();

                this._disposed = true;
            }
        }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        public void Commit()
        {
            CheckDisposed();

            if (_conn == null)
            {
                throw new InvalidOperationException(resman.GetString("Exception_NoTransaction"));
            }

            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Commit");

            EDBCommand command = new EDBCommand("COMMIT", _conn.Connector);
            command.ExecuteNonQuery();
            _conn.Connector.Transaction = null;
            _conn = null;
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        public void Rollback()
        {
            CheckDisposed();

            if (_conn == null)
            {
                throw new InvalidOperationException(resman.GetString("Exception_NoTransaction"));
            }

            EDBEventLog.LogMethodEnter(LogLevel.Debug, CLASSNAME, "Rollback");

            EDBCommand command = new EDBCommand("ROLLBACK", _conn.Connector);
            command.ExecuteNonQuery();
            _conn.Connector.Transaction = null;
            _conn = null;
        }

        /// <summary>
        /// Cancel the transaction without telling the backend about it.  This is
        /// used to make the transaction go away when closing a connection.
        /// </summary>
        internal void Cancel()
        {
            CheckDisposed();

            if (_conn != null)
            {
                _conn.Connector.Transaction = null;
                _conn = null;
            }
        }

        internal bool Disposed{
            get
            {
                return _disposed;
            }
        }


        internal void CheckDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(CLASSNAME);

        }

        ~EDBTransaction()
        {
            Dispose(false);
        }

    }
}
