#if !DNXCORE50
#region License
// The PostgreSQL License
//
// Copyright (C) 2015 The  EnterpriseDB.EDBClient Development Team
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
using System.Reflection;
using System.Transactions;
using  EnterpriseDB.EDBClient.Logging;

namespace  EnterpriseDB.EDBClient
{
    internal class EDBPromotableSinglePhaseNotification : IPromotableSinglePhaseNotification
    {
        private readonly EDBConnection _connection;
        private IsolationLevel _isolationLevel;
        private EDBTransaction _EDBTx;
        private EDBTransactionCallbacks _callbacks;
        private IEDBResourceManager _rm;
        private bool _inTransaction;
        internal bool InLocalTransaction { get { return _EDBTx != null;  } }

        static readonly EDBLogger Log = EDBLogManager.GetCurrentClassLogger();

        public EDBPromotableSinglePhaseNotification(EDBConnection connection)
        {
            _connection = connection;
        }

        public void Enlist(Transaction tx)
        {
            Log.Debug("Enlist");
            if (tx != null)
            {
                _isolationLevel = tx.IsolationLevel;
                if (!tx.EnlistPromotableSinglePhase(this))
                {
                    // must already have a durable resource
                    // start transaction
                    _EDBTx = _connection.BeginTransaction(ConvertIsolationLevel(_isolationLevel));
                    _inTransaction = true;
                    _rm = CreateResourceManager();
                    _callbacks = new EDBTransactionCallbacks(_connection);
                    _rm.Enlist(_callbacks, TransactionInterop.GetTransmitterPropagationToken(tx));
                    // enlisted in distributed transaction
                    // disconnect and cleanup local transaction
                    _connection.Connector.ClearTransaction();
                    _EDBTx.Dispose();
                    _EDBTx = null;
                }
            }
        }

        /// <summary>
        /// Used when a connection is closed
        /// </summary>
        public void Prepare()
        {
            Log.Debug("Prepare");
            if (_inTransaction)
            {
                // may not be null if Promote or Enlist is called first
                if (_callbacks == null)
                {
                    _callbacks = new EDBTransactionCallbacks(_connection);
                }
                _callbacks.PrepareTransaction();
                if (_EDBTx != null)
                {
                    // cancel the EDBTransaction since this will
                    // be handled by a two phase commit.
                    _connection.Connector.ClearTransaction();
                    _EDBTx.Dispose();
                    _EDBTx = null;
                    _connection.PromotableLocalTransactionEnded();
                }
            }
        }

        #region IPromotableSinglePhaseNotification Members

        public void Initialize()
        {
            Log.Debug("Initialize");
            _EDBTx = _connection.BeginTransaction(ConvertIsolationLevel(_isolationLevel));
            _inTransaction = true;
        }

        public void Rollback(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            Log.Debug("Rollback");
            // try to rollback the transaction with either the
            // ADO.NET transaction or the callbacks that managed the
            // two phase commit transaction.
            if (_EDBTx != null)
            {
                _EDBTx.Rollback();
                _EDBTx.Dispose();
                _EDBTx = null;
                singlePhaseEnlistment.Aborted();
                _connection.PromotableLocalTransactionEnded();
            }
            else if (_callbacks != null)
            {
                if (_rm != null)
                {
                    _rm.RollbackWork(_callbacks.GetName());
                    singlePhaseEnlistment.Aborted();
                }
                else
                {
                    _callbacks.RollbackTransaction();
                    singlePhaseEnlistment.Aborted();
                }
                _callbacks = null;
            }
            _inTransaction = false;
        }

        public void SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            Log.Debug("Single Phase Commit");
            if (_EDBTx != null)
            {
                _EDBTx.Commit();
                _EDBTx.Dispose();
                _EDBTx = null;
                singlePhaseEnlistment.Committed();
                _connection.PromotableLocalTransactionEnded();
            }
            else if (_callbacks != null)
            {
                if (_rm != null)
                {
                    _rm.CommitWork(_callbacks.GetName());
                    singlePhaseEnlistment.Committed();
                }
                else
                {
                    _callbacks.CommitTransaction();
                    singlePhaseEnlistment.Committed();
                }
                _callbacks = null;
            }
            _inTransaction = false;
        }

        #endregion

        #region ITransactionPromoter Members

        public byte[] Promote()
        {
            Log.Debug("Promote");
            _rm = CreateResourceManager();
            // may not be null if Prepare or Enlist is called first
            if (_callbacks == null)
            {
                _callbacks = new EDBTransactionCallbacks(_connection);
            }
            byte[] token = _rm.Promote(_callbacks);
            // mostly likely case for this is the transaction has been prepared.
            if (_EDBTx != null)
            {
                // cancel the EDBTransaction since this will
                // be handled by a two phase commit.
                _connection.Connector.ClearTransaction();
                _EDBTx.Dispose();
                _EDBTx = null;
                _connection.PromotableLocalTransactionEnded();
            }
            return token;
        }

        #endregion

        private static IEDBResourceManager _resourceManager;
        private static System.Runtime.Remoting.Lifetime.ClientSponsor _sponser;

        private static IEDBResourceManager CreateResourceManager()
        {
            // TODO: create network proxy for resource manager
            if (_resourceManager == null)
            {
                _sponser = new System.Runtime.Remoting.Lifetime.ClientSponsor();
                AppDomain rmDomain = AppDomain.CreateDomain("EDBResourceManager", AppDomain.CurrentDomain.Evidence, AppDomain.CurrentDomain.SetupInformation);
                _resourceManager =
                    (IEDBResourceManager)
                    rmDomain.CreateInstanceAndUnwrap(typeof (EDBResourceManager).Assembly.FullName,
                                                     typeof (EDBResourceManager).FullName);
                _sponser.Register((MarshalByRefObject)_resourceManager);
            }
            return _resourceManager;
            //return new EDBResourceManager();
        }

        private static System.Data.IsolationLevel ConvertIsolationLevel(IsolationLevel _isolationLevel)
        {
            switch (_isolationLevel)
            {
                case IsolationLevel.Chaos:
                    return System.Data.IsolationLevel.Chaos;
                case IsolationLevel.ReadCommitted:
                    return System.Data.IsolationLevel.ReadCommitted;
                case IsolationLevel.ReadUncommitted:
                    return System.Data.IsolationLevel.ReadUncommitted;
                case IsolationLevel.RepeatableRead:
                    return System.Data.IsolationLevel.RepeatableRead;
                case IsolationLevel.Serializable:
                    return System.Data.IsolationLevel.Serializable;
                case IsolationLevel.Snapshot:
                    return System.Data.IsolationLevel.Snapshot;
                case IsolationLevel.Unspecified:
                default:
                    return System.Data.IsolationLevel.Unspecified;
            }
        }
    }
}
#endif
