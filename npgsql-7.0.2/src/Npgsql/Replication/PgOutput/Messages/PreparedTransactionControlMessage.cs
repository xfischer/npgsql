using EDBTypes;
using System;

namespace EnterpriseDB.EDBClient.Replication.PgOutput.Messages;

/// <summary>
/// Abstract base class for Logical Replication Protocol prepare and begin prepare message
/// </summary>
public abstract class PreparedTransactionControlMessage : TransactionControlMessage
{
    private protected EDBLogSequenceNumber FirstLsn;
    private protected EDBLogSequenceNumber SecondLsn;
    private protected DateTime Timestamp;

    /// <summary>
    /// The user defined GID of the two-phase transaction.
    /// </summary>
    public string TransactionGid { get; private set; } = null!;

    private protected PreparedTransactionControlMessage() {}

    private protected PreparedTransactionControlMessage Populate(
        EDBLogSequenceNumber walStart, EDBLogSequenceNumber walEnd, DateTime serverClock,
        EDBLogSequenceNumber firstLsn, EDBLogSequenceNumber secondLsn, DateTime timestamp,
        uint transactionXid, string transactionGid)
    {
        base.Populate(walStart, walEnd, serverClock, transactionXid);

        FirstLsn = firstLsn;
        SecondLsn = secondLsn;
        Timestamp = timestamp;
        TransactionGid = transactionGid;

        return this;
    }
}

