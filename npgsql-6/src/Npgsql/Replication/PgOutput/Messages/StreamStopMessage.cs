using EDBTypes;
using System;

namespace EnterpriseDB.EDBClient.Replication.PgOutput.Messages
{
    /// <summary>
    /// Logical Replication Protocol stream stop message
    /// </summary>
    public sealed class StreamStopMessage : PgOutputReplicationMessage
    {
        internal StreamStopMessage() {}

        internal new StreamStopMessage Populate(EDBLogSequenceNumber walStart, EDBLogSequenceNumber walEnd, DateTime serverClock)
        {
            base.Populate(walStart, walEnd, serverClock);
            return this;
        }
    }
}
