using System;
using System.Globalization;
using System.Threading;

// This type has been moved to the EnterpriseDB.EDBClient.Util namespace for 5.0, but is kept here for backwards compatibility
// with 4.0.

namespace EnterpriseDB.EDBClient
{
    /// <summary>
    /// Represents a timeout that will expire at some point.
    /// </summary>
    public readonly struct EDBTimeout
    {
        readonly DateTime _expiration;
        internal DateTime Expiration => _expiration;

        internal static EDBTimeout Infinite = new EDBTimeout(TimeSpan.Zero);

        internal EDBTimeout(TimeSpan expiration)
        {
            _expiration = expiration == TimeSpan.Zero
                ? DateTime.MaxValue
                : DateTime.UtcNow + expiration;
        }

        internal void Check()
        {
            if (HasExpired)
                throw new TimeoutException();
        }

        internal bool IsSet => _expiration != DateTime.MaxValue;

        internal bool HasExpired => DateTime.UtcNow >= Expiration;

        internal TimeSpan TimeLeft => IsSet ? Expiration - DateTime.UtcNow : Timeout.InfiniteTimeSpan;
    }

    sealed class CultureSetter : IDisposable
    {
        readonly CultureInfo _oldCulture;

        internal CultureSetter(CultureInfo newCulture)
        {
            _oldCulture = CultureInfo.CurrentCulture;
#if NET461 || NET472 || NET48
            Thread.CurrentThread.CurrentCulture = newCulture;
#else
            CultureInfo.CurrentCulture = newCulture;
#endif
        }

        public void Dispose()
        {
#if NET461 || NET472 || NET48
            Thread.CurrentThread.CurrentCulture = _oldCulture;
#else
            CultureInfo.CurrentCulture = _oldCulture;
#endif
        }
    }
}
