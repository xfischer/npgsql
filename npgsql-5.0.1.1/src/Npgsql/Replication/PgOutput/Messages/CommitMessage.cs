using EDBTypes;
using System;

namespace EnterpriseDB.EDBClient.Replication.PgOutput.Messages
{
    /// <summary>
    /// Logical Replication Protocol commit message
    /// </summary>
    public sealed class CommitMessage : PgOutputReplicationMessage
    {
        /// <summary>
        /// Flags; currently unused (must be 0).
        /// </summary>
        public byte Flags { get; private set; }

        /// <summary>
        /// The LSN of the commit.
        /// </summary>
        public EDBLogSequenceNumber CommitLsn { get; private set; }

        /// <summary>
        /// The end LSN of the transaction.
        /// </summary>
        public EDBLogSequenceNumber TransactionEndLsn { get; private set; }

        /// <summary>
        /// Commit timestamp of the transaction.
        /// </summary>
        public DateTime TransactionCommitTimestamp { get; private set; }

        internal CommitMessage Populate(EDBLogSequenceNumber walStart, EDBLogSequenceNumber walEnd, DateTime serverClock, byte flags,
            EDBLogSequenceNumber commitLsn, EDBLogSequenceNumber transactionEndLsn, DateTime transactionCommitTimestamp)
        {
            base.Populate(walStart, walEnd, serverClock);

            Flags = flags;
            CommitLsn = commitLsn;
            TransactionEndLsn = transactionEndLsn;
            TransactionCommitTimestamp = transactionCommitTimestamp;

            return this;
        }

        /// <inheritdoc />
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP3_1
        public override PgOutputReplicationMessage Clone()
#else
        public override CommitMessage Clone()
#endif
        {
            var clone = new CommitMessage();
            clone.Populate(WalStart, WalEnd, ServerClock, Flags, CommitLsn, TransactionEndLsn, TransactionCommitTimestamp);
            return clone;
        }
    }
}
