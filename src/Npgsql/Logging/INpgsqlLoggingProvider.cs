namespace EnterpriseDB.EDBClient.Logging
{
    /// Used to create logger instances of the given name.
    public interface IEDBLoggingProvider
    {
        /// <summary>
        /// Creates a new IEDBLogger instance of the given name.
        /// </summary>
        EDBLogger CreateLogger(string name);
    }
}
