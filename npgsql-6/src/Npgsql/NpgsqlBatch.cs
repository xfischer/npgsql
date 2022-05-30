using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using EnterpriseDB.EDBClient.Internal;

namespace EnterpriseDB.EDBClient
{
    /// <inheritdoc />
    public class EDBBatch : DbBatch
    {
        readonly EDBCommand _command;

        /// <inheritdoc />
        protected override DbBatchCommandCollection DbBatchCommands => BatchCommands;

        /// <inheritdoc cref="DbBatch.BatchCommands"/>
        public new EDBBatchCommandCollection BatchCommands { get; }

        /// <inheritdoc />
        public override int Timeout
        {
            get => _command.CommandTimeout;
            set => _command.CommandTimeout = value;
        }

        /// <inheritdoc cref="DbBatch.Connection"/>
        public new EDBConnection? Connection
        {
            get => _command.Connection;
            set => _command.Connection = value;
        }

        /// <inheritdoc />
        protected override DbConnection? DbConnection
        {
            get => Connection;
            set => Connection = (EDBConnection?)value;
        }

        /// <inheritdoc cref="DbBatch.Transaction"/>
        public new EDBTransaction? Transaction
        {
            get => _command.Transaction;
            set => _command.Transaction = value;
        }

        /// <inheritdoc />
        protected override DbTransaction? DbTransaction
        {
            get => Transaction;
            set => Transaction = (EDBTransaction?)value;
        }

        /// <summary>
        /// Marks all of the batch's result columns as either known or unknown.
        /// Unknown results column are requested them from PostgreSQL in text format, and EDB makes no
        /// attempt to parse them. They will be accessible as strings only.
        /// </summary>
        internal bool AllResultTypesAreUnknown
        {
            get => _command.AllResultTypesAreUnknown;
            set => _command.AllResultTypesAreUnknown = value;
        }

        /// <summary>
        /// Initializes a new <see cref="EDBBatch"/>.
        /// </summary>
        /// <param name="connection">A <see cref="EDBConnection"/> that represents the connection to a PostgreSQL server.</param>
        /// <param name="transaction">The <see cref="EDBTransaction"/> in which the <see cref="EDBCommand"/> executes.</param>
        public EDBBatch(EDBConnection? connection = null, EDBTransaction? transaction = null)
        {
            var batchCommands = new List<EDBBatchCommand>(5);
            _command = new(batchCommands);
            BatchCommands = new EDBBatchCommandCollection(batchCommands);

            Connection = connection;
            Transaction = transaction;
        }

        internal EDBBatch(EDBConnector connector)
        {
            var batchCommands = new List<EDBBatchCommand>(5);
            _command = new(connector, batchCommands);
            BatchCommands = new EDBBatchCommandCollection(batchCommands);
        }

        /// <inheritdoc />
        protected override DbBatchCommand CreateDbBatchCommand()
            => new EDBBatchCommand();

        /// <inheritdoc />
        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
            => ExecuteReader(behavior);

        /// <inheritdoc cref="DbBatch.ExecuteReader"/>
        public new EDBDataReader ExecuteReader(CommandBehavior behavior = CommandBehavior.Default)
            => _command.ExecuteReader();

        /// <inheritdoc />
        protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(
            CommandBehavior behavior,
            CancellationToken cancellationToken)
            => await ExecuteReaderAsync(cancellationToken);

        /// <inheritdoc cref="DbBatch.ExecuteReaderAsync(CancellationToken)"/>
        public new Task<EDBDataReader> ExecuteReaderAsync(CancellationToken cancellationToken = default)
            => _command.ExecuteReaderAsync(cancellationToken);

        /// <inheritdoc cref="DbBatch.ExecuteReaderAsync(CommandBehavior,CancellationToken)"/>
        public new Task<EDBDataReader> ExecuteReaderAsync(
            CommandBehavior behavior,
            CancellationToken cancellationToken = default)
            => _command.ExecuteReaderAsync(behavior, cancellationToken);

        /// <inheritdoc />
        public override int ExecuteNonQuery()
            => _command.ExecuteNonQuery();

        /// <inheritdoc />
        public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken = default)
            => _command.ExecuteNonQueryAsync(cancellationToken);

        /// <inheritdoc />
        public override object? ExecuteScalar()
            => _command.ExecuteScalar();

        /// <inheritdoc />
        public override Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken = default)
            => _command.ExecuteScalarAsync(cancellationToken);

        /// <inheritdoc />
        public override void Prepare()
            => _command.Prepare();

        /// <inheritdoc />
        public override Task PrepareAsync(CancellationToken cancellationToken = default)
            => _command.PrepareAsync(cancellationToken);

        /// <inheritdoc />
        public override void Cancel() => _command.Cancel();
    }
}
