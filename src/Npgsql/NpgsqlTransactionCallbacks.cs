#if NET45 || NET451
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
using System.Data;
using System.Reflection;
using  EnterpriseDB.EDBClient.Logging;
using  EnterpriseDB.EDBClient.FrontendMessages;

namespace  EnterpriseDB.EDBClient
{
    internal interface IEDBTransactionCallbacks : IDisposable
    {
        string GetName();
        void PrepareTransaction();
        void CommitTransaction();
        void RollbackTransaction();
    }

    internal class EDBTransactionCallbacks : MarshalByRefObject, IEDBTransactionCallbacks
    {
        private EDBConnection _connection;
        private readonly string _connectionString;
        private bool _closeConnectionRequired;
        private bool _prepared;
        private readonly string _txName = Guid.NewGuid().ToString();
        private static readonly EDBLogger Log = EDBLogManager.GetCurrentClassLogger();

        public EDBTransactionCallbacks(EDBConnection connection)
        {
            _connection = connection;
            _connectionString = _connection.ConnectionString;
            _connection.Disposed += new EventHandler(_connection_Disposed);
        }

        private void _connection_Disposed(object sender, EventArgs e)
        {
            // TODO: what happens if this is called from another thread?
            // connections should not be shared across threads while in a transaction
            _connection.Disposed -= new EventHandler(_connection_Disposed);
            _connection = null;
        }

        private EDBConnection GetConnection()
        {
            if (_connection == null || (_connection.FullState & ConnectionState.Open) != ConnectionState.Open)
            {
                _connection = new EDBConnection(_connectionString);
                _connection.Open();
                _closeConnectionRequired = true;
                return _connection;
            }
            else
            {
                return _connection;
            }
        }

#region IEDBTransactionCallbacks Members

        public string GetName()
        {
            return _txName;
        }

        public void CommitTransaction()
        {
            Log.Debug("Commit transaction");
            var connection = GetConnection();

            if (_prepared)
            {
                connection.Connector.ExecuteInternalCommand($"COMMIT PREPARED '{_txName}'");
            }
            else
            {
                connection.Connector.ExecuteInternalCommand(PregeneratedMessage.CommitTransaction);
            }
        }

        public void PrepareTransaction()
        {
            if (!_prepared)
            {
                Log.Debug("Prepare transaction");
                EDBConnection connection = GetConnection();
                connection.Connector.ExecuteInternalCommand($"PREPARE TRANSACTION '{_txName}'");
                _prepared = true;
            }
        }

        public void RollbackTransaction()
        {
            Log.Debug("Rollback transaction");
            EDBConnection connection = GetConnection();

            if (_prepared)
                connection.Connector.ExecuteInternalCommand($"ROLLBACK PREPARED '{_txName}'");
            else
                connection.Connector.ExecuteInternalCommand(PregeneratedMessage.RollbackTransaction);
        }

#endregion

#region IDisposable Members

        public void Dispose()
        {
            if (_closeConnectionRequired)
            {
                _connection.Close();
            }
            _closeConnectionRequired = false;
        }

#endregion
    }
}
#endif
