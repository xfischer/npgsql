using EDBTypes;
using System;

namespace EnterpriseDB.EDBClient.Replication.PgOutput.Messages;

/// <summary>
/// Logical Replication Protocol begin prepare message
/// </summary>
public sealed class BeginPrepareMessage : PrepareMessageBase
{
    internal BeginPrepareMessage() {}

    internal new BeginPrepareMessage Populate(
        EDBLogSequenceNumber walStart, EDBLogSequenceNumber walEnd, DateTime serverClock,
        EDBLogSequenceNumber prepareLsn, EDBLogSequenceNumber prepareEndLsn, DateTime transactionPrepareTimestamp,
        uint transactionXid, string transactionGid)
    {
        base.Populate(walStart, walEnd, serverClock,
            prepareLsn: prepareLsn,
            prepareEndLsn: prepareEndLsn,
            transactionPrepareTimestamp: transactionPrepareTimestamp,
            transactionXid: transactionXid,
            transactionGid: transactionGid);
        return this;
    }
}

