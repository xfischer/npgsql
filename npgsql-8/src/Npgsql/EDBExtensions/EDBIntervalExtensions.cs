using EDBTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnterpriseDB.EDBClient;


/// <summary>
/// Extensions to parsing and TimeSpan conversions of EDBInterval
/// </summary>
internal static class EDBIntervalExtensions
{
    #region Constants

    /// <summary>
    /// Represents the number of ticks (100ns periods) in one microsecond. This field is constant.
    /// </summary>
    internal const long TicksPerMicrosecond = TimeSpan.TicksPerMillisecond / 1000;

    /// <summary>
    /// Represents the number of ticks (100ns periods) in one millisecond. This field is constant.
    /// </summary>
    internal const long TicksPerMillsecond = TimeSpan.TicksPerMillisecond;

    /// <summary>
    /// Represents the number of ticks (100ns periods) in one second. This field is constant.
    /// </summary>
    internal const long TicksPerSecond = TimeSpan.TicksPerSecond;

    /// <summary>
    /// Represents the number of ticks (100ns periods) in one minute. This field is constant.
    /// </summary>
    internal const long TicksPerMinute = TimeSpan.TicksPerMinute;

    /// <summary>
    /// Represents the number of ticks (100ns periods) in one hour. This field is constant.
    /// </summary>
    internal const long TicksPerHour = TimeSpan.TicksPerHour;

    /// <summary>
    /// Represents the number of ticks (100ns periods) in one day. This field is constant.
    /// </summary>
    internal const long TicksPerDay = TimeSpan.TicksPerDay;

    /// <summary>
    /// Represents the number of hours in one day (assuming no daylight savings adjustments). This field is constant.
    /// </summary>
    internal const int HoursPerDay = 24;

    /// <summary>
    /// Represents the number of days assumed in one month if month justification or unjustifcation is performed.
    /// This is set to 30 for consistency with PostgreSQL. Note that this is means that month adjustments cause
    /// a year to be taken as 30 &#xd7; 12 = 360 rather than 356/366 days.
    /// </summary>
    internal const int DaysPerMonth = 30;

    /// <summary>
    /// Represents the number of ticks (100ns periods) in one day, assuming 30 days per month. <seealso cref="DaysPerMonth"/>
    /// </summary>
    internal const long TicksPerMonth = TicksPerDay * DaysPerMonth;

    /// <summary>
    /// Represents the number of months in a year. This field is constant.
    /// </summary>
    internal const int MonthsPerYear = 12;


    #endregion


    internal static EDBInterval Parse(string str)
    {
        if (str == null)
        {
#if NET6_0_OR_GREATER
            ArgumentNullException.ThrowIfNull(str, nameof(str));
#else
            throw new ArgumentNullException(nameof(str));
#endif
        }
        str = str.Replace('s', ' '); //Quick and easy way to catch plurals.
        try
        {
            int years = 0;
            int months = 0;
            int days = 0;
            int hours = 0;
            int minutes = 0;
            decimal seconds = 0m;
            int idx = str.IndexOf("year", StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                years = int.Parse(str.Substring(0, idx));
                str = SafeSubstring(str, idx + 5);
            }
            idx = str.IndexOf("mon", StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                months = int.Parse(str.Substring(0, idx));
                str = SafeSubstring(str, idx + 4);
            }
            idx = str.IndexOf("day", StringComparison.OrdinalIgnoreCase);
            if (idx > 0)
            {
                days = int.Parse(str.Substring(0, idx));
                str = SafeSubstring(str, idx + 4).Trim();
            }
            if (str.Length > 0)
            {
                bool isNegative = str[0] == '-';
                string[] parts = str.Split(':');
                switch (parts.Length) //One of those times that fall-through would actually be good.
                {
                case 1:
                    hours = int.Parse(parts[0]);
                    break;
                case 2:
                    hours = int.Parse(parts[0]);
                    minutes = int.Parse(parts[1]);
                    break;
                default:
                    hours = int.Parse(parts[0]);
                    minutes = int.Parse(parts[1]);
                    seconds = decimal.Parse(parts[2], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    break;
                }
                if (isNegative)
                {
                    minutes *= -1;
                    seconds *= -1;
                }
            }
            long ticks = hours * TicksPerHour + minutes * TicksPerMinute + (long)(seconds * TicksPerSecond);
            return new EDBInterval(years * MonthsPerYear + months, days, ticks);
        }
        catch (OverflowException)
        {
            throw;
        }
        catch (Exception)
        {
            throw new FormatException();
        }
    }

    /// <summary>
    /// Converts an EDBInterval instance to TimeSpan
    /// </summary>
    /// <param name="interval"></param>
    /// <returns></returns>
    internal static TimeSpan ToTimeSpan(this EDBInterval interval)
    {
        var ts = TimeSpan.FromDays(DaysPerMonth * interval.Months + interval.Days);

#if NET7_0_OR_GREATER
        ts += TimeSpan.FromMicroseconds(interval.Time);
#else
        ts += TimeSpan.FromMilliseconds(interval.Time / 1_000d);
#endif

        return ts;
    }

    private static string SafeSubstring(string s, int startIndex)
    {
        if (startIndex >= s.Length)
            return string.Empty;
        else
            return s.Substring(startIndex);
    }
}
