using System;

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Thrown when trying to use a connection that is already busy performing some other operation.
    /// Provides information on the already-executing operation to help with debugging.
    /// </summary>
    public sealed class EDBOperationInProgressException : InvalidOperationException
    {
        internal EDBOperationInProgressException(EDBCommand command)
            : base("A command is already in progress: " + command.CommandText)
        {
            CommandInProgress = command;
        }

        internal EDBOperationInProgressException(ConnectorState state)
            : base($"The connection is already in state '{state}'")
        {
        }

        /// <summary>
        /// If the connection is busy with another command, this will contain a reference to that command.
        /// Otherwise, if the connection if busy with another type of operation (e.g. COPY), contains null.
        /// </summary>
        public EDBCommand? CommandInProgress { get; }
    }
}
