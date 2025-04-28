
namespace EnterpriseDB.EDBClient;

/// <summary>
/// Provides Navigation Modes available.
/// </summary>
public static class EDBAQNavigationMode
{
    /// <summary>
    /// Return the first available message that matches the search terms.
    /// </summary>
    /// <value>Returns the first available message that matches the search terms.</value>
    readonly static public int FIRST_MESSAGE = 0;
    /// <summary>
    /// Return the next available message that matches the search terms.
    /// </summary>
    /// <value>Returns the next available message that matches the search items.</value>
    readonly static public int NEXT_MESSAGE = 1;
    /// <summary>
    /// This constant is defined, but will return an error if used.
    /// </summary>
    /// <value>Returns the first message of next transaction group.</value>
    readonly static public int NEXT_TRANSACTION = 2;
}
