using EDBTypes;
using System;

namespace EnterpriseDB.EDBClient.Replication.PgOutput.Messages;

/// <summary>
/// Logical Replication Protocol commit prepared message
/// </summary>
public sealed class CommitPreparedMessage : PreparedTransactionControlMessage
{
    /// <summary>
    /// Flags for the commit prepared; currently unused.
    /// </summary>
    public CommitPreparedFlags Flags { get; private set; }

    /// <summary>
    /// The LSN of the commit prepared.
    /// </summary>
    public EDBLogSequenceNumber CommitPreparedLsn => FirstLsn;

    /// <summary>
    /// The end LSN of the commit prepared transaction.
    /// </summary>
    public EDBLogSequenceNumber CommitPreparedEndLsn => SecondLsn;

    /// <summary>
    /// Commit timestamp of the transaction.
    /// </summary>
    public DateTime TransactionCommitTimestamp => Timestamp;

    internal CommitPreparedMessage() {}

    internal CommitPreparedMessage Populate(
        EDBLogSequenceNumber walStart, EDBLogSequenceNumber walEnd, DateTime serverClock, CommitPreparedFlags flags,
        EDBLogSequenceNumber commitPreparedLsn, EDBLogSequenceNumber commitPreparedEndLsn, DateTime transactionCommitTimestamp,
        uint transactionXid, string transactionGid)
    {
        base.Populate(walStart, walEnd, serverClock,
            firstLsn: commitPreparedLsn,
            secondLsn: commitPreparedEndLsn,
            timestamp: transactionCommitTimestamp,
            transactionXid: transactionXid,
            transactionGid: transactionGid);
        Flags = flags;
        return this;
    }

    /// <summary>
    /// Flags for the commit prepared; currently unused.
    /// </summary>
    [Flags]
    public enum CommitPreparedFlags : byte
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0
    }
}
