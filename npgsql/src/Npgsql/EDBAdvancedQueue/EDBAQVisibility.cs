namespace EnterpriseDB.EDBClient;

/// <summary>
/// Provides Visibility options available
/// </summary>
public static class EDBAQVisibility
{
    /// <summary>
    /// The dequeue is part of the current transaction.
    /// </summary>
    /// <value>The dequeue is part of the current transaction.</value>
    public static readonly int ON_COMMIT = 0;
    /// <summary>
    /// This constant is defined, but will return an error if used
    /// </summary>
    /// <value> This constant is defined, but will return an error if used.</value>
    public static readonly int IMMEDIATE = 1;
}
