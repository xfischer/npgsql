

namespace EnterpriseDB.EDBClient;

/// <summary>
/// Provides Message states available.
/// </summary>
public static class EDBAQMessageState
{
    /// <summary>
    /// 
    /// </summary>
    /// <value>The message is moved to the exception queue.</value>
    public static readonly int READY = 0;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The message is processed and kept.</value>
    public static readonly int WAITING = 1;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The message is ready to be processed.</value>
    public static readonly int PROCESSED = 2;
    /// <summary>
    /// 
    /// </summary>
    /// <value>The message is in waiting state. The delay is not reached.</value>
    public static readonly int EXPIRED = 3;
}
