using EDBTypes;
using System;

namespace EnterpriseDB.EDBClient.Replication.PgOutput.Messages
{
    /// <summary>
    /// Logical Replication Protocol insert message
    /// </summary>
    public sealed class InsertMessage : PgOutputReplicationMessage
    {
        /// <summary>
        /// ID of the relation corresponding to the ID in the relation message.
        /// </summary>
        public uint RelationId { get; private set; }

        /// <summary>
        /// Columns representing the new row.
        /// </summary>
        public ReadOnlyMemory<TupleData> NewRow { get; private set; } = default!;

        internal InsertMessage Populate(
            EDBLogSequenceNumber walStart, EDBLogSequenceNumber walEnd, DateTime serverClock, uint relationId,
            ReadOnlyMemory<TupleData> newRow)
        {
            base.Populate(walStart, walEnd, serverClock);

            RelationId = relationId;
            NewRow = newRow;

            return this;
        }

        /// <inheritdoc />
#if NETSTANDARD2_0 || NETSTANDARD2_1 || NETCOREAPP3_1
        public override PgOutputReplicationMessage Clone()
#else
        public override InsertMessage Clone()
#endif
        {
            var clone = new InsertMessage();
            clone.Populate(WalStart, WalEnd, ServerClock, RelationId, NewRow.ToArray());
            return clone;
        }
    }
}
