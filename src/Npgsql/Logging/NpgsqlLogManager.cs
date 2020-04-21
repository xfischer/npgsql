using System;

namespace EnterpriseDB.EDBClient.Logging
{
    /// <summary>
    /// Manages logging for EDB, used to set the logging provider.
    /// </summary>
    public static class EDBLogManager
    {
        /// <summary>
        /// The logging provider used for logging in EnterpriseDB.EDBClient.
        /// </summary>
        public static IEDBLoggingProvider Provider
        {
            get
            {
                _providerRetrieved = true;
                return _provider!;
            }
            set
            {
                if (_providerRetrieved)
                    throw new InvalidOperationException("The logging provider must be set before any EDB action is taken");

                _provider = value ?? throw new ArgumentNullException(nameof(value));
            }
        }

        /// <summary>
        /// Determines whether parameter contents will be logged alongside SQL statements - this may reveal sensitive information.
        /// Defaults to false.
        /// </summary>
        public static bool IsParameterLoggingEnabled { get; set; }

        static IEDBLoggingProvider? _provider;
        static bool _providerRetrieved;

        internal static EDBLogger CreateLogger(string name) => Provider.CreateLogger("EnterpriseDB.EDBClient." + name);

        static EDBLogManager() => Provider = new NoOpLoggingProvider();
    }
}
