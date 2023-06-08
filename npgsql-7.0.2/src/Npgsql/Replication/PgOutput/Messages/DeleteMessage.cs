using EDBTypes;
using System;

namespace EnterpriseDB.EDBClient.Replication.PgOutput.Messages;

/// <summary>
/// Abstract base class for Logical Replication Protocol delete message types.
/// </summary>
public abstract class DeleteMessage : TransactionalMessage
{
    /// <summary>
    /// The relation for this <see cref="InsertMessage" />.
    /// </summary>
    public RelationMessage Relation { get; private set; } = null!;

    /// <summary>
    /// ID of the relation corresponding to the ID in the relation message.
    /// </summary>
    [Obsolete("Use Relation.RelationId")]
    public uint RelationId => Relation.RelationId;

    private protected DeleteMessage() {}

    private protected DeleteMessage Populate(
        EDBLogSequenceNumber walStart, EDBLogSequenceNumber walEnd, DateTime serverClock, uint? transactionXid,
        RelationMessage relation)
    {
        base.Populate(walStart, walEnd, serverClock, transactionXid);

        Relation = relation;

        return this;
    }
}