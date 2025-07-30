using EDBTypes;
using System;

namespace EnterpriseDB.EDBClient.Replication.PgOutput.Messages;

/// <summary>
/// Abstract base class for the logical replication protocol begin prepare and prepare message
/// </summary>
public abstract class PrepareMessageBase : PreparedTransactionControlMessage
{
    /// <summary>
    /// The LSN of the prepare.
    /// </summary>
    public EDBLogSequenceNumber PrepareLsn => FirstLsn;

    /// <summary>
    /// The end LSN of the prepared transaction.
    /// </summary>
    public EDBLogSequenceNumber PrepareEndLsn => SecondLsn;

    /// <summary>
    /// Prepare timestamp of the transaction.
    /// </summary>
    public DateTime TransactionPrepareTimestamp => Timestamp;

    private protected PrepareMessageBase() {}

    internal new PrepareMessageBase Populate(
        EDBLogSequenceNumber walStart, EDBLogSequenceNumber walEnd, DateTime serverClock,
        EDBLogSequenceNumber prepareLsn, EDBLogSequenceNumber prepareEndLsn, DateTime transactionPrepareTimestamp,
        uint transactionXid, string transactionGid)
    {
        base.Populate(walStart, walEnd, serverClock,
            firstLsn: prepareLsn,
            secondLsn: prepareEndLsn,
            timestamp: transactionPrepareTimestamp,
            transactionXid: transactionXid,
            transactionGid: transactionGid);
        return this;
    }
}
