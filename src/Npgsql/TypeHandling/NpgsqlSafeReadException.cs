using System;

namespace EnterpriseDB.EDBClient.TypeHandling
{
    /// <summary>
    /// Can be thrown by readers to indicate that interpreting the value failed, but the value was read wholly
    /// and it is safe to continue reading. Any other exception is assumed to leave the buffer in an unknown position,
    /// losing protocol sync and therefore setting the connector to state Broken.
    /// Note that an inner exception is mandatory, and will get thrown to the user instead of the EDBSafeReadException.
    /// </summary>
    public class EDBSafeReadException : Exception
    {
        /// <summary>
        /// Creates an instance of <see cref="EDBSafeReadException"/>.
        /// </summary>
        /// <param name="innerException"></param>
        public EDBSafeReadException(Exception innerException) : base("", innerException)
        {
            if (innerException == null)
                throw new ArgumentNullException(nameof(innerException));
        }
    }
}
