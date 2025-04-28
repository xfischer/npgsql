namespace EnterpriseDB.EDBClient;

/// <summary>
/// Provides Messaging options for enque/dequeue.
/// </summary>
public class EDBAQMessage
{
    /// <summary>
    /// Payload
    /// </summary>
    /// <value>The actual message to be queued.</value>
    public object? Payload { get; set; }
    /// <summary>
    /// MessageId
    /// </summary>
    /// <value>The ID of the queued message.</value>
    public string? MessageId { get; set; }
    
}
