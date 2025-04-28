using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace EnterpriseDB.EDBClient;

/// <summary>
/// Provides DequeueMode options available.
/// </summary>
public static class EDBAQDequeueMode
{
    /// <summary>
    /// Read the message without locking.
    /// </summary>
    /// <value>Read the message without locking.</value>
    public static readonly int BROWSE = 0;

    /// <summary>
    /// This constant is defined, but will return an error if used.
    /// </summary>
    /// <value>Reads and gets a write lock on the message. </value>
    public static readonly int LOCKED = 1;

    /// <summary>
    /// Delete the message after reading; the default.
    /// </summary>
    /// <value>Deletes the message after reading. This is the default value.</value>
    public static readonly int REMOVE = 2;

    /// <summary>
    /// This constant is defined, but will return an error if used.
    /// </summary>
    /// <value>Confirms receipt of the message.</value>
    public static readonly int REMOVE_NODATA = 3;
}
