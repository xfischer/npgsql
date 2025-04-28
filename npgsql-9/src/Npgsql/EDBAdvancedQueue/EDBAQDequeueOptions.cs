using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient;

/// <summary>
/// Provides DequeueOptions available.
/// </summary>
public class EDBAQDequeueOptions
{
    /// <summary>
    /// 
    /// </summary>
    /// <value>Confirms receipt of the message.</value>
    public string? ConsumerName { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <value>The name of the consumer for which to dequeue the message.</value>
    public int DequeueMode { get; set; } = EDBAQDequeueMode.REMOVE;
    /// <summary>
    /// 
    /// </summary>
    /// <value>This is set from EDBAQDequeueMode. It represents the locking behavior linked with the dequeue option.</value>
    public int Navigation { get; set; } = EDBAQNavigationMode.NEXT_MESSAGE;
    /// <summary>
    /// 
    /// </summary>
    /// <value>This is set from EDBAQNavigationMode.  It represents the position of the message that will be fetched.</value>
    public int? Visibility { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <value>This is set from EDBAQVisibility. It represents whether the new message is dequeued or not as part of the current transaction.</value>
    public int Wait { get; set; } = 0; // No wait
    /// <summary>
    /// 
    /// </summary>
    /// <value>The wait time for a message as per the search criteria.</value>
    public byte[]? Msgid { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <value>The message identifier.</value>
    public string? Correlation { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <value>The correlation identifier.</value>
    public string? DeqCondition { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <value>The dequeuer condition. It is a Boolean expression.</value>
    public string? Transformation { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// <value>The transformation that will be applied before dequeuing the message.</value>
    public int DeliveryMode { get; set; } = 0;

}
