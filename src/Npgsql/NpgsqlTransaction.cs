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
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using AsyncRewriter;
using JetBrains.Annotations;
using  EnterpriseDB.EDBClient.BackendMessages;
using  EnterpriseDB.EDBClient.FrontendMessages;
using  EnterpriseDB.EDBClient.Logging;

namespace  EnterpriseDB.EDBClient
{
    /// <summary>
    /// Represents a transaction to be made in a PostgreSQL database. This class cannot be inherited.
    /// </summary>
    public sealed partial class EDBTransaction : DbTransaction
    {
        #region Fields and Properties

        /// <summary>
        /// Specifies the <see cref="EDBConnection"/> object associated with the transaction.
        /// </summary>
        /// <value>The <see cref="EDBConnection"/> object associated with the transaction.</value>
        public new EDBConnection Connection { get; internal set; }

        /// <summary>
        /// Specifies the completion state of the transaction.
        /// </summary>
        /// <value>The completion state of the transaction.</value>
        public bool IsCompleted => Connection == null;

        /// <summary>
        /// Specifies the <see cref="EDBConnection"/> object associated with the transaction.
        /// </summary>
        /// <value>The <see cref="EDBConnection"/> object associated with the transaction.</value>
        protected override DbConnection DbConnection => Connection;

        EDBConnector Connector => Connection.Connector;

        bool _isDisposed;

        /// <summary>
        /// Specifies the <see cref="System.Data.IsolationLevel">IsolationLevel</see> for this transaction.
        /// </summary>
        /// <value>The <see cref="System.Data.IsolationLevel">IsolationLevel</see> for this transaction.
        /// The default is <b>ReadCommitted</b>.</value>
        public override IsolationLevel IsolationLevel
        {
            get
            {
                CheckReady();
                return _isolationLevel;
            }
        }
        readonly IsolationLevel _isolationLevel;

        const IsolationLevel DefaultIsolationLevel = IsolationLevel.ReadCommitted;

        static readonly EDBLogger Log = EDBLogManager.GetCurrentClassLogger();

        #endregion

        #region Constructors

        internal EDBTransaction(EDBConnection conn)
            : this(conn, DefaultIsolationLevel)
        {
            Contract.Requires(conn != null);
        }

        internal EDBTransaction(EDBConnection conn, IsolationLevel isolationLevel)
        {
            Contract.Requires(conn != null);
            Contract.Requires(isolationLevel != IsolationLevel.Chaos);

            Connection = conn;
            Connector.Transaction = this;
            Connector.TransactionStatus = TransactionStatus.Pending;

            switch (isolationLevel) {
                case IsolationLevel.RepeatableRead:
                    Connector.PrependInternalMessage(PregeneratedMessage.BeginTrans);
                    Connector.PrependInternalMessage(PregeneratedMessage.SetTransRepeatableRead);
                    break;
                case IsolationLevel.Serializable:
                case IsolationLevel.Snapshot:
                    Connector.PrependInternalMessage(PregeneratedMessage.BeginTrans);
                    Connector.PrependInternalMessage(PregeneratedMessage.SetTransSerializable);
                    break;
                case IsolationLevel.ReadUncommitted:
                    // PG doesn't really support ReadUncommitted, it's the same as ReadCommitted. But we still
                    // send as if.
                    Connector.PrependInternalMessage(PregeneratedMessage.BeginTrans);
                    Connector.PrependInternalMessage(PregeneratedMessage.SetTransReadUncommitted);
                    break;
                case IsolationLevel.ReadCommitted:
                    Connector.PrependInternalMessage(PregeneratedMessage.BeginTrans);
                    Connector.PrependInternalMessage(PregeneratedMessage.SetTransReadCommitted);
                    break;
                case IsolationLevel.Unspecified:
                    isolationLevel = DefaultIsolationLevel;
                    goto case DefaultIsolationLevel;
                default:
                    throw PGUtil.ThrowIfReached("Isolation level not supported: " + isolationLevel);
            }

            _isolationLevel = isolationLevel;
        }

        #endregion

        #region Commit

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        public override void Commit() => CommitInternal();

        [RewriteAsync]
        void CommitInternal()
        {
            var connector = CheckReady();
            using (connector.StartUserAction())
            {
                Log.Debug("Commit transaction", connector.Id);
                connector.ExecuteInternalCommand(PregeneratedMessage.CommitTransaction);
                Connection = null;
            }
        }

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        [PublicAPI]
        public Task CommitAsync(CancellationToken cancellationToken) => CommitInternalAsync(cancellationToken);

        /// <summary>
        /// Commits the database transaction.
        /// </summary>
        [PublicAPI]
        public Task CommitAsync() => CommitAsync(CancellationToken.None);

        #endregion

        #region Rollback

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        public override void Rollback() => RollbackInternal();

        [RewriteAsync]
        void RollbackInternal()
        {
            var connector = CheckReady();
            using (connector.StartUserAction())
            {
                connector.Rollback();
                Connection = null;
            }
        }

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        [PublicAPI]
        public Task RollbackAsync(CancellationToken cancellationToken) => RollbackInternalAsync(cancellationToken);

        /// <summary>
        /// Rolls back a transaction from a pending state.
        /// </summary>
        [PublicAPI]
        public Task RollbackAsync() => RollbackInternalAsync(CancellationToken.None);

        #endregion

        #region Savepoints

        /// <summary>
        /// Creates a transaction save point.
        /// </summary>
        public void Save(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name can't be empty", nameof(name));
            if (name.Contains(";"))
                throw new ArgumentException("name can't contain a semicolon");
            Contract.EndContractBlock();

            var connector = CheckReady();
            using (connector.StartUserAction())
            {
                Log.Debug("Create savepoint", connector.Id);
                connector.ExecuteInternalCommand($"SAVEPOINT {name}");
            }
        }

        /// <summary>
        /// Rolls back a transaction from a pending savepoint state.
        /// </summary>
        public void Rollback(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name can't be empty", nameof(name));
            if (name.Contains(";"))
                throw new ArgumentException("name can't contain a semicolon");
            Contract.EndContractBlock();

            var connector = CheckReady();
            using (connector.StartUserAction())
            {
                Log.Debug("Rollback savepoint", connector.Id);
                connector.ExecuteInternalCommand($"ROLLBACK TO SAVEPOINT {name}");
            }
        }

        /// <summary>
        /// Rolls back a transaction from a pending savepoint state.
        /// </summary>
        public void Release(string name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("name can't be empty", nameof(name));
            if (name.Contains(";"))
                throw new ArgumentException("name can't contain a semicolon");
            Contract.EndContractBlock();

            var connector = CheckReady();
            using (connector.StartUserAction())
            {
                Log.Debug("Release savepoint", connector.Id);
                connector.ExecuteInternalCommand($"RELEASE SAVEPOINT {name}");
            }
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (_isDisposed) { return; }

            if (disposing && Connection != null) {
                Rollback();
            }

            _isDisposed = true;
            base.Dispose(disposing);
        }

        #endregion

        #region Checks

        EDBConnector CheckReady()
        {
            CheckDisposed();
            CheckCompleted();
            return Connection.CheckReadyAndGetConnector();
        }

        [ContractArgumentValidator]
        void CheckCompleted()
        {
            if (IsCompleted)
                throw new InvalidOperationException("This EDBTransaction has completed; it is no longer usable.");
            Contract.EndContractBlock();
        }

        [ContractArgumentValidator]
        void CheckDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(typeof(EDBTransaction).Name);
            Contract.EndContractBlock();
        }

        [ContractInvariantMethod]
        void ObjectInvariants()
        {
        }

        #endregion
    }
}
