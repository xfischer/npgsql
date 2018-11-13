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
//
//	ConnectorPool.cs
// ------------------------------------------------------------------
//	Status
//		0.00.0000 - 06/17/2002 - ulrich sprick - creation
//		          - 05/??/2004 - Glen Parker<glenebob@nwlink.com> rewritten using
//                               System.Queue.

using System;
using System.Collections;
using System.Threading;
using System.Timers;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// This class manages all connector objects, pooled AND non-pooled.
    /// </summary>
    internal class EDBConnectorPool
    {
        /// <summary>
        /// A queue with an extra Int32 for keeping track of busy connections.
        /// </summary>
    private class ConnectorQueue : System.Collections.Queue
        {
            /// <summary>
            /// The number of pooled Connectors that belong to this queue but
            /// are currently in use.
            /// </summary>
            public Int32            UseCount = 0;
            public Int32            ConnectionLifeTime;
            public Int32            InactiveTime = 0;
            public Int32            MinPoolSize;

        }

        /// <value>Unique static instance of the connector pool
        /// mamager.</value>
        internal static EDBConnectorPool ConnectorPoolMgr = new EDBConnectorPool();
	
        public EDBConnectorPool()
        {
            PooledConnectors = new Hashtable();	
            Timer = new System.Timers.Timer(1000);
            Timer.AutoReset = true;
            Timer.Elapsed += new ElapsedEventHandler(TimerElapsedHandler);
            Timer.Start();

        }
        
        ~EDBConnectorPool()
        {
            Timer.Stop();
        }
        
        private void TimerElapsedHandler(object sender, ElapsedEventArgs e)
        {
            EDBConnector     Connector;
            lock (this)
            {
                foreach (ConnectorQueue Queue in PooledConnectors.Values)
                {
                    if (Queue.Count > 0)
                    {
                        if (Queue.Count + Queue.UseCount > Queue.MinPoolSize)
                        {
                            if (Queue.InactiveTime >= Queue.ConnectionLifeTime)
                            {
                                Int32 diff = Queue.Count + Queue.UseCount - Queue.MinPoolSize;
                                Int32 toBeClosed = (diff + 1) / 2;
                                if (diff < 2)
                                    diff = 2;
                                Queue.InactiveTime -= Queue.ConnectionLifeTime / (int)(Math.Log(diff) / Math.Log(2));
                                for (Int32 i = 0; i < toBeClosed; ++i)
                                {
                                    Connector = (EDBConnector)Queue.Dequeue();
                                    Connector.Close();
                                }
                            }
                            else
                            {
                                Queue.InactiveTime++;
                            }
                        }
                        else
                        {
                            Queue.InactiveTime = 0;
                        }
                    }
                    else
                    {
                        Queue.InactiveTime = 0;
                    }
                }
            }
        }



        /// <value>Map of index to unused pooled connectors, avaliable to the
        /// next RequestConnector() call.</value>
        /// <remarks>This hashmap will be indexed by connection string.
        /// This key will hold a list of queues of pooled connectors available to be used.</remarks>
        private Hashtable PooledConnectors;

        /// <value>Map of shared connectors, avaliable to the
        /// next RequestConnector() call.</value>
        /// <remarks>This hashmap will be indexed by connection string.
        /// This key will hold a list of shared connectors available to be used.</remarks>
        // To be implemented
        //private Hashtable SharedConnectors;

                    
        /// <value>Timer for tracking unused connections in pools.</value>
        // I used System.Timers.Timer because of bad experience with System.Threading.Timer
        // on Windows - it's going mad sometimes and don't respect interval was set.
        private System.Timers.Timer Timer;

        /// <summary>
        /// Searches the shared and pooled connector lists for a
        /// matching connector object or creates a new one.
        /// </summary>
        /// <param name="Connection">The EDBConnection that is requesting
        /// the connector. Its ConnectionString will be used to search the
        /// pool for available connectors.</param>
        /// <returns>A connector object.</returns>
        public EDBConnector RequestConnector (EDBConnection Connection)
        {
            EDBConnector     Connector;

            if (Connection.Pooling)
            {
                Connector = RequestPooledConnector(Connection);
            }
            else
            {
                Connector = GetNonPooledConnector(Connection);
            }

            return Connector;
        }

        /// <summary>
        /// Find a pooled connector.  Handle locking and timeout here.
        /// </summary>
        private EDBConnector RequestPooledConnector (EDBConnection Connection)
        {
            EDBConnector     Connector;
            Int32               timeoutMilliseconds = Connection.Timeout * 1000;

            lock(this)
            {
                Connector = RequestPooledConnectorInternal(Connection);
            }

            while (Connector == null && timeoutMilliseconds > 0)
            {
                Int32 ST = timeoutMilliseconds > 1000 ? 1000 : timeoutMilliseconds;

                Thread.Sleep(ST);
                timeoutMilliseconds -= ST;

                lock(this)
                {
                    Connector = RequestPooledConnectorInternal(Connection);
                }
            }

            if (Connector == null)
            {
                if (Connection.Timeout > 0)
                {
                    throw new Exception("Timeout while getting a connection from pool.");
                }
                else
                {
                    throw new Exception("Connection pool exceeds maximum size.");
                }
            }

            return Connector;
        }

        /// <summary>
        /// Find a pooled connector.  Handle shared/non-shared here.
        /// </summary>
        private EDBConnector RequestPooledConnectorInternal (EDBConnection Connection)
        {
            EDBConnector       Connector = null;
            Boolean               Shared = false;

            // If sharing were implemented, I suppose Shared would be set based
            // on some property on the Connection.

            if (! Shared)
            {
                Connector = GetPooledConnector(Connection);
            }
            else
            {
                // Connection sharing? What's that?
                throw new NotImplementedException("Internal: Shared pooling not implemented");
            }

            return Connector;
        }

        /// <summary>
        /// Releases a connector, possibly back to the pool for future use.
        /// </summary>
        /// <remarks>
        /// Pooled connectors will be put back into the pool if there is room.
        /// Shared connectors should just have their use count decremented
        /// since they always stay in the shared pool.
        /// </remarks>
        /// <param name="Connector">The connector to release.</param>
        /// <param name="ForceClose">Force the connector to close, even if it is pooled.</param>
        public void ReleaseConnector (EDBConnection Connection, EDBConnector Connector)
        {
            if (Connector.Pooled)
            {
                ReleasePooledConnector(Connection, Connector);
            }
            else
            {
                UngetNonPooledConnector(Connection, Connector);
            }
        }

        /// <summary>
        /// Release a pooled connector.  Handle locking here.
        /// </summary>
        private void ReleasePooledConnector (EDBConnection Connection, EDBConnector Connector)
        {
            lock(this)
            {
                ReleasePooledConnectorInternal(Connection, Connector);
            }
        }

        /// <summary>
        /// Release a pooled connector.  Handle shared/non-shared here.
        /// </summary>
        private void ReleasePooledConnectorInternal (EDBConnection Connection, EDBConnector Connector)
        {
            if (! Connector.Shared)
            {
                UngetPooledConnector(Connection, Connector);
            }
            else
            {
                // Connection sharing? What's that?
                throw new NotImplementedException("Internal: Shared pooling not implemented");
            }
        }

        /// <summary>
        /// Create a connector without any pooling functionality.
        /// </summary>
        private EDBConnector GetNonPooledConnector(EDBConnection Connection)
        {
            EDBConnector       Connector;

            Connector = CreateConnector(Connection);

            Connector.CertificateSelectionCallback += Connection.CertificateSelectionCallbackDelegate;
            Connector.CertificateValidationCallback += Connection.CertificateValidationCallbackDelegate;
            Connector.PrivateKeySelectionCallback += Connection.PrivateKeySelectionCallbackDelegate;

            Connector.Open();

            return Connector;
        }

        /// <summary>
        /// Find an available pooled connector in the non-shared pool, or create
        /// a new one if none found.
        /// </summary>
        private EDBConnector GetPooledConnector(EDBConnection Connection)
        {
            ConnectorQueue        Queue;
            EDBConnector       Connector = null;

            // Try to find a queue.
            Queue = (ConnectorQueue)PooledConnectors[Connection.ConnectionString.ToString()];

            if (Queue == null)
            {
                Queue = new ConnectorQueue();
                Queue.ConnectionLifeTime = Connection.ConnectionLifeTime;
                Queue.MinPoolSize = Connection.MinPoolSize;
                PooledConnectors[Connection.ConnectionString.ToString()] = Queue;
            }

            if (Queue.Count > 0)
            {
                // Found a queue with connectors.  Grab the top one.

                // Check if the connector is still valid.

                while (true)
                {
                    Connector = (EDBConnector)Queue.Dequeue();
                    if (Connector.IsValid())
                    {
                        Queue.UseCount++;
                        break;
                    }

                    // Don't need - we dequeue connector = decrease Queue.Count.
                    //Queue.UseCount--;

                    if (Queue.Count <= 0)
                        return GetPooledConnector(Connection);


                }



            }
            else if (Queue.Count + Queue.UseCount < Connection.MaxPoolSize)
            {
                Connector = CreateConnector(Connection);

                Connector.CertificateSelectionCallback += Connection.CertificateSelectionCallbackDelegate;
                Connector.CertificateValidationCallback += Connection.CertificateValidationCallbackDelegate;
                Connector.PrivateKeySelectionCallback += Connection.PrivateKeySelectionCallbackDelegate;

                try
                {
                    Connector.Open();
                }
                catch {
                    try
                    {
                        Connector.Close();
                        }
                        catch {}

                        throw;
                    }


                    Queue.UseCount++;
					
        }

        // Meet the MinPoolSize requirement if needed.
        if (Connection.MinPoolSize > 0)
            {
                while (Queue.Count + Queue.UseCount < Connection.MinPoolSize)
                {
                    EDBConnector Spare = CreateConnector(Connection);

                    Spare.CertificateSelectionCallback += Connection.CertificateSelectionCallbackDelegate;
                    Spare.CertificateValidationCallback += Connection.CertificateValidationCallbackDelegate;
                    Spare.PrivateKeySelectionCallback += Connection.PrivateKeySelectionCallbackDelegate;

                    Spare.Open();

                    Spare.CertificateSelectionCallback -= Connection.CertificateSelectionCallbackDelegate;
                    Spare.CertificateValidationCallback -= Connection.CertificateValidationCallbackDelegate;
                    Spare.PrivateKeySelectionCallback -= Connection.PrivateKeySelectionCallbackDelegate;

                    Queue.Enqueue(Connector);
                }
            }

            return Connector;
        }

        /// <summary>
        /// Find an available shared connector in the shared pool, or create
        /// a new one if none found.
        /// </summary>
        private EDBConnector GetSharedConnector(EDBConnection Connection)
        {
            // To be implemented

            return null;
        }

        private EDBConnector CreateConnector(EDBConnection Connection)
        {
            return new EDBConnector(
                       Connection.ConnectionStringValues.Clone(),
                       Connection.Pooling,
                       false
                   );
        }


        /// <summary>
        /// This method is only called when EDBConnection.Dispose(false) is called which means a
        /// finalization. This also means, an EDBConnection was leak. We clear pool count so that
        /// client doesn't end running out of connections from pool. When the connection is finalized, its underlying
        /// socket is closed.
        /// </summary
        public void FixPoolCountBecauseOfConnectionDisposeFalse(EDBConnection Connection)
        {
            ConnectorQueue           Queue;

            // Prevent multithread access to connection pool count.
            lock(this)
            {
                // Try to find a queue.
                Queue = (ConnectorQueue)PooledConnectors[Connection.ConnectionString.ToString()];

                if (Queue != null)
                    Queue.UseCount--;

            }
        }

        /// <summary>
        /// Close the connector.
        /// </summary>
        /// <param name="Connector">Connector to release</param>
        private void UngetNonPooledConnector(EDBConnection Connection, EDBConnector Connector)
        {
            Connector.CertificateSelectionCallback -= Connection.CertificateSelectionCallbackDelegate;
            Connector.CertificateValidationCallback -= Connection.CertificateValidationCallbackDelegate;
            Connector.PrivateKeySelectionCallback -= Connection.PrivateKeySelectionCallbackDelegate;

            if (Connector.Transaction != null)
            {
                Connector.Transaction.Cancel();
            }

            Connector.Close();
        }

        /// <summary>
        /// Put a pooled connector into the pool queue.
        /// </summary>
        /// <param name="Connector">Connector to pool</param>
        private void UngetPooledConnector(EDBConnection Connection, EDBConnector Connector)
        {
            ConnectorQueue           Queue;

            // Find the queue.
            Queue = (ConnectorQueue)PooledConnectors[Connector.ConnectionString.ToString()];

            if (Queue == null)
            {
                throw new InvalidOperationException("Internal: No connector queue found for existing connector.");
            }

            Connector.CertificateSelectionCallback -= Connection.CertificateSelectionCallbackDelegate;
            Connector.CertificateValidationCallback -= Connection.CertificateValidationCallbackDelegate;
            Connector.PrivateKeySelectionCallback -= Connection.PrivateKeySelectionCallbackDelegate;

            Queue.UseCount--;

            if (! Connector.IsInitialized)
            {
                if (Connector.Transaction != null)
                {
                    Connector.Transaction.Cancel();
                }

                Connector.Close();
            }
            else
            {
                if (Connector.Transaction != null)
                {
                    try
                    {
                        Connector.Transaction.Rollback();
                    }
                    catch {
                        Connector.Close()
                        ;
                    }
                }
        }

        if (Connector.State == System.Data.ConnectionState.Open)
            {
                // Release all plans and portals associated with this connector.
                Connector.ReleasePlansPortals();

                Queue.Enqueue(Connector);
            }
        }

        /// <summary>
        /// Stop sharing a shared connector.
        /// </summary>
        /// <param name="Connector">Connector to unshare</param>
        private void UngetSharedConnector(EDBConnection Connection, EDBConnector Connector)
        {
            // To be implemented
        }
		private void ClearQueue(ConnectorQueue Queue)
		{
			if (Queue == null)
				return;

			while (Queue.Count > 0)
			{
				EDBConnector connector = (EDBConnector)Queue.Dequeue();

				try
				{
					connector.Close();
				}
				catch 
				{
					// Maybe we should log something here to say we got an exception while closing connector?

				}

			}

		}


		internal void ClearPool(EDBConnection Connection)
		{
			// Prevent multithread access to connection pool count.
			lock(this)
			{
				// Try to find a queue.
				ConnectorQueue queue = (ConnectorQueue)PooledConnectors[Connection.ConnectionString.ToString()];
                
				ClearQueue(queue);

				PooledConnectors[Connection.ConnectionString.ToString()] = null;

			}


		}

		internal void ClearAllPools()
		{

			lock (this)
			{
				foreach (ConnectorQueue Queue in PooledConnectors.Values)
					ClearQueue(Queue);

			}


		}
	
	
	}
}
